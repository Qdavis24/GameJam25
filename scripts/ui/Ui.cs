using Godot;
using System;

public partial class Ui : CanvasLayer
{	
	private Player _player;
	private ProgressBar _xpBar;
	private GridContainer _hearts;

	public override void _Ready()
	{
		_xpBar = GetNode<ProgressBar>("XpBar");
		_hearts = GetNode<GridContainer>("HeartBar");
	}
	
	public void SetPlayer(Player player)
	{
		_player = player;
		var fireball = _player.GetNode<FireballWeapon>("FireballWeapon");
		
		fireball.Enable();

		
		_xpBar.MaxValue = _player.GetXpForLevel(_player.Level);
		_xpBar.Value = _player.Xp;
		_player.HealthChanged += v => _hearts.SetHearts(v);
	}

	public override void _Process(double delta)
	{
	}
}
