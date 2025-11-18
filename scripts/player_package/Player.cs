using Godot;
using System;
using System.Collections.Generic;
using GameJam25.scripts.damage_system;
using GameJam25.scripts.weapons.base_classes;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void StatsInitializedEventHandler(float health, float maxHealth, int xp, int maxXp, float level);

    [Signal]
    public delegate void HealthChangedEventHandler(float newHealth);

    [Signal]
    public delegate void MaxHealthChangedEventHandler(float newMaxHealth);

    [Signal]
    public delegate void XpChangedEventHandler(int newXp);

    [Signal]
    public delegate void LevelChangedEventHandler(int newLevel);

    [Signal]
    public delegate void DiedEventHandler();

    [ExportCategory("Stats")] [Export] private float _maxHealth = 100.0f;
    [Export] public float Speed = 400f;
    [Export] private float LungeSpeed = 700f;
    [Export] private float LungeDuration = 0.35f;

    [ExportCategory("Xp Behavior")] [Export]
    public int MaxXp = 100;

    [Export] private float _xpSpeedIncreaseRange = 200;
    [Export] private Curve _xpSpeedRamp;
    [Export] private float _maxXpGrowthRate = 1.5f;
    [Export] private float _xpMagnetSpeed = 20f;
    [Export] private float _xpAcquireRange = 70f;

    [ExportCategory("Required Resources")] [Export]
    public string AnimationSet = "fox";

    [Export] public PackedScene SlashAttack;

    // internal refs
    private Area2D _hurtbox;
    private Area2D _pickupRange;
    private AnimatedSprite2D _anim;
    private AudioStreamPlayer _audio;
    private Dictionary<Weapon, WeaponBase> _weapons = new();

    // Xp state vars
    private List<Xp> _xpInRange = new();

    // Player current stats
    private float _health;
    private int _xp;
    private int _level;


    // Lunge State vars
    private bool lunging = false;
    private float lungeTime = 0f;
    private Vector2 lungeDir = Vector2.Right;

    public void InitForWorldLevel()
    {
        foreach (var xp in _xpInRange)
        {
            GameManager.Instance.XpPool.ReturnXp(xp);
        }

        _xpInRange.Clear();
    }

    public override void _Ready()
    {
        // wire up internal refs

        _weapons[Weapon.Fireball] = GetNode<WeaponBase>("FireballWeapon");
        _weapons[Weapon.Stone] = GetNode<WeaponBase>("StoneWeapon");
        _weapons[Weapon.Cloud] = GetNode<WeaponBase>("CloudWeapon");
        _weapons[Weapon.Water] = GetNode<WeaponBase>("WaterWeapon");

        _anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _audio = GetNode<AudioStreamPlayer>("WalkSound");

        _hurtbox = GetNode<Area2D>("Hurtbox");
        _pickupRange = GetNode<Area2D>("PickupRange");

        _hurtbox.AreaEntered += OnHurtboxEntered;
        _pickupRange.AreaEntered += OnPickupRangeEntered;


        _health = _maxHealth;
        _xp = 0;
        _level = 1;

        EmitSignalStatsInitialized(_health, _maxHealth, _xp, MaxXp, _level);

        _anim.Play(AnimationSet + "_idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        GetInput(delta);
        MoveAndSlide();
        AttractXp();
    }

    private void AttractXp()
    {
        for (int i = _xpInRange.Count - 1; i >= 0; i--)
        {
            var xp = _xpInRange[i];
            var dir = (GlobalPosition - xp.GlobalPosition);
            var lengthSquared = dir.LengthSquared();
            if (lengthSquared < _xpAcquireRange)
            {
                _xp += xp.Amount;
                while (_xp >= MaxXp)
                {
                    _xp -= MaxXp;
                    MaxXp = (int)(MaxXp * _maxXpGrowthRate);
                    _level++;
                    EmitSignalXpChanged(_xp);
                    EmitSignalLevelChanged(_level);
                }

                GameManager.Instance.XpPool.ReturnXp(xp);
                EmitSignalXpChanged(_xp);
                _xpInRange.RemoveAt(i);
            }
            else
            {
                xp.Position += dir.Normalized() *
                               _xpSpeedRamp.Sample(
                                   (float)Mathf.Clamp(Math.Pow(_xpSpeedIncreaseRange, 2) / lengthSquared, 0, 1)) *
                               _xpMagnetSpeed;
            }
        }
    }

    private void Slash(Vector2 dir)
    {
        lunging = true;
        lungeTime = LungeDuration;
        lungeDir = dir;
        Velocity = dir * LungeSpeed;

        var slash = SlashAttack.Instantiate<SlashAttackNode2d>();
        slash.Position = dir.Normalized() * 10f; // a little in front of char
        slash.Direction = dir;
        AddChild(slash);
    }

    private void Move(Vector2 moveDir)
    {
        float offset = 8.130104f; // isometric diagonal offset

        // Nudge diagonal movement to match isometric tile perspective
        if (moveDir.X != 0 && moveDir.Y != 0)
        {
            bool up = moveDir.Y < 0;
            bool right = moveDir.X > 0;

            // Map the four diagonals to ±offset:
            // up-right  -> +offset
            // up-left   -> -offset
            // down-left -> +offset
            // down-right-> -offset
            float angleOffset =
                (right ? -1f : 1f) * (up ? -1f : 1f) * Mathf.DegToRad(offset);

            moveDir = moveDir.Rotated(angleOffset).Normalized();
        }

        // Slow down vertical only movement
        float speed = Speed;
        if (moveDir.X == 0 && moveDir.Y != 0)
            speed *= 0.9f;

        // Move
        Velocity = moveDir * speed;
    }

    private void UpdateAnimation(Vector2 moveDir)
    {
        void Play(string animation)
        {
            if (_anim.Animation != AnimationSet + "_" + animation)
                _anim.Play(AnimationSet + "_" + animation);
        }

        // Flip sprite horizontally for left vs right movement
        if (moveDir.X != 0) _anim.FlipH = moveDir.X < 0;

        if (moveDir == Vector2.Zero)
        {
            Play("idle");
        }
        else
        {
            Sfx.I.PlayFootstep(_audio.Stream, GlobalPosition);
            Play("walk");
        }
    }

    private void GetInput(double delta)
    {
        if (lunging)
        {
            lungeTime -= (float)delta;
            Velocity = lungeDir * LungeSpeed;

            _anim.Play(AnimationSet + "_walk");

            if (lungeDir.X != 0)
                _anim.FlipH = lungeDir.X < 0;

            if (lungeTime <= 0f)
                lunging = false;

            return;
        }

        if (Input.IsActionJustPressed("attack"))
        {
            Vector2 mousePos = GetGlobalMousePosition();
            Vector2 dir = (mousePos - GlobalPosition).Normalized();
            if (dir == Vector2.Zero) dir = Vector2.Right;

            Slash(dir);
            return;
        }

        Vector2 moveDir = Input.GetVector("left", "right", "up", "down");

        Move(moveDir);
        UpdateAnimation(moveDir);
    }

    private void TakeDamage(float amount, Vector2 dir)
    {
        GameManager.Instance.Cam.Shake(amount);
        _health = Mathf.Clamp(_health - amount, 0, _maxHealth);
        EmitSignalHealthChanged(_health);

        if (_health == 0)
        {
            EmitSignalDied(); // send stats here?
            return;
        }

        
    }

    private void IncreaseXp(int amount)
    {
        _xp += amount;
        if (_xp >= MaxXp)
        {
            _level++;
            EmitSignalLevelChanged(_level);
        }
    }

    public void UpgradeWeapon(WeaponUpgrade upg)
    {
        _weapons[upg.Weapon].Upgrade(upg);
    }

    public void UnlockWeapon(Weapon weapon)
    {
        _weapons[weapon].InitWeapon();
    }

    // Physics Signals below
    private void OnHurtboxEntered(Area2D area)
    {
        if (area.IsInGroup("EnemyHitbox"))
        {
            Hitbox hb = (Hitbox)area;
            TakeDamage(hb.Damage, GlobalPosition - hb.GlobalPosition);
        }
    }

    private void OnPickupRangeEntered(Area2D area)
    {
        if (!area.IsInGroup("Xp")) return;
        _xpInRange.Add(area as Xp);
    }

    private void OnPickupRangeExited(Area2D area)
    {
        if (!area.IsInGroup("Xp")) return;
        _xpInRange.Remove(area as Xp);
    }
}