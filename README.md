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

## ResNet Model Support

This package supports ResNet-based models. These models provide higher
accuracy, but they are larger and slower than lightweight alternatives. Because
of GitHub and npm.js file size limits, ResNet ONNX files are not included in
this repository. Download them from the [Releases page].

[Releases page]:
  https://github.com/keijiro/BodyPixSentis/releases/download/1.0.3/ResNet50Models.zip
