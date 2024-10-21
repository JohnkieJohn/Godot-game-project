using Godot;
using System;

public partial class Enemy1Bullet : Area2D
{
	private float speed = 150;
	private Vector2 direction;
	private float rotationSpeed = 20;
	private Enemy1 shooter;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.BodyEntered += OnBodyEnteredArea;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 movement = this.Position += direction * speed * (float)delta;

		// faire tourner le sprite
		Rotate(rotationSpeed * (float)delta);

		if (GameManager.Instance.IsOutOfScreen(GetViewportRect(), Position.Y, Position.X))
		{
			QueueFree();
			GD.Print("projectile ennemi supprimé");
		}
	}

	public void Initialize(Vector2 startPosition, Vector2 shootDirection, Enemy1 enemy1)
	{
		this.Position = startPosition;
		this.direction = shootDirection.Normalized();
		this.shooter = enemy1;
	}

	private void OnBodyEnteredArea(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			Player player = body as Player;
			player.PlayerHit();
			QueueFree();
		}
	}

	public Enemy1 GetShooter()
	{
		return shooter;
	}
}
