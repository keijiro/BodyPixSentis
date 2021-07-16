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

    BodyPixRuntime _bodypix;

    void Start()
      => _bodypix = new BodyPixRuntime(_resources, _resolution.x, _resolution.y);

    void OnDestroy()
      => _bodypix.Dispose();

    void LateUpdate()
    {
        _bodypix.ProcessImage(_source.Texture);
        _previewUI.texture = _source.Texture;
        _stencilUI.texture = _bodypix.Stencil;
    }
}
