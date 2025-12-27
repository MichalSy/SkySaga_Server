namespace SkySaga.Game.Components;

public class SeaLevelComponent : Component
{
    public int SeaFloorLevel { get; set => SetIfChanged(ref field, value); }
    public int SeaLevel { get; set => SetIfChanged(ref field, value); }

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals(nameof(SeaFloorLevel), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteInt32Reverse(SeaFloorLevel);
            return true;
        }
        else if (parameterName.Equals(nameof(SeaLevel), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteInt32Reverse(SeaLevel);
            return true;
        }

        return false;
    }
}
