using UnityEngine;
using Unity.Barracuda;

namespace BodyPix {

[CreateAssetMenu(fileName = "BodyPix",
                 menuName = "ScriptableObjects/BodyPix Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public NNModel model;
    public int stride = 8;
    public ComputeShader preprocess;
    public ComputeShader stencil;
    public ComputeShader keypoints;
}

} // namespace BodyPix
