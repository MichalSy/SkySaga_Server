namespace SkySaga.Game.Components;

public class ClientInteractionComponent : Component
{
    public bool OwnerOnly { get; set => SetIfChanged(ref field, value); } = false;
    public bool IsLootChest { get; set => SetIfChanged(ref field, value); }
    public bool HasBeenOpened { get; set => SetIfChanged(ref field, value); }
    public bool AllowMultipleUsers { get; set => SetIfChanged(ref field, value); } = true;
    public bool Enabled { get; set => SetIfChanged(ref field, value); } = true;
    public ushort[] InteractionAnglesRadians { get; set => SetIfChanged(ref field, value); } = [0, 0];

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals(nameof(OwnerOnly), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write(OwnerOnly);
            return true;
        }
        else if (parameterName.Equals(nameof(IsLootChest), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write(IsLootChest);
            return true;
        }
        else if (parameterName.Equals(nameof(HasBeenOpened), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write(HasBeenOpened);
            return true;
        }
        else if (parameterName.Equals(nameof(AllowMultipleUsers), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write(AllowMultipleUsers);
            return true;
        }
        else if (parameterName.Equals(nameof(Enabled), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write(Enabled);
            return true;
        }

        else if (parameterName.Equals(nameof(InteractionAnglesRadians), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteInt16(InteractionAnglesRadians[0], 360);
            bitStream.WriteInt16(InteractionAnglesRadians[1], 360);
            return true;
        }

        return false;
    }
}
