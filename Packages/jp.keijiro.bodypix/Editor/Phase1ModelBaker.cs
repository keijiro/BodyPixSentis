using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.InferenceEngine;

namespace BodyPix.Editor {

static class Phase1ModelBaker
{
    const string MenuPath = "Assets/BodyPix/Bake Phase1 Sentis Model";
    const string OutputDir = "Assets/StreamingAssets/BodyPix";

    [MenuItem(MenuPath, true)]
    static bool ValidateBake()
    {
        foreach (var obj in Selection.objects)
            if (obj is ResourceSet)
                return true;
        return false;
    }

    [MenuItem(MenuPath)]
    static void Bake()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "StreamingAssets/BodyPix"));

        foreach (var obj in Selection.objects)
        {
            var resources = obj as ResourceSet;
            if (resources == null)
                continue;

            var source = ModelLoader.Load(resources.model);
            if (Phase1ModelBuilder.IsPhase1Model(source))
            {
                Debug.LogWarning("ResourceSet model is already a Phase1 model.", resources);
                continue;
            }

            var edited = Phase1ModelBuilder.Build(source, resources.architecture);

            var outPath = Path.Combine
              (OutputDir, $"{resources.name}-Phase1.sentis");

            ModelWriter.Save(outPath, edited);
            AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"BodyPix Phase1 model baked: {outPath}", resources);
        }
    }
}

} // namespace BodyPix.Editor
