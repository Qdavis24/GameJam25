using Godot;
using System;
namespace GameJam25.scripts.state_machines;
public partial class DebugCurrentStateLabel : Label
{
    public void OnChangeCurrentStateLabel(String nextState)
    {
        
        Text = nextState;
    }
}