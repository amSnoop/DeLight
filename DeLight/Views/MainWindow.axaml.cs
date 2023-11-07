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
                var cueEditorViewModel = new CueEditorViewModel(new() { Number = vm.Cues.Last().Cue?.Number + 1 ?? 0 });
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
        public void CueList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var cueEditorViewModel = new CueEditorViewModel((CueList.SelectedItem as CueListCueViewModel)?.Cue ?? new());
            CueEditorWindow.DataContext = cueEditorViewModel;
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
            else if (e.Key == Avalonia.Input.Key.Space && DataContext is MainWindowViewModel vm && Keyboard.FocusedElement is not TextBox)
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
            if(DataContext is MainWindowViewModel vm && vm.CuePlaybackViewModel != null)
                vm.CuePlaybackViewModel.IsSeeking = true;
        }

        private void Slider_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.CuePlaybackViewModel != null)
                vm.CuePlaybackViewModel.IsSeeking = false;
        }
    }
}