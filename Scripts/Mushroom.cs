using Godot;
using System;

public partial class Mushroom : AnimatedSprite3D
{
	[Export] public float WalkDistance = 5.0f;
	[Export] public float Speed = 2.0f;

	private float _startX; 
	private float _currentOffset = 0.0f; // Текущее смещение от старта
	private int _direction = 1;
	private bool _isInitialized = false;

	public override void _Ready()
	{
		// Рандомизируем направление при старте (кто-то влево, кто-то вправо)
		_direction = GD.Randf() > 0.5f ? 1 : -1;
		FlipH = (_direction < 0);

		// Рандомизируем начальное смещение, чтобы они не были в одной точке цикла
		// Это "разбросает" их по дистанции WalkDistance
		_currentOffset = (float)GD.RandRange(0.0f, WalkDistance);
		
		Play("walk");
	}

	public override void _Process(double delta)
	{
		if (!_isInitialized)
		{
			_startX = Position.X;
			// Применяем рандомное смещение к позиции сразу
			Vector3 p = Position;
			p.X += _currentOffset * _direction;
			Position = p;
			
			_isInitialized = true;
			return;
		}

		Vector3 pos = Position;
		float move = Speed * (float)delta;
		pos.X += move * _direction;
		
		// Накапливаем общее смещение от начальной точки
		_currentOffset += move;

		// Если прошли всю дистанцию — разворачиваемся
		if (_currentOffset >= WalkDistance)
		{
			_direction *= -1;
			FlipH = (_direction < 0);
			_currentOffset = 0.0f; // Сбрасываем счетчик для нового отрезка
		}

		Position = pos;
	}
}
