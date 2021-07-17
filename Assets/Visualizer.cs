using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;
using BodyPix;

sealed class Visualizer : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Vector2Int _resolution = new Vector2Int(512, 384);
    [SerializeField] RawImage _previewUI = null;
    [SerializeField] RawImage _maskUI = null;
    [SerializeField] Shader _shader = null;

    BodyPixRuntime _bodypix;
    Material _material;

    void Start()
    {
        _bodypix = new BodyPixRuntime(_resources, _resolution.x, _resolution.y);
        _material = new Material(_shader);
    }

    void OnDestroy()
    {
        _bodypix.Dispose();
        Destroy(_material);
    }

    void LateUpdate()
    {
        _bodypix.ProcessImage(_source.Texture);
        _previewUI.texture = _source.Texture;
        _maskUI.texture = _bodypix.Mask;
    }

    void OnRenderObject()
    {
        _material.SetBuffer("_Keypoints", _bodypix.Keypoints);
        _material.SetFloat("_Aspect", (float)_resolution.x / _resolution.y);

        _material.SetPass(0);
        Graphics.DrawProceduralNow
          (MeshTopology.Triangles, 6, BodyPixRuntime.KeypointCount);

        _material.SetPass(1);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 2, 12);
    }
}
