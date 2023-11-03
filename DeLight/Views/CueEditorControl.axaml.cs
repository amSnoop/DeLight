using Avalonia.Controls;
using Avalonia.Interactivity;
using DeLight.ViewModels;

namespace DeLight.Views
{
    public partial class CueEditorControl : UserControl
    {
        public CueEditorControl()
        {
            InitializeComponent();
        }

        public void TBLostFocus(object? sender, RoutedEventArgs e) {
            if (DataContext is CueEditorViewModel cevm) {
                bool b = double.TryParse((sender as TextBox)?.Text, out double i) ;
                if (b) {
                    cevm.Volume = i;
                }
            }
        }
    }
}
