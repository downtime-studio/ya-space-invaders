using Godot;

public partial class HUD : CanvasLayer
{
    private Label _scoreLabel;
    private Control _messagePanel;
    private Label _messageLabel;

    public override void _Ready()
    {
        _scoreLabel = GetNode<Label>("ScoreLabel");
        _messagePanel = GetNode<Control>("MessagePanel");
        _messageLabel = GetNode<Label>("MessagePanel/MessageLabel");

        _messagePanel.Visible = false;
        SetScore(0);
    }

    public void SetScore(int score) => _scoreLabel.Text = $"Score: {score}";

    public void ShowMessage(string message)
    {
        _messageLabel.Text = message;
        _messagePanel.Visible = true;
    }

    public void HideMessage() => _messagePanel.Visible = false;
}