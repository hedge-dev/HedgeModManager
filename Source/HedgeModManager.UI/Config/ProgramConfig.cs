using Avalonia.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.ViewModels;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace HedgeModManager.UI.Config
{
    public partial class ProgramConfig : ViewModelBase
    {
        [ObservableProperty] private bool _isSetupCompleted = false;
        [ObservableProperty] private bool _testModeEnabled = true;

        public async Task LoadAsync()
        {
            string filePath = "config.json";
            if (!File.Exists(filePath))
                return;

            string jsonData = await File.ReadAllTextAsync(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<ProgramConfig>(jsonData, options);

            // Copy data
            if (config != null)
            {
                foreach (var property in GetType().GetProperties())
                    if (property.CanWrite)
                        property.SetValue(this, property.GetValue(config));
            }
        }

        public async Task SaveAsync()
        {
            string filePath = "config.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonData = JsonSerializer.Serialize(this, options);
            try
            {
                await File.WriteAllTextAsync(filePath, jsonData);
            }
            catch
            {
                // TODO: Log error
            }
        }

        public void Reset()
        {
            var config = new ProgramConfig();
            foreach (var property in GetType().GetProperties())
                if (property.CanWrite)
                    property.SetValue(this, property.GetValue(config));
        }
    }
}
