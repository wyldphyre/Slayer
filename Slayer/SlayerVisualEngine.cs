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
    public Application Application { get; set; }
    public Border MainBorder { get; set; }
    public List<Process> ProcessList { get; set; }

    private const double MinimumButtonWidth = 70;

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

    public void Install()
    {
      ProcessList.Sort(SortByStartTime);

      MainBorder.BorderBrush = Brushes.LightGray;
      MainBorder.BorderThickness = new Thickness(1, 1, 2, 2);
      MainBorder.Padding = new Thickness(4);
      MainBorder.CornerRadius = new CornerRadius(10);
      MainBorder.Background = Brushes.LightGray;

      var BackgroundGradientBrush = new LinearGradientBrush(Colors.Snow, Colors.Wheat, 45);
      MainBorder.Background = BackgroundGradientBrush;
      
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
      
      var ButtonStackPanel = new StackPanel();
      MainGrid.Children.Add(ButtonStackPanel);
      Grid.SetRow(ButtonStackPanel, 1);
      ButtonStackPanel.Orientation = Orientation.Horizontal;
      ButtonStackPanel.Margin = new Thickness(5);
      ButtonStackPanel.Height = 30;
      ButtonStackPanel.VerticalAlignment = VerticalAlignment.Bottom;
      
      var KillAllButton = new Button();
      ButtonStackPanel.Children.Add(KillAllButton);
      KillAllButton.Content = "Kill All";
      KillAllButton.VerticalAlignment = VerticalAlignment.Bottom;
      KillAllButton.Margin = new Thickness(0, 0, 5, 0);
      KillAllButton.MinWidth = MinimumButtonWidth;
      KillAllButton.Click += (object sender, RoutedEventArgs e) =>
      {
        foreach (Process Process in ProcessList)
        {
          Process.Kill();
        }

        Application.Shutdown();
      };

      var KillOldestButton = new Button();
      ButtonStackPanel.Children.Add(KillOldestButton);
      KillOldestButton.Content = "Kill Oldest";
      KillOldestButton.VerticalAlignment = VerticalAlignment.Bottom;
      KillOldestButton.Margin = new Thickness(0, 0, 5, 0);
      KillOldestButton.MinWidth = MinimumButtonWidth;
      KillOldestButton.Click += (object sender, RoutedEventArgs e) =>
        {
          Process OldestProcess = ProcessList.First();

          foreach (Process Process in ProcessList)
          {
            if (Process.StartTime < OldestProcess.StartTime)
            {
              OldestProcess = Process;
            }
          }

          OldestProcess.Kill();
          Application.Shutdown();
        };

      var KillYoungestButton = new Button();
      ButtonStackPanel.Children.Add(KillYoungestButton);
      KillYoungestButton.Content = "Kill Youngest";
      KillYoungestButton.VerticalAlignment = VerticalAlignment.Bottom;
      KillYoungestButton.Margin = new Thickness(0, 0, 5, 0);
      KillYoungestButton.MinWidth = MinimumButtonWidth;
      KillYoungestButton.Click += (object sender, RoutedEventArgs e) =>
      {
        Process YoungestProcess = ProcessList.First();

        foreach (Process Process in ProcessList)
        {
          if (Process.StartTime > YoungestProcess.StartTime)
          {
            YoungestProcess = Process;
          }
        }

        YoungestProcess.Kill();
        Application.Shutdown();
      };
    }
  }
}
