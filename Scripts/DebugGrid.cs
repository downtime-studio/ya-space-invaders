using Godot;

public partial class DebugGrid : Node2D
{
	[Export] public int GridSize = 64;
	[Export] public int Lines = 20;

	public override void _Draw()
	{
		for (int i = -Lines; i <= Lines; i++)
		{
			DrawLine(
				new Vector2(i * GridSize, -Lines * GridSize),
				new Vector2(i * GridSize, Lines * GridSize),
				Colors.DarkGray
			);

			DrawLine(
				new Vector2(-Lines * GridSize, i * GridSize),
				new Vector2(Lines * GridSize, i * GridSize),
				Colors.DarkGray
			);
		}
	}
}