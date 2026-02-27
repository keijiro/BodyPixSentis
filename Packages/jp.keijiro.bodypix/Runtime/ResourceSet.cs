using UnityEngine;
using Unity.InferenceEngine;

namespace BodyPix {

[CreateAssetMenu(fileName = "BodyPix",
                 menuName = "ScriptableObjects/BodyPix Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public ModelAsset model;
    public int stride = 8;
    public ComputeShader keypoints;
}

} // namespace BodyPix
