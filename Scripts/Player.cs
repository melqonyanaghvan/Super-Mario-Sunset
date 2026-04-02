using Godot;
using System;

public partial class Player : CharacterBody3D
{
	private AudioStreamPlayer3D _jumpSound;
	[Export] public float WalkSpeed = 10.0f;    
	[Export] public float RunSpeed = 18.0f;     
	[Export] public float JumpVelocity = 12.0f; 
	[Export] public float Acceleration = 0.1f;  
	[Export] public float Friction = 0.15f;     
	[Export] public float Sensitivity = 0.003f;
	
	[Export] public float FallMultiplier = 2.5f;
	[Export] public float LowJumpMultiplier = 3.0f;
	
	[ExportGroup("Head Bob")]
	[Export] public float BobFreq = 5.0f;    
	[Export] public float BobAmp = 0.08f;   
	[Export] public float ControllerSensitivity = 3.0f; // Чувствительность стика
	[Export] public float LookInertia = 10.0f; // Чем выше число, тем меньше инерция (быстрее реакция)
	private Vector2 _smoothLookDir = Vector2.Zero;
	
	private float _bobTimer = 0.0f;         
	private float _defaultY = 0.0f;         

	private Node3D _pivot;
	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		_pivot = GetNode<Node3D>("CameraPivot");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_jumpSound = GetNode<AudioStreamPlayer3D>("JumpSound");
		
		_defaultY = _pivot.Position.Y;
	}

	// ОБРАБОТКА КАМЕРЫ В _Process ДЛЯ ПЛАВНОСТИ (High FPS)
		public override void _Process(double delta)
		{
			float fDelta = (float)delta;

			// 1. ПОЛУЧАЕМ "СЫРЫЕ" ДАННЫЕ СО СТИКА
			Vector2 rawLookDir = Input.GetVector("look_left", "look_right", "look_up", "look_down");

			// 2. ПРИМЕНЯЕМ ИНЕРЦИЮ (LERP)
			// Мы плавно ведем _smoothLookDir к rawLookDir
			_smoothLookDir = _smoothLookDir.Lerp(rawLookDir, fDelta * LookInertia);

			// 3. ВРАЩАЕМ, ИСПОЛЬЗУЯ СГЛАЖЕННОЕ ЗНАЧЕНИЕ
			if (_smoothLookDir.Length() > 0.001f) // Проверка на микро-движения
			{
				float speedMultiplier = 2.5f; 

				RotateY(-_smoothLookDir.X * ControllerSensitivity * fDelta * speedMultiplier);
				_pivot.RotateX(-_smoothLookDir.Y * ControllerSensitivity * fDelta * speedMultiplier);
				
				Vector3 rot = _pivot.Rotation;
				rot.X = Mathf.Clamp(rot.X, Mathf.DegToRad(-80), Mathf.DegToRad(80));
				_pivot.Rotation = rot;
			}

			HandleHeadBob(fDelta);
		}
	
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		float fDelta = (float)delta;

		// ГРАВИТАЦИЯ
		if (!IsOnFloor()) 
		{
			if (velocity.Y < 0) velocity.Y -= _gravity * FallMultiplier * fDelta;
			else if (velocity.Y > 0 && !Input.IsActionPressed("ui_accept")) velocity.Y -= _gravity * LowJumpMultiplier * fDelta;
			else velocity.Y -= _gravity * fDelta;
		}

		// ПРЫЖОК
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor()) 
		{
			velocity.Y = JumpVelocity;
			_jumpSound.Play();
		}

		// ДВИЖЕНИЕ
		bool isRunning = Input.IsActionPressed("ui_shift"); 
		float currentMaxSpeed = isRunning ? RunSpeed : WalkSpeed;

		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = Mathf.Lerp(velocity.X, direction.X * currentMaxSpeed, Acceleration);
			velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * currentMaxSpeed, Acceleration);
		}
		else
		{
			velocity.X = Mathf.Lerp(velocity.X, 0, Friction);
			velocity.Z = Mathf.Lerp(velocity.Z, 0, Friction);
		}

		Velocity = velocity;
		MoveAndSlide();

		if (GlobalPosition.Y < -15.0f) GetTree().ReloadCurrentScene();
	}

	private void HandleHeadBob(float delta)
	{
		Vector3 pos = _pivot.Position;

		if (IsOnFloor() && Velocity.Length() > 0.1f)
		{
			_bobTimer += delta * Velocity.Length() * BobFreq;
			pos.Y = _defaultY + Mathf.Sin(_bobTimer) * BobAmp;
			pos.X = Mathf.Cos(_bobTimer / 2) * BobAmp * 0.5f; 
		}
		else
		{
			_bobTimer = 0;
			pos.Y = Mathf.Lerp(pos.Y, _defaultY, delta * 10.0f);
			pos.X = Mathf.Lerp(pos.X, 0.0f, delta * 10.0f);
		}

		_pivot.Position = pos;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * Sensitivity);
			_pivot.RotateX(-mouseMotion.Relative.Y * Sensitivity);
			Vector3 rot = _pivot.Rotation;
			rot.X = Mathf.Clamp(rot.X, Mathf.DegToRad(-80), Mathf.DegToRad(80));
			_pivot.Rotation = rot;
		}
	}
}
