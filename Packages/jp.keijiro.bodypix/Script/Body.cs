using System.Runtime.InteropServices;
using UnityEngine;

namespace BodyPix {

public static class Body
{
    public const int PartCount = 24;
    public const int KeypointCount = 17;

    public enum KeypointID
    {
        Nose,
        LeftEye, RightEye,
        LeftEar, RightEar,
        LeftShoulder, RightShoulder,
        LeftElbow, RightElbow,
        LeftWrist, RightWrist,
        LeftHip, RightHip,
        LeftKnee, RightKnee,
        LeftAnkle, RightAnkle
    }

    public enum PartID
    {
        LeftFace, RightFace,
        LeftUpperArmFront, LeftUpperArmBack,
        RightUpperArmFront, RightUpperArmBack,
        LeftLowerArmFront, LeftLowerArmBack,
        RightLowerArmFront, RightLowerArmBack,
        LeftHand, RightHand,
        TorsoFront, TorsoBack,
        LeftUpperLegFront, LeftUpperLegBack,
        RightUpperLegFront, RightUpperLegBack,
        LeftLowerLegFront, LeftLowerLegBack,
        RightLowerLegFront, RightLowerLegBack,
        LeftFeet, RightFeet
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Keypoint
{
    public Vector2 Position;
    public float Score;
    public uint Padding;
}

} // namespace BodyPix
