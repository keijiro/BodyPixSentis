using Unity.Barracuda;
using UnityEngine;

namespace BodyPix {

struct Config
{
    #region Variables from tensor shapes

    public int InputWidth { get; private set; }
    public int InputHeight { get; private set; }
    public int OutputWidth { get; private set; }
    public int OutputHeight { get; private set; }

    #endregion

    #region Data size calculation properties

    public int InputFootprint => InputWidth * InputHeight * 3;

    #endregion

    #region Tensor shape utilities

    public TensorShape InputShape
      => new TensorShape(1, InputHeight, InputWidth, 3);

    #endregion

    #region Constructor

    public Config(Model model)
    {
        var inShape = model.inputs[0].shape;
        var outShape = model.GetShapeByName("float_segments").Value;

        InputWidth = inShape[6]; // 6: width
        InputHeight = inShape[5]; // 5: height
        OutputWidth = outShape.width;
        OutputHeight = outShape.height;
    }

    #endregion
}

} // namespace BodyPix
