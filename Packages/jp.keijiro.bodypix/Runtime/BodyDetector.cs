using Unity.InferenceEngine;
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
    Worker _worker;
    Tensor<float> _inputTensor;
    TextureTransform _inputTransform;
    TextureTransform _maskTransform;
    (RenderTexture mask, GraphicsBuffer keypoints) _output;
    BufferReader<Keypoint> _readCache;

    void AllocateObjects(ResourceSet resources, int width, int height)
    {
        _resources = resources;

        // NN model (BodyPix + preprocess + mask postprocess)
        var sourceModel = ModelLoader.Load(_resources.model);
        _config = new Config(sourceModel, _resources, width, height);
        var model = BodyPixModelFactory.IsPhase1Model(sourceModel) ?
          sourceModel :
          BodyPixModelFactory.BuildPhase1Model
          (sourceModel, _resources.architecture);

        // GPU worker
        _worker = new Worker(model, BackendType.GPUCompute);

        // Input tensor
        _inputTensor = new Tensor<float>
          (new TensorShape(1, _config.InputHeight, _config.InputWidth, 3));
        _inputTransform = new TextureTransform()
          .SetTensorLayout(TensorLayout.NHWC)
          .SetCoordOrigin(CoordOrigin.TopLeft);
        _maskTransform = new TextureTransform()
          .SetTensorLayout(TensorLayout.NHWC)
          .SetCoordOrigin(CoordOrigin.TopLeft);

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

        _inputTensor?.Dispose();
        _inputTensor = null;

        RTUtil.Destroy(_output.mask);
        _output.keypoints?.Dispose();
        _output = (null, null);
    }

    #endregion

    #region Main inference function

    void RunModel(Texture source)
    {
        // Preprocessing
        TextureConverter.ToTensor(source, _inputTensor, _inputTransform);

        // NN worker invocation
        _worker.Schedule(_inputTensor);

        // Postprocessing (mask via Sentis output tensor)
        var maskTensor = _worker.PeekOutput("mask") as Tensor<float>;
        TextureConverter.RenderToTexture(maskTensor, _output.mask, _maskTransform);

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
