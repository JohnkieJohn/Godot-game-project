using Godot;
using System;

public partial class Enemy1Bullet : Area2D
{
	private float speed = 150;  // Vitesse du projectile
	private Vector2 direction;
	private float rotationSpeed = 20; // Vitesse de rotation en radians par seconde
	private Enemy1 shooter;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.BodyEntered += OnBodyEnteredArea;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Déplacer le projectile dans la direction spécifiée
		Vector2 movement = this.Position += direction * speed * (float)delta;

		// Faire tourner le sprite
		Rotate(rotationSpeed * (float)delta);

		if (GameManager.Instance.IsOutOfScreen(GetViewportRect(), Position.Y, Position.X))
		{
			// Supprimer le projectile
			QueueFree();
			GD.Print("projectile ennemi supprimé");
		}
	}

	public void Initialize(Vector2 startPosition, Vector2 shootDirection, Enemy1 enemy1)
	{
		this.Position = startPosition;
		this.direction = shootDirection.Normalized();  // Normaliser la direction pour éviter d'influencer la vitesse
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
