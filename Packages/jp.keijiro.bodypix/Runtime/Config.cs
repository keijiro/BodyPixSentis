namespace BodyPix {

struct Config
{
    #region Variables from tensor shapes

    public int Stride { get; private set; }
    public int InputWidth { get; private set; }
    public int InputHeight { get; private set; }
    public int OutputWidth { get; private set; }
    public int OutputHeight { get; private set; }

    #endregion

    #region Constructor

    public Config(ResourceSet resources, int width, int height)
    {
        Stride = resources.stride;
        InputWidth  = (width  + 15) / 16 * 16 + 1;
        InputHeight = (height + 15) / 16 * 16 + 1;
        OutputWidth  = InputWidth  / Stride + 1;
        OutputHeight = InputHeight / Stride + 1;
    }

    #endregion
}

} // namespace BodyPix
