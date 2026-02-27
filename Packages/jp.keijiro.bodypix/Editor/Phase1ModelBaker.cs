using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.InferenceEngine;

namespace BodyPix.Editor {

static class Phase1ModelBaker
{
    const string MobileNetMenuPath = "Assets/BodyPix/Bake Phase1 (MobileNet)";
    const string ResNetMenuPath = "Assets/BodyPix/Bake Phase1 (ResNet50)";
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
            if (Phase1ModelBuilder.IsPhase1Model(source))
            {
                Debug.LogWarning("Selected ONNX is already a Phase1 model.", sourceAsset);
                continue;
            }

            var edited = Phase1ModelBuilder.Build(source, architecture);
            var sourceName = Path.GetFileNameWithoutExtension(sourcePath);

            var outPath = Path.Combine
              (OutputDir, $"{sourceName}-Phase1-{suffix}.sentis");

            ModelWriter.Save(outPath, edited);
            AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"BodyPix Phase1 model baked: {outPath}", sourceAsset);
        }
    }
}

} // namespace BodyPix.Editor
