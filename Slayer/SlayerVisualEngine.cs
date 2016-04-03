﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Text;

namespace Slayer
{
  interface ISlayerVisualEngine
  {
    Theme Theme { get; set; }
    Application Application { get; set; }
    List<Process> ProcessList { get; set; }
    event Action KillAllEvent;
    event Action KillOldestEvent;
    event Action KillYoungestEvent;
    event Action<Process> ProcessShowMeEvent;
    event Action<Process> ProcessKillMeEvent;
    event Action<Process> ProcessKillOthersEvent;
    void RemoveProcess(Process Process);
    void Install(Window Window);
  }

  class SlayerVisualEngine : ISlayerVisualEngine
  {
    private const double MinimumButtonWidth = 70;
    private const double MinimumProcessButtonWidth = 75;
    private Window Window;
    private Border MainBorder { get; set; }

    private ScrollViewer ProcessesScrollViewer;

    public Theme Theme { get; set; }
    public Application Application { get; set; }
    public List<Process> ProcessList { get; set; }
    public event Action KillAllEvent;
    public event Action KillOldestEvent;
    public event Action KillYoungestEvent;
    public event Action<Process> ProcessShowMeEvent;
    public event Action<Process> ProcessKillMeEvent;
    public event Action<Process> ProcessKillOthersEvent;

    public SlayerVisualEngine()
    {
    }

