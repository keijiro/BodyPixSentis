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
    [SerializeField] Shader _keypointShader = null;
    [SerializeField] Shader _boneShader = null;

    BodyPixRuntime _bodypix;
    (Material keypoint, Material bone) _material;

    void Start()
    {
        _bodypix = new BodyPixRuntime(_resources, _resolution.x, _resolution.y);
        _material = (new Material(_keypointShader), new Material(_boneShader));
    }

    void OnDestroy()
    {
        _bodypix.Dispose();
        Destroy(_material.keypoint);
        Destroy(_material.bone);
    }

    void LateUpdate()
    {
        _bodypix.ProcessImage(_source.Texture);
        _previewUI.texture = _source.Texture;
        _stencilUI.texture = _bodypix.Stencil;

        _material.keypoint.SetBuffer("_Keypoints", _bodypix.Keypoints);
        _material.keypoint.SetFloat("_Aspect", (float)_resolution.x / _resolution.y);

        _material.bone.SetBuffer("_Keypoints", _bodypix.Keypoints);
        _material.bone.SetFloat("_Aspect", (float)_resolution.x / _resolution.y);

        Graphics.DrawProcedural
          (_material.keypoint, new Bounds(Vector3.zero, Vector3.one),
           MeshTopology.Triangles, 6, BodyPixRuntime.PartCount);

        Graphics.DrawProcedural
          (_material.bone, new Bounds(Vector3.zero, Vector3.one),
           MeshTopology.Lines, 2, 12);
    }
}
