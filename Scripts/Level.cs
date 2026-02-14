using Godot;


public partial class Level : Node2D
{
    [Export] public PackedScene EnemyScene;
    [Export] public NodePath EnemiesContainerPath = "Enemies";

    [ExportGroup("Formation Spawn")] 
    [Export] public int Rows = 4;
    [Export] public int Cols = 10;
    
    [Export] public float SpacingX = 48f;
    [Export] public float SpacingY = 38f;
    [Export] public Vector2 FormationTopLeft = new(80, 80);

    [ExportGroup("Formation Movement")] [Export]
    public float FormationSpeed = 80f; // px/sec

    [Export] public float StepDown = 24f; // px
    [Export] public float ScreenMargin = 10f; // keep inside edges

    private enum GameState
    {
        Playing,
        Won,
        Lost
    }

    private GameState _state = GameState.Playing;

    private HUD _hud;
    private int _score;

    [Export] public float LoseLineY = 560f;

    private Node2D _enemies;
    private int _dir = 1;

    public override void _Ready()
    {
        GD.Print("Ready");
        var vp = GetViewportRect();
        var cam = GetNode<Camera2D>("Camera2D");
        cam.GlobalPosition = vp.Size / 2f;
        cam.Enabled = true;

        GD.Print($"Viewport size: {vp.Size}, Camera pos: {cam.GlobalPosition}");

        _enemies = GetNode<Node2D>(EnemiesContainerPath);
        SpawnFormation();

        _hud = GetNode<HUD>("Hud");
        _hud.HideMessage();
        _score = 0;
        _hud.SetScore(_score);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state != GameState.Playing)
        {
            if (Input.IsKeyPressed(Key.R))
                GetTree().ReloadCurrentScene();
            return;
        }

        if (_enemies == null || _enemies.GetChildCount() == 0)
            return;

        float dt = (float)delta;

        // Move horizontally
        _enemies.GlobalPosition += new Vector2(_dir * FormationSpeed * dt, 0);

        // After moving, check bounds
        var bounds = GetAliveEnemiesWorldBounds();
        var vp = GetViewportRect();

        float leftLimit = vp.Position.X + ScreenMargin;
        float rightLimit = vp.End.X - ScreenMargin;

        bool hitRight = bounds.End.X >= rightLimit;
        bool hitLeft = bounds.Position.X <= leftLimit;

        if (hitRight || hitLeft)
        {
            // Snap back so we don't stick out of bounds
            float overflow = 0f;
            if (hitRight) overflow = bounds.End.X - rightLimit;
            if (hitLeft) overflow = leftLimit - bounds.Position.X;

            _enemies.GlobalPosition += new Vector2(hitRight ? -overflow : overflow, 0);

            // Reverse + step down (classic behavior)
            _dir *= -1;
            _enemies.GlobalPosition += new Vector2(0, StepDown);
        }

        if (bounds.End.Y >= LoseLineY)
            Lose();
    }

    private void SpawnFormation()
    {
        if (EnemyScene == null)
        {
            GD.PrintErr("EnemyScene is not assigned on Level.");
            return;
        }

        foreach (var child in _enemies.GetChildren())
            child.QueueFree();

        for (int row = 0; row < Rows; row++)
        for (int col = 0; col < Cols; col++)
        {
            var enemy = EnemyScene.Instantiate<Enemy>();
            enemy.GlobalPosition = FormationTopLeft + new Vector2(col * SpacingX, row * SpacingY);

            // CONNECT SIGNAL
            enemy.Died += OnEnemyDied;

            _enemies.AddChild(enemy);
        }
    }

    private void OnEnemyDied()
    {
        if (_state != GameState.Playing)
            return;

        _score += 10;
        _hud.SetScore(_score);

        CallDeferred(nameof(CheckWinCondition));    
    }
    
    private void CheckWinCondition()
    {
        GD.Print($"Enemies left: {_enemies.GetChildCount()}");
        if ((_enemies.GetChildCount() - 1) == 0)
            Win();
    }
    
    /// <summary>
    /// Returns a Rect2 that bounds all enemy children (world-space).
    /// Uses each enemy's GlobalPosition as a point; we add a small padding.
    /// For pixel-art squares this is usually enough; we can refine later.
    /// </summary>
    private Rect2 GetAliveEnemiesWorldBounds()
    {
        bool hasAny = false;
        Vector2 min = Vector2.Zero;
        Vector2 max = Vector2.Zero;

        foreach (var child in _enemies.GetChildren())
        {
            if (child is not Node2D n) continue;

            Vector2 p = n.GlobalPosition;
            if (!hasAny)
            {
                min = p;
                max = p;
                hasAny = true;
            }
            else
            {
                min = min.Min(p);
                max = max.Max(p);
            }
        }

        // Add padding so edges feel correct (tweak to match enemy size)
        const float pad = 16f;
        min -= new Vector2(pad, pad);
        max += new Vector2(pad, pad);

        return new Rect2(min, max - min);
    }

    private void Win()
    {
        _state = GameState.Won;
        _hud.ShowMessage($"YOU WIN!\nFinal score: {_score}");
    }

    private void Lose()
    {
        _state = GameState.Lost;
        _hud.ShowMessage($"YOU LOSE!\nFinal score: {_score}");
    }
}