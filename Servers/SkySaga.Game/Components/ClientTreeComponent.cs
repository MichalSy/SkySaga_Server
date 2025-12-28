namespace SkySaga.Game.Components;

public class ClientTreeComponent : Component
{
    public int TreeType { get; set => SetIfChanged(ref field, value); }
    public int PartsDestroyed { get; set => SetIfChanged(ref field, value); }

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals(nameof(TreeType), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteInt32Reverse(TreeType);
            return true;
        }
        else if (parameterName.Equals(nameof(PartsDestroyed), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteInt32Reverse(PartsDestroyed);
            return true;
        }

        return false;
    }
}
