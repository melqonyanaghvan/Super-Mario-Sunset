using Godot;
using System;

public partial class Coin : Area3D 
{
	[Export] public float RotationSpeed = 3.0f; 
	[Export] public float FloatAmplitude = 0.2f; 
	[Export] public float FloatSpeed = 2.0f;    

	private float _initialY;
	private float _timePassed = 0.0f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;

		_initialY = GlobalPosition.Y;

		_timePassed = (float)GD.RandRange(0.0, 10.0);
	}


	public void SetHeight(float targetY)
	{
		_initialY = targetY;
		GlobalPosition = new Vector3(GlobalPosition.X, targetY, GlobalPosition.Z);
	}

	public override void _Process(double delta)
	{
		_timePassed += (float)delta;


		RotateY(RotationSpeed * (float)delta);

	
		float newY = _initialY + Mathf.Sin(_timePassed * FloatSpeed) * FloatAmplitude;
		
		GlobalPosition = new Vector3(GlobalPosition.X, newY, GlobalPosition.Z);
	}

	private async void OnBodyEntered(Node3D body)
	{
		
		if (!Visible) return; 
	
		if (body is CharacterBody3D || body.IsInGroup("player"))
		{
			var sfx = GetNodeOrNull<AudioStreamPlayer3D>("CoinSound");
			
			if (sfx != null)
			{
				
				Visible = false;
				SetDeferred("monitoring", false);
	
				sfx.Play();
				await ToSignal(sfx, "finished");
			}
	
			
			var generator = GetTree().Root.FindChild("WorldGenerator", true, false) as CameraMovement;
			generator?.AddCoin();
	
			
			QueueFree();
		}
	}
}
