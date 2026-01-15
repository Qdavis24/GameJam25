namespace GameJam25.scripts;

public partial class Xp : Pickup
{
    public override PickupType Type
    {
        get { return PickupType.Xp; }
    }

    protected override void RewardPlayer()
    {
        _player.Xp += Amount;
    }
}