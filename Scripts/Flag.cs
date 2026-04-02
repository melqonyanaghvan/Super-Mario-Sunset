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
		// Проверяем, что вошел игрок
		if (!_isActivated && body.Name.ToString().ToLower().Contains("player"))
		{
			_isActivated = true;
			
			// 1. ОСТАНАВЛИВАЕМ ФОНОВУЮ МУЗЫКУ
			StopBackgroundMusic();

			// 2. Запускаем анимацию флага
			if (_animPlayer != null) 
				_animPlayer.Play(AnimationName);

			// 3. Запускаем победную музыку
			if (_victoryMusic != null)
				_victoryMusic.Play();

			
		}
	}

	private void StopBackgroundMusic()
	{
		// Способ 1: Если музыка играет в CameraMovement (которая управляет уровнем)
		// Ищем узел по имени (замени "MusicPlayer" на имя своего узла с музыкой)
		var mainMusic = GetTree().Root.FindChild("MusicPlayer", true, false) as AudioStreamPlayer;
		
		if (mainMusic != null)
		{
			mainMusic.Stop();
		}
		else
		{
			// Способ 2: Если не нашли по имени, попробуем перебрать все AudioStreamPlayer в сцене
			// и остановить тот, который сейчас играет (кроме нашего победного)
			StopAllOtherAudio(GetTree().Root);
		}
	}

		private void StopAllOtherAudio(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			// 1. Проверяем обычные AudioStreamPlayer
			if (child is AudioStreamPlayer asp)
			{
				// Здесь ошибки не будет, так как asp — это AudioStreamPlayer,
				// а _victoryMusic — это AudioStreamPlayer3D. Они по определению разные.
				asp.Stop();
			}

			// 2. Проверяем AudioStreamPlayer3D
			if (child is AudioStreamPlayer3D asp3d)
			{
				// Сравниваем только если оба объекта — AudioStreamPlayer3D
				if (asp3d != _victoryMusic)
				{
					asp3d.Stop();
				}
			}

			// Рекурсия: идем глубже по дереву сцены
			StopAllOtherAudio(child);
		}
	}
}
