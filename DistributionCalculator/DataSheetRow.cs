using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DistributionCalculator
{
    internal class DataSheetRow : INotifyPropertyChanged
    {

        private ObservableCollection<double> _data;
        public ObservableCollection<double> Data
        {
            get => _data;
            set
            {
                if (_data != value)
                {
                    _data = value;
                    OnPropertyChanged(nameof(Data));
                }
            }
        }

        public DataSheetRow()
        {
            Data = new(new double[11]);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
        private void OnPropertyChanged(string PropertyName) => OnPropertyChanged(new PropertyChangedEventArgs(PropertyName));
    }
}
