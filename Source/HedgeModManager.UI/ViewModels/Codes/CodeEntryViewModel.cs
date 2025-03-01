using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.CodeCompiler;

namespace HedgeModManager.UI.ViewModels.Codes;

public partial class CodeEntryViewModel : ViewModelBase
{
    [ObservableProperty] private CSharpCode _code;

    public bool Enabled
    {
        get => Code.Enabled;
        set
        {
            Code.Enabled = value;
            OnPropertyChanged(nameof(Enabled));
        }
    }

    public CodeEntryViewModel(CSharpCode code)
    {
        Code = code;
    }
}
