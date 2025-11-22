using System;
using Godot;

namespace GameJam25.scripts.sfx_system;

public partial class PooledAudioStreamPlayer : AudioStreamPlayer
{
    public AudioPlayerType _type = AudioPlayerType.Regular;
    public int PoolIndex { get; set; }
    public Action<int, AudioPlayerType> OnFinishedCallback { get; set; }
    
    public override void _Ready()
    {
        Finished += HandleFinished;
    }
    
    private void HandleFinished()
    {
        OnFinishedCallback?.Invoke(PoolIndex, _type);  // Call back with index
    }
}