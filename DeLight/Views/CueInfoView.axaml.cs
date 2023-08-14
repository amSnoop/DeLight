using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using DeLight.Interfaces;
using DeLight.Models;
using DeLight.Models.Files;
using DeLight.ViewModels;
using System;
using System.Linq;

namespace DeLight.Views
{
    public class EditButtonClickedEventArgs : EventArgs
    {
        public Cue? Cue { get; set; }

        public EditButtonClickedEventArgs(Cue? cue)
        {
            Cue = cue;
        }
    }

    public partial class CueInfoView : UserControl
    {

        public event EventHandler<EditButtonClickedEventArgs>? EditButtonClicked;
        public CueInfoView()
        {
            InitializeComponent();
            RecalculateGridChildren();
            DataContextChanged += CueInfoView_DataContextChanged;
            EditButton.Click += EditButton_Click;
        }

        private void EditButton_Click(object? sender, RoutedEventArgs e)
        {
            if(DataContext is CueInfoViewModel vm)
            {
                EditButtonClicked?.Invoke(this, new(vm.Cue));
            }
        }

        public void CueInfoView_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is CueInfoViewModel)
            {
                RecalculateGridChildren();
            }
        }

        public void RecalculateGridChildren()
        {
            FileViewGrid.Children.Clear();
            FileViewGrid.RowDefinitions.Clear();
            if(DataContext is CueInfoViewModel vm)
            {
                if (vm.Cue is null)
                {
                    return;
                }
                int count = 0;
                foreach(var file in vm.CueFiles.ToList())
                {
                    FileViewGrid.RowDefinitions.Add(new(GridLength.Auto));
                    Border preBorder = new();
                    SetBorderOptions(preBorder);
                    TextBlock preTextBlock = new();
                    string pretext = file switch
                    {
                        LightFile => "Lights: ",
                        VideoFile => "Video: ",
                        AudioFile => "Audio: ",
                        ImageFile => "Image: ",
                        GifFile => "Gif: ",
                        _ => "Screen: "
                    };
                    preTextBlock.Text = pretext;
                    preBorder.Child = preTextBlock;
                    FileViewGrid.Children.Add(preBorder);
                    Grid.SetRow(preBorder, count);
                    Grid.SetColumn(preBorder, 0);
                    Border filePathBorder = new();
                    SetBorderOptions(filePathBorder);
                    TextBlock filePathBlock = new();
                    if(file is IBlackoutFile)
                    {
                        filePathBlock.Text = "None";
                    }
                    else
                    {
                      filePathBlock.Text = file.FilePath;
                    }
                    if (file.ErrorState != FileErrorState.None)
                    {
                        filePathBlock.Classes.Add("error");
                        preTextBlock.Classes.Add("error");
                        preBorder.Classes.Add("error");
                        filePathBorder.Classes.Add("error");
                    }
                    filePathBorder.Child = filePathBlock;
                    FileViewGrid.Children.Add(filePathBorder);
                    Grid.SetRow(filePathBorder, count);
                    Grid.SetColumn(filePathBorder, 1);
                    count++;
                }

            }
        }

        public void SetBorderOptions(Border border)
        {
            border.BorderBrush = Application.Current?.Resources["ForegroundBrush"] as SolidColorBrush;
            border.BorderThickness = new(0,0,0,1);
            border.Padding = new(3);
        }
    }
}
