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

	_mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");

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
		var mat = _mesh.GetActiveMaterial(0) as StandardMaterial3D;
		
		if (mat != null)
		{

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
			
			_material.AlbedoTexture = AnimationFrames[_currentFrame];

			_mesh.SetSurfaceOverrideMaterial(0, _material); 
			
		}
	}
}
