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
			// Подключаем сигнал
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
			// Вместо резкого SetPhysicsProcess(false), 
			// просто обнуляем скорость. Если у тебя в скрипте игрока
			// управление зависит от Velocity, он просто перестанет бежать.
			player.Velocity = Vector3.Zero;
			
			// Чтобы он не замер в странной позе, можно подождать 0.1 сек 
			// перед тем как полностью отключать физику
			GetTree().CreateTimer(0.1).Timeout += () => player.SetPhysicsProcess(false);
		}

		// Таймер на перезагрузку уровня. Поставь 3-5 секунд, 
		// чтобы успеть прочитать надпись Тоада!
		GetTree().CreateTimer(6).Timeout += () => 
		{
			GetTree().ReloadCurrentScene();
		};
	}
}
}
