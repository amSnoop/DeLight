using Avalonia.Controls;
using DeLight.ViewModels;
using System;
using DeLight.Utilities;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using static DeLight.ViewModels.CueListContextMenuButtonClickedEventArgs;
using System.Linq;
using DeLight.Utilities.VideoOutput;
using Avalonia.Platform.Storage;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DeLight.Views
{
    public partial class MainWindow : Window
    {
        private int count;

        public MainWindow()
        {
            WindowState = WindowState.Normal;
            Position = new(GlobalSettings.Instance.LastScreenLeft, GlobalSettings.Instance.LastScreenTop);
            InitializeComponent();
            SettingsDisplay.DataContext = GlobalSettings.Instance;
            CueList.SelectionChanged += CueList_SelectionChanged;
            AddCueButton.Click += CueListAddButtonClicked;
            KeyDown += OnKeyDown;
            SizeChanged += MainWindow_OnSizeChanged;
            PointerPressed += MainWindow_MouseDown;
            Loaded += MainWindow_Loaded;
            CueList.Focusable = false;
            if (Design.IsDesignMode)
            {
                DataContext = new MainWindowViewModel(new ShowRunner(Models.Show.LoadTestShow()));
            }
            PlayBackSlider.AddHandler(PointerPressedEvent, Slider_PointerPressed, RoutingStrategies.Tunnel);
            PlayBackSlider.AddHandler(PointerReleasedEvent, Slider_PointerReleased, RoutingStrategies.Tunnel);
        }

        private void Info_EditButtonClicked(object? sender, EditButtonClickedEventArgs e)
        {
            if (e.Cue != null)
            {
                var cueEditorViewModel = new CueEditorViewModel(e.Cue);
                CueEditorWindow.DataContext = cueEditorViewModel;

            }
        }

        public void CueListAddButtonClicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                var cueEditorViewModel = new CueEditorViewModel(new() { Number = vm.Cues.LastOrDefault()?.Cue?.Number + 1 ?? 0 });
                CueEditorWindow.DataContext = cueEditorViewModel;
                vm.InsertCue(cueEditorViewModel.Cue);
            }
        }

        public void CueListContextMenuClicked(object? sender, CueListContextMenuButtonClickedEventArgs e)
        {
            if (DataContext is MainWindowViewModel && sender is not null && sender is CueListCueViewModel clcvm)
            {
                if (e.Action == CueListContextMenuButton.Edit)
                {
                    Info_EditButtonClicked(this, new(clcvm.Cue));
                }
                else if (e.Action == CueListContextMenuButton.Delete)
                {
                    var result = System.Windows.MessageBox.Show("Are you sure you want to delete this cue?", "Delete Cue", System.Windows.MessageBoxButton.YesNo);
                    if (result == System.Windows.MessageBoxResult.Yes)
                        (DataContext as MainWindowViewModel)?.DeleteCue(clcvm.Cue);
                }
            }
        }
        public async void CueList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var cueEditorViewModel = new CueEditorViewModel((CueList.SelectedItem as CueListCueViewModel)?.Cue);
            CueEditorWindow.DataContext = cueEditorViewModel;
            await VideoManager.PrepareCue(cueEditorViewModel.Cue);
            Focus();
        }

        public void MainWindow_Loaded(object? sender, EventArgs e)
        {
            WindowState = GlobalSettings.Instance.WindowState;
            PlaybackBar.DataContext = (DataContext as MainWindowViewModel)?.CuePlaybackViewModel;
            if (DataContext is MainWindowViewModel)
                foreach (var cue in (DataContext as MainWindowViewModel)!.Cues)
                {
                    cue.ButtonClicked += CueListContextMenuClicked;
                }
            VideoManager.HideVideoWindow();
        }
        public void MainWindow_MouseDown(object? sender, PointerPressedEventArgs e)
        {
            GetTopLevel(this)?.FocusManager?.ClearFocus();
            Activate();
            Focus();
        }
        //unused atm
        //private bool IsPointOutsideControl(Point point, Control control)
        //{
        //    var controlBounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
        //    return !controlBounds.Contains(point);
        //}

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            GlobalSettings.Instance.WindowState = WindowState;
            WindowState = WindowState.Normal;
            GlobalSettings.Instance.LastScreenTop = Position.Y;
            GlobalSettings.Instance.LastScreenLeft = Position.X;
            GlobalSettings.Save();
            base.OnClosing(e);
            (DataContext as MainWindowViewModel)?.HideVideoWindow();
        }
        protected async void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)

        {
            base.OnKeyDown(e);
            if (e.Key == Avalonia.Input.Key.Escape && Keyboard.FocusedElement is not TextBox)
            {
                count++;
                if (count == 2)
                {
                    (DataContext as MainWindowViewModel)?.HideVideoWindow();
                    count = 0;
                }
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Space && DataContext is MainWindowViewModel vm && FocusManager!.GetFocusedElement() is not TextBox)
            {
                await vm.PlayCue();
                e.Handled = true;
            }
            else
                count = 0;
        }

        public void Play_Button_Clicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.ResumeCue();
        }
        public void Stop_Button_Clicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.StopCue();
        }
        public void Pause_Button_Clicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.PauseCue();
        }

        private void MainWindow_OnSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.UpdateWindowSize(Bounds.Height);
            }
        }

        private void Slider_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.CuePlaybackViewModel != null)
                vm.CuePlaybackViewModel.IsSeeking = true;
        }

        private void Slider_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.CuePlaybackViewModel != null)
                vm.CuePlaybackViewModel.IsSeeking = false;
        }

        private void Save_Button_Clicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
                vm.SaveShow();
        }
        private async void Load_Button_Clicked(object? sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open DeLight File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("DeLight Shows") { Patterns = new[] {"*.dlt" } }
                },
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(GlobalSettings.Instance.LastShowPath))
            });

            if (files.Count >= 1 && files[0].TryGetLocalPath() is not null)
            {
                ((MainWindowViewModel)DataContext!).LoadShow(files[0].TryGetLocalPath()!);
            }
        }
        private async void New_Button_Clicked(object? sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);

            var save = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save DeLight File",
                SuggestedFileName = "New Show.dlt",
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(GlobalSettings.Instance.LastShowPath)),
            });
            if (save is not null && save.TryGetLocalPath() is not null)
                ((MainWindowViewModel)DataContext!).NewShow(save.TryGetLocalPath()!);
        }
    }
}