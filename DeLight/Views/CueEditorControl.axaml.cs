using Avalonia;
using Avalonia.Controls;
using DeLight.ViewModels;

namespace DeLight.Views
{
    public partial class CueEditorControl : UserControl
    {
        public CueEditorControl()
        {
            InitializeComponent();
            Volume.PropertyChanged += Property_Changed;
            DurationString.PropertyChanged += Property_Changed;
            Number.PropertyChanged += Property_Changed;
        }

        public void Property_Changed(object? sender, AvaloniaPropertyChangedEventArgs e) {
            if (DataContext is CueEditorViewModel cevm && sender is TextBox tb && e.Property.Name == nameof(TextBox.Text)) {
                bool valid = true;
                var txt = tb.Text;
                if(tb.Name == "Number")
                {
                    valid = int.TryParse(txt, out int number);
                    if (valid)
                        valid = cevm.Validate(tb.Name, number);
                }
                else if (tb.Name == "DurationString")
                {
                    valid = double.TryParse(txt, out double number);
                    if (valid)
                        valid = cevm.Validate(tb.Name, number);
                }
                else if (tb.Name == "Volume")
                {
                    valid = int.TryParse(txt, out int number);
                    if (valid)
                        valid = cevm.Validate(tb.Name, number);
                }
                if (!valid)
                    tb.Classes.Add("Error");
                else
                    tb.Classes.Remove("Error");
            }
        }

    }
}