    public void Install(Window Window)
    {
      this.Window = Window;

      Window.FontFamily = new System.Windows.Media.FontFamily("Calibri");
      Window.FontSize = 13;
      Window.Width = 400;
      Window.Height = 470;
      Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      Window.KeyUp += (object sender, System.Windows.Input.KeyEventArgs Event) =>
      {
        if (Event.Key == System.Windows.Input.Key.Escape)
          Window.Close();
      };

      MainBorder = new Border
      {
        Padding = new Thickness(4),
        Background = Theme.ApplicationBackground
      };
      Window.Content = MainBorder;

      ProcessList = ProcessList.OrderBy(Process => Process.StartTime).ToList();

      Window.Title = String.Format("{0} v{1}", AssemblyHelper.ExecutingAssemblyName().Name, AssemblyHelper.CompactExecutingAssemblyVersion());

      var IconUri = new Uri("pack://application:,,,/Slayer;component/Images/Close-128.ico"); // File needs to be set as a resource in it's properties
      Window.Icon = new System.Windows.Media.Imaging.BitmapImage(IconUri);

      var DockPanel = new DockPanel
      {
        LastChildFill = false,
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      MainBorder.Child = DockPanel;

      var ProcessNameBorder = new Border { Background = Theme.ApplicationButtonToolbarBackground };
      DockPanel.Children.Add(ProcessNameBorder);
      DockPanel.SetDock(ProcessNameBorder, Dock.Top);

      var ProcessNameCaption = new Label
      {
        Content = ProcessList.First().ProcessName,
        HorizontalAlignment = HorizontalAlignment.Center,
        FontSize = 20,
        FontWeight = FontWeights.Bold
      };
      ProcessNameBorder.Child = ProcessNameCaption;

      var ButtonBorder = new Border
      {
        Background = Theme.ApplicationButtonToolbarBackground,
        Padding = new Thickness(0, 5, 0, 5)
      };
      DockPanel.Children.Add(ButtonBorder);
      DockPanel.SetDock(ButtonBorder, Dock.Bottom);

      var ButtonStackPanel = new StackPanel()
      {
        Orientation = Orientation.Horizontal,
        Margin = new Thickness(5),
        Height = 30,
        HorizontalAlignment = HorizontalAlignment.Center
      };
      ButtonBorder.Child = ButtonStackPanel;
      Grid.SetRow(ButtonStackPanel, 1);

      ButtonStackPanel.Children.Add(NewGlobalButton("Kill All", (object sender, RoutedEventArgs e) =>
      {
        var KillAll = KillAllEvent;
        if (KillAll != null)
          KillAll();
      }));

      ButtonStackPanel.Children.Add(NewGlobalButton("Kill Oldest", (object sender, RoutedEventArgs e) =>
      {
        var KillOldest = KillOldestEvent;
        if (KillOldest != null)
          KillOldest();
      }));
      
      ButtonStackPanel.Children.Add(NewGlobalButton("Kill Youngest", (object sender, RoutedEventArgs e) =>
      {
        var KillYoungest = KillYoungestEvent;
        if (KillYoungest != null)
          KillYoungest();
      }));

      DockPanel.LastChildFill = true; // use to make the process border fill the window and stretch with it
      var ProcessBorder = new Border();
      DockPanel.Children.Add(ProcessBorder);

      this.ProcessesScrollViewer = new ScrollViewer();
      ProcessBorder.Child = ProcessesScrollViewer;
      ProcessesScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

      ComposeProcesses();
    }
    public void RemoveProcess(Process Process)
    {
      ProcessList.Remove(Process);
    }

    private void ComposeProcesses()
    {
      var ProcessesStackPanel = new StackPanel();
      ProcessesScrollViewer.Content = ProcessesStackPanel;
      ProcessesStackPanel.Margin = new Thickness(2);

      foreach (var Process in ProcessList)
      {
        // build a panel to stick on MainStackPanel
        var ProcessBorder = new Border()
        {
          BorderThickness = new Thickness(1, 1, 2, 2),
          Background = Theme.ProcessBorderBackground,
          BorderBrush = Theme.ProcessBorder,
          Margin = new Thickness(4, 8, 8, 8),
          Padding = new Thickness(2),
          CornerRadius = new CornerRadius(5)
        };
        ProcessesStackPanel.Children.Add(ProcessBorder);

        var ProcessStackPanel = new StackPanel();
        ProcessBorder.Child = ProcessStackPanel;

        ProduceProcessDataRow(ProcessStackPanel, "Main Window Title", Process.MainWindowTitle);
        ProduceProcessDataRow(ProcessStackPanel, "Started", TimeSpanAsWords(DateTime.Now - Process.StartTime) + " ago");
        ProduceProcessDataRow(ProcessStackPanel, new string[] { "Physical Memory", "Process ID" }, new string[] { string.Format("{0} MB", Process.WorkingSet64 / (1024 * 1024)), Process.Id.ToString() });
        ProduceProcessDataRow(ProcessStackPanel, "Total Processor Time", TimeSpanAsWords(Process.TotalProcessorTime));

        // Buttons for the process
        var ButtonStackPanel = new StackPanel()
        {
          Orientation = Orientation.Horizontal,
          HorizontalAlignment = HorizontalAlignment.Center,
          FlowDirection = FlowDirection.RightToLeft,
          Margin = new Thickness(5)
        };
        ProcessStackPanel.Children.Add(ButtonStackPanel);

        ButtonStackPanel.Children.Add(NewProcessButton("Show Me", (Sender, Event) =>
        {
          var ProcessShowMe = ProcessShowMeEvent;
          if (ProcessShowMe != null)
            ProcessShowMe(Process);
        }));

        ButtonStackPanel.Children.Add(NewProcessButton("Kill Me", (Sender, Event) =>
        {
          if (ProcessKillMeEvent != null)
          {
            ProcessKillMeEvent(Process);
            ComposeProcesses();
          }
        }));

        ButtonStackPanel.Children.Add(NewProcessButton("Kill Others", (Sender, Event) =>
        {
          if (ProcessKillOthersEvent != null)
          {
            ProcessKillOthersEvent(Process);
            ComposeProcesses();
          }
        }));
      }
    }

    private Button NewGlobalButton(string Caption, RoutedEventHandler ClickAction)
    {
      var Result = new Button()
      {
        Content = Caption,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new Thickness(0, 0, 7, 0),
        Padding = new Thickness(5),
        BorderBrush = Theme.ApplicationButtonBorder,
        Background = Theme.ApplicationButtonBackground,
        Foreground = Theme.ApplicationButtonForeground,
        FontSize = 15,
        MinWidth = MinimumButtonWidth
      };
      Result.Click += ClickAction;

      return Result;
    }
    private Button NewProcessButton(string Caption, RoutedEventHandler ClickAction)
    {
      var Result = new Button()
      {
        Content = Caption,
        Background = Theme.ProcessButtonBackground,
        BorderBrush = Theme.ProcessButtonBorder,
        Margin = new Thickness(0, 0, 7, 0),
        Padding = new Thickness(5),
        Foreground = Theme.ProcessButtonForeground,
        FontSize = 15,
        MinWidth = MinimumProcessButtonWidth
      };
      Result.Click += ClickAction;

      return Result;
    }
    private string TimeSpanAsWords(TimeSpan TimeSpan)
    {
      StringBuilder Result = new StringBuilder("");

      if (TimeSpan.Days > 0)
        Result.AppendFormat("{0} days", TimeSpan.Days);

      if (TimeSpan.Hours > 0)
      {
        if (Result.Length != 0)
          Result.Append(", ");

        Result.AppendFormat("{0} hours", TimeSpan.Hours);
      }

      if (TimeSpan.Minutes > 0)
      {
        if (Result.Length != 0)
          Result.Append(", ");

        Result.AppendFormat("{0} minutes", TimeSpan.Minutes);
      }

      if (TimeSpan.Seconds > 0)
      {
        if (Result.Length != 0)
          Result.Append(", ");

        Result.AppendFormat("{0} seconds", TimeSpan.Seconds);
      }

      if (TimeSpan.Milliseconds > 0)
      {
        if (Result.Length != 0)
          Result.Append(", ");

        Result.AppendFormat("{0} milliseconds", TimeSpan.Milliseconds);
      }

      return Result.ToString();
    }
    private void ProduceProcessDataRow(StackPanel Parent, string Caption, string Data)
    {
      ProduceProcessDataRow(Parent, new string[] { Caption }, new string[] { Data });
    }
    private void ProduceProcessDataRow(StackPanel Parent, string[] Caption, string[] Data)
    {
      Debug.Assert(Caption.Length == Data.Length, "Caption array and Data array must have the same length");

      var RowStackPanel = new StackPanel()
      {
        Orientation = Orientation.Horizontal,
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      Parent.Children.Add(RowStackPanel);

      for (int i = 0; i < Caption.Length; i++)
      {
        var DataStackPanel = new StackPanel();
        RowStackPanel.Children.Add(DataStackPanel);
        DataStackPanel.Orientation = Orientation.Horizontal;

        var CaptionLabel = new Label()
        {
          Content = Caption[i],
          FontWeight = FontWeights.Heavy,
          Foreground = Theme.ProcessCaptionForeground
        };
        DataStackPanel.Children.Add(CaptionLabel);

        var DataTextBlock = new TextBlock()
        {
          Text = Data[i],
          TextTrimming = TextTrimming.CharacterEllipsis,
          TextWrapping = TextWrapping.Wrap,
          Foreground = Theme.ProcessDataForeground
        };

        var DataLabel = new Label();
        DataStackPanel.Children.Add(DataLabel);
        DataLabel.Content = DataTextBlock;

        var DataToolTip = new ToolTip()
        {
          PlacementTarget = DataLabel,
          Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint,
          Content = Data[i]
        };
        DataLabel.ToolTip = DataToolTip;
      }
    }
  }
}