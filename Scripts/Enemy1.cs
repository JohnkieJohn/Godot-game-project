using Godot;
using System;
using System.Numerics;

public partial class Enemy1 : CharacterBody2D
{
	private PackedScene bullet = (PackedScene)ResourceLoader.Load("res://Scenes/enemy_1_bullet.tscn");
	public float health = 200;
	private Player player = null;
	private bool canShoot = true;
	private Marker2D spawnPos;
	private Timer shootTimer;

	private Area2D areaDetection;

	private AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		areaDetection = GetNode<Area2D>("Detection");
		areaDetection.BodyEntered += OnBodyEnteredArea;
		areaDetection.BodyExited += OnBodyExitedArea;

		spawnPos = GetNode<Marker2D>("Spawnpos");

		shootTimer = GetNode<Timer>("Shootspeed");
		shootTimer.Timeout += OnShootSpeedTimeOut;

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.AnimationFinished += BaseAnimation;
		animationPlayer.Play("enemy1");
	}

	public override void _PhysicsProcess(double delta)
	{
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

	private void OnBodyEnteredArea(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			player = body as Player;
			if (shootTimer.IsStopped())
			{
				canShoot = true;
				shootTimer.Start();
			}
		}
	}

	private void OnBodyExitedArea(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			player = null;
			if (!shootTimer.IsStopped())
			{
				canShoot = false;
				shootTimer.Stop();
			}
		}
	}

	private void OnShootSpeedTimeOut()
	{
		if (canShoot)
		{
			Shoot();
			canShoot = true;
		}
	}

	private void Shoot()
	{
		if (canShoot)
		{
			Godot.Vector2 hitBoxDirection = player.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition;

			Node instance = bullet.Instantiate();

			((Enemy1Bullet)instance).Initialize(spawnPos.GlobalPosition, this.GlobalPosition.DirectionTo(hitBoxDirection), this);

			GetTree().Root.AddChild(instance);

			shootTimer.Start();
		}
	}

	private void BaseAnimation(StringName animName)
	{
		animationPlayer.Play("enemy1");
	}

	public void EnemyHit(float damage)
	{
		animationPlayer.Stop();
		animationPlayer.Play("enemy1_hit");	

		health -= damage;
		if (health <= 0)
		{
			if (!GameManager.Instance.isBulletTime)
			{
				GameManager.Instance.Ui.UpdateBulletTimeBar(5); // si le bullet time n'est pas actif, mettre à jour le score du joueur

			}
			else if (GameManager.Instance.isBulletTime) // bullet cancel en mode bullet time
			{
				foreach (Node child in GetTree().Root.GetChildren())
				{
					if (child is Enemy1Bullet)
					{
						Enemy1Bullet bullet = (Enemy1Bullet)child;
						if (bullet.GetShooter() == this)
						{
							bullet.QueueFree(); // Supprimer toutes les instances de bullet en cours de l'ennemi
							GameManager.Instance.Ui.UpdateScore(35);
						}
					}
				}
			}
			GameManager.Instance.Ui.UpdateScore(250);
			// lance une animation d'explosion via le GameManager
			GameManager.Instance.TriggerExplosion(this.GlobalPosition);
			QueueFree();
		}
	}
}
