using Godot;
using System.Collections.Generic;

public partial class BoardPoint : Area2D
{
	// 定义节点类型
	public enum PointType { Normal, Railroad, Camp, HQ }

	// 基础数据
	public Vector2I Coordinate { get; private set; }
	public PointType Type { get; private set; }
	public List<BoardPoint> Neighbors { get; private set; } = new List<BoardPoint>();
	
	// 视觉引用
	private Sprite2D _baseSprite;
	private Label _debugLabel;

	public override void _Ready()
	{
		_baseSprite = GetNode<Sprite2D>("BaseSprite");
		_debugLabel = GetNode<Label>("DebugLabel");
	}

	// 初始化函数：由 BoardManager 调用
	public void Initialize(int x, int y, PointType type)
	{
		Coordinate = new Vector2I(x, y);
		Type = type;
		
		// 视觉调试反馈
		_debugLabel.Text = $"{x},{y}";
		
		// 用颜色区分类型 (临时美术)
		switch (type)
		{
			case PointType.Camp:
				Modulate = new Color(0.9f, 0.9f, 0.2f); // 黄色：行营
				_debugLabel.Text += "\nCamp";
				break;
			case PointType.HQ:
				Modulate = new Color(1f, 0.3f, 0.3f);   // 红色：大本营
				_debugLabel.Text += "\nHQ";
				break;
			case PointType.Railroad:
				Modulate = new Color(0.6f, 0.8f, 1f);   // 蓝色：铁路
				break;
			default:
				Modulate = new Color(1f, 1f, 1f);       // 白色：公路
				break;
		}
	}

	// 建立连接关系 (由 BoardManager 调用)
	public void AddNeighbor(BoardPoint neighbor)
	{
		if (!Neighbors.Contains(neighbor) && neighbor != this)
		{
			Neighbors.Add(neighbor);
		}
	}
}
