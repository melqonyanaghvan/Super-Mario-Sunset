using Godot;
using System;

public partial class Flag : Node3D
{
	[Export] public string AnimationName = "Take 001"; 
	private AnimationPlayer _animPlayer;
	private AudioStreamPlayer3D _victoryMusic;
	private bool _isActivated = false;

	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_victoryMusic = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
		
		var area = GetNode<Area3D>("Area3D");
		area.BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (!_isActivated && body.Name.ToString().ToLower().Contains("player"))
		{
			_isActivated = true;

			StopBackgroundMusic();

	
			if (_animPlayer != null) 
				_animPlayer.Play(AnimationName);

			if (_victoryMusic != null)
				_victoryMusic.Play();

			
		}
	}

	private void StopBackgroundMusic()
	{

		var mainMusic = GetTree().Root.FindChild("MusicPlayer", true, false) as AudioStreamPlayer;
		
		if (mainMusic != null)
		{
			mainMusic.Stop();
		}
		else
		{

			StopAllOtherAudio(GetTree().Root);
		}
	}

		private void StopAllOtherAudio(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is AudioStreamPlayer asp)
			{
				asp.Stop();
			}

			if (child is AudioStreamPlayer3D asp3d)
			{

				if (asp3d != _victoryMusic)
				{
					asp3d.Stop();
				}
			}

		
			StopAllOtherAudio(child);
		}
	}
}
