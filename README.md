# BodyPixSentis

![gif](https://user-images.githubusercontent.com/343936/126066328-9bb01b01-d16f-4a38-8b7e-fb463bd0aac2.gif)
![gif](https://user-images.githubusercontent.com/343936/126066334-c8d7ea3f-a1b2-49c0-b094-cf55d8f80610.gif)

**BodyPixSentis** is an implementation of the [BodyPix] model for person
segmentation and pose estimation. It runs on the [Unity Inference Engine],
Unity’s official neural network runtime optimized for real-time applications.

[BodyPix]: https://blog.tensorflow.org/2019/11/updated-bodypix-2.html
[Unity Inference Engine]: https://docs.unity3d.com/Packages/com.unity.ai.inference@latest

## System Requirements

- Unity 6
- Compute shader support

## ONNX Model Information

The original BodyPix model, provided in TensorFlow.js format, has been converted
to ONNX using [tfjs-to-tf] and [tf2onnx]. For details, see [the Colab notebook].

[tfjs-to-tf]: https://github.com/patlevin/tfjs-to-tf
[tf2onnx]: https://github.com/onnx/tensorflow-onnx
[the Colab notebook]:
  https://colab.research.google.com/drive/1ikOMoqOX7TSBNId0lGaQ_kIyDF2GV3M3?usp=sharing

## ResNet Model Support

This package supports ResNet-based models, which offer higher accuracy but are
larger and slower. Due to GitHub and npm.js file size limits, these ONNX files
are not included in the repository. You can download them from
[ResNetZip] instead.

To use ResNet models, create a new BodyPix ResourceSet and set the model,
architecture, and stride fields accordingly.

![ResNet50](https://user-images.githubusercontent.com/343936/127449759-a5294794-4a60-454c-8f9d-7899c14b0d48.png)

[ResNetZip]:
  https://github.com/keijiro/BodyPixSentis/releases/download/1.0.3/ResNet50Models.zip

## Installation

You can install the BodyPixSentis package (`jp.keijiro.bodypix`) via the
"Keijiro" scoped registry using the Unity Package Manager. To add the registry
to your project, follow [these instructions].

[these instructions]:
  https://gist.github.com/keijiro/f8c7e8ff29bfe63d86b888901b82644c
