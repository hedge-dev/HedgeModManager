using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ProfileRenameModal : WindowModal
{
    public ModProfile SelectedProfile { get; set; }
    public string NewName { get; set; } = string.Empty;
    public Action? OnProfileRenamed { get; set; }

    // Preview
    public ProfileRenameModal() : this(new ModProfile("No Profile"), false) { }

    public ProfileRenameModal(ModProfile modProfile, bool newProfile)
    {
        SelectedProfile = modProfile;
        AvaloniaXamlLoader.Load(this);
        if (newProfile)
            Title = Localize("Modal.Title.NewProfile");
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        NewName = NewName?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(NewName))
        {
            Close();
            return;
        }
        SelectedProfile.Name = NewName;
        OnProfileRenamed?.Invoke();
        Close();
    }
}