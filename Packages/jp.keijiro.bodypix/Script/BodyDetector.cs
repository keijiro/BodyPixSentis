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
      => _buffers.mask;

    public GraphicsBuffer KeypointBuffer
      => _buffers.keypoints;

    #endregion

    #region Private objects

    ResourceSet _resources;
    Config _config;
    IWorker _worker;

    (Tensor tensor, ComputeTensorData data) _preprocess;

    (RenderTexture segment,
     RenderTexture parts,
     RenderTexture heatmaps,
     RenderTexture offsets,
     RenderTexture mask,
     GraphicsBuffer keypoints) _buffers;

    KeypointCache _readCache;

    void AllocateObjects(ResourceSet resources, int width, int height)
    {
        // NN model loading
        var model = ModelLoader.Load(resources.model);

        // Private object initialization
        _resources = resources;
        _config = new Config(model, _resources, width, height);
        _worker = model.CreateWorker(WorkerFactory.Device.GPU);

        // Buffer allocation
#if BARRACUDA_4_0_0_OR_LATER
        _preprocess.data = new ComputeTensorData
          (_config.InputShape, "Preprocess", false);
        _preprocess.tensor = TensorFloat.Zeros(_config.InputShape);
        _preprocess.tensor.AttachToDevice(_preprocess.data);
#else
        _preprocess.data = new ComputeTensorData
          (_config.InputShape, "Preprocess",
           ComputeInfo.ChannelsOrder.NHWC, false);
        _preprocess.tensor = new Tensor(_config.InputShape, _preprocess.data);
#endif

        _buffers.segment = RTUtil.NewFloat
          (_config.OutputWidth, _config.OutputHeight);

        _buffers.parts = RTUtil.NewFloat
          (_config.OutputWidth * 24, _config.OutputHeight);

        _buffers.heatmaps = RTUtil.NewFloat
          (_config.OutputWidth * Body.KeypointCount, _config.OutputHeight);

        _buffers.offsets = RTUtil.NewFloat
          (_config.OutputWidth * Body.KeypointCount * 2, _config.OutputHeight);

        _buffers.mask = RTUtil.NewArgbUav
          (_config.OutputWidth, _config.OutputHeight);

        _buffers.keypoints = new GraphicsBuffer
          (GraphicsBuffer.Target.Structured,
           Body.KeypointCount, sizeof(float) * 4);

        // Keypoint data read cache initialization
        _readCache = new KeypointCache(_buffers.keypoints);
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _preprocess.tensor?.Dispose();
        _preprocess = (null, null);

        ObjectUtil.Destroy(_buffers.segment);
        _buffers.segment = null;

        ObjectUtil.Destroy(_buffers.parts);
        _buffers.parts = null;

        ObjectUtil.Destroy(_buffers.heatmaps);
        _buffers.heatmaps = null;

        ObjectUtil.Destroy(_buffers.offsets);
        _buffers.offsets = null;

        ObjectUtil.Destroy(_buffers.mask);
        _buffers.mask = null;

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
        pre.SetBuffer(0, "Output", _preprocess.data.buffer);
        pre.SetInts("InputSize", _config.InputWidth, _config.InputHeight);
        pre.SetVector("ColorCoeffs", _config.PreprocessCoeffs);
        pre.SetBool("InputIsLinear", ColorUtil.IsLinear);
        pre.DispatchThreads(0, _config.InputWidth, _config.InputHeight, 1);

        // NN worker invocation
        _worker.Execute(_preprocess.tensor);

        // NN output retrieval
        _worker.CopyOutput("segments", _buffers.segment);
        _worker.CopyOutput("part_heatmaps", _buffers.parts);
        _worker.CopyOutput("heatmaps", _buffers.heatmaps);
        _worker.CopyOutput("short_offsets", _buffers.offsets);

        // Postprocessing (mask)
        var post1 = _resources.mask;
        post1.SetTexture(0, "Segments", _buffers.segment);
        post1.SetTexture(0, "Heatmaps", _buffers.parts);
        post1.SetTexture(0, "Output", _buffers.mask);
        post1.SetInts("InputSize", _config.OutputWidth, _config.OutputHeight);
        post1.DispatchThreads(0, _config.OutputWidth, _config.OutputHeight, 1);

        // Postprocessing (keypoints)
        var post2 = _resources.keypoints;
        post2.SetTexture(0, "Heatmaps", _buffers.heatmaps);
        post2.SetTexture(0, "Offsets", _buffers.offsets);
        post2.SetInts("InputSize", _config.OutputWidth, _config.OutputHeight);
        post2.SetInt("Stride", _config.Stride);
        post2.SetBuffer(0, "Keypoints", _buffers.keypoints);
        post2.Dispatch(0, 1, 1, 1);

        // Cache data invalidation
        _readCache.Invalidate();
    }

    #endregion
}

} // namespace BodyPix
