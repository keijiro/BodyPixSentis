# BodyPixSentis

![gif](https://user-images.githubusercontent.com/343936/126066328-9bb01b01-d16f-4a38-8b7e-fb463bd0aac2.gif)
![gif](https://user-images.githubusercontent.com/343936/126066334-c8d7ea3f-a1b2-49c0-b094-cf55d8f80610.gif)

**BodyPixSentis** provides person segmentation and pose estimation based on the
[BodyPix] model. It runs on [Sentis], Unity's neural network runtime for
real-time applications.

[BodyPix]: https://blog.tensorflow.org/2019/11/updated-bodypix-2.html
[Sentis]: https://docs.unity3d.com/Packages/com.unity.ai.inference@latest

## Requirements

- Unity 6.0 or newer
- GPU with compute shader support

BodyPixSentis uses GPU inference only. WebGL is not supported because it does
not provide compute shader support.

## Installation

Install the BodyPixSentis package (`jp.keijiro.bodypix`) from the "Keijiro"
scoped registry by using the Unity Package Manager. To add the registry to your
project, follow [these instructions].

[these instructions]:
  https://gist.github.com/keijiro/f8c7e8ff29bfe63d86b888901b82644c

## Sample Overview

The repository includes two sample scenes:

- `Assets/Visualizer.unity`: Segmentation mask visualization.
- `Assets/Keypoints.unity`: Keypoint visualization.

These samples are configured to keep the main output path on the GPU. The
segmentation mask is consumed as `RenderTexture` and keypoints are available as
`GraphicsBuffer`, so rendering can proceed without mandatory CPU readback.
This GPU-only path is the primary reason for the high runtime performance.

## API Usage

```csharp
// Create a detector with a model resource set and processing dimensions.
var detector = new BodyPix.BodyDetector(resourceSet, 512, 384);

// Run inference on an input image texture.
detector.ProcessImage(inputTexture);

// Segmentation mask output texture.
var mask = detector.MaskTexture;

// Keypoint output GraphicsBuffer (GPU-side).
var keypointBuffer = detector.KeypointBuffer;

// Keypoint output data (with CPU readback).
var nose = detector.Keypoints[(int)BodyPix.Body.KeypointID.Nose];

// Dispose the detector and associated resources.
detector.Dispose();
detector = null;
```

## Supported Models

The BodyPixSentis package includes lightweight MobileNet models:

- MobileNetV1-x050-stride8
- MobileNetV1-x050-stride16
- MobileNetV1-x075-stride8
- MobileNetV1-x075-stride16
- MobileNetV1-x100-stride8
- MobileNetV1-x100-stride16

The width multiplier (`x050`, `x075`, `x100`) controls model capacity. Larger
multipliers generally improve accuracy, but increase computation cost.

The stride value controls output resolution. `stride8` usually produces more
detailed results than `stride16`, but requires more computation.

BodyPixSentis also supports ResNet-based models. These models provide higher
accuracy, but they are larger and slower than MobileNet models. Because of
GitHub and npm.js file size limits, ResNet ONNX files are not included in this
repository. Download them from the [Releases page].

[Releases page]:
  https://github.com/keijiro/BodyPixSentis/releases/download/1.0.3/ResNet50Models.zip

## Model Conversion

The original BodyPix model is distributed in TensorFlow.js format. Convert it
to ONNX by using [tfjs-to-tf] and [tf2onnx]. For the detailed conversion
procedure, refer to [the Colab notebook].

After conversion, add preprocessing and postprocessing operations to the ONNX
models, then export baked models as `.sentis` files. For this step, refer to
the Fused Model baking tools in `Assets/Editor`.

[tfjs-to-tf]: https://github.com/patlevin/tfjs-to-tf
[tf2onnx]: https://github.com/onnx/tensorflow-onnx
[the Colab notebook]:
  https://colab.research.google.com/drive/1ikOMoqOX7TSBNId0lGaQ_kIyDF2GV3M3?usp=sharing
