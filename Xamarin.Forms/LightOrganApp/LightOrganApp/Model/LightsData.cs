using System.ComponentModel;
using Xamarin.Forms;

namespace LightOrganApp.Model
{
    public class LightsData: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _bassColor;
        public Color BassColor
        {
            get { return _bassColor; }
            set
            {
                _bassColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BassColor"));
            }
        }

        private Color _midColor;
        public Color MidColor
        {
            get { return _midColor; }
            set
            {
                _midColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MidColor"));
            }
        }

        private Color _trebleColor;
        public Color TrebleColor
        {
            get { return _trebleColor; }
            set
            {
                _trebleColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TrebleColor"));
            }
        }
    }
}
