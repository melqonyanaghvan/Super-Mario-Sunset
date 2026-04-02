using Godot;
using System;

public partial class Disclaimer : Control
{
	// Флаг, чтобы переход в меню не сработал дважды (например, если игрок нажал кнопку во время анимации)
	private bool _isTransitioning = false;

	public override void _Ready()
	{
		// Ищем наш текст. Убедись, что в редакторе у Label включено "Access as Unique Name" (%)
		var label = GetNodeOrNull<Label>("%DisclaimerLabel");
		
		if (label != null)
		{
			// Начинаем с полной прозрачности
			label.Modulate = new Color(1, 1, 1, 0);
			// Запускаем нашу "режиссуру"
			StartDisclaimerSequence();
		}
		else
		{
			GD.PrintErr("Ошибка: Узел %DisclaimerLabel не найден! Проверь Unique Name в редакторе.");
			// Если текста нет, сразу идем в меню через 1 секунду, чтобы игра не зависла на черном экране
			GetTree().CreateTimer(1.0).Timeout += () => GoToMenu();
		}
	}

private async void StartDisclaimerSequence()
{
	var label = GetNode<Label>("%DisclaimerLabel");

	// 1. Сначала делаем сам узел видимым (Modulate Alpha = 1)
	// Но текст не виден, так как VisibleRatio = 0
	label.Modulate = new Color(1, 1, 1, 1);
	label.VisibleRatio = 0.0f;

	// 2. ЭФФЕКТ ПЕЧАТНОЙ МАШИНКИ
	Tween typewriting = CreateTween();
	// Анимируем появление букв за 5-6 секунд (подбери скорость под себя)
	typewriting.TweenProperty(label, "visible_ratio", 1.0f, 6.0f)
			   .SetTrans(Tween.TransitionType.Linear); // Чтобы печаталось равномерно
	
	await ToSignal(typewriting, "finished");

	// 3. ПАУЗА (даем дочитать последние слова)
	await ToSignal(GetTree().CreateTimer(5.0), "timeout");

	if (_isTransitioning) return;

	// 4. ПЛАВНОЕ ИСЧЕЗНОВЕНИЕ ВСЕГО ТЕКСТА (FADE OUT)
	Tween fadeOut = CreateTween();
	fadeOut.TweenProperty(label, "modulate:a", 0.0f, 1.5f);
	await ToSignal(fadeOut, "finished");

	// 5. ПЕРЕХОД В МЕНЮ
	GoToMenu();
}

	// Позволяет пропустить дисклеймер любой кнопкой (клавиатура, мышь, геймпад)
	public override void _Input(InputEvent @event)
	{
		if (@event.IsPressed() && !_isTransitioning)
		{
			GD.Print("Дисклеймер пропущен игроком.");
			GoToMenu();
		}
	}

	private void GoToMenu()
	{
		if (_isTransitioning) return;
		_isTransitioning = true;

		// ВАЖНО: Проверь, чтобы путь к твоей сцене меню был именно таким!
		// Если папка называется 'scenes' с маленькой буквы, исправь путь ниже.
		string menuPath = "res://Menu.tscn"; 
		
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, menuPath);
	}
}
