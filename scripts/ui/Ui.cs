using Godot;
using System;

public partial class Ui : CanvasLayer
{	
	private Player _player;
	private ProgressBar _xpBar;
	private ProgressBar _healthBar;
	private UpgradeScreen _upgradeScreen;

	public override void _Ready()
	{
		_xpBar = GetNode<ProgressBar>("XpBar");
		_healthBar = GetNode<ProgressBar>("HealthBar");
		_upgradeScreen = GetNode<UpgradeScreen>("UpgradeScreen");
	}
	
	public void InitializeUiFromPlayer(Player player)
	{
		_player = player;
		var fireball = _player.GetNode<FireballWeapon>("FireballWeapon");
		
		fireball.Enable();
		
		// set initial progress bar values
		_healthBar.MaxValue = _player.MaxHealth;
		_healthBar.Value = _player.Health;
		_xpBar.MaxValue = _player.GetXpForLevel(_player.Level);
		_xpBar.Value = _player.Xp;

		// subscribe 
		_player.HealthChanged += health => UpdateHealth(health);
		_player.MaxHealthChanged += maxHealth => UpdateMaxHealth(maxHealth);
		_player.XpChanged += xp => UpdateXp(xp);
	}
	
	private void UpdateHealth(int health) 
	{
		// check if died and do death screen logic here
		_healthBar.Value = health;
	}
	
	private void UpdateMaxHealth (int maxHealth)
	{
		_healthBar.MaxValue = maxHealth;
	}
	
	private void UpdateXp(int xp) {
		int xpForLevel = _player.GetXpForLevel(_player.Level);
		
		if (xp >= xpForLevel) {
			_upgradeScreen.Show();
		}
		
		_xpBar.MaxValue = xpForLevel;
		_xpBar.Value = xp;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("DEBUG-trigger-upgrade"))
		{
			_upgradeScreen.Show();
		}
	}
}
