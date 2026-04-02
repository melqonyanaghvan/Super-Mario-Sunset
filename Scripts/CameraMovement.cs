using Godot;
using System;
using System.Collections.Generic;

public partial class CameraMovement : Node3D
{
	[Export] public CharacterBody3D PlayerNode; 
	[Export] public PackedScene GapCutterScene; 
	[Export] public PackedScene StairBlockScene; 
	[Export] public PackedScene CastleScene; // Сюда перетащи замок в инспекторе!
	[Export] public PackedScene FlagScene; // Сюда перетащи модель из интернета
	[ExportGroup("Финальные настройки")]
	[Export] public PackedScene ToadScene;   // Сцена Тоада
	[Export] public Texture2D LuigiTexture; // Текстура Луиджи
	[Export] public ColorRect FadeOverlay;
	private Node3D _spawnedLuigi; // Ссылка на самого Луиджи
	private Label3D _luigiLabel; // Ссылка на надпись над ним
	[Export] public float LuigiScale = 0.5f;

	private bool _isLuigiSpawned = false;
	[ExportGroup("Логика")]
	[Export] public float TileLength = 100.0f;
	private float _currentTime;
	
	[ExportGroup("Объекты")]
	[Export] public PackedScene MushroomScene;
	[Export] public PackedScene PipeScene;
	[Export] public PackedScene CloudScene;
	[Export] public PackedScene HillScene;
	[Export] public PackedScene CoinScene;
	[Export] public PackedScene BushScene;

	[ExportGroup("Настройки Блоков")]
	[Export] public PackedScene BlockScene;
	[Export] public PackedScene QuestionBlockScene;
	[Export] public float QuestionBlockChance = 0.15f; 
	[Export] public float BlockSize = 2.0f;            
	[Export] public float BlockSpawnChance = 0.6f;     
	[Export] public float BlockGroupInterval = 15.0f;  
	[Export] public int MaxBlockInLine = 4;            
	[Export] public bool SpawInLines = true;       

	[ExportGroup("Настройки Труб")]
	[Export] public float PipeSpawnChance = 0.5f;      
	[Export] public float PipeMinInterval = 8.0f;     
	[Export] public float PipeMaxInterval = 20.0f;    
	[Export] public float PipeWidthScale = 1.0f;      
	[Export] public float PipeMinHeight = 0.8f;       
	[Export] public float PipeMaxHeight = 1.8f;       

	[ExportGroup("Интерфейс")]
	[Export] public Label ScoreLabel;
	[Export] public Label3D WorldLabel3D; 
	[Export] public Label3D CoinLabel3D;
	
	[ExportGroup("Полы")]
	[Export] public Node3D[] Floors;

	// ПЕРЕМЕННЫЕ ДЛЯ ЗОН (ЗОЛОТАЯ И ФИНАЛЬНАЯ)
	private bool _isGoldenZone = false;
	private bool _goldenZoneSpawned = false; 
	private bool _isFinalZone = false;    // Исправляет ошибку
	private bool _castleSpawned = false;  // Исправляет ошибку
	
	private List<Node3D> _activeFloors = new List<Node3D>();
	private float _farthestZ = 0f;
	private float _lastThreshold = 0f;
	private Random random = new Random(); 
	private int _score = 0;
	private int _killScore = 0;
	private int _coins = 0; 
	
	private float _offsetY = 25.0f;

	public override void _Ready()
	{
		if (FadeOverlay != null)
		{
			FadeOverlay.Modulate = new Color(1, 1, 1, 1); // Начинаем с черного
			Tween fadeIn = CreateTween();
			fadeIn.TweenProperty(FadeOverlay, "modulate:a", 0.0f, 1.0f); // Медленно проявляем мир
		}
		Input.MouseMode = Input.MouseModeEnum.Captured;
		if (Floors != null)
		{
			foreach (var f in Floors) if (f != null) _activeFloors.Add(f);
		}

		for (int i = 0; i < _activeFloors.Count; i++)
		{
			float zPos = -i * TileLength;
			_activeFloors[i].GlobalPosition = new Vector3(0, -_offsetY, zPos);
			_farthestZ = zPos;
			SpawnObjects(_activeFloors[i]);
		}
	}
	
