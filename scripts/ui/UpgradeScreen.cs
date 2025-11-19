using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradeScreen : Panel
{
	[Export] public AudioStream SlotSound;

	private Ui _ui;
	private Player _player;

	private readonly Dictionary<Weapon, bool> _unlocked = new()
	{
		{ Weapon.Fireball, false },
		{ Weapon.Water, false },
		{ Weapon.Cloud, false },
		{ Weapon.Stone, false }
	};

	// BUTTONS
	private UpgradeButton _button1;
	private UpgradeButton _button2;
	private UpgradeButton _button3;

	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.WhenPaused;
		this.Visible = false;

		_ui = GetParent<Ui>();

		_button1 = GetNode<UpgradeButton>("TextureRect/MarginContainer/VBoxContainer/UpgradeButton");
		_button2 = GetNode<UpgradeButton>("TextureRect/MarginContainer/VBoxContainer/UpgradeButton2");
		_button3 = GetNode<UpgradeButton>("TextureRect/MarginContainer/VBoxContainer/UpgradeButton3");

		_button1.UpgradeClicked += UpgradeOrUnlock;
		_button2.UpgradeClicked += UpgradeOrUnlock;
		_button3.UpgradeClicked += UpgradeOrUnlock;
	}

	public void Show()
	{
		GetTree().Paused = true;
		Visible = true;
		Sfx.I.PlayUi(SlotSound);
		_button1.SpinAsync(_unlocked);
		_button2.SpinAsync(_unlocked);
		_button3.SpinAsync(_unlocked);
	}

	public async void Hide()
	{
		// slight delay to hack around input propogating
		var timer = GetTree().CreateTimer(0.01, processInPhysics: false, processAlways: true);
		await ToSignal(timer, "timeout");

		Visible = false;
		GetTree().Paused = false;
	}

	private void UpgradeOrUnlock(WeaponUpgrade weaponUpgrade)
	{
		if (_unlocked[weaponUpgrade.Weapon])
		{
			Upgrade(weaponUpgrade);
		}
		else
		{
			Unlock(weaponUpgrade.Weapon);
		}
	}

	private void Unlock(Weapon weapon)
	{
		_unlocked[weapon] = true;
		_ui.Player.UnlockWeapon(weapon);
		Hide();
	}

	private void Upgrade(WeaponUpgrade weaponUpgrade)
	{
		_ui.Player.UpgradeWeapon(weaponUpgrade);
		Hide();
	}
	
}
