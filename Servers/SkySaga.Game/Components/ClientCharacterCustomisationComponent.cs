namespace SkySaga.Game.Components;

public class ClientCharacterCustomisationComponent : Component
{
    public uint Gender { get; set => SetIfChanged(ref field, value); } = 0;
    public uint Tribe { get; set => SetIfChanged(ref field, value); } = 2876448639;
    public uint SkinColor { get; set => SetIfChanged(ref field, value); } = 2712510180;
    public uint EyeColor { get; set => SetIfChanged(ref field, value); } = 2222741576;
    public uint SignatureColor { get; set => SetIfChanged(ref field, value); } = 1798687955;
    public uint HairStyle { get; set => SetIfChanged(ref field, value); } = 4291343107;
    public uint HairColor { get; set => SetIfChanged(ref field, value); } = 3725297217;

    public override bool TrySync(string parameterName, BitStream bitStream)
    {

        bitStream.WriteBits(BitConverter.GetBytes(Gender), 32 - Util.NumBitsRequiredUInt32(2), true);

        bitStream.Write1();
        bitStream.WriteUInt32Reverse(Tribe);


        bitStream.Write1();
        bitStream.WriteUInt32Reverse(3);


        bitStream.Write1();
        bitStream.WriteUInt32Reverse(SkinColor);
        bitStream.Write1();
        bitStream.WriteUInt32Reverse(EyeColor);
        bitStream.Write1();
        bitStream.WriteUInt32Reverse(SignatureColor);


        bitStream.Write1();
        bitStream.WriteUInt32Reverse(1);


        bitStream.Write1();
        bitStream.WriteUInt32Reverse(HairStyle);
        bitStream.Write1();
        bitStream.WriteUInt32Reverse(HairColor);

        return true;
    }

}