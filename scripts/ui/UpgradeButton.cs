using Godot;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public partial class UpgradeButton : Button
{
	[Signal] public delegate void UpgradeClickedEventHandler(WeaponUpgrade weaponUpgrade);
	
	private WeaponUpgrade _weaponUpgrade;
	
	private TextureRect _icon;
	private Label _status;
	private Label _title;
	private Label _descr;

	public static readonly Dictionary<Weapon, Texture2D> Icons = new()
	{
		{ Weapon.Fireball, GD.Load<Texture2D>("res://assets/ui/upgrade_screen/icons/fireball.png") },
		{ Weapon.Water,    GD.Load<Texture2D>("res://assets/ui/upgrade_screen/icons/water.png") },
		{ Weapon.Cloud,     GD.Load<Texture2D>("res://assets/ui/upgrade_screen/icons/cloud.png") },
		{ Weapon.Stone,    GD.Load<Texture2D>("res://assets/ui/upgrade_screen/icons/stone.png") },
	};

	public static readonly Dictionary<Weapon, string> Names = new()
	{
		{ Weapon.Fireball, "Feasting Fireball" },
		{ Weapon.Water,    "Darting Droplet" },
		{ Weapon.Cloud,    "Creeping Cloud" },
		{ Weapon.Stone,    "Shielding Stone" },
	};
	
	public readonly record struct UpgradeDisplayData(
		Texture2D Icon,
		string Status,
		string Title,
		string Description,
		Rarity Rarity
	);

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("HBoxContainer/MarginContainer/Icon");
		_status = GetNode<Label>("HBoxContainer/VBoxContainer/Status");
		_title = GetNode<Label>("HBoxContainer/VBoxContainer/Title");
		_descr = GetNode<Label>("HBoxContainer/VBoxContainer/Description");
	}
	
	// Automatically called when the button is pressed
	private void _on_pressed()
	{
		EmitSignal(SignalName.UpgradeClicked, _weaponUpgrade);
	}
	
	public async Task SpinAsync(Dictionary<Weapon, bool> unlocked)
	{
		// optional: disable input during spin
		MouseFilter = MouseFilterEnum.Ignore;

		// spin parameters (tweak to taste)
		int steps = 7;              // total flashes before stopping
		float startDelay = 0.03f;    // fastest step
		float endDelay = 0.20f;      // last slow step

		// quick fade-out -> swap -> fade-in each tick
		for (int i = 0; i < steps; i++)
		{
			// ease the delay from fast to slow
			float t = (float)i / (steps - 1);
			float delay = Mathf.Lerp(startDelay, endDelay, EaseOutCubic(t));

			// CREATE FLASH/BLINK WeaponUpgrade 
			var flashUpgrade = WeaponUpgrade.Roll();
			await FlashSwapAsync(GetDisplayData(flashUpgrade, unlocked), 0.06f); // tiny flash for juice

			await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
		}

		// CREATE FINAL WeaponUpgrade 
		_weaponUpgrade = WeaponUpgrade.Roll();
		await FlashSwapAsync(GetDisplayData(_weaponUpgrade, unlocked), 0.12f);

		// reenable mouse press
		MouseFilter = MouseFilterEnum.Pass;
	}
	
	private UpgradeDisplayData GetDisplayData(WeaponUpgrade upg, Dictionary<Weapon, bool> unlocked)
	{
		if (unlocked[upg.Weapon])
		{
			return new UpgradeDisplayData(
				Icons[upg.Weapon],
				"upgrade",
				Names[upg.Weapon],
				upg.Description,
				upg.Rarity
			);
		}
		return new UpgradeDisplayData(
			Icons[upg.Weapon],
			"unlock",
			Names[upg.Weapon],
			" ",
			upg.Rarity
		);
	}

	private async Task FlashSwapAsync(UpgradeDisplayData data, float flashTime)
	{
		// fade out a bit
		var tweenOut = CreateTween().SetParallel();
		tweenOut.TweenProperty(_icon,  "modulate:a", 0.2f, flashTime * 0.5f);
		tweenOut.TweenProperty(_status, "modulate:a", 0.2f, flashTime * 0.5f);
		tweenOut.TweenProperty(_title, "modulate:a", 0.2f, flashTime * 0.5f);
		tweenOut.TweenProperty(_descr, "modulate:a", 0.2f, flashTime * 0.5f);

		await ToSignal(tweenOut, Tween.SignalName.Finished);

		// IMPORTANT - displays new visual content
		Display(data);

		// fade back in
		var tweenIn = CreateTween().SetParallel();
		tweenIn.TweenProperty(_icon,  "modulate:a", 1f, flashTime * 0.5f);
		tweenIn.TweenProperty(_status, "modulate:a", 1f, flashTime * 0.5f);
		tweenIn.TweenProperty(_title, "modulate:a", 1f, flashTime * 0.5f);
		tweenIn.TweenProperty(_descr, "modulate:a", 1f, flashTime * 0.5f);
		await ToSignal(tweenIn, Tween.SignalName.Finished);
	}
	
	private float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
	
	private void Display(UpgradeDisplayData data)
	{
		_descr.Visible = true;
		
		Texture2D tex = data.Icon;
		string status = data.Status;
		string title = data.Title;
		string desc = data.Description;
		Rarity rarity = data.Rarity;
		
		// swap contents
		if (tex != null) _icon.Texture = tex;
		if (!string.IsNullOrEmpty(status)) _status.Text = status;
		if (!string.IsNullOrEmpty(title)) _title.Text = title;
		if (!string.IsNullOrEmpty(desc))  _descr.Text  = desc;
		
			//Rarity.Grey   => new Color("#a0a0a0"),
			//Rarity.Blue   => new Color("#3b82f6"),
			//Rarity.Purple => new Color("#a855f7"),
			//Rarity.Yellow => new Color("#facc15"),
		Color rarityColor = rarity switch
		{
			Rarity.Grey   => new Color("434343ff"),
			Rarity.Blue   => new Color("0e4ca7ff"),
			Rarity.Purple => new Color("74008fff"),
			Rarity.Yellow => new Color("c59a00ff"),
			_             => new Color("white")
		};
				
		if (status == "unlock") {
			rarityColor = new Color("2e5c00ff");
			_descr.Visible = false;
		}
				
		// RARITY BORDER ---
		var style = new StyleBoxFlat
		{
			BgColor = rarityColor with { A = 0.4f },
			//////////
			//BorderWidthLeft = 10,
			//BorderWidthRight = 10,
			//BorderWidthTop = 10,
			//BorderWidthBottom = 10,
			//BorderColor = rarityColor,
			//BgColor = new Color(0.0f, 0.0f, 0.0f, 0.2f),
		};
		var hover = style.Duplicate() as StyleBoxFlat;
		//hover.BgColor = new Color(0.0f, 0.0f, 0.0f, 0.15f);
		hover.BgColor = rarityColor with { A = 0.3f };
		AddThemeStyleboxOverride("normal", style);
		AddThemeStyleboxOverride("hover", hover);
		AddThemeStyleboxOverride("pressed", style);
		AddThemeStyleboxOverride("disabled", style);
		AddThemeStyleboxOverride("focus", hover);
		// --- end of borders
	}
}
