using Godot;
using System;

public partial class Menu : Node3D
{
	private Label _warningLabel;
	private ColorRect _fadeOverlay; // Ссылка на черный прямоугольник
	private bool _isStarting = false; // Флаг, чтобы не нажать кнопку дважды

	public override void _Ready()
	{
		

		// 1. Ищем узлы (обязательно добавь % к ColorRect в редакторе)
		_fadeOverlay = GetNodeOrNull<ColorRect>("%MenuFade");
		_warningLabel = GetNodeOrNull<Label>("%WarningLabel");
		
		var endlessBtn = GetNodeOrNull<Button>("%EndlessButton");
		var levelBtn = GetNodeOrNull<Button>("%LevelButton");
		var exitBtn = GetNodeOrNull<Button>("%ExitButton");

		// --- ЭФФЕКТ ПОЯВЛЕНИЯ ПРИ ЗАПУСКЕ (FADE IN) ---
		if (_fadeOverlay != null)
		{
			// Убеждаемся, что он черный, и плавно делаем прозрачным
			_fadeOverlay.Modulate = new Color(1, 1, 1, 1);
			Tween fadeIn = CreateTween();
			fadeIn.TweenProperty(_fadeOverlay, "modulate:a", 0.0f, 0.8f);
		}

		if (_warningLabel != null) _warningLabel.Visible = false;

		// Подключаем кнопки
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
		
		// Настройка звуков для всех кнопок
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
		if (_isStarting) return; // Если уже уходим в игру, игнорируем нажатия
		_isStarting = true;

		
		
		var music = GetNodeOrNull<AudioStreamPlayer>("MenuMusic");
		
		// Создаем Tween для финального затухания
		Tween exitTween = CreateTween();
		
		// .SetParallel(true) заставит музыку и экран затухать ОДНОВРЕМЕННО
		exitTween.SetParallel(true);

		if (music != null)
		{
			exitTween.TweenProperty(music, "volume_db", -80.0f, 0.6f);
		}

		if (_fadeOverlay != null)
		{
			exitTween.TweenProperty(_fadeOverlay, "modulate:a", 1.0f, 0.6f);
		}

		// Когда анимации закончатся, вызываем смену сцены
		exitTween.SetParallel(false); 
		exitTween.Finished += () => ChangeToGameScene();
	}

	private void ChangeToGameScene()
	{
		var global = GetNodeOrNull<Global>("/root/Global");
		if (global != null) global.IsEndlessMode = true;

		// Используем CallDeferred для безопасной смены сцены
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
