using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgeModManager.UI.Models
{
    public partial class UIGame : ObservableObject
    {
        // TODO
        [ObservableProperty] private IModdableGame _game;
        [ObservableProperty] private IImage _icon;
        public string TranslatedName => Game.Name;

        public UIGame(IModdableGame game, IImage icon)
        {
            _game = game;
            _icon = icon;
        }
    }
}
