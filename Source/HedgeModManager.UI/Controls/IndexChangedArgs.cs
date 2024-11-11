using Avalonia.Interactivity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgeModManager.UI.Controls
{
    public class IndexChangedArgs : RoutedEventArgs
    {
        public IndexChangedArgs(RoutedEvent routedEvent, int oldIndex, int newIndex)
            : base(routedEvent)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }

        public int OldIndex { get; }
        public int NewIndex { get; }
    }
}
