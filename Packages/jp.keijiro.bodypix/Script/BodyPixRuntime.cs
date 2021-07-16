using Unity.Barracuda;
using UnityEngine;

namespace BodyPix {

public sealed class BodyPixRuntime : System.IDisposable
{
    #region Public methods/properties

    public BodyPixRuntime(ResourceSet resources)
      => AllocateObjects(resources);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture sourceTexture)
      => RunModel(sourceTexture);

    public RenderTexture Stencil
      => _buffers.stencil;

    #endregion

    #region Private objects

    ResourceSet _resources;
    Config _config;
    IWorker _worker;

    (ComputeBuffer preprocess,
     RenderTexture segment,
     RenderTexture stencil) _buffers;

    void AllocateObjects(ResourceSet resources)
    {
        // NN model loading
        var model = ModelLoader.Load(resources.model);

        // Private object initialization
        _resources = resources;
        _config = new Config(model);
        _worker = model.CreateWorker();

        // Buffer allocation
        _buffers.preprocess = new ComputeBuffer
          (_config.InputFootprint, sizeof(float));

        _buffers.segment = RTUtil.NewFloat
          (_config.OutputWidth, _config.OutputHeight);

        _buffers.stencil = RTUtil.NewUAV
          (_config.OutputWidth, _config.OutputHeight);
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _buffers.preprocess?.Dispose();
        _buffers.preprocess = null;

        ObjectUtil.Destroy(_buffers.segment);
        _buffers.segment = null;

        ObjectUtil.Destroy(_buffers.stencil);
        _buffers.stencil = null;
    }

    #endregion

    #region Main inference function

    void RunModel(Texture source)
    {
        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetInts("Size", _config.InputWidth, _config.InputHeight);
        pre.SetTexture(0, "Image", source);
        pre.SetBuffer(0, "Tensor", _buffers.preprocess);
        pre.DispatchThreads(0, _config.InputWidth, _config.InputHeight, 1);

        // NN worker invocation
        using (var t = new Tensor(_config.InputShape, _buffers.preprocess))
            _worker.Execute(t);

        // NN output retrieval
        _worker.CopyOutput("float_segments", _buffers.segment);

        // Postprocessing
        var post = _resources.postprocess;
        post.SetTexture(0, "Input", _buffers.segment);
        post.SetTexture(0, "Output", _buffers.stencil);
        post.SetInts("InputSize", _config.OutputWidth, _config.OutputHeight);
        post.DispatchThreads(0, _config.OutputWidth, _config.OutputHeight, 1);
    }

    #endregion
}

} // namespace BodyPix
