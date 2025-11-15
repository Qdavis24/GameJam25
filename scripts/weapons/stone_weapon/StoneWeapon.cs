using System.Collections.Generic;
using GameJam25.scripts.weapons.base_classes;
using Godot;


public partial class StoneWeapon : WeaponBase
{
    [Export] private PackedScene _stonePackedScene;
    [Export] private float _offset = 50.0f;

    private Stone[] _stones;
    private float[] _stoneAngles;
    private float _angleBetweenStones;


    public override void InitWeapon()
    {
        if (_stones != null)
            foreach (Stone stone in _stones)
            {
                stone.QueueFree();
            }

        _stones = new Stone[(int)_projCount];
        _stoneAngles = new float[(int)_projCount];
        GetStartingAngles();
        for (int i = 0; i < (int)_projCount; i++)
        {
            var newStone = _stonePackedScene.Instantiate<Stone>();
            newStone.Position += Vector2.FromAngle(_stoneAngles[i]) * _offset;
            AddChild(newStone);

            newStone.Damage = _projDamage;
            newStone.Scale *= _projSize;
            newStone.Timer.WaitTime = _projCooldown;
            
            _stones[i] = newStone;
        }
        _active = true;
    }

    private void GetStartingAngles()
    {
        _angleBetweenStones = Mathf.Tau / (int)_projCount;
        var currRad = 0.0f;
        for (int i = 0; i < (int)_projCount; i++)
        {
            _stoneAngles[i] = currRad;
            currRad += _angleBetweenStones;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!_active) return;
        
        var currRadIncrease = (_projSpeed / _offset) * (float)delta;

        for (int i = 0; i < (int)_projCount; i++)
        {
            var newPos = _stones[i].Position.Rotated(currRadIncrease);
            _stones[i].Rotation = newPos.Angle();
            _stones[i].Position = newPos;
        }
    }
}