	private float _gameTimer = 600f;
	
	public override void _Process(double delta)
	{
		if (PlayerNode == null) return;

		if (WorldLabel3D != null)
		{
			float offsetX = -67.0f; 
			float offsetY = 30.0f;  
			float offsetZ = -130.0f; 

			WorldLabel3D.GlobalPosition = new Vector3(
				offsetX, 
				offsetY, 
				PlayerNode.GlobalPosition.Z + offsetZ
			);
		}

		if (_gameTimer > 0)
		{
			_gameTimer -= (float)delta * 2.5f; 
		}

		if (PlayerNode.GlobalPosition.Z < _lastThreshold - TileLength - 50.0f) 
		{
			_lastThreshold -= TileLength;
			RecycleFloor();
		}

		int currentDistance = (int)Mathf.Abs(PlayerNode.GlobalPosition.Z);
		if (currentDistance > _score) _score = currentDistance;
		
		// ЛОГИКА АКТИВАЦИИ ЗОН
		if (_score >= 2000 && !_goldenZoneSpawned)
		{
			_isGoldenZone = true;
		}

		// Внутри _Process
		if (_coins >= 25 && !_isLuigiSpawned)
		{
			_isLuigiSpawned = true;
			SpawnLuigiDirectly(); // Этот метод мы создадим ниже
		}
	if (_isLuigiSpawned && _spawnedLuigi != null && _luigiLabel != null)
	{
		float distance = PlayerNode.GlobalPosition.DistanceTo(_spawnedLuigi.GlobalPosition);

		// Если подошли ближе 15 метров И текст еще прозрачный
		if (distance < 15.0f && _luigiLabel.Modulate.A < 0.1f)
		{
			Tween tween = GetTree().CreateTween();
			// Parallel() заставляет обе анимации идти одновременно
			tween.Parallel().TweenProperty(_luigiLabel, "modulate:a", 1.0f, 1.5f);
			tween.Parallel().TweenProperty(_luigiLabel, "outline_modulate:a", 1.0f, 1.5f);
		}
		// Если отошли дальше 20 метров И текст еще видимый
		else if (distance > 20.0f && _luigiLabel.Modulate.A > 0.9f)
		{
			Tween tween = GetTree().CreateTween();
			tween.Parallel().TweenProperty(_luigiLabel, "modulate:a", 0.0f, 1.0f);
			tween.Parallel().TweenProperty(_luigiLabel, "outline_modulate:a", 0.0f, 1.0f);
		}
	}

		UpdateWorldLabels();
	}

private void SpawnLuigiDirectly()
{
	if (ToadScene == null || LuigiTexture == null) return;

	// 1. ПАРАМЕТРЫ ПОИСКА
	float startZ = PlayerNode.GlobalPosition.Z - 100f; 
	float safeZ = startZ;
	float finalY = 0.7f; // Высота по умолчанию (если луч ничего не найдет)
	bool foundSafeSpot = false;

	// 2. ЦИКЛ ПОИСКА БЕЗОПАСНОГО МЕСТА (RayCast)
	// Проверяем 10 точек вперед, чтобы не упасть в пропасть
	for (int i = 0; i < 10; i++)
	{
		float checkZ = startZ - (i * 2.0f);
		Vector3 rayStart = new Vector3(-9.0f, 20.0f, checkZ); // Стреляем с высоты 20м
		Vector3 rayEnd = new Vector3(-9.0f, -40.0f, checkZ);  // Вниз до -40м

		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd);
		var result = spaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			Vector3 hitPos = (Vector3)result["position"];
			
			// Если поверхность на уровне земли (не слишком высоко и не слишком низко)
			if (hitPos.Y < 2.0f && hitPos.Y > -26.0f) 
			{
				safeZ = checkZ;
				// ФИКС ВЫСОТЫ: берем точку удара луча и добавляем 1.1 метра, 
				// чтобы спрайт (scale 0.5) стоял ногами на полу
				finalY = hitPos.Y + 0.7f; 
				foundSafeSpot = true;
				break; 
			}
		}
	}

	// 3. СОЗДАНИЕ ОБЪЕКТА
	var luigi = ToadScene.Instantiate<Node3D>();
	GetTree().Root.AddChild(luigi);
	_spawnedLuigi = luigi;

	// Применяем найденные координаты и масштаб
	luigi.GlobalPosition = new Vector3(-15.0f, finalY, safeZ);
	luigi.RotationDegrees = new Vector3(0, 180, 0);
	luigi.Scale = Vector3.One * LuigiScale;

	// 4. ТЕКСТУРА
	Sprite3D sprite = luigi as Sprite3D ?? luigi.FindChild("*", true, false) as Sprite3D;
	if (sprite != null) 
	{
		sprite.Texture = LuigiTexture;
		
		sprite.AlphaCut = Sprite3D.AlphaCutMode.Discard;
		sprite.RenderPriority = 10;
	}
	// 5. НАДПИСЬ (С исправлением Billboard и зеркальности)

	_luigiLabel = new Label3D();
	luigi.AddChild(_luigiLabel);
	_luigiLabel.AlphaCut = Label3D.AlphaCutMode.Discard;
	Font marioFont = GD.Load<Font>("res://PressStart2P-Regular.ttf");
	if (marioFont != null) 
	{
	_luigiLabel.Font = marioFont;
	}


	_luigiLabel.Text = "Look, Mario... the sunset is finally parting.\n Can you see it? The castle towers are glowing in the dust.\n The Princess is there, waiting for the light to return.\n Go, brother... I'll watch your back from here.";
	_luigiLabel.FontSize = 90;
	_luigiLabel.OutlineSize = 12;
	_luigiLabel.Position = new Vector3(0, 3.5f, 0);

	
	_luigiLabel.Modulate = new Color(1, 1, 1, 0);
	_luigiLabel.OutlineModulate = new Color(0, 0, 0, 0);
	
	// ПРАВИЛЬНЫЙ Billboard (через класс BaseMaterial3D)
	_luigiLabel.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
	_luigiLabel.DoubleSided = false;
	
	// Если текст всё еще отзеркален - включи FlipH (убери //)
	// _luigiLabel.FlipH = true; 

	_isFinalZone = true;
	
}
	public void AddCoin() => _coins += 1;

	public void AddEnemyScore(int amount)
	{
		_killScore += amount;
	}
	
	private void UpdateWorldLabels()
	{
		if (WorldLabel3D == null) return;
		
		// Складываем текущую дистанцию и очки за убийства
		int totalScore = _score + _killScore; 
		
		string scoreStr = totalScore.ToString("D6");
		string coinStr = _coins.ToString("D2"); 
		string timeStr = ((int)_gameTimer).ToString("D3");
		WorldLabel3D.Text = $" MARIO    COINS  WORLD  TIME\n{scoreStr}   x{coinStr}    1-1    {timeStr}";
	}

	private void RecycleFloor()
	{
		Node3D oldFloor = _activeFloors[0];
		_activeFloors.RemoveAt(0);

		_farthestZ -= TileLength;
		oldFloor.GlobalPosition = new Vector3(0, -_offsetY, _farthestZ);
		_activeFloors.Add(oldFloor);

		foreach (var child in oldFloor.GetChildren())
		{
			if (child.Name.ToString().ToLower().Contains("floor")) continue;
			child.QueueFree();
		}
		
		SpawnObjects(oldFloor);
	}


	private void SpawnObjects(Node3D floor)
	{
		bool isStartZone = floor.GlobalPosition.Z > -50.0f && floor.GlobalPosition.Z <= 5.0f;
		// ПРИОРЕТЕТ 1: ФИНАЛЬНЫЙ ЗАМОК (100 МОНЕТ)
		if (_isFinalZone)
			{
				SpawnCastleScene(floor);
				_isFinalZone = false; 
				_castleSpawned = true;
				return; 
			}

	
		// ПРИОРЕТЕТ 2: ЗОЛОТАЯ ЗОНА (1000 ОЧКОВ)
		if (_isGoldenZone)
		{
			SpawnGoldenContent(floor);
			_isGoldenZone = false; 
			_goldenZoneSpawned = true; 
			return;
		}

		List<float> gapZones = new List<float>();

		// --- ГЕНЕРАЦИЯ ОБРЫВОВ ---
		if (GapCutterScene != null && !isStartZone)
		{
			for (int i = 0; i < 4; i++) 
			{
				if (random.NextDouble() < 0.4) 
				{
					float randomZ = (float)(random.NextDouble() * (TileLength - 20) - (TileLength / 2 - 10));
					bool tooClose = false;
					foreach (float existingGap in gapZones) 
					{
						if (Mathf.Abs(randomZ - existingGap) < 15.0f) { tooClose = true; break; }
					}
					if (tooClose) continue; 

					var cutter = GapCutterScene.Instantiate<Node3D>(); 
					floor.AddChild(cutter);
					cutter.Position = new Vector3(0, _offsetY, randomZ);
					gapZones.Add(randomZ);
				}
			}
		}

		bool IsInGap(float z)
		{
			foreach (float gapZ in gapZones)
			{
				if (Mathf.Abs(z - gapZ) < 6.0f) return true;
			}
			return false;
		}

		// --- ГЕНЕРАЦИЯ ПИРАМИД ---
		if (StairBlockScene != null && gapZones.Count > 0)
		{
			gapZones.Sort();
			foreach (float gapZ in gapZones)
			{
				if (random.NextDouble() > 0.5) continue;
				float roadLeft = -20.0f;
				float roadRight = 20.0f;
				float stairWidth = 3 * BlockSize; 
				float randomX = (float)(random.NextDouble() * (roadRight - roadLeft - stairWidth) + roadLeft);

				float[] basesZ = { gapZ + 3.0f, gapZ - 3.0f };
				foreach (float baseZ in basesZ)
				{
					int maxSteps = 4;
					for (int h = 0; h < maxSteps; h++)
					{
						int blocksInRow = maxSteps - h;
						for (int s = 0; s < blocksInRow; s++)
						{
							for (int xStep = 0; xStep < 3; xStep++)
							{
								var stepBlock = StairBlockScene.Instantiate<Node3D>();
								floor.AddChild(stepBlock);
								float direction = (baseZ > gapZ) ? 1.0f : -1.0f;
								float finalZ = baseZ + (s * BlockSize * direction);
								float finalY = (h * BlockSize) + (BlockSize / 2.0f) + _offsetY;
								float finalX = randomX + (xStep * BlockSize);
								stepBlock.Position = new Vector3(finalX, finalY, finalZ);
							}
						}
					}
				}
			}
		}

		// --- БЛОКИ И МОНЕТКИ ---
		if (BlockScene != null)
		{
			float bZ = -(TileLength / 2) + 10;
			while (bZ < (TileLength / 2) - 15)
			{
				bZ += (float)(random.NextDouble() * 5.0f + BlockGroupInterval); 
				if (bZ >= (TileLength / 2) - 10) break;
				if (random.NextDouble() < BlockSpawnChance)
				{
					float currentTierY = (random.NextDouble() > 0.5) ? 2.7f : 5.0f;
					float bX = new float[] { -4.0f, 0.0f, 4.0f }[random.Next(3)];
					int count = SpawInLines ? random.Next(1, MaxBlockInLine + 1) : 1;
					for (int i = 0; i < count; i++)
					{
						float finalZ = bZ + (i * BlockSize);
						if (IsInGap(finalZ)) continue;
						PackedScene selectedScene = (QuestionBlockScene != null && random.NextDouble() < QuestionBlockChance) ? QuestionBlockScene : BlockScene;
						var block = (Node3D)selectedScene.Instantiate();
						floor.AddChild(block);
						block.Position = new Vector3(bX, currentTierY + _offsetY, finalZ);

						if (CoinScene != null && random.NextDouble() < 0.4) 
						{
							var coinNode = CoinScene.Instantiate();
							floor.AddChild(coinNode);
							float coinY = currentTierY + 1.8f + _offsetY;
							if (coinNode is Coin coinScript)
							{
								coinScript.Position = new Vector3(bX, coinY, finalZ);
								coinScript.SetHeight(floor.GlobalPosition.Y + coinY);
							}
							else ((Node3D)coinNode).Position = new Vector3(bX, coinY, finalZ);
						}
					}
					bZ += (count * BlockSize); 
				}
			}
		}

		// --- ТРУБЫ И ВРАГИ ---
		float zPos = -(TileLength / 2) + 5;
		while (zPos < (TileLength / 2) - 10)
		{
			zPos += (float)(random.NextDouble() * (PipeMaxInterval - PipeMinInterval) + PipeMinInterval);
			if (zPos >= (TileLength / 2) - 5) break;
			if (IsInGap(zPos)) continue;
			
			float absoluteZ = floor.GlobalPosition.Z + zPos;
			if (isStartZone && Mathf.Abs(absoluteZ) < 15.0f) continue;
			float xPos = new float[] { -7.0f, -3.5f, 0.0f, 3.5f, 7.0f }[random.Next(5)];
			if (random.NextDouble() < PipeSpawnChance && PipeScene != null)
			{
				var p = (Node3D)PipeScene.Instantiate();
				floor.AddChild(p);
				float h = (float)(random.NextDouble() * (PipeMaxHeight - PipeMinHeight) + PipeMinHeight);
				p.Scale = new Vector3(PipeWidthScale, h, PipeWidthScale);
				p.Position = new Vector3(xPos, h + _offsetY, zPos);
			}
			else if (MushroomScene != null && random.NextDouble() < 0.4) 
			{
				var m = (Node3D)MushroomScene.Instantiate();
				floor.AddChild(m);
				m.Position = new Vector3(xPos, 0.37f + _offsetY, zPos);
			}
		}
		SpawnDecorations(floor);
	}

	private void SpawnGoldenContent(Node3D floor)
	{
		for (float z = -40; z <= 40; z += 12)
		{
			float[] linesX = { -6f, 0f, 6f };
			foreach (float x in linesX)
			{
				if (QuestionBlockScene != null)
				{
					var block = QuestionBlockScene.Instantiate<Node3D>();
					floor.AddChild(block);
					block.Position = new Vector3(x, 4.0f + _offsetY, z);
				}
				if (CoinScene != null)
				{
					for (int m = 0; m < 3; m++)
					{
						var coin = CoinScene.Instantiate();
						floor.AddChild(coin);
						float coinZ = z + (m * 2.5f) - 2.5f;
						if (coin is Coin coinScript)
						{
							coinScript.Position = new Vector3(x, 7.0f + _offsetY, coinZ);
							coinScript.SetHeight(floor.GlobalPosition.Y + 7.0f + _offsetY);
						}
					}
				}
			}
		}
		SpawnDecorations(floor);
	}

