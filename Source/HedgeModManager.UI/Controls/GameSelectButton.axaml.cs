using Avalonia;
using HedgeModManager.UI.Controls.Primitives;
using HedgeModManager.UI.Models;

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