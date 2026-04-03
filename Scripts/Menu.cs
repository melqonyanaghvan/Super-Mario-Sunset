using Godot;
using System;

public partial class Menu : Node3D
{
	private Label _warningLabel;
	private ColorRect _fadeOverlay;
	private bool _isStarting = false; 

	public override void _Ready()
	{

		_fadeOverlay = GetNodeOrNull<ColorRect>("%MenuFade");
		_warningLabel = GetNodeOrNull<Label>("%WarningLabel");
		
		var endlessBtn = GetNodeOrNull<Button>("%EndlessButton");
		var levelBtn = GetNodeOrNull<Button>("%LevelButton");
		var exitBtn = GetNodeOrNull<Button>("%ExitButton");

		if (_fadeOverlay != null)
		{
			_fadeOverlay.Modulate = new Color(1, 1, 1, 1);
			Tween fadeIn = CreateTween();
			fadeIn.TweenProperty(_fadeOverlay, "modulate:a", 0.0f, 0.8f);
		}

		if (_warningLabel != null) _warningLabel.Visible = false;

		if (endlessBtn != null)
		{
			endlessBtn.Pressed += OnEndlessPressed;
			endlessBtn.GrabFocus(); 
		}

		if (levelBtn != null)
		{
			levelBtn.Pressed += OnLevelPressed;
		}

		if (exitBtn != null)
		{
			exitBtn.Pressed += () => GetTree().Quit();
		}
		
		Button[] allButtons = { endlessBtn, levelBtn, exitBtn };

		foreach (var btn in allButtons)
		{
			if (btn == null) continue;
			btn.FocusEntered += () => PlaySound("AudioSelect");
			btn.MouseEntered += () => PlaySound("AudioSelect");
			btn.Pressed += () => PlaySound("AudioConfirm");
		}
	}

	private void OnEndlessPressed()
	{
		if (_isStarting) return;
		_isStarting = true;

		var music = GetNodeOrNull<AudioStreamPlayer>("MenuMusic");

		Tween exitTween = CreateTween();
		exitTween.SetParallel(true);

		if (music != null)
		{
			exitTween.TweenProperty(music, "volume_db", -80.0f, 0.6f);
		}

		if (_fadeOverlay != null)
		{
			exitTween.TweenProperty(_fadeOverlay, "modulate:a", 1.0f, 0.6f);
		}

		exitTween.SetParallel(false); 
		exitTween.Finished += () => ChangeToGameScene();
	}

	private void ChangeToGameScene()
	{
		var global = GetNodeOrNull<Global>("/root/Global");
		if (global != null) global.IsEndlessMode = true;

		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://main_world.tscn");
	}

	private void PlaySound(string nodeName)
	{
		var player = GetNodeOrNull<AudioStreamPlayer>(nodeName);
		if (player != null)
		{
			player.Stop(); 
			player.Play();
		}
	}

	private void OnLevelPressed()
	{
		if (_warningLabel == null) return;
		
		
		_warningLabel.Visible = true;

		GetTree().CreateTimer(2.0).Timeout += () => {
			if (IsInstanceValid(_warningLabel)) 
				_warningLabel.Visible = false;
		};
	}
}
