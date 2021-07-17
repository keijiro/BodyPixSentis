using Unity.Barracuda;
using UnityEngine;

namespace BodyPix {

public sealed class BodyPixRuntime : System.IDisposable
{
    #region Public methods/properties

    public const int PartCount = 17;

    public BodyPixRuntime(ResourceSet resources, int width, int height)
      => AllocateObjects(resources, width, height);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture sourceTexture)
      => RunModel(sourceTexture);

    public RenderTexture Stencil
      => _buffers.stencil;

    public ComputeBuffer Keypoints
      => _buffers.keypoints;

    #endregion

    #region Private objects

    ResourceSet _resources;
    Config _config;
    IWorker _worker;

    (ComputeBuffer preprocess,
     RenderTexture segment,
     RenderTexture heatmaps,
     RenderTexture offsets,
     RenderTexture stencil,
     ComputeBuffer keypoints) _buffers;

    void AllocateObjects(ResourceSet resources, int width, int height)
    {
        // NN model loading
        var model = ModelLoader.Load(resources.model);

        // Private object initialization
        _resources = resources;
        _config = new Config(model, _resources, width, height);
        _worker = model.CreateWorker();

        // Buffer allocation
        _buffers.preprocess = new ComputeBuffer
          (_config.InputFootprint, sizeof(float));

        _buffers.segment = RTUtil.NewFloat
          (_config.OutputWidth, _config.OutputHeight);

        _buffers.heatmaps = RTUtil.NewFloat
          (_config.OutputWidth * PartCount, _config.OutputHeight);

        _buffers.offsets = RTUtil.NewFloat
          (_config.OutputWidth * PartCount * 2, _config.OutputHeight);

        _buffers.stencil = RTUtil.NewUAV
          (_config.OutputWidth, _config.OutputHeight);

        _buffers.keypoints = new ComputeBuffer
          (PartCount, sizeof(float) * 4);
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _buffers.preprocess?.Dispose();
        _buffers.preprocess = null;

        ObjectUtil.Destroy(_buffers.segment);
        _buffers.segment = null;

        ObjectUtil.Destroy(_buffers.heatmaps);
        _buffers.heatmaps = null;

        ObjectUtil.Destroy(_buffers.offsets);
        _buffers.offsets = null;

        ObjectUtil.Destroy(_buffers.stencil);
        _buffers.stencil = null;

        _buffers.keypoints?.Dispose();
        _buffers.keypoints = null;
    }

    #endregion

    #region Main inference function

    void RunModel(Texture source)
    {
        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetTexture(0, "Input", source);
        pre.SetBuffer(0, "Output", _buffers.preprocess);
        pre.SetInts("InputSize", _config.InputWidth, _config.InputHeight);
        pre.DispatchThreads(0, _config.InputWidth, _config.InputHeight, 1);

        // NN worker invocation
        using (var t = new Tensor(_config.InputShape, _buffers.preprocess))
            _worker.Execute(t);

        // NN output retrieval
        _worker.CopyOutput("segments", _buffers.segment);
        _worker.CopyOutput("heatmaps", _buffers.heatmaps);
        _worker.CopyOutput("short_offsets", _buffers.offsets);

        // Postprocessing (stencil)
        var post = _resources.stencil;
        post.SetTexture(0, "Input", _buffers.segment);
        post.SetTexture(0, "Output", _buffers.stencil);
        post.SetInts("InputSize", _config.OutputWidth, _config.OutputHeight);
        post.DispatchThreads(0, _config.OutputWidth, _config.OutputHeight, 1);

        // Postprocessing (keypoints)
        post = _resources.keypoints;
        post.SetTexture(0, "Heatmaps", _buffers.heatmaps);
        post.SetTexture(0, "Offsets", _buffers.offsets);
        post.SetInts("InputSize", _config.OutputWidth, _config.OutputHeight);
        post.SetInt("Stride", _config.Stride);
        post.SetBuffer(0, "Keypoints", _buffers.keypoints);
        post.Dispatch(0, 1, 1, 1);
    }

    #endregion
}

} // namespace BodyPix
