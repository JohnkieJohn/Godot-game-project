using Godot;
using System;

public partial class GameManager : Node
{
	public static GameManager Instance; // Singleton instance
	public ColorRect filterRect;
	public Ui Ui;
	public bool isBulletTime;
	private Timer bulletTimeTimer;

	public override void _Ready()
	{
		Instance = this;

		// Créer et ajouter un ColorRect
		filterRect = new ColorRect();
		Rect2 viewportRect = GetViewport().GetVisibleRect(); // Obtenir la taille du viewport
		filterRect.Size = viewportRect.Size; // Appliquer la taille du viewport
		filterRect.Color = new Color(1, 1, 1, 0); // Transparent au départ

		// Charger et appliquer le shader
		Shader shader = GD.Load<Shader>("res://bullet_time_filter.gdshader");

		if (shader == null)
		{
			GD.PrintErr("Shader not loaded properly.");
			return;
		}
		
		ShaderMaterial shaderMaterial = new ShaderMaterial();
		shaderMaterial.Shader = shader;
		filterRect.Material = shaderMaterial;

		PackedScene uiScene = (PackedScene)ResourceLoader.Load("res://Scenes/ui.tscn");
		Ui = uiScene.Instantiate() as Ui;
		AddChild(Ui);

		bulletTimeTimer = new Timer();
		bulletTimeTimer.WaitTime = 0.1f; // Exemple de temps d'attente
		AddChild(bulletTimeTimer);
		bulletTimeTimer.Timeout += OnBulletTimeTimerOut;
	}

	public bool IsOutOfScreen(Rect2 viewport, float positionY, float positionX)
	{		
		// Vérifier si le projectile est en dehors des limites de l'écran avec une marge
		return positionY < 0 || positionY > viewport.Size.Y || 
			   positionX < 0 || positionX > viewport.Size.X;
	}

	// Méthode pour déclencher une explosion
	public void TriggerExplosion(Vector2 position)
	{
		// Récupérer la scène principale (root)
		var mainScene = GetTree().CurrentScene;

		// Charger la scène d'explosion
		PackedScene explosionScene = (PackedScene)ResourceLoader.Load("res://Scenes/explosion_1.tscn");
		Node2D explosionInstance = (Node2D)explosionScene.Instantiate();
		explosionInstance.Position = position;

		// Ajouter l'explosion à la scène
		mainScene.AddChild(explosionInstance);

		// Jouer l'animation d'explosion
		AnimationPlayer animPlayer = explosionInstance.GetNode<AnimationPlayer>("AnimationPlayer");
		animPlayer.Play("explosion1");

		// Optionnel : Supprimer l'explosion après l'animation
		animPlayer.AnimationFinished += (StringName anim) =>
		{
			if (anim == "explosion1")
			{
				explosionInstance.QueueFree();
			}	
		};
	}

	public void ToggleBulletTime()
	{
		if (!isBulletTime)
		{
			// Revenir à la vitesse normale
			Engine.TimeScale = 1.0f;

			ShaderMaterial shaderMaterial = (ShaderMaterial)filterRect.Material;

			// Remettre les paramètres à leur état initial
			shaderMaterial.SetShaderParameter("saturation", 1.0f);  // Saturation normale
			shaderMaterial.SetShaderParameter("contrast", 1.0f);

			bulletTimeTimer.Stop();
		}
		else
		{
			// Activer le bullet time
			Engine.TimeScale = 0.5f;

			ShaderMaterial shaderMaterial = (ShaderMaterial)filterRect.Material;

			// Changer la désaturation et ajouter du flou
			shaderMaterial.SetShaderParameter("saturation", 0.3f);  // Réduire la saturation
			shaderMaterial.SetShaderParameter("contrast", 1.2f);

			bulletTimeTimer.Start();
		}
	}

	private void OnBulletTimeTimerOut()
	{
		Ui.UpdateBulletTimeBar(-3);
	}
}
