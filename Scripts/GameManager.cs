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

		filterRect = new ColorRect();
		Rect2 viewportRect = GetViewport().GetVisibleRect(); // taille de la fenêtre
		filterRect.Size = viewportRect.Size;
		filterRect.Color = new Color(1, 1, 1, 0);

		// charger et appliquer le shader
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
		bulletTimeTimer.WaitTime = 0.1f;
		AddChild(bulletTimeTimer);
		bulletTimeTimer.Timeout += OnBulletTimeTimerOut;
	}

	public bool IsOutOfScreen(Rect2 viewport, float positionY, float positionX)
	{		
		// vérifier si le projectile est en dehors des limites de la fenêtre
		return positionY < 0 || positionY > viewport.Size.Y || 
			   positionX < 0 || positionX > viewport.Size.X;
	}

	//déclencher une explosion
	public void TriggerExplosion(Vector2 position)
	{
		var mainScene = GetTree().CurrentScene;

		PackedScene explosionScene = (PackedScene)ResourceLoader.Load("res://Scenes/explosion_1.tscn");
		Node2D explosionInstance = (Node2D)explosionScene.Instantiate();
		explosionInstance.Position = position;

		mainScene.AddChild(explosionInstance);

		AnimationPlayer animPlayer = explosionInstance.GetNode<AnimationPlayer>("AnimationPlayer");
		animPlayer.Play("explosion1");

		// supprimer l'explosion après l'animation
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
			// vitesse normale
			Engine.TimeScale = 1.0f;

			ShaderMaterial shaderMaterial = (ShaderMaterial)filterRect.Material;

			// paramètres à leur état initial
			shaderMaterial.SetShaderParameter("saturation", 1.0f);  // Saturation normale
			shaderMaterial.SetShaderParameter("contrast", 1.0f);

			bulletTimeTimer.Stop();
		}
		else
		{
			// activer le bullet time
			Engine.TimeScale = 0.5f;

			ShaderMaterial shaderMaterial = (ShaderMaterial)filterRect.Material;

			// paramètres du shader
			shaderMaterial.SetShaderParameter("saturation", 0.3f);
			shaderMaterial.SetShaderParameter("contrast", 1.2f);

			bulletTimeTimer.Start();
		}
	}

	private void OnBulletTimeTimerOut()
	{
		Ui.UpdateBulletTimeBar(-3);
	}
}
