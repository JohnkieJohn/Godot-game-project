using Godot;
using System;

public partial class Path2dBossStage1 : Path2D
{
	private PathFollow2D path;
	private double speed = 0.2;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		path = GetNode<PathFollow2D>("PathFollow2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		path.ProgressRatio += (float)(delta * speed);
	}

	public void Initialize(Vector2 spawnPos)
	{
		this.Position = spawnPos;
	}
}
