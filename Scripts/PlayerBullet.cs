using Godot;
using System;

public partial class PlayerBullet : Area2D
{
	private float speed = 900;
	private float damage;
	private Vector2 direction;

	private Sprite2D bulletSprite;

	private AnimationPlayer animationPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.BodyEntered += OnBodyEnteredArea;

		bulletSprite = GetNode<Sprite2D>("Sprite2D");

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.Play("player_bullet");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.Position += direction * speed * (float)delta;

		// vérifier si le tir est hors de l'écran
		if (GameManager.Instance.IsOutOfScreen(GetViewportRect(), Position.Y, Position.X))
		{
			QueueFree();
			GD.Print("projectile supprimé");
		}
	}

	public void Initialize(Vector2 startPosition, Vector2 shootDirection, float damageValue)
	{
		this.Position = startPosition;
		this.direction = shootDirection.Normalized();
		this.damage = damageValue;
	}

	private void OnBodyEnteredArea(Node2D body)
	{
		if (body.HasMethod("EnemyHit"))
		{
			if (body is Enemy1 enemy1)
			{
				enemy1.EnemyHit(damage);
				QueueFree();
			}
			else if (body is Enemy2 enemy2)
			{
				enemy2.EnemyHit(damage);
				QueueFree();
			}
		}
	}
}
