using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Config;
using System.Collections.ObjectModel;

namespace HedgeModManager.UI.ViewModels.Codes;

public partial class CodeCategoryViewModel : ViewModelBase
{
    private bool _expanded = false;

    [ObservableProperty] private string _name = "Unnamed Category";
    [ObservableProperty] private bool _isLastElement = false;
    [ObservableProperty] private CodeCategoryViewModel? _parent;
    [ObservableProperty] private ObservableCollection<CodeCategoryViewModel> _categories = [];
    [ObservableProperty] private ObservableCollection<CodeEntryViewModel> _codes = [];
    [ObservableProperty] private MainWindowViewModel? mainViewModel = null;

    public bool Expanded
    {
        get => _expanded;
        set
        {
            SetProperty(ref _expanded, value);
            if (MainViewModel != null && MainViewModel.SelectedGameConfig is GameConfig gameConfig)
            {
                string path = ToString();
                if (value)
                {
                    if (!gameConfig.ExpandedCodes.Contains(path))
                        gameConfig.ExpandedCodes.Add(path);
                }
                else
                {
                    if (gameConfig.ExpandedCodes.Contains(path))
                        gameConfig.ExpandedCodes.Remove(path);
                }
            }
        }
    }

    public CodeCategoryViewModel(CodeCategoryViewModel? parent, string name, MainWindowViewModel? mainViewModel)
    {
        Parent = parent;
        Name = name;
        MainViewModel = mainViewModel;

        int firstIndex = name.IndexOf('/');
        if (firstIndex != -1)
        {
            string newCategoryName = name[(firstIndex + 1)..];
            Name = name[..firstIndex];
            Categories.Add(new CodeCategoryViewModel(this, newCategoryName, mainViewModel));
        }
    }

    public string GetPathToRoot(CodeCategoryViewModel? parent, string str)
    {
        if (parent == null)
            return str;

        return GetPathToRoot(parent.Parent, parent.Name + "/" + str);
    }

    public override string ToString()
    {
        return GetPathToRoot(Parent, Name);
    }
}
