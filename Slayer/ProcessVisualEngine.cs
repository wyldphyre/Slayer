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
    private Dictionary<Button, Process> buttonProcessDictionary = new Dictionary<Button, Process>();

    public Theme Theme { get; set; }
    public Application Application { get; set; }
    public Border MainBorder { get; set; }
    public List<Process> ProcessList { get; set; }

    private const double MinimumButtonWidth = 75;

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
        ProcessBorder.Background = Theme.ProcessBorderBackground;
        ProcessBorder.BorderBrush = Theme.ProcessBorder;
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
          // TODO: This can be simplified by no longer using the dictionary
          Button ClickedButton = (Button)Sender;
          Process ButtonProcess = null;

          if (buttonProcessDictionary.TryGetValue(ClickedButton, out ButtonProcess))
            NativeMethods.SetForegroundWindow(ButtonProcess.MainWindowHandle);
        };

        var KillMeButton = NewButton("Kill Me");
        ButtonStackPanel.Children.Add(KillMeButton);
        buttonProcessDictionary.Add(KillMeButton, process);
        KillMeButton.Click += (Sender, Event) =>
        {
          // TODO: This can be simplified by no longer using the dictionary
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
          // TODO: This can be simplified by no longer using the dictionary

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
      Result.Background = Theme.ProcessButtonBackground;
      Result.BorderBrush = Theme.ProcessButtonBorder;
      Result.Margin = new Thickness(0, 0, 7, 0);
      Result.Padding = new Thickness(5);
      Result.Foreground = Theme.ProcessButtonForeground;
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
      HeadingLabel.Foreground = Theme.ProcessHeadingForeground;
    }
    private void ProduceDataRow(StackPanel Parent, string Caption, string Data)
    {
      ProduceDataRow(Parent, new string[] { Caption }, new string[] { Data });
    }
    private void ProduceDataRow(StackPanel Parent, string[] Caption, string[] Data)
    {
      Debug.Assert(Caption.Length == Data.Length, "Caption array and Data array must have the same length");

      var RowStackPanel = new StackPanel();
      Parent.Children.Add(RowStackPanel);
      RowStackPanel.Orientation = Orientation.Horizontal;
      RowStackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

      for (int i = 0; i < Caption.Length; i++)
      {
        var DataStackPanel = new StackPanel();
        RowStackPanel.Children.Add(DataStackPanel);
        DataStackPanel.Orientation = Orientation.Horizontal;

        var CaptionLabel = new Label();
        DataStackPanel.Children.Add(CaptionLabel);
        CaptionLabel.Content = Caption[i];
        CaptionLabel.FontWeight = FontWeights.Heavy;
        CaptionLabel.Foreground = Theme.ProcessCaptionForeground;

        var DataTextBlock = new TextBlock();
        DataTextBlock.Text = Data[i];
        //DataTextBlock.ClipToBounds = true;
        DataTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        DataTextBlock.TextWrapping = TextWrapping.Wrap;
        DataTextBlock.Foreground = Theme.ProcessDataForeground;

        var DataLabel = new Label();
        DataStackPanel.Children.Add(DataLabel);
        DataLabel.Content = DataTextBlock;
        //DataLabel.ClipToBounds = true;

        var DataToolTip = new ToolTip();
        DataLabel.ToolTip = DataToolTip;
        DataToolTip.PlacementTarget = DataLabel;
        DataToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
        DataToolTip.Content = Data[i];
      }
    }
  }
}
