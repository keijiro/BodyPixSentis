using System.Collections.Generic;
using Unity.InferenceEngine;
using UnityEngine;

namespace BodyPix.Editor {

enum ModelArchitecture { MobileNetV1, ResNet50 }

static class FusedModelBuilder
{
    static Vector4 GetPreprocessCoeffs(ModelArchitecture architecture)
      => architecture == ModelArchitecture.MobileNetV1 ?
         new Vector4(-1, -1, -1, 2) :
         new Vector4(-123.15f, -115.90f, -103.06f, 255);

    static FunctionalTensor GetOutputByName
      (Model sourceModel, IReadOnlyList<FunctionalTensor> outputs, string name)
    {
        for (var i = 0; i < sourceModel.outputs.Count; i++)
            if (sourceModel.outputs[i].name == name) return outputs[i];
        throw new KeyNotFoundException($"Output '{name}' was not found.");
    }

    public static Model Build(Model sourceModel, ModelArchitecture architecture)
    {
        var graph = new FunctionalGraph();
        var input = graph.AddInput(sourceModel, 0);

        // Match the existing preprocess equation: rgb * scale + bias.
        var coeffs = GetPreprocessCoeffs(architecture);
        var biasTensor = Functional.Constant
          (new TensorShape(1, 1, 1, 3), new[] { coeffs.x, coeffs.y, coeffs.z });
        var preprocessed = input * coeffs.w + biasTensor;

        var sourceOutputs = Functional.Forward(sourceModel, preprocessed);
        var segments = GetOutputByName(sourceModel, sourceOutputs, "segments");
        var partHeatmaps = GetOutputByName(sourceModel, sourceOutputs, "part_heatmaps");
        var heatmaps = GetOutputByName(sourceModel, sourceOutputs, "heatmaps");
        var shortOffsets = GetOutputByName(sourceModel, sourceOutputs, "short_offsets");

        var segmentMask = Functional.Sigmoid(segments);
        var partMask = Functional.ArgMax(partHeatmaps, 3, true).Float() / Body.PartCount;
        var packedMask = Functional.Concat
          (new[] { partMask, partMask, partMask, segmentMask }, 3);

        graph.AddOutput(packedMask, "mask");
        graph.AddOutput(heatmaps, "heatmaps");
        graph.AddOutput(shortOffsets, "short_offsets");
        return graph.Compile();
    }
}

} // namespace BodyPix.Editor
