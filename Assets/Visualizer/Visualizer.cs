using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;
using BodyPix;

public sealed class Visualizer : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Vector2Int _resolution = new Vector2Int(512, 384);
    [SerializeField] RawImage _previewUI = null;
    [SerializeField] RawImage _maskUI = null;
    [SerializeField] bool _drawSkeleton = false;
    [SerializeField] Shader _shader = null;

    BodyDetector _detector;
    Material _material;
    RenderTexture _mask;

    void Start()
    {
        _detector = new BodyDetector(_resources, _resolution.x, _resolution.y);

        _material = new Material(_shader);

        var reso = _source.OutputResolution;
        _mask = new RenderTexture(reso.x, reso.y, 0);
        _maskUI.texture = _mask;
    }

    void OnDestroy()
    {
        _detector.Dispose();
        Destroy(_material);
        Destroy(_mask);
    }

    void LateUpdate()
    {
        _detector.ProcessImage(_source.Texture);
        _previewUI.texture = _source.Texture;

        Graphics.Blit(_detector.MaskTexture, _mask, _material, 0);
    }

    void OnRenderObject()
    {
        if (!_drawSkeleton) return;

        _material.SetBuffer("_Keypoints", _detector.KeypointBuffer);
        _material.SetFloat("_Aspect", (float)_resolution.x / _resolution.y);

        _material.SetPass(1);
        Graphics.DrawProceduralNow
          (MeshTopology.Triangles, 6, Body.KeypointCount);

        _material.SetPass(2);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 2, 12);
    }
}
