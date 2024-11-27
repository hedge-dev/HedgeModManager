using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Models;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace HedgeModManager.UI.ViewModels.Mods;

public partial class ModConfigViewModel : ViewModelBase
{
    public ModGeneric Mod;
    public ModConfig Config = new();

    [ObservableProperty] private string _description = "";

    public ModConfigViewModel(ModGeneric mod)
    {
        Mod = mod;
    }

    public async Task DeserializeSchema(string? jsonPath)
    {
        if (!File.Exists(jsonPath))
            return;

        string jsonData = await File.ReadAllTextAsync(jsonPath);

        Config = JsonSerializer.Deserialize<ModConfig>(jsonData, Program.JsonSerializerOptions) ?? new();
    }

    public partial class ValidatableConfigElement : ObservableObject, INotifyDataErrorInfo
    {
        public ModConfig.ConfigElement Element;
        public string? ErrorText { get; set; } = null;

        [ObservableProperty] private string _value = string.Empty;

        public ValidatableConfigElement(ModConfig.ConfigElement element)
        {
            Element = element;
            var value = element.Value.ToString();
            if (value != null)
                Value = value;
        }

        public bool HasErrors => throw new NotImplementedException();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            return propertyName switch
            {
                nameof(Value) => [ErrorText],
                _ => Array.Empty<string?>(),
            };
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Value))
            {
                switch (Element.Type)
                {
                    case "bool":
                        if (bool.TryParse(Value, out bool valBool))
                        {
                            Element.Value = valBool;
                            ErrorText = null;
                        }
                        else
                            ErrorText = "Invalid value";
                        break;
                    case "string":
                        break;
                    case "float":
                        if (float.TryParse(Value, out float valFloat))
                        {
                            Element.Value = valFloat;
                            ErrorText = null;
                        }
                        else
                            ErrorText = "Invalid value";
                        break;
                    case "int":
                        if (int.TryParse(Value, out int valInt))
                        {
                            Element.Value = valInt;
                            ErrorText = null;
                        }
                        else
                            ErrorText = "Invalid value";
                        break;
                    default:
                        break;
                }
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(e.PropertyName));
            }
            base.OnPropertyChanged(e);
        }
    }
}
