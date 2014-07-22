/*! 2 !*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Configuration;

namespace Slayer
{
  class SlayerVisualEngine
  {
    private const double MinimumButtonWidth = 70;
    private Window Window;
    private Border MainBorder { get; set; }

    private Brush BackgroundBrush = Brushes.White;
    private Brush ButtonBorderBrush = Brushes.DarkGray;
    private Brush ButtonBackground = Brushes.WhiteSmoke;
    private Brush ButtonForeground = Brushes.OrangeRed;

    public Application Application { get; set; }
    public List<Process> ProcessList { get; set; }

    public SlayerVisualEngine(Window Window)
    {
      this.Window = Window;

      Window.FontFamily = new FontFamily("Calibri");
      Window.FontSize = 13;
      Window.Width = 400;
      Window.Height = 470;
      Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      Window.Background = BackgroundBrush;
      Window.KeyUp += (object sender, System.Windows.Input.KeyEventArgs Event) =>
      {
        if (Event.Key == Key.Escape)
          Window.Close();
      };

      MainBorder = new Border();
      Window.Content = MainBorder;
      MainBorder.Padding = new Thickness(4);
      MainBorder.Background = Brushes.White;
    }

    public void Install()
    {
      ProcessList.Sort(SortByStartTime);

      var AssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
      Window.Title = String.Format("{0} v{1}", AssemblyName.Name, AssemblyName.Version.ToString().TrimEnd('.', '0'));
      
      var IconUri = new Uri("pack://application:,,,/Slayer;component/Images/Close-128.ico"); // File needs to be set as a resource in it's properties
      Window.Icon = new System.Windows.Media.Imaging.BitmapImage(IconUri);
      
      var MainGrid = new Grid();
      MainBorder.Child = MainGrid;
      MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1,GridUnitType.Star) });
      MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
      MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
      
      var ProcessBorder = new Border();
      MainGrid.Children.Add(ProcessBorder);
      Grid.SetRow(ProcessBorder, 0);
      
      var ProcessVisualEngine = new ProcessVisualEngine();
      ProcessVisualEngine.Application = Application;
      ProcessVisualEngine.MainBorder = ProcessBorder;
      ProcessVisualEngine.ProcessList = ProcessList;
      ProcessVisualEngine.Install();

      var ButtonBorder = new Border();
      MainGrid.Children.Add(ButtonBorder);
      ButtonBorder.Background = ButtonBorderBrush;
      ButtonBorder.Padding = new Thickness(0, 5, 0, 5);
      ButtonBorder.VerticalAlignment = VerticalAlignment.Bottom;

      var ButtonStackPanel = new StackPanel();
      ButtonBorder.Child = ButtonStackPanel;
      Grid.SetRow(ButtonStackPanel, 1);
      ButtonStackPanel.Orientation = Orientation.Horizontal;
      ButtonStackPanel.Margin = new Thickness(5);
      ButtonStackPanel.Height = 30;
      ButtonStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
      
      var KillAllButton = NewButton("Kill All");
      ButtonStackPanel.Children.Add(KillAllButton);
      KillAllButton.Click += (object sender, RoutedEventArgs e) =>
      {
        ProcessList.ForEach(Process => Process.Kill());
        Application.Shutdown();
      };

      var KillOldestButton = NewButton("Kill Oldest");
      ButtonStackPanel.Children.Add(KillOldestButton);
      KillOldestButton.Click += (object sender, RoutedEventArgs e) =>
      {
        var OldestProcess = ProcessList.First();

        foreach (Process Process in ProcessList)
        {
          if (Process.StartTime < OldestProcess.StartTime)
            OldestProcess = Process;
        }

        OldestProcess.Kill();
        Application.Shutdown();
      };

      var KillYoungestButton = NewButton("Kill Youngest");
      ButtonStackPanel.Children.Add(KillYoungestButton);
      KillYoungestButton.Click += (object sender, RoutedEventArgs e) =>
      {
        var YoungestProcess = ProcessList.First();

        foreach (Process Process in ProcessList)
        {
          if (Process.StartTime > YoungestProcess.StartTime)
            YoungestProcess = Process;
        }

        YoungestProcess.Kill();
        Application.Shutdown();
      };
    }

    private Button NewButton(string Caption)
    {
      var Result = new Button();
      Result.Content = Caption;
      Result.VerticalAlignment = VerticalAlignment.Bottom;
      Result.Margin = new Thickness(0, 0, 7, 0);
      Result.Padding = new Thickness(5);
      Result.Background = ButtonBackground;
      Result.Foreground = ButtonForeground;
      Result.FontSize = 15;
      Result.MinWidth = MinimumButtonWidth;

      return Result;
    }
    private int SortByStartTime(Process Process1, Process Process2)
    {
      if (Process1 == null)
      {
        if (Process2 == null)
          return 0;
        else
          return -1;
      }
      else
      {
        if (Process2 == null) // ...and process2 is null, process1 is greater.
          return 1;
        else
          return Process1.StartTime.CompareTo(Process2.StartTime);
      }
    }
  }
}
