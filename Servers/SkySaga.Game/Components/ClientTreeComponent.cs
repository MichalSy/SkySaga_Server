using SkySaga.Game.Extensions;
using System.Linq;
using SkySaga.Game;

namespace SkySaga.Game.Components;

public class ClientTreeComponent : Component
{
    public byte[] DecorationLootTable { get; set => SetIfChanged(ref field, value); } = [];
    public uint TreeType { get; set => SetIfChanged(ref field, value); }
    public List<TreeDescriptionItemDTO> Description { get; set => SetIfChanged(ref field, value); } = [];
    public byte[] PartsDestroyed { get; set => SetIfChanged(ref field, value); } = [];
    public uint TrunkLootTable { get; set => SetIfChanged(ref field, value); } = Util.ComputeCrc32("Tree_Pine_Leaves_LootTable");

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals(nameof(TreeType), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write1();
            bitStream.WriteUInt32(TreeType, null, reverse: true);
            return true;
        }
        else if (parameterName.Equals(nameof(Description), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteBits(BitConverter.GetBytes(126), 32 - Util.NumBitsRequiredUInt32(126), true);

            bitStream.Write1();
            bitStream.WriteInt32(Description.Count, null, true);

            foreach (var currentNode in Description)
            {
                bitStream.WriteBits([currentNode.HasDestoryEffect], 8 - Util.NumBitsRequiredByte(55), true);
                bitStream.WriteBits([(byte)currentNode.OffsetDirection], 8 - Util.NumBitsRequiredByte(5), true);
                bitStream.WriteBits([currentNode.NextNodeIndex], 8 - Util.NumBitsRequiredByte(127), true);

                if (currentNode.DestroyOnAnyDamage)
                    bitStream.Write1();
                else
                    bitStream.Write0();

                bitStream.Write(currentNode.VoxelIndex);
            }

            return true;
        }
        else if (parameterName.Equals(nameof(PartsDestroyed), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteBits(BitConverter.GetBytes(126), 32 - Util.NumBitsRequiredUInt32(126), true);

            bitStream.Write1();
            bitStream.WriteInt32(Description.Count, null, true);

            foreach (var currentNode in Description)
            {
                bitStream.Write0();
            }

            return true;
        }
        else if (parameterName.Equals(nameof(DecorationLootTable), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write0();
            //bitStream.Write((int)TrunkLootTable);
            return true;
        }
        else if (parameterName.Equals(nameof(TrunkLootTable), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write0();
            //bitStream.Write((int)TrunkLootTable);
            return true;
        }


        return false;
    }
}