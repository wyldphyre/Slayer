/*! 2 !*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Runtime.InteropServices;

namespace Slayer
{
  class ProcessVisualEngine
  {
    public Application Application { get; set; }
    public Border MainBorder { get; set; }
    public List<Process> ProcessList { get; set; }
    private Dictionary<Button, Process> buttonProcessDictionary = new Dictionary<Button, Process>();

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    private const double MinimumButtonWidth = 75;
    private Brush ProcessBorderBackground = Brushes.WhiteSmoke;
    private Brush ProcessBorderBorderBrush = Brushes.DarkGray;
    private Brush ProcessButtonBorderBrush = Brushes.Transparent;
    private Brush ProcessButtonBackground = Brushes.Transparent;
    private Brush ProcessButtonForeground = Brushes.OrangeRed;

    public void Install()
    {
      var ProcessesScrollViewer = new ScrollViewer();
      MainBorder.Child = ProcessesScrollViewer;
      ProcessesScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

      Compose(ProcessesScrollViewer);
    }

    private void Compose(ScrollViewer Parent)
    {
      var ProcessesStackPanel = new StackPanel();
      Parent.Content = ProcessesStackPanel;
      ProcessesStackPanel.Margin = new Thickness(2);

      foreach (Process process in ProcessList)
      {
        // build a panel to stick on MainStackPanel
        var ProcessBorder = new Border();
        ProcessesStackPanel.Children.Add(ProcessBorder);
        ProcessBorder.BorderThickness = new Thickness(1, 1, 2, 2);
        ProcessBorder.Background = ProcessBorderBackground;
        ProcessBorder.BorderBrush = ProcessBorderBorderBrush;
        ProcessBorder.Margin = new Thickness(4, 8, 8, 8);
        ProcessBorder.Padding = new Thickness(2);
        ProcessBorder.CornerRadius = new CornerRadius(5);

        var ProcessStackPanel = new StackPanel();
        ProcessBorder.Child = ProcessStackPanel;

        ProduceHeader(ProcessStackPanel, process.ProcessName);
        ProduceDataRow(ProcessStackPanel, "Main Window Title", process.MainWindowTitle);
        ProduceDataRow(ProcessStackPanel, new string[] { "Physical Memory", "Process ID" }, new string[] { string.Format("{0} MB", process.WorkingSet64 / (1024 * 1024)), process.Id.ToString() });
        ProduceDataRow(ProcessStackPanel, "Total Processor Time", TimeSpanAsWords(process.TotalProcessorTime));

        // Buttons for the process
        var ButtonStackPanel = new StackPanel();
        ProcessStackPanel.Children.Add(ButtonStackPanel);
        ButtonStackPanel.Orientation = Orientation.Horizontal;
        ButtonStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
        ButtonStackPanel.FlowDirection = FlowDirection.RightToLeft;
        ButtonStackPanel.Margin = new Thickness(5);

        var ShowMeButton = NewButton("Show Me");
        ButtonStackPanel.Children.Add(ShowMeButton);
        buttonProcessDictionary.Add(ShowMeButton, process);
        ShowMeButton.Click += (Sender, Event) =>
        {
          Button ClickedButton = (Button)Sender;
          Process ButtonProcess = null;

          if (buttonProcessDictionary.TryGetValue(ClickedButton, out ButtonProcess))
            SetForegroundWindow(ButtonProcess.MainWindowHandle);
        };

        var KillMeButton = NewButton("Kill Me");
        ButtonStackPanel.Children.Add(KillMeButton);
        buttonProcessDictionary.Add(KillMeButton, process);
        KillMeButton.Click += (Sender, Event) =>
        {
          Button ClickedButton = (Button)Sender;
          Process ButtonProcess = null;

          if (buttonProcessDictionary.TryGetValue(ClickedButton, out ButtonProcess))
          {
            ButtonProcess.Kill();
            ProcessList.Remove(ButtonProcess);
            Compose(Parent);
          }

          if (ProcessList.Count < 1)
            Application.Shutdown();
        };

        var KillOthersButton = NewButton("Kill Others");
        ButtonStackPanel.Children.Add(KillOthersButton);
        buttonProcessDictionary.Add(KillOthersButton, process);
        KillOthersButton.Click += (Sender, Event) =>
        {
          //kill all processes except the one associated with the sending button

          Button ClickedButton = (Button)Sender;
          Process ButtonProcess = null;
          List<Process> KilledProcesses = new List<Process>();

          if (buttonProcessDictionary.TryGetValue(ClickedButton, out ButtonProcess))
          {
            var KillableProcesses = ProcessList.Where(searchprocess => searchprocess != ButtonProcess);
            foreach (Process killableProcess in KillableProcesses)
            {
              killableProcess.Kill();
              KilledProcesses.Add(killableProcess);
            };

            foreach (Process KilledProcess in KilledProcesses)
              ProcessList.Remove(KilledProcess);

            Compose(Parent);
          }
        };
      }
    }

    private Button NewButton(string Caption)
    {
      var Result = new Button();
      Result.Content = Caption;
      Result.Background = ProcessButtonBackground;
      Result.BorderBrush = ProcessButtonBorderBrush;
      Result.Margin = new Thickness(0, 0, 7, 0);
      Result.Padding = new Thickness(5);
      Result.Foreground = ProcessButtonForeground;
      Result.FontSize = 15;
      Result.MinWidth = MinimumButtonWidth;

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
    private void ProduceHeader(StackPanel Parent, string Heading)
    {
      var HeadingLabel = new Label();
      Parent.Children.Add(HeadingLabel);
      HeadingLabel.Content = Heading;
      HeadingLabel.FontWeight = FontWeights.Bold;
      HeadingLabel.FontSize = 18;
      HeadingLabel.HorizontalAlignment = HorizontalAlignment.Center;
    }
    private void ProduceDataRow(StackPanel Parent, string Caption, string Data)
    {
      ProduceDataRow(Parent, new string[] { Caption}, new string[] { Data });
    }
    private void ProduceDataRow(StackPanel Parent, string[] Caption, string[] Data)
    {
      Debug.Assert(Caption.Length == Data.Length, "Caption array and Data array must have the same length");

      var RowStackPanel = new StackPanel();
      Parent.Children.Add(RowStackPanel);
      RowStackPanel.Orientation = Orientation.Horizontal;

      for (int i = 0; i < Caption.Length; i++)
      {
        var DataStackPanel = new StackPanel();
        RowStackPanel.Children.Add(DataStackPanel);
        DataStackPanel.Orientation = Orientation.Horizontal;

        var CaptionLabel = new Label();
        DataStackPanel.Children.Add(CaptionLabel);
        CaptionLabel.Content = Caption[i];
        CaptionLabel.FontWeight = FontWeights.Heavy;

        var DataTextBlock = new TextBlock();
        DataTextBlock.Text = Data[i];
        DataTextBlock.ClipToBounds = true;
        DataTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;

        var DataLabel = new Label();
        DataStackPanel.Children.Add(DataLabel);
        DataLabel.Content = DataTextBlock;

        var DataToolTip = new ToolTip();
        DataLabel.ToolTip = DataToolTip;
        DataToolTip.PlacementTarget = DataLabel;
        DataToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
        DataToolTip.Content = Data;
      }
    }
  }
}
