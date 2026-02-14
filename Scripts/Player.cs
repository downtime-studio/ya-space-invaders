using Godot;

namespace DodgeTheSquares.Scripts;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 450f;
	[Export] public float BottomMargin = 40f;
	[Export] public float HalfWidth = 16f;

	[Export] public PackedScene BulletScene;
	[Export] public float ShootCooldown = 0.20f;

	private double _cooldownLeft;
	private Marker2D _muzzle;
	
	private Node _activeBullet;

	public override void _Ready()
	{
		_muzzle = GetNode<Marker2D>("Muzzle");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Movement (X only)
		float axis = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		Velocity = new Vector2(axis * Speed, 0);
		MoveAndSlide();

		ClampToViewportX();
		LockToBottom();

		// Shooting
		_cooldownLeft -= delta;
		
		bool isBulletAlive = GodotObject.IsInstanceValid(_activeBullet);
		if (!Input.IsActionJustPressed("shoot") || !(_cooldownLeft <= 0) || isBulletAlive) return;
		Fire();
		_cooldownLeft = ShootCooldown;
	}

	private void Fire()
	{
		if (BulletScene == null)
		{
			GD.PrintErr("BulletScene not assigned on Player.");
			return;
		}

		var bullet = BulletScene.Instantiate<Area2D>();
		bullet.GlobalPosition = _muzzle.GlobalPosition;

		// Add bullets to the Level (not as child of Player) to keep scene clean
		GetTree().CurrentScene.AddChild(bullet);
		_activeBullet = bullet;
	}

	private void ClampToViewportX()
	{
		var vp = GetViewportRect();
		var pos = GlobalPosition;

		float minX = vp.Position.X + HalfWidth;
		float maxX = vp.End.X - HalfWidth;

		pos.X = Mathf.Clamp(pos.X, minX, maxX);
		GlobalPosition = pos;
	}

	private void LockToBottom()
	{
		var vp = GetViewportRect();
		var pos = GlobalPosition;
		pos.Y = vp.End.Y - BottomMargin;
		GlobalPosition = pos;
	}
}
