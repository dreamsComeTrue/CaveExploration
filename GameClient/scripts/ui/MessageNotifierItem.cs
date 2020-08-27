using Godot;

public class MessageNotifierItem : Control
{
    private Tween offsetTween;

    private MessageNotifier messageNotifier;

    private Vector2 startPos;
    private float heightOffsetTotal = 0.0f;

    public override void _Ready()
    {
        offsetTween = new Tween();
        startPos = RectPosition;
        AddChild(offsetTween);
    }

    public void ShowMessage(MessageNotifier messageNotifier, string message)
    {
        this.messageNotifier = messageNotifier;

        GetNode<Label>("MessageNotifierLabel").Text = message;
    }

    public void OffsetTo(float heightOffset)
    {
        startPos.y = heightOffsetTotal;
        heightOffsetTotal -= heightOffset;

        Vector2 endPosition = new Vector2(0, heightOffsetTotal);

        offsetTween.InterpolateProperty(this, "rect_position", startPos, endPosition, 0.3f);
        offsetTween.Start();
    }

    public void BeforeDestroy()
    {
        messageNotifier.RemoveMessageItem(this);
    }
}
