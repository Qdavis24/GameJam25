using Godot;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;


public partial class UpgradeOption : PanelContainer
{
	private TextureRect _icon;
	private RichTextLabel _title;
	private RichTextLabel _descr;
	private ColorRect _rarityBorder;

	public static readonly Dictionary<Item, Texture2D> Icons = new()
	{
		{ Item.Fireball, GD.Load<Texture2D>("res://assets/ui/icons/fireball.png") },
		{ Item.Stick,    GD.Load<Texture2D>("res://assets/ui/icons/stick.png") },
		{ Item.Bird,     GD.Load<Texture2D>("res://assets/ui/icons/bird.png") },
		{ Item.Stone,    GD.Load<Texture2D>("res://assets/ui/icons/stone.png") },
		{ Item.Thorn,    GD.Load<Texture2D>("res://assets/ui/icons/thorn.png") },
	};

	public static readonly Dictionary<Item, string> Names = new()
	{
		{ Item.Fireball, "Flaming Fireball" },
		{ Item.Stick,    "Stabbing Stick" },
		{ Item.Bird,     "Homing Hatchling" },
		{ Item.Stone,    "Shielding Stone" },
		{ Item.Thorn,    "Throwing Thorn" },
	};

	public override void _Ready()
	{
		//_icon = GetNode<TextureRect>("MarginContainer/ColorRect2/HBoxContainer/MarginContainer/Icon");
		//_title = GetNode<RichTextLabel>("MarginContainer/ColorRect2/HBoxContainer/VBoxContainer/Title");
		//_descr = GetNode<RichTextLabel>("MarginContainer/ColorRect2/HBoxContainer/VBoxContainer/Description");
		_icon = GetNode<TextureRect>("HBoxContainer/MarginContainer/Icon");
		_title = GetNode<RichTextLabel>("HBoxContainer/VBoxContainer/Title");
		_descr = GetNode<RichTextLabel>("HBoxContainer/VBoxContainer/Description");
		_rarityBorder = GetNode<ColorRect>("RarityBorder");

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

			var flashUpgrade = UpgradeItem.Roll();
			var tex   = Icons[flashUpgrade.Item];
			var title = Names[flashUpgrade.Item];
			var desc  = flashUpgrade.Description;

			await FlashSwapAsync(tex, title, desc, 0.06f, flashUpgrade.Rarity); // tiny flash for juice

			await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
		}

		var upgrade = UpgradeItem.Roll();
		var finalTex   = Icons[upgrade.Item];
		var finalTitle = Names[upgrade.Item];
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
		tweenOut.TweenProperty(_rarityBorder, "modulate:a", 0.8f, flashTime * 0.5f);

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
		_rarityBorder.Color = rarityColor;

		// fade back in
		var tweenIn = CreateTween().SetParallel();
		tweenIn.TweenProperty(_icon,  "modulate:a", 1f, flashTime * 0.5f);
		tweenIn.TweenProperty(_title, "modulate:a", 1f, flashTime * 0.5f);
		tweenIn.TweenProperty(_descr, "modulate:a", 1f, flashTime * 0.5f);
		tweenIn.TweenProperty(_rarityBorder, "modulate:a", 1f, flashTime * 0.5f);
		await ToSignal(tweenIn, Tween.SignalName.Finished);
	}

	private float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
}
