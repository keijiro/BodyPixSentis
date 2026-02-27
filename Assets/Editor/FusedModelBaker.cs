using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.InferenceEngine;

namespace BodyPix.Editor {

static class FusedModelBaker
{
    const string MobileNetMenuPath = "Assets/BodyPix/Bake Fused (MobileNet)";
    const string ResNetMenuPath = "Assets/BodyPix/Bake Fused (ResNet50)";
    const string OutputDir = "Assets/StreamingAssets/BodyPix";

    static bool IsOnnxAsset(Object obj)
    {
        var path = AssetDatabase.GetAssetPath(obj);
        return Path.GetExtension(path).ToLowerInvariant() == ".onnx";
    }

    [MenuItem(MobileNetMenuPath, true)]
    static bool ValidateMobileNetBake()
      => Selection.objects != null &&
         System.Array.Exists(Selection.objects, IsOnnxAsset);

    [MenuItem(ResNetMenuPath, true)]
    static bool ValidateResNetBake()
      => Selection.objects != null &&
         System.Array.Exists(Selection.objects, IsOnnxAsset);

    [MenuItem(MobileNetMenuPath)]
    static void BakeMobileNet()
      => Bake(ModelArchitecture.MobileNetV1, "MobileNet");

    [MenuItem(ResNetMenuPath)]
    static void BakeResNet()
      => Bake(ModelArchitecture.ResNet50, "ResNet50");

    static void Bake(ModelArchitecture architecture, string suffix)
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "StreamingAssets/BodyPix"));

        foreach (var obj in Selection.objects)
        {
            if (!IsOnnxAsset(obj))
                continue;

            var sourcePath = AssetDatabase.GetAssetPath(obj);
            var sourceAsset = AssetDatabase.LoadAssetAtPath<ModelAsset>(sourcePath);
            if (sourceAsset == null)
            {
                Debug.LogWarning($"ModelAsset load failed: {sourcePath}", obj);
                continue;
            }

            var source = ModelLoader.Load(sourceAsset);
            if (FusedModelBuilder.IsFusedModel(source))
            {
                Debug.LogWarning("Selected ONNX is already a fused model.", sourceAsset);
                continue;
            }

            var edited = FusedModelBuilder.Build(source, architecture);
            var sourceName = Path.GetFileNameWithoutExtension(sourcePath);

            var outPath = Path.Combine
              (OutputDir, $"{sourceName}-Fused-{suffix}.sentis");

            ModelWriter.Save(outPath, edited);
            AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"BodyPix fused model baked: {outPath}", sourceAsset);
        }
    }
}

} // namespace BodyPix.Editor
