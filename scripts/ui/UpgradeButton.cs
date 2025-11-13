using Godot;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public partial class UpgradeButton : Button
{
	private TextureRect _icon;
	private Label _title;
	private Label _descr;

	public static readonly Dictionary<Weapon, Texture2D> Icons = new()
	{
		{ Weapon.Fireball, GD.Load<Texture2D>("res://assets/ui/upgrade_screen/icons/fireball.png") },
		{ Weapon.Stick,    GD.Load<Texture2D>("res://assets/ui/upgrade_screen/icons/stick.png") },
		{ Weapon.Bird,     GD.Load<Texture2D>("res://assets/ui/upgrade_screen/icons/bird.png") },
		{ Weapon.Stone,    GD.Load<Texture2D>("res://assets/ui/upgrade_screen/icons/stone.png") },
		{ Weapon.Thorn,    GD.Load<Texture2D>("res://assets/ui/upgrade_screen/icons/thorn.png") },
	};

	public static readonly Dictionary<Weapon, string> Names = new()
	{
		{ Weapon.Fireball, "flaming Fireball" },
		{ Weapon.Stick,    "stabbing Stick" },
		{ Weapon.Bird,     "homing Hatchling" },
		{ Weapon.Stone,    "shielding Stone" },
		{ Weapon.Thorn,    "throwing Thorn" },
	};

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("HBoxContainer/MarginContainer/Icon");
		_title = GetNode<Label>("HBoxContainer/VBoxContainer/Title");
		_descr = GetNode<Label>("HBoxContainer/VBoxContainer/Description");
		
	}
	
	public async Task SpinAsync()
	{
		// optional: disable input during spin
		MouseFilter = MouseFilterEnum.Ignore;

		// spin parameters (tweak to taste)
		int steps = 8;              // total flashes before stopping
		float startDelay = 0.03f;    // fastest step
		float endDelay = 0.20f;      // last slow step

		// quick fade-out -> swap -> fade-in each tick
		for (int i = 0; i < steps; i++)
		{
			// ease the delay from fast to slow
			float t = (float)i / (steps - 1);
			float delay = Mathf.Lerp(startDelay, endDelay, EaseOutCubic(t));

			var flashUpgrade = WeaponUpgrade.Roll();
			var tex   = Icons[flashUpgrade.Weapon];
			var title = Names[flashUpgrade.Weapon];
			var desc  = flashUpgrade.Description;

			await FlashSwapAsync(tex, title, desc, 0.06f, flashUpgrade.Rarity); // tiny flash for juice

			await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
		}

		var upgrade = WeaponUpgrade.Roll();
		var finalTex   = Icons[upgrade.Weapon];
		var finalTitle = Names[upgrade.Weapon];
		var finalDesc  = upgrade.Description;
		await FlashSwapAsync(finalTex, finalTitle, finalDesc, 0.12f, upgrade.Rarity);

		MouseFilter = MouseFilterEnum.Pass;
	}

	private async Task FlashSwapAsync(Texture2D tex, string title, string desc, float flashTime, Rarity rarity)
	{
		// fade out a bit
		var tweenOut = CreateTween().SetParallel();
		tweenOut.TweenProperty(_icon,  "modulate:a", 0.2f, flashTime * 0.5f);
		tweenOut.TweenProperty(_title, "modulate:a", 0.2f, flashTime * 0.5f);
		tweenOut.TweenProperty(_descr, "modulate:a", 0.2f, flashTime * 0.5f);

		await ToSignal(tweenOut, Tween.SignalName.Finished);

		// swap contents
		if (tex != null) _icon.Texture = tex;
		if (!string.IsNullOrEmpty(title)) _title.Text = title;
		if (!string.IsNullOrEmpty(desc))  _descr.Text  = desc;
		Color rarityColor = rarity switch
		{
			Rarity.Grey   => new Color("#a0a0a0"),
			Rarity.Blue   => new Color("#3b82f6"),
			Rarity.Purple => new Color("#a855f7"),
			Rarity.Yellow => new Color("#facc15"),
			_             => new Color("white")
		};
				
		// RARITY BORDER ---
		var style = new StyleBoxFlat
		{
			BorderWidthLeft = 10,
			BorderWidthRight = 10,
			BorderWidthTop = 10,
			BorderWidthBottom = 10,
			BorderColor = rarityColor,
			BgColor = new Color(0.0f, 0.0f, 0.0f, 0.2f),
		};
		var hover = style.Duplicate() as StyleBoxFlat;
		hover.BgColor = new Color(0.0f, 0.0f, 0.0f, 0.15f);
		AddThemeStyleboxOverride("normal", style);
		AddThemeStyleboxOverride("hover", hover);
		AddThemeStyleboxOverride("pressed", style);
		AddThemeStyleboxOverride("disabled", style);
		AddThemeStyleboxOverride("focus", hover);
		// ---

		// fade back in
		var tweenIn = CreateTween().SetParallel();
		tweenIn.TweenProperty(_icon,  "modulate:a", 1f, flashTime * 0.5f);
		tweenIn.TweenProperty(_title, "modulate:a", 1f, flashTime * 0.5f);
		tweenIn.TweenProperty(_descr, "modulate:a", 1f, flashTime * 0.5f);
		await ToSignal(tweenIn, Tween.SignalName.Finished);
	}

	private float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
}
