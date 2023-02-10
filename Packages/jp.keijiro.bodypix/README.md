BodyPixBarracuda
================

![gif](https://user-images.githubusercontent.com/343936/126066328-9bb01b01-d16f-4a38-8b7e-fb463bd0aac2.gif)
![gif](https://user-images.githubusercontent.com/343936/126066334-c8d7ea3f-a1b2-49c0-b094-cf55d8f80610.gif)

**BodyPixBarracuda** is an implementation of the [BodyPix] person segmentation and pose estimation model
that runs on the [Unity Barracuda] neural network inference library.

[BodyPix]: https://blog.tensorflow.org/2019/11/updated-bodypix-2.html
[Unity Barracuda]: https://docs.unity3d.com/Packages/com.unity.barracuda@latest

System requirements
-------------------

- Unity 2020.3 LTS or later

About the ONNX file
-------------------

I converted the original BodyPix model (provided as tfjs) into ONNX using tfjs-to-tf and tf2onnx.
See [the Colab notebook] for further details.

[tfjs-to-tf]: https://github.com/patlevin/tfjs-to-tf
[tf2onnx]: https://github.com/onnx/tensorflow-onnx
[the Colab notebook]:
  https://colab.research.google.com/drive/1ikOMoqOX7TSBNId0lGaQ_kIyDF2GV3M3?usp=sharing

ResNet support
--------------

This package supports the ResNet architecture (more accurate but slower and bigger models)
but doesn't contain those ONNX files due to the file size limit of GitHub and npm.js.
You can download them from [here][ResNetZip] instead.

To use those models, create a new BodyPix ResourceSet file and set the model, architecture,
and stride fields accordingly.

![ResNet50](https://user-images.githubusercontent.com/343936/127449759-a5294794-4a60-454c-8f9d-7899c14b0d48.png)

[ResNetZip]:
  https://github.com/keijiro/BodyPixBarracuda/releases/download/1.0.3/ResNet50Models.zip

How to install
--------------

This package uses the [scoped registry] feature to resolve package
dependencies. Open the Package Manager page in the Project Settings window and
add the following entry to the Scoped Registries list:

- Name: `Keijiro`
- URL: `https://registry.npmjs.com`
- Scope: `jp.keijiro`

![Scoped Registry](https://user-images.githubusercontent.com/343936/162576797-ae39ee00-cb40-4312-aacd-3247077e7fa1.png)

Now you can install the package from My Registries page in the Package Manager
window.

![My Registries](https://user-images.githubusercontent.com/343936/162576825-4a9a443d-62f9-48d3-8a82-a3e80b486f04.png)

[scoped registry]: https://docs.unity3d.com/Manual/upm-scoped.html
