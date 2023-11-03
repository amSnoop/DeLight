using Avalonia.Controls;
using DeLight.ViewModels;
using System;
using DeLight.Utilities;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using static DeLight.ViewModels.CueListContextMenuButtonClickedEventArgs;

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
            //DefaultCueDisplay.DataContext = GlobalSettings.Instance.DefaultCue;
            SettingsDisplay.DataContext = GlobalSettings.Instance;
            CueList.SelectionChanged += CueList_SelectionChanged;
            ActiveInfoView.DataContext = new CueInfoViewModel(null, "active");
            SelectedInfoView.DataContext = new CueInfoViewModel(null, "selected");
            ActiveInfoView.EditButtonClicked += Info_EditButtonClicked;
            SelectedInfoView.EditButtonClicked += Info_EditButtonClicked;
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
        }

        private void Info_EditButtonClicked(object? sender, EditButtonClickedEventArgs e)
        {
            if (e.Cue != null)
            {
                var cueEditorViewModel = new CueEditorViewModel(e.Cue, (cue, useLetters) =>
                {
                    CueEditorWindow.IsVisible = false;
                    if(e.Cue.Number == cue.Number)
                        (DataContext as MainWindowViewModel)?.UpdateCue(cue, useLetters);
                    else
                        (DataContext as MainWindowViewModel)?.InsertCue(cue, useLetters);
                });
                CueEditorWindow.DataContext = cueEditorViewModel;
                CueEditorWindow.IsVisible = true;

            }
        }

        public void CueListAddButtonClicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel)
            {
                var cueEditorViewModel = new CueEditorViewModel(new(), (cue, useLetters) =>
                {
                    CueEditorWindow.IsVisible = false;
                    (DataContext as MainWindowViewModel)?.InsertCue(cue, useLetters);
                });
                CueEditorWindow.DataContext = cueEditorViewModel;
                CueEditorWindow.IsVisible = true;
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
            if (CueEditorWindow.IsVisible)
            {
                if (!TryCloseCueEditor())
                {
                    CueList.SelectionChanged -= CueList_SelectionChanged;
                    CueList.SelectedItem = e.RemovedItems[0];
                    CueList.SelectionChanged += CueList_SelectionChanged;
                }
                else
                { 
                    var cueEditorViewModel = new CueEditorViewModel((CueList.SelectedItem as CueListCueViewModel)?.Cue ?? new(), (cue, useLetters) =>
                    {
                        CueEditorWindow.IsVisible = false;
                        (DataContext as MainWindowViewModel)?.InsertCue(cue, useLetters);
                    });
                    CueEditorWindow.DataContext = cueEditorViewModel;
                    CueEditorWindow.IsVisible = true;
                    Focus();
                }
            }
            SelectedInfoView.DataContext = new CueInfoViewModel((CueList.SelectedItem as CueListCueViewModel)?.Cue, "selected");
        }

        public bool TryCloseCueEditor()
        {
            if (CueEditorWindow.DataContext is CueEditorViewModel cevm)
            {
                if (cevm.IsSaved)
                    return true;

                var result = System.Windows.MessageBox.Show("You have unsaved changes. Would you like to save them?", "Unsaved Changes", System.Windows.MessageBoxButton.YesNoCancel);
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    cevm.Save();
                    return true;
                }
                else if (result == System.Windows.MessageBoxResult.No)
                    return true;
                else
                    return false;
            }
            return true;
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
        }
        public void MainWindow_MouseDown(object? sender, PointerPressedEventArgs e)
        {
            GetTopLevel(this)?.FocusManager?.ClearFocus();
            Activate();
            Focus();
            if (CueEditorWindow.IsVisible)
            {
                var currentPointRelativeToCueEditor = e.GetCurrentPoint(CueEditorWindow.ActualControl).Position;
                var currentPointRelativeToCueList = e.GetCurrentPoint(CueList).Position;

                if (IsPointOutsideControl(currentPointRelativeToCueEditor, CueEditorWindow.ActualControl) &&
                    IsPointOutsideControl(currentPointRelativeToCueList, CueList))
                {
                    if (TryCloseCueEditor())
                    {
                        CueEditorWindow.IsVisible = false;
                        Focus();
                    }
                    else
                        CueEditorWindow.IsVisible = true;
                }
            }
        }
        private bool IsPointOutsideControl(Point point, Control control)
        {
            var controlBounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            return !controlBounds.Contains(point);
        }

        protected override void OnClosed(EventArgs e)
        {
            GlobalSettings.Instance.LastScreenTop = Position.Y;
            GlobalSettings.Instance.LastScreenLeft = Position.X;
            GlobalSettings.Instance.WindowState = WindowState;
            GlobalSettings.Save();
            base.OnClosed(e);
            (DataContext as MainWindowViewModel)?.HideVideoWindow();

        }
        protected void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)

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
                vm.PlayCue();
                ActiveInfoView.DataContext = new CueInfoViewModel(vm.CuePlaybackViewModel?.Cue, "active");
                e.Handled = true;
            }
            else
                count = 0;
        }

        //private void Play_Button_Clicked(object sender, RoutedEventArgs e)
        //{
        //    if (DataContext is MainWindowViewModel viewModel)
        //        viewModel.Play();
        //}
        //private void Stop_Button_Clicked(object sender, RoutedEventArgs e)
        //{
        //    if (DataContext is MainWindowViewModel viewModel)
        //        viewModel.Stop();
        //}

        private void MainWindow_OnSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.UpdateWindowSize(Bounds.Height);
            }
        }
    }
}