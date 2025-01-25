using Godot;
using System;

public partial class SadPaper : Area2D
{
	[Export] private int healingAmount;
	[Export] private Sprite2D spriteBody;

  public override void _Ready() {
    BodyEntered += OnPointEnter;
  }

  public void OnPointEnter(Node other) {
    var p = other as player;
    if (p == null) return;

    p.Heal(healingAmount);
    
		var tween = CreateTween().TweenProperty(spriteBody, "modulate", Colors.Transparent, 1f);
		tween.Finished += () => QueueFree();
  }

	public override void _Process(double delta)
	{
	}
}
