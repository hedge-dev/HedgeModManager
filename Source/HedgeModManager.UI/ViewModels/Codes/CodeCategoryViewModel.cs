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

    public int EnabledCount => GetTotalEnabledCount();

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

    public int GetTotalEnabledCount()
    {
        int count = Codes.Count(x => x.Enabled);
        count += Categories.Sum(x => x.GetTotalEnabledCount());
        return count;
    }

    public string GetPathToRoot(CodeCategoryViewModel? parent, string str)
    {
        if (parent == null)
            return str;

        return GetPathToRoot(parent.Parent, parent.Name + "/" + str);
    }

    public void Update(bool updateChild, bool updateParent)
    {
        if (updateChild)
            Categories.ForEach(x => x.Update(updateChild, false));

        OnPropertyChanged(nameof(EnabledCount));

        if (updateParent && Parent != null)
            Parent.Update(false, updateParent);
    }

    public override string ToString()
    {
        return GetPathToRoot(Parent, Name);
    }
}
