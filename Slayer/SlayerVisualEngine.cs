using System;
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
    Application Application { get; set; }
    List<Process> ProcessList { get; set; }
    event Action KillAllEvent;
    event Action KillOldestEvent;
    event Action KillYoungestEvent;
    event Action<Process> ProcessShowMeEvent;
    event Action<Process> ProcessKillMeEvent;
    event Action<Process> ProcessKillOthersEvent;
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

      ProcessList.Sort(SortByStartTime);

      var AssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
      Window.Title = String.Format("{0} v{1}", AssemblyName.Name, AssemblyName.Version.ToString().TrimEnd('.', '0'));

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

      var KillAllButton = NewGlobalButton("Kill All");
      ButtonStackPanel.Children.Add(KillAllButton);
      KillAllButton.Click += (object sender, RoutedEventArgs e) =>
      {
        var KillAll = KillAllEvent;
        if (KillAll != null)
          KillAll();
      };

      var KillOldestButton = NewGlobalButton("Kill Oldest");
      ButtonStackPanel.Children.Add(KillOldestButton);
      KillOldestButton.Click += (object sender, RoutedEventArgs e) =>
      {
        var KillOldest = KillOldestEvent;
        if (KillOldest != null)
          KillOldest();
      };

      var KillYoungestButton = NewGlobalButton("Kill Youngest");
      ButtonStackPanel.Children.Add(KillYoungestButton);
      KillYoungestButton.Click += (object sender, RoutedEventArgs e) =>
      {
        var KillYoungest = KillYoungestEvent;
        if (KillYoungest != null)
          KillYoungest();
      };

      DockPanel.LastChildFill = true; // use to make the process border fill the window and stretch with it
      var ProcessBorder = new Border();
      DockPanel.Children.Add(ProcessBorder);

      this.ProcessesScrollViewer = new ScrollViewer();
      ProcessBorder.Child = ProcessesScrollViewer;
      ProcessesScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

      ComposeProcesses();
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

        var ShowMeButton = NewProcessButton("Show Me");
        ButtonStackPanel.Children.Add(ShowMeButton);
        ShowMeButton.Click += (Sender, Event) =>
        {
          if (ProcessShowMeEvent != null)
            ProcessShowMeEvent(Process);
        };

        var KillMeButton = NewProcessButton("Kill Me");
        ButtonStackPanel.Children.Add(KillMeButton);
        KillMeButton.Click += (Sender, Event) =>
        {
          if (ProcessKillMeEvent != null)
          {
            ProcessKillMeEvent(Process);
            ComposeProcesses();
          }
        };

        var KillOthersButton = NewProcessButton("Kill Others");
        ButtonStackPanel.Children.Add(KillOthersButton);
        KillOthersButton.Click += (Sender, Event) =>
        {
          if (ProcessKillOthersEvent != null)
          {
            ProcessKillOthersEvent(Process);
            ComposeProcesses();
          }
        };
      }
    }

    private Button NewGlobalButton(string Caption)
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
    private Button NewProcessButton(string Caption)
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