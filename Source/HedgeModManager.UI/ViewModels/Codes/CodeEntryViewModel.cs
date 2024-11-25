using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;

namespace HedgeModManager.UI.ViewModels.Codes;

public partial class CodeEntryViewModel : ViewModelBase
{
    [ObservableProperty] private ICode _code;

    public bool Enabled
    {
        get => Code.Enabled;
        set
        {
            Code.Enabled = value;
            OnPropertyChanged(nameof(Enabled));
        }
    }

    public CodeEntryViewModel(ICode code)
    {
        Code = code;
    }

}
