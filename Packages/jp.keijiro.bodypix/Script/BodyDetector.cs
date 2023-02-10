using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

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

    public IEnumerable<Keypoint> Keypoints
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
    (Tensor tensor, ComputeTensorData data) _preprocess;
    (RenderTexture mask, GraphicsBuffer keypoints) _output;
    KeypointCache _readCache;

    void AllocateObjects(ResourceSet resources, int width, int height)
    {
        // NN model
        var model = ModelLoader.Load(resources.model);

        // Private objects
        _resources = resources;
        _config = new Config(model, _resources, width, height);
        _worker = model.CreateWorker(WorkerFactory.Device.GPU);

        // Preprocessing buffers
#if BARRACUDA_4_0_0_OR_LATER
        _preprocess.data =
          new ComputeTensorData(_config.InputShape, "Preprocess", false);
        _preprocess.tensor = TensorFloat.Zeros(_config.InputShape);
        _preprocess.tensor.AttachToDevice(_preprocess.data);
#else
        _preprocess.data =
          new ComputeTensorData(_config.InputShape, "Preprocess",
                                ComputeInfo.ChannelsOrder.NHWC, false);
        _preprocess.tensor = new Tensor(_config.InputShape, _preprocess.data);
#endif

        // Output buffers
        _output.mask = BufferUtil.NewArgbUav(_config.OutputWidth, _config.OutputHeight);
        _output.keypoints = BufferUtil.NewFloat4(Body.KeypointCount);

        // Keypoint read cache
        _readCache = new KeypointCache(_output.keypoints);
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _preprocess.tensor?.Dispose();
        _preprocess = (null, null);

        BufferUtil.Destroy(_output.mask);
        _output.mask = null;

        _output.keypoints?.Dispose();
        _output.keypoints = null;
    }

    #endregion

    #region Main inference function

    void RunModel(Texture source)
    {
        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetTexture(0, "Input", source);
        pre.SetBuffer(0, "Output", _preprocess.data.buffer);
        pre.SetInts("InputSize", _config.InputWidth, _config.InputHeight);
        pre.SetVector("ColorCoeffs", _config.PreprocessCoeffs);
        pre.SetBool("InputIsLinear", ColorUtil.IsLinear);
        pre.DispatchThreads(0, _config.InputWidth, _config.InputHeight, 1);

        // NN worker invocation
        _worker.Execute(_preprocess.tensor);

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
        _readCache.Invalidate();
    }

    #endregion
}

} // namespace BodyPix
