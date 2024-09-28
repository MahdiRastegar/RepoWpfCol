using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public class ComboboxViewModel : INotifyPropertyChanged
    {

        public ObservableCollection<FilterElement> People { get; set; }

        private FilterElement _selectedFilterElement;
        public FilterElement SelectedFilterElement
        {
            get { return _selectedFilterElement; }
            set
            {
                if (_selectedFilterElement != value)
                {
                    _selectedFilterElement = value;
                    OnPropertyChanged("SelectedFilterElement");
                }
            }
        }

        public ComboboxViewModel()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}