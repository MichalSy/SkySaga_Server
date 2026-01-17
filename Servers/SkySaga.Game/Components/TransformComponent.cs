using SkySaga.Game.Extensions;

namespace SkySaga.Game.Components;

public class TransformComponent : Component
{
    public Vector3 Position { get; set => SetIfChanged(ref field, value); }
    public float YawDegrees { get; set => SetIfChanged(ref field, value); } = 0;
    public Vector3Int Size { get; set => SetIfChanged(ref field, value); }
    public float Scale { get; set => SetIfChanged(ref field, value); } = 1;

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals(nameof(Position), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteBits(BitConverter.GetBytes((int)(Position.X * 64f)), 32 - Util.NumBitsRequiredUInt32(0x10000u), true);
            bitStream.WriteBits(BitConverter.GetBytes((int)(Position.Y * 64f)), 32 - Util.NumBitsRequiredUInt32(0x10000u), true);
            bitStream.WriteBits(BitConverter.GetBytes((int)(Position.Z * 64f)), 32 - Util.NumBitsRequiredUInt32(0x10000u), true);

            return true;
        }
        else if (parameterName.Equals(nameof(YawDegrees), StringComparison.OrdinalIgnoreCase))
        {
            if (YawDegrees == 0)
                return false;

            bitStream.WriteBits(BitConverter.GetBytes((int)((YawDegrees * 32f) + 12800)), 32 - Util.NumBitsRequiredUInt32(25600), true);
            return true;
        }
        else if (parameterName.Equals(nameof(Size), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteInt32(Size.X, 9);
            bitStream.WriteInt32(Size.Y, 9);
            bitStream.WriteInt32(Size.Z, 9);

            return true;
        }
        else if (parameterName.Equals(nameof(Scale), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteInt32((int)(Scale * 8), 56);

            return true;
        }

        return false;
    }
}