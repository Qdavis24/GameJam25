using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradeScreen : HBoxContainer
{
	private Ui _ui;
	private Player _player;
	private readonly Dictionary<Weapon, bool> _unlocked = new()
	{
		{ Weapon.Fireball, false },
		{ Weapon.Water,    false },
		{ Weapon.Cloud,    false },
		{ Weapon.Stone,    false }
	};
	
	// BUTTONS
	private UpgradeButton _button1;
	private UpgradeButton _button2;
	private UpgradeButton _button3;
	
	public override void _Ready()
	{
		Hide();
		
		_ui = GetParent<Ui>();
		_player = _ui.GetPlayer();
		
		_button1 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton");
		_button2 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton2");
		_button3 = GetNode<UpgradeButton>("VBoxContainer/TextureRect/MarginContainer/VBoxContainer/UpgradeButton3");
		
		_button1.UpgradeClicked += UpgradeOrUnlock;
		_button2.UpgradeClicked += UpgradeOrUnlock;
		_button3.UpgradeClicked += UpgradeOrUnlock;
	}
	
	public void Show()
	{
		this.Visible = true;
		_button1.SpinAsync(_unlocked);
		_button2.SpinAsync(_unlocked);
		_button3.SpinAsync(_unlocked);
	}
	
	public void Hide()
	{
		this.Visible = false;
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
		switch (weapon)
		{
		case Weapon.Fireball:
			//_player.FireballW.Unlock();
			break;
		}
		Hide();
	}
		
	private void Upgrade(WeaponUpgrade weaponUpgrade)
	{
		switch (weaponUpgrade.Weapon)
		{
		case Weapon.Fireball:
			// TODO: Quinn uncomment
			//_player.FireballW.Upgrade(weaponUpgrade);
			break;
		}
		Hide();
	}

	public override void _Process(double delta)
	{
	}
}
