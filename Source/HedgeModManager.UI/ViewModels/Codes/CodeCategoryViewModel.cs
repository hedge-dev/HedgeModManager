using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace HedgeModManager.UI.ViewModels.Codes;

public partial class CodeCategoryViewModel : ViewModelBase
{
    private CodeCategoryViewModel? _parent;
    
    [ObservableProperty] private string _name = "Unnamed Category";
    [ObservableProperty] private bool _expanded = false;
    [ObservableProperty] private ObservableCollection<CodeCategoryViewModel> _cateories = [];
    [ObservableProperty] private ObservableCollection<CodeEntryViewModel> _codes = [];

    public CodeCategoryViewModel(CodeCategoryViewModel? parent, string name)
    {
        _parent = parent;
        Name = name;

        int firstIndex = name.IndexOf('/');
        if (firstIndex != -1)
        {
            string newCategoryName = name.Substring(firstIndex + 1);
            Name = name.Substring(0, firstIndex);
            Cateories.Add(new CodeCategoryViewModel(this, newCategoryName));
        }
    }

    public string GetPathToRoot(CodeCategoryViewModel? parent, string str)
    {
        if (parent == null)
            return str;

        return GetPathToRoot(parent._parent, parent.Name + "/" + str);
    }
}
