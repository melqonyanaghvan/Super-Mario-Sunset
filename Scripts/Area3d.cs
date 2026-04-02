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
			// Проверка прыжка сверху
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
	// Ищем звук
	var deathSound = GetNodeOrNull<AudioStreamPlayer3D>("DeathSound");
	
	if (deathSound != null)
	{
		// ХИТРОСТЬ: Чтобы звук не удалился вместе с грибом, 
		// мы можем временно перекинуть его в корень сцены
		var globalPos = deathSound.GlobalPosition;
		RemoveChild(deathSound);
		GetTree().Root.AddChild(deathSound);
		deathSound.GlobalPosition = globalPos;

		deathSound.Play();
		

		// Авто-удаление звука после того, как он доиграет
		deathSound.Finished += () => deathSound.QueueFree();
	}
	
	// 1. Подброс игрока
	var v = player.Velocity;
	v.Y = 10.0f;
	player.Velocity = v;

	// 2. Остановка и смена визуала
	var root = GetParent() as Node3D;
	if (root != null)
	{
		root.SetPhysicsProcess(false);
		root.SetProcess(false);

		// Останавливаем анимации
		if (root.HasMethod("stop")) root.Call("stop");
		root.GetNodeOrNull<AnimationPlayer>("AnimationPlayer")?.Stop();

		if (CrushedTexture != null)
		{
			// Скрываем старый гриб
			root.Visible = false;

			// Создаем новый спрайт смерти
			Sprite3D deathSprite = new Sprite3D();
			deathSprite.Texture = CrushedTexture;
			
			// --- НОВЫЕ НАСТРОЙКИ МАСШТАБА И РАСПОЛОЖЕНИЯ ---
			
			// 1. УМЕНЬШАЕМ РАЗМЕР. 
			// Попробуй значение 0.2 или даже 0.1, если он слишком большой.
			deathSprite.Scale = new Vector3(0.2f, 0.2f, 0.2f); 
			
			// 2. ОРИЕНТАЦИЯ. 
			// Делаем спрайт плоским, лежащим на земле (как текстуру пола).
			deathSprite.Axis = Vector3.Axis.Y;
			
			// 3. ТОЧКА ПРИВЯЗКИ.
			// Спрайт будет создаваться от центра, чтобы он не проваливался под пол.
			deathSprite.Centered = true;

			// Добавляем в сцену
			GetParent().GetParent().AddChild(deathSprite);
			deathSprite.GlobalPosition = root.GlobalPosition;
			
			

			// --- НОВОЕ ВРЕМЯ ЗАДЕРЖКИ ---
			// Увеличим паузу, чтобы ты успел рассмотреть картинку.
			await ToSignal(GetTree().CreateTimer(4f), "timeout"); 
			
			if (IsInstanceValid(deathSprite)) deathSprite.QueueFree(); 
		}
		else
		{
			// Если картинки нет — плющим старый
			root.Scale = new Vector3(1.5f, 0.1f, 1.0f);
			await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
		}
	}

	// 3. Удаляем сам гриб
	if (IsInstanceValid(root)) root.QueueFree();
}
}
