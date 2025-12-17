using Godot;

public partial class UIManager : Control
{
    public void ShowMenu()
    {
        GD.Print("UI: Showing Start Menu");
        // 未来在这里控制 UI 节点的 Visible 属性
    }

    public void UpdateTurnIndicator(string message)
    {
        GD.Print($"UI: Turn Indicator Updated - {message}");
    }

    public void ShowGameOverScreen()
    {
        GD.Print("UI: Showing Game Over Screen");
    }
}
