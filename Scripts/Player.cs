using Godot;
using System;
using System.Runtime.Intrinsics;

public partial class Player : CharacterBody2D
{
	private float speed = 300;

	private PackedScene bullet = (PackedScene)ResourceLoader.Load("res://Scenes/player_bullet.tscn");
	private Sprite2D playerSprite;

	// Booléens pour savoir quel type de tir est actif
	private bool isShootingAuto = false;
	private bool isShootingConcentrate = false;

	private Timer shootSpeedTimerAuto;
	private Timer shootSpeedTimerConcentrate;

	private Marker2D spawnPos1;
	private Marker2D spawnPos2;
	private Marker2D spawnPos3;

	private AnimationPlayer animationPlayer;
	private AnimationPlayer muzzleflashPlayer;

	public override void _Ready()
	{
		// // Récupérer la taille actuelle de la fenêtre
		// Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

		// // Calculer la position du bas au centre
		// float x = viewportSize.X / 2; // Centre en X
		// float y = viewportSize.Y - 50; // Assez près du bas, ajuste la valeur selon la hauteur du Player

		// // Appliquer la nouvelle position au joueur
		// Position = new Vector2(x, y);

		// Récupère le Timer
		shootSpeedTimerAuto = GetNode<Timer>("Shootspeedauto");
		shootSpeedTimerAuto.Timeout += OnShootSpeedAutoTimeOut;

		shootSpeedTimerConcentrate = GetNode<Timer>("Shootspeedconcentrate");
		shootSpeedTimerConcentrate.Timeout += OnShootSpeedConcentrateTimeOut;

		spawnPos1 = GetNode<Marker2D>("Spawnbulletpos1");
		spawnPos2 = GetNode<Marker2D>("Spawnbulletpos2");
		spawnPos3 = GetNode<Marker2D>("Spawnbulletpos3");

		playerSprite = GetNode<Sprite2D>("Sprite2D");

		// Récupérer le AnimationPlayer
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.AnimationFinished += OnAnimationFinishedFinalLoop;

		muzzleflashPlayer = GetNode<AnimationPlayer>("MuzzleflashPlayer");

		// Par défaut, jouer l'animation "neutral" en boucle
		animationPlayer.Play("neutral");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 movement = GetMovementInput();

		// Appliquer le mouvement en tenant compte du delta pour une vitesse constante
		Vector2 newPosition = this.Position + movement * speed * (float)delta;

		// Obtenir la taille du Viewport (la fenêtre de jeu)
		Rect2 viewportRect = GetViewportRect();

		// Taille du sprite du joueur pour les limites
		Vector2 playerSize = playerSprite.GetTexture().GetSize() * Scale;

		// Clamper la position pour empêcher de dépasser les bords avec une marge de 30
		newPosition.X = Mathf.Clamp(newPosition.X, 0 + (playerSize.X - 30) / 2, viewportRect.Size.X - (playerSize.X - 30) / 2);
		newPosition.Y = Mathf.Clamp(newPosition.Y, 0 + (playerSize.Y - 30) / 2, viewportRect.Size.Y - (playerSize.Y - 30) / 2);

		// Appliquer la nouvelle position clampée
		this.Position = newPosition;
		MoveAndSlide();

		// Tir auto
		if (Input.IsActionJustPressed("shoot_auto"))
		{
			if (!isShootingConcentrate)  // Si tir concentré n'est pas actif
			{
				isShootingAuto = true;  // Activer le tir auto
				shootSpeedTimerAuto.Start();
			}
		}
		if (Input.IsActionPressed("shoot_auto"))
		{
			if (!isShootingConcentrate && !isShootingAuto)  // Si tir concentré n'est pas actif
			{
				ShootAuto();
				isShootingAuto = true;  // Activer le tir auto
				shootSpeedTimerAuto.Start();
			}
		}
		if (Input.IsActionJustReleased("shoot_auto"))
		{
			ShootAuto();
			isShootingAuto = false;  // Désactiver le tir auto
			shootSpeedTimerAuto.Stop();
		}

		// Tir concentré
		if (Input.IsActionJustPressed("shoot_concentrate"))
		{
			if (isShootingAuto)  // Si le tir auto est actif
			{
				// Arrêter le tir auto si le tir concentré est pressé
				isShootingAuto = false;
				shootSpeedTimerAuto.Stop();
			}

			isShootingConcentrate = true;  // Activer le tir concentré
			shootSpeedTimerConcentrate.Start();
		}
		if (Input.IsActionJustReleased("shoot_concentrate"))
		{
			isShootingConcentrate = false;  // Désactiver le tir concentré
			shootSpeedTimerConcentrate.Stop();
			speed = 300;
		}

		AnimationToPlay(movement);

		if (Input.IsActionJustPressed("bullet_time"))
		{
			if (!GameManager.Instance.isBulletTime && GameManager.Instance.Ui.GetBulletTimeBarValue() > 0)
			{
				GameManager.Instance.isBulletTime = true;
				GameManager.Instance.ToggleBulletTime();
			}
			else if (GameManager.Instance.isBulletTime)
			{
				GameManager.Instance.isBulletTime = false;
				GameManager.Instance.ToggleBulletTime();
			}
		}

		// Si tu veux obtenir la collision après le mouvement
		var collision = GetLastSlideCollision();
		if (collision != null)
		{
			PlayerHit();
		}
	}

