namespace SkySaga.Game.Components;

public class ClientVoxelDamageComponent : VoxelDamageComponent
{
    public override bool TrySync(string parameterName, BitStream bitStream)
    {
        return base.TrySync(parameterName, bitStream);
    }
}