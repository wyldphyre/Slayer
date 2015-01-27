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
  static class NativeMethods
  {
    [Flags]

    public enum AssocF
    {
      Init_NoRemapCLSID = 0x1,
      Init_ByExeName = 0x2,
      Open_ByExeName = 0x2,
      Init_DefaultToStar = 0x4,
      Init_DefaultToFolder = 0x8,
      NoUserSettings = 0x10,
      NoTruncate = 0x20,
      Verify = 0x40,
      RemapRunDll = 0x80,
      NoFixUps = 0x100,
      IgnoreBaseClass = 0x200
    }

    public enum AssocStr
    {
      Command = 1,
      Executable,
      FriendlyDocName,
      FriendlyAppName,
      NoOpen,
      ShellNewValue,
      DDECommand,
      DDEIfExec,
      DDEApplication,
      DDETopic
    }

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
    public static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);
  }

  static class NativeMethodHelper
  {
    public static string AssociatedApplicationPathForExtension(NativeMethods.AssocStr association, string extension)
    {
      const int S_OK = 0;
      const int S_FALSE = 1;

      uint length = 0;
      uint ret = NativeMethods.AssocQueryString(NativeMethods.AssocF.NoUserSettings, association, extension, null, null, ref length);
      if (ret != S_FALSE)
        throw new InvalidOperationException("Could not determine associated string");

      var sb = new StringBuilder((int)length); // (length-1) will probably work too as the marshaller adds null termination
      ret = NativeMethods.AssocQueryString(NativeMethods.AssocF.NoUserSettings, association, extension, null, sb, ref length);
      if (ret != S_OK)
        throw new InvalidOperationException("Could not determine associated string");

      return sb.ToString();
    }
  }

  class ProcessVisualEngine
  {
    public Theme Theme { get; set; }
    public Application Application { get; set; }
    public Border MainBorder { get; set; }
    public List<Process> ProcessList { get; set; }

    private const double MinimumProcessButtonWidth = 75;
    private ScrollViewer ProcessesScrollViewer;

    public void Install()
    {
      this.ProcessesScrollViewer = new ScrollViewer();
      MainBorder.Child = ProcessesScrollViewer;
      ProcessesScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

      ComposeProcesses();
    }

    private void ComposeProcesses()
    {
      var ProcessesStackPanel = new StackPanel();
      ProcessesScrollViewer.Content = ProcessesStackPanel;
      ProcessesStackPanel.Margin = new Thickness(2);

      foreach (var process in ProcessList)
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

        ProduceProcessHeader(ProcessStackPanel, process.ProcessName);
        ProduceProcessDataRow(ProcessStackPanel, "Main Window Title", process.MainWindowTitle);
        //ProduceDataRow(ProcessStackPanel, "Started", string.Format("{0} ago", DateTime.Now - TimeSpanAsWords(process.StartTime)));
        ProduceProcessDataRow(ProcessStackPanel, new string[] { "Physical Memory", "Process ID" }, new string[] { string.Format("{0} MB", process.WorkingSet64 / (1024 * 1024)), process.Id.ToString() });
        ProduceProcessDataRow(ProcessStackPanel, "Total Processor Time", TimeSpanAsWords(process.TotalProcessorTime));

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
          NativeMethods.SetForegroundWindow(process.MainWindowHandle);
        };

        var KillMeButton = NewProcessButton("Kill Me");
        ButtonStackPanel.Children.Add(KillMeButton);
        KillMeButton.Click += (Sender, Event) =>
        {
          process.Kill();
          ProcessList.Remove(process);

          if (ProcessList.Count < 1)
            Application.Shutdown();

          ComposeProcesses();
        };

        var KillOthersButton = NewProcessButton("Kill Others");
        ButtonStackPanel.Children.Add(KillOthersButton);
        KillOthersButton.Click += (Sender, Event) =>
        {
          List<Process> KilledProcesses = new List<Process>();

          foreach (Process KillableProcess in ProcessList.Where(searchprocess => searchprocess != process))
          {
            KillableProcess.Kill();
            KilledProcesses.Add(KillableProcess);
          };

          foreach (Process KilledProcess in KilledProcesses)
            ProcessList.Remove(KilledProcess);

          ComposeProcesses();
        };
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
    private void ProduceProcessHeader(StackPanel Parent, string Heading)
    {
      var HeadingLabel = new Label()
      {
        Content = Heading,
        FontWeight = FontWeights.Bold,
        FontSize = 18,
        HorizontalAlignment = HorizontalAlignment.Center,
        Foreground = Theme.ProcessHeadingForeground
      };
      Parent.Children.Add(HeadingLabel);
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
