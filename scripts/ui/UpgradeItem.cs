using Godot;
using System;
using System.Collections.Generic;

public enum Item { Fireball, Stick, Bird, Stone, Thorn }
public enum Rarity { Grey, Blue, Purple, Yellow }
public enum Stat
{
	SizePct,
	CooldownPct,
	ProjectileSpeedPct,
	ProjectileCount,
	DamagePct,
}

public sealed class UpgradeItem
{
	// -------- instance data (per rolled upgrade) --------
	public Item Item { get; private set; }
	public Rarity Rarity { get; private set; }
	public Stat Stat { get; private set; }
	public float Value { get; private set; }
	public string Description { get; private set; }

	// -------- shared stuff (static) --------
	private static readonly Dictionary<(Stat, Rarity), float> Values = new()
	{
		// Size %
		{ (Stat.SizePct,            Rarity.Grey),   5f },
		{ (Stat.SizePct,            Rarity.Blue),   7f },
		{ (Stat.SizePct,            Rarity.Purple),10f },
		{ (Stat.SizePct,            Rarity.Yellow),15f },

		// Cooldown %
		{ (Stat.CooldownPct,        Rarity.Grey),   3f },
		{ (Stat.CooldownPct,        Rarity.Blue),   5f },
		{ (Stat.CooldownPct,        Rarity.Purple), 7f },
		{ (Stat.CooldownPct,        Rarity.Yellow),10f },

		// Projectile speed %
		{ (Stat.ProjectileSpeedPct, Rarity.Grey),  10f },
		{ (Stat.ProjectileSpeedPct, Rarity.Blue),  15f },
		{ (Stat.ProjectileSpeedPct, Rarity.Purple),20f },
		{ (Stat.ProjectileSpeedPct, Rarity.Yellow),25f },

		// Projectile count (fractional ladder)
		{ (Stat.ProjectileCount,       Rarity.Grey),   0.5f },
		{ (Stat.ProjectileCount,       Rarity.Blue),   1.0f },
		{ (Stat.ProjectileCount,       Rarity.Purple), 1.5f },
		{ (Stat.ProjectileCount,       Rarity.Yellow), 2.0f },

		// Damage %
		{ (Stat.DamagePct,          Rarity.Grey),   5f },
		{ (Stat.DamagePct,          Rarity.Blue),   7f },
		{ (Stat.DamagePct,          Rarity.Purple),10f },
		{ (Stat.DamagePct,          Rarity.Yellow),15f },
	};

	private static readonly RandomNumberGenerator Rng = new();

	static UpgradeItem() => Rng.Randomize();

	// -------- factory: roll a NEW upgrade each call --------
	public static UpgradeItem Roll()
	{
		var upg = new UpgradeItem
		{
			Item   = GetRandomEnumValue<Item>(),
			Rarity = GetRandomEnumValue<Rarity>(),
			Stat   = GetRandomEnumValue<Stat>()
		};

		// look up value, default 0 if missing
		upg.Value = Values.TryGetValue((upg.Stat, upg.Rarity), out var v) ? v : 0f;

		// human-friendly description (note cooldown uses "-%")
		upg.Description = upg.Stat switch
		{
			Stat.SizePct             => $"+{upg.Value:0}% Size",
			Stat.CooldownPct         => $"-{upg.Value:0}% Cooldown",
			Stat.ProjectileSpeedPct  => $"+{upg.Value:0}% Projectile Speed",
			Stat.ProjectileCount        => $"+{FormatProjectileValue(upg.Value)} Projectile{Plural(upg.Value)}",
			Stat.DamagePct           => $"+{upg.Value:0}% Damage",
			_ => ""
		};

		return upg;
	}

	// -------- helpers --------
	private static T GetRandomEnumValue<T>() where T : Enum
	{
		var values = (T[])Enum.GetValues(typeof(T));
		int index = Rng.RandiRange(0, values.Length - 1);
		return values[index];
	}

	
	private static string FormatProjectileValue(float v)
	{
		// Check for halves
		if (Mathf.Abs(v - 0.5f) < 0.01f)
			return "0.5";
		if (Mathf.Abs(v % 1f - 0.5f) < 0.01f)
			return $"{(int)v}.5"; // e.g., 1.5 → "1.5"
		return v.ToString("0");
	}
	private static string Plural(float v) => v >= 1.5f ? "s" : "";
}