	private Vector2 GetMovementInput()
	{
		Vector2 movement = new();

		if (Input.IsActionPressed("ui_up"))
		{
			movement.Y -= 1;
		}
		else if (Input.IsActionPressed("ui_down"))
		{
			movement.Y += 1;
		}
		else if (Input.IsActionPressed("ui_left"))
		{
			movement.X -= 1;
		}
		else if (Input.IsActionPressed("ui_right"))
		{
			movement.X += 1;;
		}

		return movement.Normalized();
	}

	private void ShootAuto()
	{
		// Instancier la scène (le nœud racine de cette scène)
		Node instance1 = bullet.Instantiate();
		Node instance2 = bullet.Instantiate();
		Node instance3 = bullet.Instantiate();

		((PlayerBullet)instance1).Initialize(spawnPos1.GlobalPosition, new Vector2(-0.15f, -1), 25);
		((PlayerBullet)instance2).Initialize(spawnPos2.GlobalPosition, new Vector2(0.15f, -1), 25);
		((PlayerBullet)instance3).Initialize(spawnPos3.GlobalPosition, new Vector2(0, -1), 25);

		Sprite2D bulletSprite1 = ((PlayerBullet)instance1).GetNode<Sprite2D>("Sprite2D");
		bulletSprite1.Rotation = new Vector2(-0.18f, -1).Angle() + Mathf.Pi / 2;
		Sprite2D bulletSprite2 = ((PlayerBullet)instance2).GetNode<Sprite2D>("Sprite2D");
		bulletSprite2.Rotation = new Vector2(0.18f, -1).Angle() + Mathf.Pi / 2;

		// Ajouter l'instance de la scène à la racine de la scène actuelle (parent du joueur)
		GetTree().Root.AddChild(instance1);
		GetTree().Root.AddChild(instance2);
		GetTree().Root.AddChild(instance3);

		if (speed != 300)
		{
			speed = 300;
		}

		muzzleflashPlayer.Stop();
		muzzleflashPlayer.Play("muzzleflash");
	}

	private void ShootConcentrate()
	{
		// Instancier la scène (le nœud racine de cette scène)
		Node instance = bullet.Instantiate();

		((PlayerBullet)instance).Initialize(spawnPos3.GlobalPosition, new Vector2(0, -1), 80);
		Sprite2D bulletSprite = ((PlayerBullet)instance).GetNode<Sprite2D>("Sprite2D");
		bulletSprite.Scale = new Vector2(2.5f, 2.5f);

		// Ajouter l'instance de la scène à la racine de la scène actuelle (parent du joueur)
		GetTree().Root.AddChild(instance);

		if (speed != 200)
		{
			speed = 200;
		}

		muzzleflashPlayer.Stop();
		muzzleflashPlayer.Play("muzzleflash");
	}

	private void OnShootSpeedAutoTimeOut()
	{
		ShootAuto();
	}

	private void OnShootSpeedConcentrateTimeOut()
	{
		ShootConcentrate();
	}

