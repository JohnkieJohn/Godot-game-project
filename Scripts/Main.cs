using Godot;
using System;
using System.Numerics;

public partial class Main : Node2D
{
	// private PackedScene player = (PackedScene)ResourceLoader.Load("res://Scenes/player.tscn");
	private PackedScene enemy1 = (PackedScene)ResourceLoader.Load("res://Scenes/Paths/path_2d_1_enemy_1.tscn");
	private PackedScene enemy2 = (PackedScene)ResourceLoader.Load("res://Scenes/enemy_2.tscn");
	private Timer spawnTimer1;
	private Timer spawnTimer2;
	public ColorRect filterRect;
	private ParallaxBackground background;
	private ParallaxBackground backgroundOver;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		spawnTimer1 = GetNode<Timer>("Enemy1spawn");
		spawnTimer1.Timeout += OnEnemy1SpawnTimeOut;
		spawnTimer1.Start();

		spawnTimer2 = GetNode<Timer>("Enemy2spawn");
		spawnTimer2.Timeout += OnEnemy2SpawnTimeOut;
		spawnTimer2.Start();

		filterRect = GameManager.Instance.filterRect;
		AddChild(filterRect);
		filterRect.MoveToFront();

		// Player instance = (Player)player.Instantiate();
		// AddChild(instance);
		// instance.MoveToFront();

		background = GetNode<ParallaxBackground>("ParallaxBackground");
		backgroundOver = GetNode<ParallaxBackground>("ParallaxBackground2");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		background.ScrollBaseOffset += new Godot.Vector2(0, 40) * (float)delta;
		backgroundOver.ScrollBaseOffset += new Godot.Vector2(0, 120) * (float)delta;
	}

	private void OnEnemy1SpawnTimeOut()
	{
		Random rand = new Random();
		Node instance = enemy1.Instantiate();

		// X entre 0 et 1150
		float x = rand.Next(100, 300);
		// Y entre 0 et 250
		float y = rand.Next(-50, -10);
		
		((Path2d1Enemy1)instance).Initialize(new Godot.Vector2(x, y));
		AddChild(instance);
		MoveChild(instance, 2);
	}

	private void OnEnemy2SpawnTimeOut()
	{
		Random rand = new Random();
		Node instance = enemy2.Instantiate();

		 // X entre 0 et 1150
		float x = rand.Next(40, 440);
		// Y entre 0 et 250
		float y = rand.Next(-100, -10);
		
		((Enemy2)instance).Initialize(new Godot.Vector2(x, y));
		AddChild(instance);
		MoveChild(instance, 2);
	}
}
