/*! 2 !*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace Slayer
{
  class SlayerVisualEngine
  {
    private const double MinimumButtonWidth = 70;
    private Window Window;
    private Border MainBorder { get; set; }

    public Theme Theme { get; set; }
    public Application Application { get; set; }
    public List<Process> ProcessList { get; set; }
    public event Action KillAllEvent;
    public event Action KillOldestEvent;
    public event Action KillYoungestEvent;

    public SlayerVisualEngine(Window Window)
    {
      this.Window = Window;
      this.Theme = Theme;

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

      MainBorder = new Border();
      Window.Content = MainBorder;
      MainBorder.Padding = new Thickness(4);
    }

    public void Install()
    {
      MainBorder.Background = Theme.ApplicationBackground;

      ProcessList.Sort(SortByStartTime);

      var AssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
      Window.Title = String.Format("{0} v{1}", AssemblyName.Name, AssemblyName.Version.ToString().TrimEnd('.', '0'));

      var IconUri = new Uri("pack://application:,,,/Slayer;component/Images/Close-128.ico"); // File needs to be set as a resource in it's properties
      Window.Icon = new System.Windows.Media.Imaging.BitmapImage(IconUri);

      var DockPanel = new DockPanel();
      MainBorder.Child = DockPanel;
      DockPanel.LastChildFill = false;
      DockPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

      var ButtonBorder = new Border();
      DockPanel.Children.Add(ButtonBorder);
      DockPanel.SetDock(ButtonBorder, Dock.Bottom);
      ButtonBorder.Background = Theme.ApplicationButtonToolbarBackground;
      ButtonBorder.Padding = new Thickness(0, 5, 0, 5);

      var ButtonStackPanel = new StackPanel()
      {
        Orientation = Orientation.Horizontal,
        Margin = new Thickness(5),
        Height = 30,
        HorizontalAlignment = HorizontalAlignment.Center
      };
      ButtonBorder.Child = ButtonStackPanel;
      Grid.SetRow(ButtonStackPanel, 1);

      var KillAllButton = NewButton("Kill All");
      ButtonStackPanel.Children.Add(KillAllButton);
      KillAllButton.Click += (object sender, RoutedEventArgs e) =>
      {
        var KillAll = KillAllEvent;
        if (KillAll != null)
          KillAll();
      };

      var KillOldestButton = NewButton("Kill Oldest");
      ButtonStackPanel.Children.Add(KillOldestButton);
      KillOldestButton.Click += (object sender, RoutedEventArgs e) =>
      {
        var KillOldest = KillOldestEvent;
        if (KillOldest != null)
          KillOldest();
      };

      var KillYoungestButton = NewButton("Kill Youngest");
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

      var ProcessVisualEngine = new ProcessVisualEngine
      {
        Theme = Theme,
        Application = Application,
        MainBorder = ProcessBorder,
        ProcessList = ProcessList
      };
      ProcessVisualEngine.Install();
    }

    private Button NewButton(string Caption)
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
  }
}