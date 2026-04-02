using Godot;
using System;

public partial class QuestionBlock : StaticBody3D
{
	[Export] public Texture2D[] AnimationFrames; 
	[Export] public float AnimationSpeed = 0.2f; 

	private MeshInstance3D _mesh;
	private StandardMaterial3D _material;
	private int _currentFrame = 0;
	private float _timer = 0f;

public override void _Ready()
{
	// 1. Ищем меш ВНУТРИ блока по его точному имени. 
	// Если твой куб в сцене называется "MeshInstance3D", оставляем так. 
	// Если он называется "Cube" или "Box", замени имя в кавычках!
	_mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");

	// Если не нашел по имени, попробуем найти любого ребенка, который является мешем
	if (_mesh == null)
	{
		foreach (var child in GetChildren())
		{
			if (child is MeshInstance3D meshChild)
			{
				_mesh = meshChild;
				break;
			}
		}
	}

	if (_mesh != null)
	{
		;
		
		// 2. Берем материал из меша
		var mat = _mesh.GetActiveMaterial(0) as StandardMaterial3D;
		
		if (mat != null)
		{
			// Делаем копию, чтобы анимировался только ЭТОТ блок
			_material = (StandardMaterial3D)mat.Duplicate();
			_mesh.SetSurfaceOverrideMaterial(0, _material);
			
		}
	}
}

	public override void _Process(double delta)
	{
		if (AnimationFrames == null || AnimationFrames.Length == 0 || _material == null) return;
		
		_timer += (float)delta;
		if (_timer >= AnimationSpeed)
		{
			_timer = 0f;
			_currentFrame = (_currentFrame + 1) % AnimationFrames.Length;
			
			// 1. Меняем текстуру в нашем объекте материала
			_material.AlbedoTexture = AnimationFrames[_currentFrame];
			
			// 2. ВОТ ЭТО ДОБАВЬ ОБЯЗАТЕЛЬНО:
			// Мы заново назначаем наш измененный материал в слот меша
			_mesh.SetSurfaceOverrideMaterial(0, _material); 
			
			
		}
	}
}
