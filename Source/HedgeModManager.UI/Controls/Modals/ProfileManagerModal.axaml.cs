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
        int count = 0;
        while (profiles.Any(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
        {
            ++count;
            name = $"{baseName} ({count})";
        }

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

    private void OnNewClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainWindowViewModel == null)
            return;
        if (ViewModel.MainWindowViewModel.SelectedGame?.Game is not ModdableGameGeneric game)
            return;

        // Create profile
        var profile = new ModProfile()
        {
            Name = GenerateProfileName("New Profile")
        };

        var modal = new ProfileRenameModal(profile, true);
        modal.OnConfirm += async (s, e) =>
        {
            profile.FileName = profile.GenerateFileNameFromName();
            profile.ModDBPath = profile.FileName;

            ViewModel.MainWindowViewModel.Profiles.Add(profile);
            ViewModel.MainWindowViewModel.SelectedProfile = ViewModel.SelectedProfile = profile;
            ProfileListBox.Bind(ListBox.ItemsSourceProperty, new Binding("Profiles"));
            await ViewModel.MainWindowViewModel.SaveProfilesAsync(game);
            ViewModel.Update();
        };
        modal.Open(ViewModel.MainWindowViewModel);
    }

    private void OnDuplicateClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProfile == null ||
            ViewModel.MainWindowViewModel?.SelectedGame?.Game is not ModdableGameGeneric game)
            return;

        var baseProfile = ViewModel.SelectedProfile;
        var newProfile = new ModProfile(GenerateProfileName(baseProfile.Name));

        // Aquire the new profile name
        var modal = new ProfileRenameModal(newProfile, true);
        modal.OnConfirm += async (s, e) =>
        {
            newProfile.FileName = newProfile.GenerateFileNameFromName();
            newProfile.ModDBPath = newProfile.FileName;

            string modsPath = game.ModDatabase.Root;
            string baseProfilePath = Path.Combine(modsPath, baseProfile.ModDBPath);
            string newProfilePath = Path.Combine(modsPath, newProfile.ModDBPath);

            if (File.Exists(baseProfilePath))
            {
                await MainWindowViewModel.CreateSimpleDownload("Download.Text.DuplicateProfile", "Failed to duplicate profile", async (d, p, c) =>
                {
                    bool isbaseSelected = ViewModel.MainWindowViewModel.SelectedProfile == baseProfile;
                    await newProfile.BackupModConfigAsync(game.ModDatabase, isbaseSelected ? null : baseProfile, d.CreateProgress());
                    File.Copy(baseProfilePath, newProfilePath, true);

                }).RunAsync(ViewModel.MainWindowViewModel);
            }

            ViewModel.MainWindowViewModel.Profiles.Add(newProfile);
            ViewModel.MainWindowViewModel.SelectedProfile = ViewModel.SelectedProfile = newProfile;
            await ViewModel.MainWindowViewModel.SaveProfilesAsync(game);
            ViewModel.Update();
        };
        modal.Open(ViewModel.MainWindowViewModel);

    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainWindowViewModel == null || ViewModel.SelectedProfile == null)
            return;

        var messageBox = new MessageBoxModal("Modal.Title.DeleteProfile", Localize("Modal.Message.DeleteProfile", ViewModel.SelectedProfile.Name));
        messageBox.AddButton("Common.Button.No", (s, e) => messageBox.Close());
        messageBox.AddButton("Common.Button.Yes", async (s, e) =>
        {
            if (ViewModel.MainWindowViewModel.SelectedGame?.Game is not ModdableGameGeneric game)
                return;

            var profile = ViewModel.SelectedProfile;
            try
            {
                await MainWindowViewModel.CreateSimpleDownload("Download.Text.DeleteProfile", "Failed to delete profile", async (d, p, c) =>
                {
                    await profile.DeleteModConfigAsync(game.ModDatabase, d.CreateProgress());
                }).RunAsync(ViewModel.MainWindowViewModel);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to delete profile: {profile.Name} - {profile.FileName}");
                Logger.Error(ex);
            }

            int pos = ViewModel.MainWindowViewModel.Profiles.IndexOf(ViewModel.SelectedProfile);
            if (pos == ViewModel.MainWindowViewModel.Profiles.Count - 1)
                pos -= 1;
            ViewModel.MainWindowViewModel.Profiles.Remove(ViewModel.SelectedProfile);
            ViewModel.SelectedProfile = ViewModel.MainWindowViewModel.Profiles[pos];
            ViewModel.Update();
            await ViewModel.MainWindowViewModel.SaveProfilesAsync(game);
            messageBox.Close();
        });
        messageBox.SetDanger();
        messageBox.Open(ViewModel.MainWindowViewModel);
    }

    private void OnRenameClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainWindowViewModel == null || ViewModel.SelectedProfile == null)
            return;

        var modal = new ProfileRenameModal(ViewModel.SelectedProfile, false);
        modal.OnConfirm += async (s, e) =>
        {
            ProfileListBox.Bind(ListBox.ItemsSourceProperty, new Binding("Profiles"));
        };
        modal.Open(ViewModel.MainWindowViewModel);
        ViewModel.Update();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ViewModel.Update();
    }

    private void OnDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (ViewModel.MainWindowViewModel == null || ViewModel.SelectedProfile == null)
            return;

        ViewModel.MainWindowViewModel.SelectedProfile = ViewModel.SelectedProfile;
        ViewModel.Update();
    }

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.IsBusy))
            ViewModel?.Update();
    }
}