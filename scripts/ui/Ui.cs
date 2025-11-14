using Godot;
using System;

public partial class Ui : CanvasLayer
{
    [ExportCategory("Required Resources")]
    [Export] ProgressBar _xpBar;
    [Export] ProgressBar _healthBar;
    [Export]UpgradeScreen _upgradeScreen;
    
    private Player _player;
    
    public void InitializeUiFromPlayer(Player player)
    {
        _player = player;

        _player.FireballW.Unlock();
        
        // subscribe 
        _player.StatsInitialized += (health, maxHealth, xp, maxXp, level) =>
        {
            // set initial progress bar values
            _healthBar.MaxValue = health;
            _healthBar.Value = maxHealth;
            _xpBar.MaxValue = maxXp;
            _xpBar.Value = xp;
        };
        _player.HealthChanged += health => UpdateHealth(health);
        _player.MaxHealthChanged += maxHealth => UpdateMaxHealth(maxHealth);
        _player.XpChanged += xp => UpdateXp(xp);
    }

    private void UpdateHealth(float health)
    {
        // check if died and do death screen logic here
        _healthBar.Value = health;
    }

    private void UpdateMaxHealth(float maxHealth)
    {
        _healthBar.MaxValue = maxHealth;
    }

    private void UpdateXp(int xp)
    {
        int xpForLevel = xp;

        if (xp >= xpForLevel)
        {
            _upgradeScreen.Show();
        }

        _xpBar.MaxValue = xpForLevel;
        _xpBar.Value = xp;
    }


    public void Upgrade(WeaponUpgrade weaponUpgrade)
    {
        switch (weaponUpgrade.Weapon)
        {
            case Weapon.Fireball:

                _player.FireballW.Upgrade(weaponUpgrade);
                break;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("DEBUG-trigger-upgrade"))
        {
            _upgradeScreen.Show();
        }
    }
}