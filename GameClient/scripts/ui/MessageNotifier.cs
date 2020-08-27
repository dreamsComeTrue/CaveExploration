using System.Collections.Generic;
using Godot;

public class MessageNotifier : Control
{
    private const float ITEM_HEIGHT_SIZE = 25.0f;
    private const int MAX_ITEM_TO_SHOW = 5;

    private PackedScene messageNotifierLabelScene;

    private LinkedList<MessageNotifierItem> messages;
    private Stack<string> stashedMessages;

    public override void _Ready()
    {
        messages = new LinkedList<MessageNotifierItem>();
        stashedMessages = new Stack<string>();
        messageNotifierLabelScene = (PackedScene)ResourceLoader.Load("res://scenes/ui/MessageNotifierItem.tscn");
    }

    public void RemoveMessageItem(MessageNotifierItem item)
    {
        messages.Remove(item);

        if (stashedMessages.Count > 0)
        {
            OffsetAllMessages();
            ShowMessage(stashedMessages.Pop());
        }
    }

    public void AddMessage(string message)
    {
        if (messages.Count >= MAX_ITEM_TO_SHOW)
        {
            stashedMessages.Push(message);
        }
        else
        {
            OffsetAllMessages();
            ShowMessage(message);
        }
    }

    private void OffsetAllMessages()
    {
        for (LinkedListNode<MessageNotifierItem> node = messages.First; node != null; node = node.Next)
        {
            node.Value.OffsetTo(ITEM_HEIGHT_SIZE);
        }
    }

    private void ShowMessage(string message)
    {
        MessageNotifierItem messageLabel = (MessageNotifierItem)messageNotifierLabelScene.Instance();
        messageLabel.ShowMessage(this, message);

        AddChild(messageLabel);
        messages.AddLast(messageLabel);
    }
}
