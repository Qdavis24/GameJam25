using Godot;
using System;
namespace GameJam25.scripts.Enemies;
public partial class CurrentState : Label
{
    public void OnChangeCurrentStateLabel(String nextState)
    {
        
        Text = nextState;
    }

    public override void _Ready()
    {
        GD.Print("enter");
    }
}