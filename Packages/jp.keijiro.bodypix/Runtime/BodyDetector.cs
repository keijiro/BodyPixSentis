using Unity.Barracuda;
using UnityEngine;
using Klak.NNUtils;
using Klak.NNUtils.Extensions;

namespace BodyPix {

public sealed class BodyDetector : System.IDisposable
{
    #region Public methods/properties

    public BodyDetector(ResourceSet resources, int width, int height)
      => AllocateObjects(resources, width, height);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture sourceTexture)
      => RunModel(sourceTexture);

    public System.ReadOnlySpan<Keypoint> Keypoints
      => _readCache.Cached;

    public RenderTexture MaskTexture
      => _output.mask;

    public GraphicsBuffer KeypointBuffer
      => _output.keypoints;

    #endregion

    #region Private objects

    ResourceSet _resources;
    Config _config;
    IWorker _worker;
    ImagePreprocess _preprocess;
    (RenderTexture mask, GraphicsBuffer keypoints) _output;
    BufferReader<Keypoint> _readCache;

    void AllocateObjects(ResourceSet resources, int width, int height)
    {
        _resources = resources;

        // NN model
        var model = ModelLoader.Load(_resources.model);
        _config = new Config(model, _resources, width, height);

        // GPU worker
        _worker = model.CreateWorker(WorkerFactory.Device.GPU);

        // Preprocessing buffers
        _preprocess = new ImagePreprocess(_config.InputWidth, _config.InputHeight)
          { ColorCoeffs = _config.PreprocessCoeffs };

        // Output buffers
        _output.mask = RTUtil.NewArgbUav(_config.OutputWidth, _config.OutputHeight);
        _output.keypoints = BufferUtil.NewStructured<Vector4>(Body.KeypointCount);

        // Read cache
        _readCache = new BufferReader<Keypoint>(_output.keypoints, Body.KeypointCount);
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _preprocess?.Dispose();
        _preprocess = null;

        RTUtil.Destroy(_output.mask);
        _output.keypoints?.Dispose();
        _output = (null, null);
    }

    #endregion

    #region Main inference function

    void RunModel(Texture source)
    {
        // Preprocessing
        _preprocess.Dispatch(source, _resources.preprocess);

        // NN worker invocation
        _worker.Execute(_preprocess.Tensor);

        // Postprocessing (mask)
        var post1 = _resources.mask;
        post1.SetBuffer(0, "Segments", _worker.PeekOutputBuffer("segments"));
        post1.SetBuffer(0, "Heatmaps", _worker.PeekOutputBuffer("part_heatmaps"));
        post1.SetTexture(0, "Output", _output.mask);
        post1.SetInts("InputSize", _config.OutputWidth, _config.OutputHeight);
        post1.DispatchThreads(0, _config.OutputWidth, _config.OutputHeight, 1);

        // Postprocessing (keypoints)
        var post2 = _resources.keypoints;
        post2.SetBuffer(0, "Heatmaps", _worker.PeekOutputBuffer("heatmaps"));
        post2.SetBuffer(0, "Offsets", _worker.PeekOutputBuffer("short_offsets"));
        post2.SetInts("InputSize", _config.OutputWidth, _config.OutputHeight);
        post2.SetInt("Stride", _config.Stride);
        post2.SetBuffer(0, "Keypoints", _output.keypoints);
        post2.Dispatch(0, 1, 1, 1);

        // Cache data invalidation
        _readCache.InvalidateCache();
    }

    #endregion
}

} // namespace BodyPix
