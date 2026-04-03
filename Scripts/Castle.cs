using Godot;
using System;

public partial class Castle : Node3D
{
	private Label3D _speechBubble;
	private bool _activated = false;

	public override void _Ready()
	{
		_speechBubble = GetNodeOrNull<Label3D>("%SpeechBubble");
		
		var area = GetNodeOrNull<Area3D>("%EntryTrigger");
		if (area != null)
		{
			area.BodyEntered += OnMarioEntered;
			
		}
		
	}

	private void OnMarioEntered(Node3D body)
	{
		if (!_activated && (body.Name.ToString().ToLower().Contains("player") || body is CharacterBody3D))
		{
			_activated = true;
			
	
			if (_speechBubble != null) _speechBubble.Visible = true;
	
			if (body is CharacterBody3D player)
			{
				player.Velocity = Vector3.Zero;
				GetTree().CreateTimer(0.1).Timeout += () => player.SetPhysicsProcess(false);
			}
	
			GetTree().CreateTimer(6).Timeout += () => 
			{
				GetTree().ReloadCurrentScene();
			};
		}
	}
}
