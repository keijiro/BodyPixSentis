using UnityEngine;
using Unity.Barracuda;

namespace BodyPix {

public enum Architecture { MobileNetV1, ResNet50 }

[CreateAssetMenu(fileName = "BodyPix",
                 menuName = "ScriptableObjects/BodyPix Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public NNModel model;
    public Architecture architecture;
    public int stride = 8;
    public ComputeShader preprocess;
    public ComputeShader mask;
    public ComputeShader keypoints;
}

} // namespace BodyPix
