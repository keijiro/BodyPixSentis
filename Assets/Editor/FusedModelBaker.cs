using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.InferenceEngine;

namespace BodyPix.Editor {

static class FusedModelBaker
{
    // Menu items

    const string MobileNetMenuPath = "Assets/BodyPix/Bake Fused (MobileNet)";
    const string ResNetMenuPath = "Assets/BodyPix/Bake Fused (ResNet50)";

    [MenuItem(MobileNetMenuPath, true)]
    static bool ValidateMobileNetBake() => ValidateBake();

    [MenuItem(ResNetMenuPath, true)]
    static bool ValidateResNetBake() => ValidateBake();

    [MenuItem(MobileNetMenuPath)]
    static void BakeMobileNet() => Bake(ModelArchitecture.MobileNetV1);

    [MenuItem(ResNetMenuPath)]
    static void BakeResNet() => Bake(ModelArchitecture.ResNet50);

    // Helpers

    static bool IsOnnxAsset(Object obj)
      => Path.GetExtension(AssetDatabase.GetAssetPath(obj)).ToLowerInvariant() == ".onnx";

    static bool ValidateBake()
      => Selection.objects != null && System.Array.Exists(Selection.objects, IsOnnxAsset);

    // Fused model baker

    static void Bake(ModelArchitecture architecture)
    {
        foreach (var obj in Selection.objects)
        {
            if (!IsOnnxAsset(obj)) continue;

            var sourcePath = AssetDatabase.GetAssetPath(obj);
            var sourceName = Path.GetFileNameWithoutExtension(sourcePath);
            var sourceDir = Path.GetDirectoryName(sourcePath);

            var sourceAsset = AssetDatabase.LoadAssetAtPath<ModelAsset>(sourcePath);
            var source = ModelLoader.Load(sourceAsset);
            var edited = FusedModelBuilder.Build(source, architecture);

            var outPath = Path.Combine(sourceDir, $"{sourceName}-Fused.sentis");
            ModelWriter.Save(outPath, edited);
            AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
        }
    }
}

} // namespace BodyPix.Editor
