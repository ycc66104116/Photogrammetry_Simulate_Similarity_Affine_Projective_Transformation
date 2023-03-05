using System.ComponentModel;
namespace ImageProcessing
{
    public enum AngleFormat
    {
        Gradient,
        Degree
    }
    public enum CoordinateFormat { mm, Pixel }
    public enum AdjustFocalLength { No, Auto, UserDefine };
    public enum CollinearityCondition { BottomUp, TopDown };
    public enum RemoveBlunder { Ransac, Linear, Quadrtic, None }

    public enum ImageFormat { Jpeg, Tiff, Bmp, Png, Wmp, Gif }

    public enum Rotation { Right, Left, None }

    public enum Orientation { Horizontal, Vertical }

    public enum ImagePair { L, M, R }

    public enum MultiSpectralCamera { MCA_4, MCA_6, MCA_12, RedEdge_MX, RedEdge_Altum, Sequoia, MAIA_WV, MAIA_S2, FLIR_DuoProR,Wiris, DJI_H20T }

    public enum TransformationModel
    {
        Translation,
        Rotation,
        Rigid,
        Similarity,
        Affine,
        Projective
    }

    public enum SURF_Scale
    {
        [Description("Filter = 9")]
        Scale1,

        [Description("Filter = 15")]
        Scale2,

        [Description("Filter = 21")]
        Scale3,

        [Description("Filter = 27")]
        Scale4,

        [Description("Filter = 33")]
        Scale5,

        [Description("Filter = 9+15")]
        MultiScale1,

        [Description("Filter = 9+15+21")]
        MultiScale2,

        [Description("Filter = 9+15+21+27")]
        MultiScale3 
    }


}
