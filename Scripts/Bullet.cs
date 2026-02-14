using Godot;

namespace DodgeTheSquares.Scripts;

public partial class Bullet : Area2D
{
	[Export] public float Speed = 650f;

	public override void _Ready()
	{
		// When bullet overlaps something (enemy), we handle it
		AreaEntered += OnAreaEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += Vector2.Up * Speed * (float)delta;

		// Kill bullet if it leaves the screen
		var vp = GetViewportRect();
		if (GlobalPosition.Y < vp.Position.Y - 50)
			QueueFree();
	}

	private void OnAreaEntered(Area2D other)
	{
		// If it hit an enemy, remove enemy and bullet
		if (other.IsInGroup("enemies"))
		{
			other.QueueFree();
			QueueFree();
		}
	}
}
