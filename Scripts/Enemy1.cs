using Godot;
using System;
using System.Numerics;

public partial class Enemy1 : CharacterBody2D
{
	private PackedScene bullet = (PackedScene)ResourceLoader.Load("res://Scenes/enemy_1_bullet.tscn");
	// private float speed = 80f;
	public float health = 200;
	private Player player = null;
	private bool canShoot = true;
	private Marker2D spawnPos;
	private Timer shootTimer;

	// private float zigzagSpeed = 100f; // Amplitude du zigzag
	// private float zigzagFrequency = 2f; // Fréquence du zigzag (changements de direction)
	// private double elapsedTime = 0f; // Temps écoulé

	// private float speedVariationAmplitude = 50f; // Amplitude de la variation de vitesse
	// private float speedVariationFrequency = 1f; // Fréquence de variation de la vitesse

	private Area2D areaDetection;
	// private Area2D deathOnContact;

	private AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		areaDetection = GetNode<Area2D>("Detection");
		areaDetection.BodyEntered += OnBodyEnteredArea;
		areaDetection.BodyExited += OnBodyExitedArea;

		// deathOnContact = GetNode<Area2D>("DeathOnContact");
		// deathOnContact.BodyEntered += OnBodyEnteredDeathArea;

		spawnPos = GetNode<Marker2D>("Spawnpos");

		shootTimer = GetNode<Timer>("Shootspeed");
		shootTimer.Timeout += OnShootSpeedTimeOut;

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.AnimationFinished += BaseAnimation;
		animationPlayer.Play("enemy1");
	}

	public override void _PhysicsProcess(double delta)
	{
		// // Incrémentation du temps
		// elapsedTime += delta;

		// // Variation de la vitesse verticale au cours du temps
		// float verticalSpeed = (float)(speed + Mathf.Sin(elapsedTime * speedVariationFrequency) * speedVariationAmplitude);

		// // Calculer la direction en zigzag avec alternance basée sur le temps
		// float zigzagDirection = (float)(Mathf.Sin(elapsedTime * zigzagFrequency) * zigzagSpeed);

		// // Calculer la nouvelle position X
		// float newX = Position.X + zigzagDirection;

		// // Limiter la position X (par exemple aux bords de l'écran ou à une zone spécifique)
		// newX = Mathf.Clamp(newX, -10, 490);  // Limite gauche et droite (peut être ajusté selon la zone de jeu)

		// // Créer le vecteur de déplacement avec la nouvelle position X limitée
		// Godot.Vector2 velocity = new Godot.Vector2(newX - Position.X, verticalSpeed);

		// Velocity = velocity;
		// MotionMode = MotionModeEnum.Floating;
		// WallMinSlideAngle = 0;
		// MoveAndSlide();

		// Si tu veux obtenir la collision après le mouvement
		var collision = GetLastSlideCollision();
		if (collision != null)
		{
			Node2D collider = (Node2D)collision.GetCollider();
			if (collider.IsInGroup("Player"))
			{
				player.PlayerHit();
			}
		}

		// if (GameManager.Instance.IsOutOfScreen(GetViewportRect(), Position.Y, Position.X))
		// {
		// 	QueueFree();
		// 	GD.Print("ennemi supprimé");
		// }
	}

	// public void Initialize(Godot.Vector2 spawnPos)
	// {
	// 	this.Position = spawnPos;
	// }

	private void OnBodyEnteredArea(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			player = body as Player;
			if (shootTimer.IsStopped()) // Si le timer est arrêté, redémarrer pour le tir
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
			if (!shootTimer.IsStopped()) // Si le timer est activé
			{
				canShoot = false;
				shootTimer.Stop();
			}
		}
	}

	// private void OnBodyEnteredDeathArea(Node2D body)
	// {
	// 	if (body.IsInGroup("Player"))
	// 	{
	// 		if(player == null)
	// 		{
	// 			player = body as Player;
	// 			player.PlayerHit();
	// 		}
	// 		else
	// 		{
	// 			player.PlayerHit();
	// 		}
	// 	}
	// }

	private void OnShootSpeedTimeOut()
	{
		// Tirer uniquement si canShoot est vrai
		if (canShoot)
		{
			Shoot();
			canShoot = true; // Permettre de tirer à nouveau après chaque timeout
		}
	}

	private void Shoot()
	{
		if (canShoot)
		{
			Godot.Vector2 hitBoxDirection = player.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition;

			// Instancier la scène (le nœud racine de cette scène)
			Node instance = bullet.Instantiate();

			((Enemy1Bullet)instance).Initialize(spawnPos.GlobalPosition, this.GlobalPosition.DirectionTo(hitBoxDirection), this);

			// Ajouter l'instance de la scène à la racine de la scène actuelle (parent du joueur)
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
				GameManager.Instance.Ui.UpdateBulletTimeBar(5);

			}
			else if (GameManager.Instance.isBulletTime) // Bullet cancel en mode bullet time
			{
				foreach (Node child in GetTree().Root.GetChildren())
				{
					if (child is Enemy1Bullet)
					{
						Enemy1Bullet bullet = (Enemy1Bullet)child;
						if (bullet.GetShooter() == this)
						{
							bullet.QueueFree();
							GameManager.Instance.Ui.UpdateScore(35);
						}
					}
				}
			}
			GameManager.Instance.Ui.UpdateScore(250);
			// Notifier le GameManager que l'ennemi est mort et demander une explosion
			GameManager.Instance.TriggerExplosion(this.GlobalPosition);
			QueueFree();
		}
	}
}
