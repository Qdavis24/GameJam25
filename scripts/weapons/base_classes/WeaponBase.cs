using Godot;

namespace GameJam25.scripts.weapons.base_classes;

public partial class WeaponBase : Node2D
{
    [Export] protected Timer _timer;
    [Export] protected float _projDamage = 10.0f;
    [Export] protected float _projSpeed = 100.0f;
    [Export] protected float _projCount = 5.0f;
    [Export] protected float _projSize = 1.0f;

    public void Unlock()
    {
        _timer.Start();
    }

    public void Upgrade(WeaponUpgrade upgrade)
    {
        switch (upgrade.Stat)
        {
            case Stat.CooldownPct:
                _timer.WaitTime *= 1.0f - upgrade.Value/100.0f;
                break;
            case Stat.DamagePct:
                _projDamage *= 1.0f + upgrade.Value/100.0f;
                break;
            case Stat.ProjectileCount:
                _projCount *= 1 + upgrade.Value/100.0f;
                break;
            case Stat.ProjectileSpeedPct:
                _projSpeed *= 1.0f + upgrade.Value/100.0f;
                break;
            case Stat.SizePct:
                _projSize *= 1.0f + upgrade.Value/100.0f;
                break;
            
        }
    }
  
}