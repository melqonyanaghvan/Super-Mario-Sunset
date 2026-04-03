using Godot;
using System;

public partial class Area3d : Area3D
{
	[Export] public Texture2D CrushedTexture; 
	private bool _isDead = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (_isDead) return;

		if (body is CharacterBody3D player)
		{
			if (player.Velocity.Y < 0 && player.GlobalPosition.Y > GlobalPosition.Y + 0.3f)
			{
				Die(player);
			}
			else
			{
				GetTree().CallDeferred("reload_current_scene");
			}
		}
	}

private async void Die(CharacterBody3D player)
{
	if (_isDead) return;
	_isDead = true;
	
	var cameraMovement = GetTree().GetFirstNodeInGroup("game_manager") as CameraMovement;

	if (cameraMovement != null)
	{
		cameraMovement.AddEnemyScore(100);
		
	}
	var deathSound = GetNodeOrNull<AudioStreamPlayer3D>("DeathSound");
	
	if (deathSound != null)
	{
		var globalPos = deathSound.GlobalPosition;
		RemoveChild(deathSound);
		GetTree().Root.AddChild(deathSound);
		deathSound.GlobalPosition = globalPos;

		deathSound.Play();
		

		deathSound.Finished += () => deathSound.QueueFree();
	}
	
	var v = player.Velocity;
	v.Y = 10.0f;
	player.Velocity = v;

	var root = GetParent() as Node3D;
	if (root != null)
	{
		root.SetPhysicsProcess(false);
		root.SetProcess(false);

		if (root.HasMethod("stop")) root.Call("stop");
		root.GetNodeOrNull<AnimationPlayer>("AnimationPlayer")?.Stop();

		if (CrushedTexture != null)
		{
			root.Visible = false;

			Sprite3D deathSprite = new Sprite3D();
			deathSprite.Texture = CrushedTexture;
			
			deathSprite.Scale = new Vector3(0.2f, 0.2f, 0.2f); 
			
			deathSprite.Axis = Vector3.Axis.Y;
			deathSprite.Centered = true;

			GetParent().GetParent().AddChild(deathSprite);
			deathSprite.GlobalPosition = root.GlobalPosition;
			
			await ToSignal(GetTree().CreateTimer(4f), "timeout"); 
			
			if (IsInstanceValid(deathSprite)) deathSprite.QueueFree(); 
		}
		else
		{

			root.Scale = new Vector3(1.5f, 0.1f, 1.0f);
			await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
		}
	}

	if (IsInstanceValid(root)) root.QueueFree();
}
}
