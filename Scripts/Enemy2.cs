using Godot;
using System;

public partial class Enemy2 : CharacterBody2D
{
	private float speed = 200f;
	private float health = 160;
	private Sprite2D hitSprite;
	private Timer hitSpriteTimer;

	private Player player = null;
	private Area2D areaDetection;

	private AnimationPlayer animationPlayer;
	private PackedScene bullet = (PackedScene)ResourceLoader.Load("res://Scenes/enemy_1_bullet.tscn");

	public override void _Ready()
	{
		areaDetection = GetNode<Area2D>("Detection");
		areaDetection.BodyEntered += OnBodyEnteredArea;

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.AnimationFinished += BaseAnimation;
		animationPlayer.Play("enemy2");

		hitSprite = GetNode<Sprite2D>("HitSprite");
		hitSpriteTimer = GetNode<Timer>("HitSpriteTimer");
		hitSpriteTimer.Timeout += OnTimeOutHitSpriteTimer;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 movement = new Vector2(0, 1) * speed;
		if (player == null || GameManager.Instance.IsOutOfScreen(GetViewportRect(), Position.Y, Position.X))
		{
			Rotate((float)delta);
			GD.Print(player, GameManager.Instance.IsOutOfScreen(GetViewportRect(), Position.Y, Position.X));
		}
		else
		{
			Rotate((float)(10 * delta));
			if (speed > 0)
			{
				speed -= 100 * (float)delta;
				if (speed < 0)
				{
					speed = 0; // s'assurer que la vitesse ne devienne pas négative
					Explode();
					QueueFree();
				}
			}
			movement = new Vector2(0, 1) * speed;
		}

		Velocity = movement;
		MotionMode = MotionModeEnum.Floating;
		WallMinSlideAngle = 0;
		this.MoveAndSlide();

		// collision avec le player
		var collision = GetLastSlideCollision();
		if (collision != null)
		{
			Node2D collider = (Node2D)collision.GetCollider();
			if (collider.IsInGroup("Player"))
			{
				player.PlayerHit();
			}
		}
	}

	public void Initialize(Vector2 spawnPos)
	{
		this.Position = spawnPos;
	}

	private void OnBodyEnteredArea(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			player = body as Player;
		}
	}

	private void BaseAnimation(StringName animName)
	{
		animationPlayer.Play("enemy2");
	}

	private void OnTimeOutHitSpriteTimer()
	{
		hitSprite.Visible = false;
	}

	private void Explode()
	{
		int bulletCount = 16; // nombre de bullet tirées lors de l'explosion
		float angleStep = Mathf.Tau / bulletCount;
		Vector2 spawnPosition = GlobalPosition;

		for (int i = 0; i < bulletCount; i++)
		{
			// calculer l'angle pour chaque bullet
			float angle = i * angleStep;

			// direction du tir
			Vector2 bulletDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized();

			Node instance = bullet.Instantiate();
			((Enemy1Bullet)instance).Initialize(spawnPosition, bulletDirection, null);

			GetTree().Root.AddChild(instance);
		}
	}

	public void EnemyHit(float damage)
	{
		hitSprite.Visible = true;
		hitSpriteTimer.Start();

		health = health - damage;
		if (health <= 0)
		{
			if (!GameManager.Instance.isBulletTime)
			{
				GameManager.Instance.Ui.UpdateBulletTimeBar(3);
			}
			GameManager.Instance.Ui.UpdateScore(200);
			// explosion via le GameManager
			GameManager.Instance.TriggerExplosion(this.Position);
			
			QueueFree();
		}
	}
}
