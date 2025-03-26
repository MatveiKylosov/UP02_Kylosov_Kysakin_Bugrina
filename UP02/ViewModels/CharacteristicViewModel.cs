using System.ComponentModel;

namespace UP02.ViewModels
{
    public class CharacteristicViewModel : INotifyPropertyChanged
    {
        public CharacteristicViewModel(int characteristicID, int? characteristicsValueID, int? consumablesID, string? characteristicName, string? value)
        {
            CharacteristicID = characteristicID;
            CharacteristicsValueID = characteristicsValueID;
            ConsumablesID = consumablesID;

            _characteristicName = characteristicName;
            _value = value;
            _originalValue = value;
        }

        public int CharacteristicID { get; set; }
        public int? CharacteristicsValueID { get; set; }
        public int? ConsumablesID { get; set; }

        private string? _characteristicName;
        public string? CharacteristicName
        {
            get => _characteristicName;
            set
            {
                if (_characteristicName != value)
                {
                    _characteristicName = value;
                    OnPropertyChanged(nameof(CharacteristicName));
                }
            }
        }


        private string? _originalValue;
        private string? _value;
        public string? Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public bool ValueChanged { get { return  _value != _originalValue; } }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
