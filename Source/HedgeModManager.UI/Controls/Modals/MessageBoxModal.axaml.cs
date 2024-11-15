using Avalonia;
using Avalonia.Controls;
using HedgeModManager.UI.Events;
using HedgeModManager.UI.ViewModels.Mods;
using System;

namespace HedgeModManager.UI.Controls.Modals;

public partial class MessageBoxModal : UserControl
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
        if (string.IsNullOrEmpty(Message))
            Message = "Message";
        AddButton("Button 1", (s, e) => { });
        AddButton("Button 2", (s, e) => { });
    }

    public MessageBoxModal(string message)
    {
        InitializeComponent();
        Message = message;
    }

    public MessageBoxModal AddButton(string text, EventHandler<ButtonClickEventArgs> handler)
    {
        var button = new SimpleModalButton
        {
            Text = text
        };
        button.Click += handler;

        ButtonStackPanel.Children.Add(button);
        return this;
    }
}