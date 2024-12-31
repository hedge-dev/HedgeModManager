using Avalonia;
using HedgeModManager.UI.Events;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Modals;

public partial class MessageBoxModal : WindowModal
{
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<MessageBoxModal, string>(nameof(Message),
            defaultValue: string.Empty);

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    // Preview only
    public MessageBoxModal()
    {
        InitializeComponent();
        Title = "Title";
        Message = "Message\nLine 2";
        AddButton("Button 1", (s, e) => { Message = "Button 1 Clicked"; });
        AddButton("Button 2", (s, e) => { Message = "Button 2 Clicked"; });
    }

    public MessageBoxModal(string title, string message)
    {
        InitializeComponent();
        Title = title;
        Message = message;
    }

    public static MessageBoxModal CreateOK(string title, string message, params object[] args)
    {
        var messageBox = new MessageBoxModal(Localize(title), Localize(message, args));
        messageBox.AddButton("Common.Button.OK", (s, e) => messageBox.Close());
        return messageBox;
    }

    public MessageBoxModal AddButton(string text, EventHandler<ButtonClickEventArgs> handler)
    {
        var button = new Basic.Button
        {
            Text = text
        };
        button.Click += handler;

        ButtonStackPanel.Children.Add(button);
        return this;
    }
}