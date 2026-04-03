using Godot;
using System;

public partial class Disclaimer : Control
{
	private bool _isTransitioning = false;

	public override void _Ready()
	{
		var label = GetNodeOrNull<Label>("%DisclaimerLabel");
		
		if (label != null)
		{
			label.Modulate = new Color(1, 1, 1, 0);
			StartDisclaimerSequence();
		}
		else
		{
			GD.PrintErr("Ошибка: Узел %DisclaimerLabel не найден! Проверь Unique Name в редакторе.");
			GetTree().CreateTimer(1.0).Timeout += () => GoToMenu();
		}
	}

	private async void StartDisclaimerSequence()
	{
		var label = GetNode<Label>("%DisclaimerLabel");
	
		label.Modulate = new Color(1, 1, 1, 1);
		label.VisibleRatio = 0.0f;
	
		Tween typewriting = CreateTween();
	
		typewriting.TweenProperty(label, "visible_ratio", 1.0f, 6.0f)
				   .SetTrans(Tween.TransitionType.Linear);
		
		await ToSignal(typewriting, "finished");
	
		await ToSignal(GetTree().CreateTimer(5.0), "timeout");
	
		if (_isTransitioning) return;
	
		Tween fadeOut = CreateTween();
		fadeOut.TweenProperty(label, "modulate:a", 0.0f, 1.5f);
		await ToSignal(fadeOut, "finished");
	
		GoToMenu();
	}

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

	
		string menuPath = "res://Menu.tscn"; 
		
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, menuPath);
	}
}
