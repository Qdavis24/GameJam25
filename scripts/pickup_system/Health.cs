namespace GameJam25.scripts;

public partial class Health : Pickup
{
    public override PickupType Type
    {
        get { return PickupType.Health; }
    }
    protected override void RewardPlayer()
    {
        _player.Health += Amount;
    }
}