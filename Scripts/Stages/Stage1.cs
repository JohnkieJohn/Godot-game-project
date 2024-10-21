using Godot;
using System;

public partial class Stage1 : Node2D
{
	private ParallaxBackground background;
	private ParallaxBackground backgroundOver;
	private Node2D enemy2Cluster;
	private Node2D enemy1Cluster;
	private PackedScene enemy2 = (PackedScene)ResourceLoader.Load("res://Scenes/enemy_2.tscn");
	private PackedScene enemy1 = (PackedScene)ResourceLoader.Load("res://Scenes/Paths/path_2d_1_enemy_1.tscn");
	private PackedScene boss = (PackedScene)ResourceLoader.Load("res://Scenes/Paths/path_2d_boss_stage1.tscn");
	private Node2D bossCluster;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		background = GetNode<ParallaxBackground>("ParallaxBackground");
		backgroundOver = GetNode<ParallaxBackground>("ParallaxBackground2");

		enemy2Cluster = GetNode<Node2D>("Enemy2Cluster1");
		enemy1Cluster = GetNode<Node2D>("Enemy1Cluster");
		bossCluster = GetNode<Node2D>("BossCluster");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		background.ScrollBaseOffset += new Vector2(0, 40) * (float)delta;
		backgroundOver.ScrollBaseOffset += new Vector2(0, 120) * (float)delta;
	}

	public override void _PhysicsProcess(double delta)
	{
		foreach (Marker2D enemy2SpawnPos in enemy2Cluster.GetChildren())
		{
			if (Mathf.Abs(enemy2SpawnPos.Position.Y) - background.ScrollBaseOffset.Y < 0)
			{
				Node instance = enemy2.Instantiate();
				((Enemy2)instance).Initialize(new Vector2(enemy2SpawnPos.Position.X, enemy2SpawnPos.Position.Y));
				AddChild(instance);
				MoveChild(instance, 2);
				enemy2Cluster.RemoveChild(enemy2SpawnPos);
				enemy2SpawnPos.QueueFree();
			}
		}

		foreach (Marker2D enemy1SpawnPos in enemy1Cluster.GetChildren())
		{
			if (Mathf.Abs(enemy1SpawnPos.Position.Y) - background.ScrollBaseOffset.Y < 0)
			{
				GD.Print(enemy1SpawnPos.Position, background.ScrollBaseOffset);
				Node instance = enemy1.Instantiate();
				((Path2d1Enemy1)instance).Initialize(new Vector2(enemy1SpawnPos.Position.X, enemy1SpawnPos.Position.Y));
				AddChild(instance);
				MoveChild(instance, 2);
				enemy1Cluster.RemoveChild(enemy1SpawnPos);
				enemy1SpawnPos.QueueFree();
			}
		}

		foreach (Marker2D bossSpawnPos in bossCluster.GetChildren())
		{
			if (Mathf.Abs(bossSpawnPos.Position.Y) - background.ScrollBaseOffset.Y < 0)
			{
				GD.Print("GO");
				Node instance = boss.Instantiate();
				((Path2dBossStage1)instance).Initialize(new Vector2(bossSpawnPos.Position.X, bossSpawnPos.Position.Y));
				AddChild(instance);
				MoveChild(instance, 2);
				bossCluster.RemoveChild(bossSpawnPos);
				bossSpawnPos.QueueFree();
			}
		}
	}
}
