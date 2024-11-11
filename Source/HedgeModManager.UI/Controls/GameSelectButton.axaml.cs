using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using HedgeModManager.UI.Controls.Primitives;
using HedgeModManager.UI.Models;
using System;
using System.Linq;
using ValveKeyValue;

namespace HedgeModManager.UI.Controls;

public partial class GameSelectButton : ButtonUserControl
{
    public static readonly StyledProperty<UIGame> GameProperty =
        AvaloniaProperty.Register<GameSelectButton, UIGame>(nameof(Game));

    public UIGame Game
    {
        get => GetValue(GameProperty);
        set => SetValue(GameProperty, value);
    }

    public GameSelectButton()
    {
        InitializeComponent();
    }
}