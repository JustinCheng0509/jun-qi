using Godot;

public partial class GameManager : Node
{
    [Export]
    public Board BoardNode { get; set; }
    [Export]
    public RuleChecker RuleCheckerNode { get; set; }
    [Export]
    public UIManager UIManagerNode { get; set; }

    public enum GameState
    {
        Playing,
        GameOver,
        StartMenu,
        Settings,
        Setup,
        PlayerTurn,
        EnemyTurn,
        MoveExecution,
        GamePaused
    }

    private Player _player;

    public override void _Ready()
    {
        // Dependency check
        if (BoardNode == null || RuleCheckerNode == null || UIManagerNode == null)
        {
            GD.PrintErr("GameManager is missing required nodes. Please assign them in the inspector.");
            return;
        }
        // Initialize the GameState
        SetGameState(GameState.StartMenu);
    }

   public void SetState(GameState newState)
{
    // 1. 退出当前状态的清理逻辑 (可选：用于复杂的状态清理)
    _ExitState(CurrentState); 

    // 2. 状态更新
    CurrentState = newState;
    GD.Print($"Game state changed to: {newState}");

    // 3. 进入新状态的启动逻辑
    _EnterState(CurrentState);
}

private void _EnterState(GameState state)
{
    switch (state)
    {
        case GameState.START_MENU:
            // 启动菜单界面
            UIManagerNode.ShowMenu();
            break;
            
        case GameState.PLAYER_TURN:
            // 通知 UI 更新回合指示器
            UIManagerNode.UpdateTurnIndicator("Human Player's Turn");
            // 启用玩家输入监听（例如：BoardNode.EnableInput()）
            break;

        case GameState.AI_TURN:
            // 禁用玩家输入
            UIManagerNode.UpdateTurnIndicator("AI is Thinking...");
            // 触发 AI 计算
            StartAiTurn(); // 独立方法，调用 AI 模块
            break;
            
        case GameState.GAME_OVER:
            // 显示胜利/失败界面
            UIManagerNode.ShowGameOverScreen(/* ... result ... */);
            break;
    }
}

private void _ExitState(GameState state)
{
    // 例如：退出 MOVE_EXECUTION 时，清除动画效果
    // 例如：退出 START_MENU 时，隐藏菜单界面
}

// 由 UI 按钮调用
public void StartNewGame(bool isPVE)
{
    // 初始化棋盘数据、设置玩家
    BoardNode.InitializeBoardData(isPVE);
    
    // 如果需要布阵，进入 SETUP 状态，否则直接进入 PLAYER_TURN
    SetState(GameState.SETUP); 
}

// 在 PLAYER_TURN 状态下，由 BoardNode 调用 (玩家点击棋子)
public void HandlePlayerInput(Vector2 selectedPosition)
{
    if (CurrentState != GameState.PLAYER_TURN) return;
    
    // 检查移动合法性，如果合法，进入执行状态
    // RuleCheckerNode.IsMoveLegal(...)
    SetState(GameState.MOVE_EXECUTION);
}

// 供 AI_TURN 状态调用
private void StartAiTurn()
{
    // 假设 AI 模块位于 RuleCheckerNode 或一个专门的 AiPlayer 类中
    // 启动一个异步任务或使用 Timer 等待 AI 返回 Move 对象
    // Move aiMove = AiPlayer.GetBestMove(BoardNode.GetCurrentState());
    
    // 假设 AI 立即返回 Move:
    ExecuteMove(aiMove);
}

// 移动执行完成后，由 BoardNode 或动画完成信号调用
public void OnMoveExecutionFinished(Move move)
{
    // 检查胜负条件
    if (RuleCheckerNode.CheckWinCondition())
    {
        SetState(GameState.GAME_OVER);
    }
    else
    {
        // 切换到下一个玩家
        // GetNextPlayer()
        SetState(GameState.PLAYER_TURN); // 或 AI_TURN
    }
}

}