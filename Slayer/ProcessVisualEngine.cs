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

    private const double MinimumButtonWidth = 65;
    private Brush ProcessButtonBorderBrush = Brushes.LightGray;
    private Brush ProcessButtonBackground = Brushes.WhiteSmoke;

    public void Install()
    {
      var ProcessesScrollViewer = new ScrollViewer();
      MainBorder.Child = ProcessesScrollViewer;
      ProcessesScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

      BuildProcessDisplay(ProcessesScrollViewer);
    }

    private void BuildProcessDisplay(ScrollViewer Parent)
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
        ProcessBorder.BorderBrush = Brushes.LightGray;
        ProcessBorder.Background = Brushes.Snow;
        ProcessBorder.CornerRadius = new CornerRadius(10, 10, 10, 10);
        System.Windows.Media.Effects.DropShadowEffect DropShadowEffect = new System.Windows.Media.Effects.DropShadowEffect();
        DropShadowEffect.Color = Colors.DarkGray;
        ProcessBorder.Effect = DropShadowEffect;
        ProcessBorder.Margin = new Thickness(4, 8, 8, 8);
        ProcessBorder.Padding = new Thickness(2);

        var ProcessStackPanel = new StackPanel();
        ProcessBorder.Child = ProcessStackPanel;

        ProduceDataRow(ProcessStackPanel, "Name:", process.ProcessName);
        ProduceDataRow(ProcessStackPanel, "ID:", process.Id.ToString());
        ProduceDataRow(ProcessStackPanel, "Main Window Title:", process.MainWindowTitle);
        ProduceDataRow(ProcessStackPanel, "Physical Memory:", string.Format("{0} MB", process.WorkingSet64 / (1024 * 1024)));
        ProduceDataRow(ProcessStackPanel, "Total Processor Time:", TimeSpanAsWords(process.TotalProcessorTime));

        // Buttons for the process
        var ButtonStackPanel = new StackPanel();
        ProcessStackPanel.Children.Add(ButtonStackPanel);
        ButtonStackPanel.Orientation = Orientation.Horizontal;
        ButtonStackPanel.FlowDirection = FlowDirection.RightToLeft;
        ButtonStackPanel.Margin = new Thickness(5);

        var ShowMeButton = new Button();
        ButtonStackPanel.Children.Add(ShowMeButton);
        buttonProcessDictionary.Add(ShowMeButton, process);
        ShowMeButton.Content = "Show Me";
        ShowMeButton.Background = ProcessButtonBackground;
        ShowMeButton.Margin = new Thickness(0, 0, 2, 0);
        ShowMeButton.BorderBrush = ProcessButtonBorderBrush;
        ShowMeButton.Width = MinimumButtonWidth;
        ShowMeButton.Click += (Sender, Event) =>
          {
            Button ClickedButton = (Button)Sender;
            Process ButtonProcess = null;

            if (buttonProcessDictionary.TryGetValue(ClickedButton, out ButtonProcess))
            {
              SetForegroundWindow(ButtonProcess.MainWindowHandle);
            }
          };

        var KillMeButton = new Button();
        ButtonStackPanel.Children.Add(KillMeButton);
        buttonProcessDictionary.Add(KillMeButton, process);
        KillMeButton.Content = "Kill Me";
        KillMeButton.Background = ProcessButtonBackground;
        KillMeButton.Margin = new Thickness(0, 0, 2, 0);
        KillMeButton.BorderBrush = ProcessButtonBorderBrush;
        KillMeButton.Width = MinimumButtonWidth;
        KillMeButton.Click += (Sender, Event) =>
          {
            Button ClickedButton = (Button)Sender;
            Process ButtonProcess = null;

            if (buttonProcessDictionary.TryGetValue(ClickedButton, out ButtonProcess))
            {
              ButtonProcess.Kill();
              ProcessList.Remove(ButtonProcess);
              BuildProcessDisplay(Parent);
            }

            if (ProcessList.Count < 1)
            {
              Application.Shutdown();
            }
          };

        var KillOthersButton = new Button();
        ButtonStackPanel.Children.Add(KillOthersButton);
        buttonProcessDictionary.Add(KillOthersButton, process);
        KillOthersButton.Content = "Kill Others";
        KillOthersButton.Background = ProcessButtonBackground;
        KillOthersButton.BorderBrush = ProcessButtonBorderBrush;
        KillOthersButton.Width = MinimumButtonWidth;
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
              {
                ProcessList.Remove(KilledProcess);
              }

              BuildProcessDisplay(Parent);
            }
        };
      }
    }

    private string TimeSpanAsWords(TimeSpan TimeSpan)
    {
      StringBuilder Result = new StringBuilder("");
      
      if (TimeSpan.Days > 0)
      {
        Result.AppendFormat("{0} days", TimeSpan.Days);
      }

      if (TimeSpan.Hours > 0)
      {
        if (Result.Length != 0)
        {
          Result.Append(", ");
        }

        Result.AppendFormat("{0} hours", TimeSpan.Hours);
      }

      if (TimeSpan.Minutes > 0)
      {
        if (Result.Length != 0)
        {
          Result.Append(", ");
        }

        Result.AppendFormat("{0} minutes", TimeSpan.Minutes);
      }

      if (TimeSpan.Seconds > 0)
      {
        if (Result.Length != 0)
        {
          Result.Append(", ");
        }

        Result.AppendFormat("{0} seconds", TimeSpan.Seconds);
      }

      if (TimeSpan.Milliseconds > 0)
      {
        if (Result.Length != 0)
        {
          Result.Append(", ");
        }

        Result.AppendFormat("{0} milliseconds", TimeSpan.Milliseconds);
      }

      return Result.ToString();
    }

    private void ProduceDataRow(StackPanel ParentStackPanel, string Caption, string Data)
    {
      var DataStackPanel = new StackPanel();
      ParentStackPanel.Children.Add(DataStackPanel);
      DataStackPanel.Orientation = Orientation.Horizontal;

      var CaptionLabel = new Label();
      DataStackPanel.Children.Add(CaptionLabel);
      CaptionLabel.Content = Caption;
      CaptionLabel.FontWeight = FontWeights.Heavy;

      var DataTextBlock = new TextBlock();//new System.Windows.Documents.Run(data));
      DataTextBlock.Text = Data;
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
