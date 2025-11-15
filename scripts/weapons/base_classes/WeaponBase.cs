using Godot;

namespace GameJam25.scripts.weapons.base_classes;

public abstract partial class WeaponBase : Node2D
{
    protected bool _active;
    
    [Export] protected float _projCooldown = 5.0f;
    [Export] protected float _projDamage = 10.0f;
    [Export] protected float _projSpeed = 100.0f;
    [Export] protected float _projCount = 5.0f;
    [Export] protected float _projSize = 1.0f;

    public void Upgrade(WeaponUpgrade upgrade)
    {
        switch (upgrade.Stat)
        {
            case Stat.CooldownPct:
                _projCooldown *= 1.0f - upgrade.Value/100.0f;
                InitWeapon();
                break;
            case Stat.DamagePct:
                _projDamage *= 1.0f + upgrade.Value/100.0f;
                break;
            case Stat.ProjectileCount:
                _projCount += upgrade.Value;
                InitWeapon();
                break;
            case Stat.ProjectileSpeedPct:
                _projSpeed *= 1.0f + upgrade.Value/100.0f;
                break;
            case Stat.SizePct:
                _projSize *= 1.0f + upgrade.Value/100.0f;
                InitWeapon();
                break;
        }
    }
    
    public abstract void InitWeapon();

}