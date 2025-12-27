using SkySaga.Game.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace SkySaga.Game.Packets;

public static class SetCharacterCustomisationData
{
    public static bool Handle(PlayerConnection connection, BitStream bitStream)
    {
        if (!bitStream.Read(out int entityId))
            return false;

        bitStream.ReadInt32(2, out var gender);

        var hasTribe = bitStream.ReadBit();
        bitStream.ReadUInt32Reverse(out var tribe);


        uint attributeCount = 3;
        if (bitStream.ReadBit())
        {
            bitStream.ReadUInt32Reverse(out attributeCount);
        }

        // Skin Tone
        uint skinTone = 0;
        if (bitStream.ReadBit())
        {
            bitStream.ReadUInt32Reverse(out skinTone);
        }

        // Eye Color
        uint eyeColor = 0;
        if (bitStream.ReadBit())
        {
            bitStream.ReadUInt32Reverse(out eyeColor);
        }

        // Signature Color
        uint signatureColor = 0;
        if (bitStream.ReadBit())
        {
            bitStream.ReadUInt32Reverse(out signatureColor);
        }

        if (attributeCount > 3)
        {
            for (int i = 0; i < 3; i++)
            {
                if (bitStream.ReadBit())
                {
                    bitStream.ReadUInt32Reverse(out var tmpData);
                }
            }
        }

        uint hairAttributes = 1;
        if (bitStream.ReadBit())
        {
            bitStream.ReadUInt32Reverse(out hairAttributes);
        }

        // Hair Style
        uint hairStyle = 0;
        if (bitStream.ReadBit())
        {
            bitStream.ReadUInt32Reverse(out hairStyle);
        }

        // Hair Style
        uint hairColor = 0;
        if (bitStream.ReadBit())
        {
            bitStream.ReadUInt32Reverse(out hairColor);
        }

        Debug.WriteLine($"Entity: {entityId}, Gender: {gender}, Tribe: {tribe}, SkinTone: {skinTone}, " +
            $"EyeColor: {eyeColor}, SignatureColor: {signatureColor}, HairStyle: {hairStyle}, HairColor: {hairColor})",
            nameof(SetCharacterCustomisationData));

        return true;
    }

}