private void SpawnCastleScene(Node3D floor)
{
	// --- 1. ЛЕСТНИЦА (ПИРАМИДА) ---
	float stairsStartZ = 40.0f; 
	int stairSteps = 8; 

	if (StairBlockScene != null)
	{
		for (int h = 0; h < stairSteps; h++)
		{
			int rowsUnder = stairSteps - h; 
			for (int s = 0; s < rowsUnder; s++)
			{
				for (int x = -1; x <= 1; x++)
				{
					var block = StairBlockScene.Instantiate<Node3D>();
					floor.AddChild(block);
					float finalX = x * BlockSize;
					float finalY = (h * BlockSize) + (BlockSize / 2.0f) + _offsetY;
					// Блоки уходят "вглубь" (минус по Z)
					float finalZ = stairsStartZ - (s * BlockSize) - (h * BlockSize);
					block.Position = new Vector3(finalX, finalY, finalZ);
				}
			}
		}
	}

	// --- 2. ФЛАГШТОК ---
	float flagZ = 0; // Переменная для хранения Z флага, чтобы привязать к ней замок
	if (FlagScene != null)
	{
		var flag = FlagScene.Instantiate<Node3D>();
		floor.AddChild(flag);
		
		float flagCorrectionX = -147f; // Твоя коррекция для кривой модели
		flagZ = stairsStartZ - (stairSteps * BlockSize) - 10.0f;
		
		// Опускаем на землю (offsetY - 1 метр)
		flag.Position = new Vector3(flagCorrectionX, _offsetY - 1.0f, flagZ);
	}

	// --- 3. ЗАМОК (СТАВИМ БЛИЖЕ) ---
	if (CastleScene != null)
	{
		var castle = CastleScene.Instantiate<Node3D>();
		floor.AddChild(castle);
		
		// Ставим замок всего в 12 метрах за флагом (раньше было -40 или -15)
		// Если он все еще кажется далеким, поставь -5.0f вместо -12.0f
		float castleZ = flagZ -8.0f; 

		castle.Position = new Vector3(-5.0f, _offsetY-0.01f, castleZ); 
		castle.Rotation = new Vector3(0, Mathf.Pi / 2, 0);
	}
}

	private void SpawnDecorations(Node3D floor)
	{
		// --- ГОРЫ (Дальний план) ---
		if (HillScene != null)
		{
			for (int i = 0; i < random.Next(2, 4); i++)
			{
				var hill = (Node3D)HillScene.Instantiate();
				floor.AddChild(hill);
				// Горы стоят далеко (от 30 до 60 метров от центра)
				float xHill = (float)(random.NextDouble() * 30 + 30) * ((random.NextDouble() > 0.5) ? 1 : -1);
				hill.Position = new Vector3(xHill, -0.5f + _offsetY, (float)(random.NextDouble() * TileLength - (TileLength / 2)));
				float scale = (float)(random.NextDouble() * 3 + 4.0);
				hill.Scale = Vector3.One * scale;
			}
		}

		// --- КУСТЫ (Ближний план) ---
		if (BushScene != null)
		{
			// Спавним от 3 до 6 кустов на одну плиту
			for (int i = 0; i < random.Next(3, 7); i++)
			{
				var bush = (Node3D)BushScene.Instantiate();
				floor.AddChild(bush);
				
				// Кусты стоят ближе к дороге (от 12 до 25 метров от центра)
				float xBush = (float)(random.NextDouble() * 13 + 12) * ((random.NextDouble() > 0.5) ? 1 : -1);
				float zBush = (float)(random.NextDouble() * TileLength - (TileLength / 2));
				
				bush.Position = new Vector3(xBush, 0.0f + _offsetY, zBush);
				
				// Немного случайного масштаба, чтобы кусты не были одинаковыми
				float scale = (float)(random.NextDouble() * 0.8 + 1.2); 
				bush.Scale = Vector3.One * scale;
			}
		}

		// --- ОБЛАКА ---
		if (CloudScene != null)
		{
			for (int i = 0; i < 6; i++)
			{
				var c = (Node3D)CloudScene.Instantiate();
				floor.AddChild(c);
				c.Position = new Vector3(
					(float)random.NextDouble() * 140 - 70, 
					(float)random.NextDouble() * 4.0f + 14.0f + _offsetY, 
					(float)random.NextDouble() * TileLength - (TileLength / 2)
				);
			}
		}
	}
	
		public override void _Input(InputEvent @event)
	{
		// Проверяем, нажата ли наша кнопка "exit_to_menu"
		if (@event.IsActionPressed("exit_to_menu"))
		{
			ReturnToMenu();
		}
	}

	private async void ReturnToMenu()
	{
		if (FadeOverlay == null)
		{
			// Если забыл привязать узел, просто выходим без эффекта
			ChangeSceneToMenu();
			return;
		}

		// 1. Создаем Tween для плавности
		Tween fadeTween = CreateTween();
		
		// 2. Анимируем прозрачность (Modulate:Alpha) от 0 до 1 за 0.5 секунды
		fadeTween.TweenProperty(FadeOverlay, "modulate:a", 1.0f, 0.5f);
		
		// 3. Ждем, пока анимация закончится
		await ToSignal(fadeTween, "finished");

		// 4. Теперь меняем сцену
		ChangeSceneToMenu();
	}

	private void ChangeSceneToMenu()
	{
		// Обязательно возвращаем время в норму (если была пауза)
		Engine.TimeScale = 1.0f;
		
		// Проверь путь! На прошлом скриншоте была ошибка 'res://Scenes/Menu.tscn'
		// Напиши здесь ТОЧНЫЙ путь, который ты скопировал из FileSystem
		string menuPath = "res://Menu.tscn"; 
		
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, menuPath);
	}
}
