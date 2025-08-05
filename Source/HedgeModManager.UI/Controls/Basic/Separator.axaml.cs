using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls.Basic;

public partial class Separator : UserControl
{
    public static readonly StyledProperty<bool> FullWidthProperty =
        AvaloniaProperty.Register<Separator, bool>(nameof(FullWidth), false);

    // Not sure what is the right way to handle this
    public static readonly StyledProperty<SeparatorViewModel> ViewModelProperty =
        AvaloniaProperty.Register<Separator, SeparatorViewModel>(nameof(ViewModel), new());

    public bool FullWidth
    {
        get => ViewModel.FullWidth;
        set => ViewModel.FullWidth = value;
    }

    public SeparatorViewModel ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public Separator()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public partial class SeparatorViewModel : ViewModelBase
    {
        [ObservableProperty] private int _columnStart = 1;
        [ObservableProperty] private int _columnLength = 1;

        private bool _fullWidth = false;

        public bool FullWidth
        {
            get => _fullWidth;
            set
            {
                ColumnStart = value ? 0 : 1;
                ColumnLength = value ? 3 : 1;
            }
        }
    }
}