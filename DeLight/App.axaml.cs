using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DeLight.Models;
using DeLight.Utilities;
using DeLight.Utilities.LightingOutput;
using DeLight.Utilities.VideoOutput;
using DeLight.ViewModels;
using DeLight.Views;

namespace DeLight
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                GlobalSettings.Load();
                ShowRunner show = new(Show.Load(GlobalSettings.Instance.LastShowPath));
                LightingController.Start();
                desktop.MainWindow = new MainWindow() 
                {
                    DataContext = new MainWindowViewModel(show)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}