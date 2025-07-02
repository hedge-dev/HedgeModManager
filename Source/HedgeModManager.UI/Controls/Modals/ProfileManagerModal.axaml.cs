using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HedgeModManager.Foundation;
using HedgeModManager.UI.ViewModels;
using System.ComponentModel;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ProfileManagerModal : WindowModal
{
    public ProfileManagerViewModel ViewModel { get; set; }

    public ProfileManagerModal()
    {
        ViewModel = new();
        AvaloniaXamlLoader.Load(this);
    }

    public string GenerateProfileName(string baseName)
    {
        if (ViewModel.MainWindowViewModel == null)
            return string.Empty;

        var profiles = ViewModel.MainWindowViewModel.Profiles;
        string name = baseName;
        while (profiles.Any(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            name += " - Copy";
        return name;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ViewModel.MainWindowViewModel = DataContext as MainWindowViewModel;
        if (ViewModel.MainWindowViewModel != null)
        {
            ViewModel.SelectedProfile = ViewModel.MainWindowViewModel.SelectedProfile;
            ViewModel.MainWindowViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        }
        ViewModel.Update();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainWindowViewModel != null)
            ViewModel.MainWindowViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnDuplicateClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainWindowViewModel == null || ViewModel.SelectedProfile == null)
            return;

        if (ViewModel.MainWindowViewModel.SelectedGame?.Game is not ModdableGameGeneric game)
            return;

        var baseProfile = ViewModel.SelectedProfile;
        var profile = new ModProfile(GenerateProfileName(baseProfile.Name), baseProfile.ModDBPath, baseProfile.FileName);
        profile.FileName = profile.GenerateFileNameFromName();

        // Copy profile
        string modsPath = game.ModDatabase.Root;
        string baseProfilePath = Path.Combine(modsPath, baseProfile.FileName);
        string newProfilePath = Path.Combine(modsPath, profile.FileName);
        if (File.Exists(baseProfilePath))
        {
            try
            {
                File.Copy(baseProfilePath, newProfilePath, true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to copy profile: {baseProfilePath} -> {newProfilePath}");
                Logger.Error(ex);
            }
        }

        ViewModel.MainWindowViewModel.Profiles.Add(profile);
        ViewModel.MainWindowViewModel.SelectedProfile = ViewModel.SelectedProfile = profile;
        Dispatcher.UIThread.Invoke(async () => await ViewModel.MainWindowViewModel.SaveProfilesAsync(game));
        ViewModel.Update();
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainWindowViewModel == null || ViewModel.SelectedProfile == null)
            return;

        var messageBox = new MessageBoxModal("Modal.Title.DeleteProfile", Localize("Modal.Message.DeleteProfile", ViewModel.SelectedProfile.Name));
        messageBox.AddButton("Common.Button.No", (s, e) => messageBox.Close());
        messageBox.AddButton("Common.Button.Yes", (s, e) =>
        {
            if (ViewModel.MainWindowViewModel.SelectedGame?.Game is not ModdableGameGeneric game)
                return;

            var profile = ViewModel.SelectedProfile;
            string profilePath = Path.Combine(game.ModDatabase.Root, profile.FileName);
            if (File.Exists(profilePath))
            {
                try
                {
                    File.Delete(profilePath);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to delete profile: {profilePath}");
                    Logger.Error(ex);
                }
            }

            int pos = ViewModel.MainWindowViewModel.Profiles.IndexOf(ViewModel.SelectedProfile);
            if (pos == ViewModel.MainWindowViewModel.Profiles.Count - 1)
                pos -= 1;
            ViewModel.MainWindowViewModel?.Profiles.Remove(ViewModel.SelectedProfile);
            ViewModel.SelectedProfile = ViewModel.MainWindowViewModel?.Profiles[pos];
            ViewModel.Update();
            messageBox.Close();
        });
        messageBox.SetDanger();
        messageBox.Open(ViewModel.MainWindowViewModel);
    }

    private void OnRenameClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainWindowViewModel == null || ViewModel.SelectedProfile == null)
            return;

        var modal = new ProfileRenameModal(ViewModel.SelectedProfile);
        modal.OnProfileRenamed = () =>
        {
            ProfileListBox.Bind(ListBox.ItemsSourceProperty, new Binding("Profiles"));
        };
        modal.Open(ViewModel.MainWindowViewModel);
        ViewModel.Update();
    }

    private void OnSelectClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainWindowViewModel == null || ViewModel.SelectedProfile == null)
            return;

        ViewModel.MainWindowViewModel.SelectedProfile = ViewModel.SelectedProfile;
        ViewModel.Update();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ViewModel.Update();
    }

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.IsBusy))
            ViewModel?.Update();
    }
}