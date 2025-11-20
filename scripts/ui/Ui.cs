using Godot;
using System;

public partial class Ui : CanvasLayer
{
	[Signal]
	public delegate void MainMenuRequestedUiEventHandler();
	
	[Export] private AudioStream _levelUpSounds;
	[Export] private AudioStream _chestOpenSounds;

	
	// required internal children
	private ProgressBar _xpBar;
	private ProgressBar _healthBar;
	private UpgradeScreen _upgradeScreen;
	private Label _killCounter;
	private Label _levelCounter;

	public Player Player {get; private set;}

	public override void _Ready()
	{		
		_xpBar = GetNode<ProgressBar>("XpBar");
		_healthBar = GetNode<ProgressBar>("HealthBar");
		_upgradeScreen = GetNode<UpgradeScreen>("UpgradeScreen");
		_killCounter = GetNode<Label>("Stats/KillCounter");
		_levelCounter = GetNode<Label>("Stats/LevelCounter");

	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("DEBUG-trigger-upgrade"))
		{
			_upgradeScreen.Show();
		}
	}

	public void InitializeUiFromPlayer(Player player)
	{
		Player = player;
		
		Player.LevelChanged += PlayerLevelUp;

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
		_xpBar.MaxValue = Player.MaxXp;
		_xpBar.Value = xp;
	}
	
	public void PlayerLevelUp(int level)
	{
		Sfx.I.PlayUi(_levelUpSounds, 6f);
		_upgradeScreen.Show();
	}
	public void OpenChest()
	{
		Sfx.I.PlayUi(_chestOpenSounds, 8f);
		_upgradeScreen.Show();
	}
	public void HideUpgrade() // called when resetting world
	{
		_upgradeScreen.Visible = false;
	}
	
	public void OnMainMenuRequested()
	{
		this.Visible = false;
		EmitSignalMainMenuRequestedUi();
	}
	
	public void IncrementKillCounter()
	{
		int current = int.Parse(_killCounter.Text);
		current += 1;
		_killCounter.Text = current.ToString();
	}
	public void IncrementLevelCounter()
	{
		int current = int.Parse(_levelCounter.Text);
		current += 1;
		_levelCounter.Text = current.ToString();
	}
	public (string kills, string level) GetCounters()
	{
		return (_killCounter.Text, _levelCounter.Text);
	}
	
}
