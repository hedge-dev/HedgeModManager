using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.CodeCompiler;

namespace HedgeModManager.UI.ViewModels.Codes;

public partial class CodeEntryViewModel : ViewModelBase
{
    [ObservableProperty] private CSharpCode _code;
    [ObservableProperty] private bool _isLastElement = false;

    public CodeCategoryViewModel? Category { get; set; } = null;
    public MainWindowViewModel MainViewModel { get; set; }

    public bool Enabled
    {
        get => Code.Enabled;
        set
        {
            Code.Enabled = value;
            OnPropertyChanged(nameof(Enabled));
            Category?.Update(false, true);
        }
    }

    public CodeEntryViewModel(CSharpCode code, MainWindowViewModel viewModel)
    {
        Code = code;
        MainViewModel = viewModel;
    }
}
