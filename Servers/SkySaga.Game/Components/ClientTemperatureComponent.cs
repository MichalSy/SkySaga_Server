namespace SkySaga.Game.Components;

public class ClientTemperatureComponent : Component
{
    public byte TemperatureState { get; set => SetIfChanged(ref field, value); } = 1;
    public byte MinTemperature { get; set => SetIfChanged(ref field, value); } = 0;
    public byte MaxTemperature { get; set => SetIfChanged(ref field, value); } = 200;

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals(nameof(TemperatureState), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteBits([TemperatureState], 8 - Util.NumBitsRequiredByte(3), true);

            return true;
        }
        else if (parameterName.Equals(nameof(MinTemperature), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteBits([MinTemperature], 8 - Util.NumBitsRequiredByte(200), true);

            return true;
        }
        else if (parameterName.Equals(nameof(MaxTemperature), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteBits([MaxTemperature], 8 - Util.NumBitsRequiredByte(200), true);

            return true;
        }
        return false;
    }
}
