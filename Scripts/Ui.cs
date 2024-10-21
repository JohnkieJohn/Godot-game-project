using Godot;
using System;

public partial class Ui : Node2D
{
	private TextureProgressBar bulletTimeBar;
	private Label score;
	private int scoreValue = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bulletTimeBar = GetNode<TextureProgressBar>("Control/BulletTimeBar");
		bulletTimeBar.Value = 0;

		score = GetNode<Label>("Control/Score");
		score.Text = scoreValue.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void UpdateBulletTimeBar(int value)
	{
		bulletTimeBar.Value += value;
		if (bulletTimeBar.Value <= 0)
		{
			GameManager.Instance.isBulletTime = false;
			GameManager.Instance.ToggleBulletTime();
		}
	}

	public void UpdateScore(int value)
	{
		scoreValue += value;
		score.Text = scoreValue.ToString();
	}

	public double GetBulletTimeBarValue()
	{
		return bulletTimeBar.Value;
	}
}
