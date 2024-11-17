using Avalonia;
using Avalonia.Controls;
using HedgeModManager.UI.Events;
using HedgeModManager.UI.ViewModels;
using System;
using System.Linq;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Modals;

public partial class MessageBoxModal : UserControl
{
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<MessageBoxModal, string>(nameof(Message),
            defaultValue: string.Empty);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<MessageBoxModal, string>(nameof(Title),
            defaultValue: string.Empty);

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // Preview only
    public MessageBoxModal()
    {
        InitializeComponent();
        Title = "Title";
        Message = "Message\nLine 2";
        AddButton("Button 1", (s, e) => { });
        AddButton("Button 2", (s, e) => { });
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
        var button = new SimpleModalButton
        {
            Text = text
        };
        button.Click += handler;

        ButtonStackPanel.Children.Add(button);
        return this;
    }

    public void Open(MainWindowViewModel viewModel)
    {
        viewModel.Modals.Add(new Modal(this, new Thickness(0)));
    }

    public void Close()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var modalInstance = viewModel.Modals.FirstOrDefault(x => x.Control == this);
            if (modalInstance != null)
                viewModel.Modals.Remove(modalInstance);
        }
    }
}