using Godot;
using System.Collections.Generic;

public partial class BoardManager : Node2D
{
	[Export] public PackedScene PointScene; // 把 BoardPoint.tscn 拖进来
	[Export] public Vector2 StartPosition = new Vector2(60, 100); // 棋盘在屏幕的起始位置
	[Export] public Vector2 GridGap = new Vector2(80, 80);        // 格子间距

	// 核心数据：通过坐标快速找到格子对象
	public Dictionary<Vector2I, BoardPoint> GridMap = new Dictionary<Vector2I, BoardPoint>();

	public override void _Ready()
	{
		GenerateGrid();
		ConnectGrid();
		DrawDebugLines(); // 调试完成后可注释掉
	}

	private void GenerateGrid()
	{
		// 军棋标准布局：5列 x 12行 (0-11)
		for (int y = 0; y < 12; y++)
		{
			for (int x = 0; x < 5; x++)
			{
				// 1. 实例化
				var point = PointScene.Instantiate<BoardPoint>();

				// 2.【关键修改】先将节点加入场景树
				// 这一步会立即触发 BoardPoint._Ready()，从而获取 Label 和 Sprite 节点引用
				AddChild(point);

				// 3. 计算位置
				Vector2 pos = StartPosition + new Vector2(x * GridGap.X, y * GridGap.Y);
				// 处理“山界” (第6行和第7行之间留空隙)
				if (y >= 6) pos.Y += GridGap.Y * 0.5f;

				point.Position = pos;

				// 4. 确定类型并初始化
				// 此时 _debugLabel 已不再为空，可以安全调用 Initialize
				BoardPoint.PointType type = DetermineType(x, y);
				point.Initialize(x, y, type);

				// 5. 存入字典
				GridMap[new Vector2I(x, y)] = point;
			}
		}
	}

	private BoardPoint.PointType DetermineType(int x, int y)
	{
		// 将坐标标准化到半场 (0-5) 方便判断，因为红黑是对称的
		// 如果 y >= 6, 映射回 11-y (即 11->0, 6->5)
		int localY = y >= 6 ? 11 - y : y;

		// 1. 行营 (Camp) - 固定坐标
		// (1,2), (3,2), (2,3), (1,4), (3,4)
		bool isCamp = (localY == 2 && (x == 1 || x == 3)) ||
					  (localY == 3 && x == 2) ||
					  (localY == 4 && (x == 1 || x == 3));
		if (isCamp) return BoardPoint.PointType.Camp;

		// 2. 大本营 (HQ)
		// (1,0), (3,0)
		if (localY == 0 && (x == 1 || x == 3)) return BoardPoint.PointType.HQ;

		// 3. 铁路 (Railroad)
		// 第一行(localY=1) 和 第五行(localY=5) 是每一方的底线铁路和前线铁路
		// 最左列(x=0) 和 最右列(x=4) 是侧边铁路
		// 注意：军棋的最底行(localY=0)不是铁路，通常第二行(localY=1)才是
		if (localY == 1 || localY == 5 || x == 0 || x == 4) return BoardPoint.PointType.Railroad;

		// 4. 其余全部是公路
		return BoardPoint.PointType.Normal;
	}

	private void ConnectGrid()
	{
		foreach (var kvp in GridMap)
		{
			Vector2I coord = kvp.Key;
			BoardPoint current = kvp.Value;

			// --- 尝试 4 个正交方向 (上下左右) ---
			Vector2I[] directions = { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };
			foreach (var dir in directions)
			{
				Vector2I neighborPos = coord + dir;
				if (GridMap.ContainsKey(neighborPos))
				{
					// 特殊处理：山界阻断
					// 只有在山界 (y=5 和 y=6 之间)，且 x=1 或 x=3 时是不通的
					bool crossingMountain = (coord.Y == 5 && neighborPos.Y == 6) || (coord.Y == 6 && neighborPos.Y == 5);
					bool mountainBlocked = crossingMountain && (coord.X == 1 || coord.X == 3);

					if (!mountainBlocked)
					{
						current.AddNeighbor(GridMap[neighborPos]);
					}
				}
			}

			// --- 尝试斜向连接 (只有行营相关才通) ---
			// 规则：如果你是行营，或者你要去的地方是行营，且你们是斜角相邻，则连通
			Vector2I[] diagonals = { new Vector2I(1, 1), new Vector2I(1, -1), new Vector2I(-1, 1), new Vector2I(-1, -1) };
			foreach (var diag in diagonals)
			{
				Vector2I neighborPos = coord + diag;
				if (GridMap.ContainsKey(neighborPos))
				{
					BoardPoint neighbor = GridMap[neighborPos];
					if (current.Type == BoardPoint.PointType.Camp || neighbor.Type == BoardPoint.PointType.Camp)
					{
						current.AddNeighbor(neighbor);
					}
				}
			}
		}
	}

	// [调试用] 画出连接线，确保逻辑正确
	private void DrawDebugLines()
	{
		var lineContainer = new Node2D();
		AddChild(lineContainer);

		foreach (var p in GridMap.Values)
		{
			foreach (var n in p.Neighbors)
			{
				// 只画单向，避免重复画
				if (p.Coordinate.X < n.Coordinate.X || (p.Coordinate.X == n.Coordinate.X && p.Coordinate.Y < n.Coordinate.Y))
				{
					Line2D line = new Line2D();
					// 这里添加一个空判断，防止万一 Position 没准备好
					if (p != null && n != null) 
					{
						line.Points = new Vector2[] { p.Position, n.Position };
						line.Width = 2.0f;
						line.DefaultColor = new Color(0, 1, 0, 0.2f); // 绿色半透明
						lineContainer.AddChild(line);
}
}
}
}
}
}
