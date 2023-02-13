using Unity.Barracuda;
using UnityEngine;

namespace BodyPix {

struct Config
{
    #region Variables from tensor shapes

    public Architecture Architecture { get; private set; }
    public int Stride { get; private set; }
    public int InputWidth { get; private set; }
    public int InputHeight { get; private set; }
    public int OutputWidth { get; private set; }
    public int OutputHeight { get; private set; }

    #endregion

    #region Coefficients for preprocessing

    public Vector4 PreprocessCoeffs
      => Architecture == Architecture.MobileNetV1 ?
        new Vector4(-1, -1, -1, 2) :
        new Vector4(-123.15f, -115.90f, -103.06f, 255);

    #endregion

    #region Data size calculation properties

    public int InputFootprint => InputWidth * InputHeight * 3;

    #endregion

    #region Constructor

    public Config(Model model, ResourceSet resources, int width, int height)
    {
        Architecture = resources.architecture;
        Stride = resources.stride;
        InputWidth  = (width  + 15) / 16 * 16 + 1;
        InputHeight = (height + 15) / 16 * 16 + 1;
        OutputWidth  = InputWidth  / Stride + 1;
        OutputHeight = InputHeight / Stride + 1;
    }

    #endregion
}

} // namespace BodyPix
