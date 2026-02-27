using UnityEngine;
using Unity.InferenceEngine;

namespace BodyPix {

#if UNITY_EDITOR
public enum Architecture { MobileNetV1, ResNet50 }
#endif

[CreateAssetMenu(fileName = "BodyPix",
                 menuName = "ScriptableObjects/BodyPix Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public ModelAsset model;
    public int stride = 8;
    public ComputeShader keypoints;

#if UNITY_EDITOR
    public Architecture architecture;
#endif
}

} // namespace BodyPix
