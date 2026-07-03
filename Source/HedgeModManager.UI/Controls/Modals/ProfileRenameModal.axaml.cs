using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ProfileRenameModal : WindowModal
{
    public ModProfile SelectedProfile { get; set; }
    public string NewName { get; set; } = string.Empty;
    public delegate Task OnConfirmEventHandler(object? sender, EventArgs e);
    public event OnConfirmEventHandler? OnConfirm;

    // Preview
    public ProfileRenameModal() : this(new ModProfile("No Profile"), false) { }

    public ProfileRenameModal(ModProfile modProfile, bool newProfile)
    {
        SelectedProfile = modProfile;
        AvaloniaXamlLoader.Load(this);
        if (newProfile)
            Title = Localize("Modal.Title.NewProfile");
    }

    private async void OnOkClick(object? sender, RoutedEventArgs e)
    {
        NewName = NewName?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(NewName))
        {
            Close();
            return;
        }
        SelectedProfile.Name = NewName;
        if (OnConfirm != null)
            await OnConfirm(this, e);
        Close();
    }
}