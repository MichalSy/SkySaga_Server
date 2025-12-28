using SkySaga.Game.Extensions;

namespace SkySaga.Game.Components;

public class ClientCampFireComponent : Component
{
    public bool BurntOut { get; set => SetIfChanged(ref field, value); } = true;
    public bool EnemiesTooClose { get; set => SetIfChanged(ref field, value); }
    public int Duration { get; set => SetIfChanged(ref field, value); }

    private bool _canStart = false;
    private DateTime? _lastStart = null;

    public override void Tick()
    {
        if (!_canStart)
            return;

        if (_lastStart == null)
        {
            Duration = 20;
            BurntOut = false;
        }
        else if (_lastStart != null && Duration > 0)
        {
            var elapsed = DateTime.UtcNow - _lastStart;
            var duration = Math.Max(0, 20 - (int)elapsed!.Value.TotalSeconds);
            if (duration == 0)
            {
                Duration = 0;
                BurntOut = true;
            }
        }
    }

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals(nameof(BurntOut), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write(BurntOut);
            return true;
        }
        else if (parameterName.Equals(nameof(EnemiesTooClose), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write(EnemiesTooClose);
            return true;
        }
        else if (parameterName.Equals(nameof(Duration), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteBits(BitConverter.GetBytes((int)(Duration * 256f)), 32 - Util.NumBitsRequiredUInt32(15360), true);
            _canStart = true;
            if (Duration > 0 && _lastStart == null)
            {
                _lastStart = DateTime.UtcNow;
            }
            return true;
        }

        return false;
    }
}
