using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using HedgeModManager.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HedgeModManager.UI.ViewModels.Mods
{
    public partial class ModEntryViewModel : ViewModelBase
    {
        [ObservableProperty] private IMod _mod;
        [ObservableProperty] private bool _isVisible = true;
        [ObservableProperty] private InlineCollection _modTitle = new ();
        [ObservableProperty] private InlineCollection _modAuthor = new ();

        private Regex? _search;

        public Regex? Search
        {
            get => _search;
            set
            {
                _search = value;
                UpdateSearch();
            }
        }

        private bool _isFiltered;

        public bool IsFiltered
        {
            get => _isFiltered;
            set
            {
                _isFiltered = value;
                UpdateSearch();
            }
        }


        public bool ModEnabled
        {
            get => Mod.Enabled;
            set
            {
                Mod.Enabled = value;
                OnPropertyChanged(nameof(ModEnabled));
            }
        }

        public string Authors => string.Join(", ", Mod.Authors.Select(x => x.Name));

        public ModEntryViewModel(IMod mod)
        {
            Mod = mod;
        }

        public void UpdateSearch()
        {
            bool updateInlines(InlineCollection inlines, Regex? regex, string str)
            {
                inlines.Clear();

                if (regex == null)
                {
                    inlines.Add(new Run(str));
                    return true;
                }
                var matches = regex.Matches(str);

                var lastIndex = 0;
                foreach (Match match in matches)
                {
                    if (match.Index > lastIndex)
                        inlines.Add(new Run(str[lastIndex..match.Index]));

                    inlines.Add(new Run(str[match.Index..(match.Index + match.Length)])
                    {
                        FontWeight = FontWeight.Bold
                    });
                    lastIndex = match.Index + match.Length;
                }
                if (lastIndex < str.Length)
                    inlines.Add(new Run(str[lastIndex..]));

                return matches.Count > 0;
            }

            Dispatcher.UIThread.Post(() =>
            {
                bool hasMatch = false;
                hasMatch |= updateInlines(ModTitle, Search, Mod.Title);
                hasMatch |= updateInlines(ModAuthor, Search, Authors);
                IsVisible = hasMatch && !IsFiltered;
            });
        }

    }
}
