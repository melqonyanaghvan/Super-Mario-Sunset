using Godot;
using System;

public partial class Coin : Area3D 
{
	[Export] public float RotationSpeed = 3.0f; // Скорость вращения
	[Export] public float FloatAmplitude = 0.2f; // Высота покачивания (0.5 метра)
	[Export] public float FloatSpeed = 2.0f;    // Скорость покачивания

	private float _initialY;
	private float _timePassed = 0.0f;

	public override void _Ready()
	{
		// Подключаем сигнал сбора (если еще не подключен в редакторе)
		BodyEntered += OnBodyEntered;
		
		// Запоминаем стартовую высоту, чтобы качаться относительно неё
		_initialY = GlobalPosition.Y;
		
		// Рандомизируем начальное время, чтобы монетки не двигались синхронно
		_timePassed = (float)GD.RandRange(0.0, 10.0);
	}

	// Метод для установки высоты из генератора
	public void SetHeight(float targetY)
	{
		_initialY = targetY;
		GlobalPosition = new Vector3(GlobalPosition.X, targetY, GlobalPosition.Z);
	}

	public override void _Process(double delta)
	{
		_timePassed += (float)delta;

		// 1. Вращение
		RotateY(RotationSpeed * (float)delta);

		// 2. Плавное покачивание вверх-вниз по синусоиде
		float newY = _initialY + Mathf.Sin(_timePassed * FloatSpeed) * FloatAmplitude;
		
		GlobalPosition = new Vector3(GlobalPosition.X, newY, GlobalPosition.Z);
	}

	private async void OnBodyEntered(Node3D body)
{
	// Проверка, чтобы не собрать одну монету дважды за один кадр
	if (!Visible) return; 

	if (body is CharacterBody3D || body.IsInGroup("player"))
	{
		var sfx = GetNodeOrNull<AudioStreamPlayer3D>("CoinSound");
		
		if (sfx != null)
		{
			// 1. Прячем монету и выключаем коллизию, чтобы игрок её больше не трогал
			Visible = false;
			SetDeferred("monitoring", false);
			
			// 2. Запускаем звук
			sfx.Play();
			

			// 3. Ждем, пока звук доиграет (сигнал "finished")
			await ToSignal(sfx, "finished");
		}

		// 4. Логика генератора
		var generator = GetTree().Root.FindChild("WorldGenerator", true, false) as CameraMovement;
		generator?.AddCoin();

		// 5. Теперь можно удалять
		QueueFree();
	}
}
}
