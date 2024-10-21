using Godot;
using System;

public partial class BossStage1 : CharacterBody2D
{
	private AnimationPlayer wingsAnim;
	private AnimationPlayer headAnim;
	public override void _Ready()
	{
		wingsAnim = GetNode<AnimationPlayer>("AnimationPlayer");
		headAnim = GetNode<AnimationPlayer>("AnimationPlayer2");

		wingsAnim.Play("boss1_wings");
		headAnim.Play("boss1_head");
	}
	public override void _PhysicsProcess(double delta)
	{
		
	}

}
