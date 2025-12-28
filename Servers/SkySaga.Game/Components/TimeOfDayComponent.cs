namespace SkySaga.Game.Components;

public class TimeOfDayComponent : Component
{
    /// <summary>
    /// value between 0.0 and 1.0 representing the start time of day (0 = midnight, 0.5 = noon, 1 = next midnight)
    /// </summary>
    public float StartTimeOfDay { get; set => SetIfChanged(ref field, value); }

    public bool FixedTimeOfDay { get; set => SetIfChanged(ref field, value); }

    /// <summary>
    /// between 0 and 30 minutes, representing how long a full day/night cycle takes
    /// </summary>
    public int DayNightCycleDuration { get; set => SetIfChanged(ref field, value); }
    public ulong RealWorldStartTime { get; set => SetIfChanged(ref field, value); }

    /// <summary>
    /// between 0 and 127, how fast time progresses (0 = stop, 1 = normal speed, 2 = faster, etc.)
    /// </summary>
    public int TimeStretch { get; set => SetIfChanged(ref field, value); }

    /// <summary>
    /// value between 0.0 and 1.0 representing an offset to apply to the time of day
    /// </summary>
    public float TimeOfDayOffset { get; set => SetIfChanged(ref field, value); }

    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        if (parameterName.Equals(nameof(StartTimeOfDay), StringComparison.OrdinalIgnoreCase))
        {
            var writeValue = (int)(StartTimeOfDay * 65535f);
            bitStream.WriteInt32(writeValue, 65536);

            return true;
        }
        else if (parameterName.Equals(nameof(FixedTimeOfDay), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.Write(FixedTimeOfDay);

            return true;
        }
        else if (parameterName.Equals(nameof(DayNightCycleDuration), StringComparison.OrdinalIgnoreCase))
        {
            var writeValue = (int)(DayNightCycleDuration * 64);
            bitStream.WriteInt32(writeValue, 1920);

            return true;
        }
        else if (parameterName.Equals(nameof(RealWorldStartTime), StringComparison.OrdinalIgnoreCase))
        {
            bitStream.WriteUInt64(RealWorldStartTime);

            return true;
        }
        else if (parameterName.Equals(nameof(TimeStretch), StringComparison.OrdinalIgnoreCase))
        {
            var writeValue = (int)(TimeStretch * 64);
            bitStream.WriteInt32(writeValue, 8128);

            return true;
        }
        else if (parameterName.Equals(nameof(TimeOfDayOffset), StringComparison.OrdinalIgnoreCase))
        {
            var writeValue = (int)(TimeOfDayOffset * 65536f);
            bitStream.WriteInt32(writeValue, 65536);

            return true;
        }

        return false;
    }
}