	private void AnimationToPlay(Vector2 inputMovement)
	{
		if (Input.IsActionPressed("ui_right"))
		{
			if (isShootingConcentrate && animationPlayer.CurrentAnimation != "concentrate_right2")
			{
				if (animationPlayer.CurrentAnimation == "neutral_right2")
				{
					animationPlayer.Stop();
					animationPlayer.Play("concentrate_right2");
				}
				else if (animationPlayer.CurrentAnimation != "concentrate_right")
				{
					animationPlayer.Stop();
					animationPlayer.Play("concentrate_right");
				}
			}
			else if (isShootingAuto && animationPlayer.CurrentAnimation != "neutral_right2" || !isShootingAuto && !isShootingConcentrate && animationPlayer.CurrentAnimation != "neutral_right2")
			{
				if (animationPlayer.CurrentAnimation == "concentrate_right2")
				{
					animationPlayer.Stop();
					animationPlayer.Play("neutral_right2");
				}
				else if (animationPlayer.CurrentAnimation != "neutral_right")
				{
					animationPlayer.Stop();
					animationPlayer.Play("neutral_right");
				}
			}
		}
		else if (Input.IsActionJustReleased("ui_right"))
		{
			if (isShootingConcentrate)
			{
				animationPlayer.Stop();
				animationPlayer.Play("concentrate_right_bis");
			}
			else if (!isShootingConcentrate && !isShootingAuto || isShootingAuto)
			{
				animationPlayer.Stop();
				animationPlayer.Play("neutral_right_bis");
			}
		}
		else if (Input.IsActionPressed("ui_left"))
		{
			if (isShootingConcentrate && animationPlayer.CurrentAnimation != "concentrate_left2")
			{
				if (animationPlayer.CurrentAnimation == "neutral_left2")
				{
					animationPlayer.Stop();
					animationPlayer.Play("concentrate_left2");
				}
				else if (animationPlayer.CurrentAnimation != "concentrate_left")
				{
					animationPlayer.Stop();
					animationPlayer.Play("concentrate_left");
				}
			}
			else if (isShootingAuto && animationPlayer.CurrentAnimation != "neutral_left2" || !isShootingAuto && !isShootingConcentrate && animationPlayer.CurrentAnimation != "neutral_left2")
			{
				if (animationPlayer.CurrentAnimation == "concentrate_left2")
				{
					animationPlayer.Stop();
					animationPlayer.Play("neutral_left2");
				}
				else if (animationPlayer.CurrentAnimation != "neutral_left")
				{
					animationPlayer.Stop();
					animationPlayer.Play("neutral_left");
				}
			}
		}
		else if (Input.IsActionJustReleased("ui_left"))
		{
			if (isShootingConcentrate)
			{
				animationPlayer.Stop();
				animationPlayer.Play("concentrate_left_bis");
			}
			else if (!isShootingConcentrate && !isShootingAuto || isShootingAuto)
			{
				animationPlayer.Stop();
				animationPlayer.Play("neutral_left_bis");
			}
		}
		else if (animationPlayer.CurrentAnimation == "neutral" && isShootingConcentrate)
		{
			animationPlayer.Stop();
			animationPlayer.Play("concentrate");
		}
		else if (animationPlayer.CurrentAnimation == "concentrate" && isShootingAuto 
		|| animationPlayer.CurrentAnimation == "concentrate" && !isShootingAuto && !isShootingConcentrate)
		{
			animationPlayer.Stop();
			animationPlayer.Play("neutral");
		}
		
	}

	private void OnAnimationFinishedFinalLoop(StringName animName)
	{
		switch (animName)
		{
			case "concentrate_right":
				animationPlayer.Play("concentrate_right2");
				break;

			case "neutral_right":
				animationPlayer.Play("neutral_right2");
				break;

			case "concentrate_left":
				animationPlayer.Play("concentrate_left2");
				break;

			case "neutral_left":
				animationPlayer.Play("neutral_left2");
				break;

			case "concentrate_right_bis":
			case "concentrate_left_bis":
				animationPlayer.Play("concentrate");
				break;

			case "neutral_right_bis":
			case "neutral_left_bis":
				animationPlayer.Play("neutral");
				break;

			default:
				// Optionnel : gérer des cas non prévus
				break;
		}
	}

	public void PlayerHit()
	{
		GameManager.Instance.TriggerExplosion(this.Position);
		QueueFree();
	}
}
