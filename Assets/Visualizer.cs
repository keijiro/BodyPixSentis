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
    [SerializeField] RawImage _stencilUI = null;
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
        _stencilUI.texture = _bodypix.Stencil;

        _material.SetBuffer("_Keypoints", _bodypix.Keypoints);
        _material.SetFloat("_Aspect", (float)_resolution.x / _resolution.y);

        Graphics.DrawProcedural
          (_material, new Bounds(Vector3.zero, Vector3.one),
           MeshTopology.Triangles, 6, BodyPixRuntime.PartCount);
    }
}
