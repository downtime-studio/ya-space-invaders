using Godot;


public partial class Enemy : Area2D
{
    [Signal]
    public delegate void DiedEventHandler();
    
    public void Die()
    {
        EmitSignal(SignalName.Died);
        QueueFree();
    }
}