using Godot;
using System;

public partial class Ui : CanvasLayer
{
    // required internal children
    private ProgressBar _xpBar;
    private ProgressBar _healthBar;
    private UpgradeScreen _upgradeScreen;

    public Player Player {get; private set;}

    public override void _Ready()
    {
        _xpBar = GetNode<ProgressBar>("XpBar");
        _healthBar = GetNode<ProgressBar>("HealthBar");
        _upgradeScreen = GetNode<UpgradeScreen>("UpgradeScreen");
    }

    public void InitializeUiFromPlayer(Player player)
    {
        Player = player;

        // subscribe 
        Player.StatsInitialized += (health, maxHealth, xp, maxXp, level) =>
        {
            // set initial progress bar values
            _healthBar.MaxValue = health;
            _healthBar.Value = maxHealth;
            _xpBar.MaxValue = maxXp;
            _xpBar.Value = xp;
        };
        Player.HealthChanged += health => UpdateHealth(health);
        Player.MaxHealthChanged += maxHealth => UpdateMaxHealth(maxHealth);
        Player.XpChanged += xp => UpdateXp(xp);
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


    public void TriggerRoll()
    {
        _upgradeScreen.Show();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("DEBUG-trigger-upgrade"))
        {
            _upgradeScreen.Show();
        }
    }
}