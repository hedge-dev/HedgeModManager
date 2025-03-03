using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Immutable;
using HedgeModManager.UI.Controls.Basic;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Mods;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ModConfigModal : UserControl
{
    public ModEntryViewModel ModViewModel { get; set; }
    public ModConfigViewModel ConfigViewModel { get; set; }

    public string Title { get; set; } = "Common.Text.Loading";

    public string ConfigIniPath = string.Empty;

    // Preview only
    public ModConfigModal()
    {
        ModViewModel = new();
        var mod = (ModGeneric)ModViewModel.Mod;
        ConfigViewModel = new(mod);
        Title = Localize("Modal.Title.ConfigureMod", mod.Title);
        
        AvaloniaXamlLoader.Load(this);
    }

    public ModConfigModal(ModEntryViewModel modViewModel)
    {
        var mod = (ModGeneric)modViewModel.Mod;
        ModViewModel = modViewModel;
        ConfigViewModel = new(mod);
        Title = Localize("Modal.Title.ConfigureMod", mod.Title);

        AvaloniaXamlLoader.Load(this);
    }

    public Control CreateControl(ModConfig.ConfigElement element, ModConfig modConfig)
    {
        switch (element.Type)
        {
            case "bool":
                var checkBox = new Basic.CheckBox();
                checkBox.Bind(Basic.CheckBox.IsCheckedProperty, new Binding("Value")
                {
                    Source = element,
                    Mode = BindingMode.TwoWay
                });
                return checkBox;
            case "string":
                var textBoxStr = new TextBox();
                textBoxStr.Bind(TextBox.TextProperty, new Binding()
                {
                    Source = element.Value,
                    Mode = BindingMode.TwoWay
                });
                return textBoxStr;
            case "float":
            case "int":
                var textBox = new TextBox();
                var validatableElement = textBox.Tag = new ModConfigViewModel.ValidatableConfigElement(element);
                textBox.Bind(TextBox.TextProperty, new Binding("Value")
                {
                    Source = validatableElement,
                    Mode = BindingMode.TwoWay
                });
                return textBox;
            default:
                if (modConfig.Enums.TryGetValue(element.Type, out List<ModConfig.ConfigEnum>? value))
                {
                    var selectedValue = value.FirstOrDefault(x => x.Value == element.Value?.ToString());

                    var comboBox = new ComboBox
                    {
                        ItemsSource = value,
                        SelectedItem = selectedValue,
                        Foreground = App.GetStyleResource<ImmutableSolidColorBrush>("ForegroundBrush", this)
                    };
                    comboBox.SelectionChanged += (s, e) =>
                    {
                        if (comboBox.SelectedItem is ModConfig.ConfigEnum configEnum)
                            element.Value = configEnum.Value;
                    };
                    return comboBox;
                }
                return new Avalonia.Controls.TextBlock()
                {
                    Text = "Unknown Type",
                    VerticalAlignment = VerticalAlignment.Center
                };
        }
    }

    private async void OnInitialized(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(ConfigViewModel.Mod.ConfigSchemaFile))
        {
            string jsonPath = Path.Combine(ConfigViewModel.Mod.Root, ConfigViewModel.Mod.ConfigSchemaFile);
            await ConfigViewModel.DeserializeSchema(jsonPath);
            ConfigIniPath = Path.Combine(ConfigViewModel.Mod.Root, ConfigViewModel.Config.IniFile);
            await ConfigViewModel.Config.Load(ConfigIniPath);
        }
        else
        {
            ConfigViewModel.Config.GenerateSchemaExample();
        }

        foreach (var group in ConfigViewModel.Config.Groups)
        {
            var groupBox = new GroupBox()
            {
                Header = group.DisplayName,
                Margin = new Thickness(16, 4, 16, 4)
            };
            var configStackPanel = new StackPanel()
            {
                Margin = new Thickness(4)
            };
            foreach (var element in group.Elements)
            {
                var grid = new Grid()
                {
                    ColumnDefinitions = new ColumnDefinitions("*,*"),
                    Margin = new Thickness(4),
                    Background = App.GetStyleResource<ImmutableSolidColorBrush>("BackgroundL0Brush", this)
                };
                grid.PointerMoved += (s, e) =>
                {
                    ConfigViewModel.Description = string.Join("\n", element.Description);
                };
                grid.Children.Add(new Avalonia.Controls.TextBlock()
                {
                    Text = element.DisplayName,
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    [Grid.ColumnProperty] = 0
                });

                var editControl = CreateControl(element, ConfigViewModel.Config);
                editControl.SetValue(Grid.ColumnProperty, 1);
                editControl.SetValue(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                editControl.SetValue(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Center);
                grid.Children.Add(editControl);
                
                configStackPanel.Children.Add(grid);
            }
            groupBox.Data = configStackPanel;
            FormStackPanel.Children.Add(groupBox);
        }
    }

    public void Open(MainWindowViewModel viewModel)
    {
        viewModel.Modals.Add(new Modal(this, new Thickness(0)));
    }

    public void Close()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var modalInstance = viewModel.Modals.FirstOrDefault(x => x.Control == this);
            if (modalInstance != null)
                viewModel.Modals.Remove(modalInstance);
        }
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            bool isWindows = viewModel.SelectedGame != null &&
                viewModel.SelectedGame.Game.NativeOS == "Windows";
            if (!string.IsNullOrEmpty(ConfigIniPath))
                await ConfigViewModel.Config.Save(ConfigIniPath, isWindows);
        }

        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}