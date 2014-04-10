/*! 2 !*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.IO;

namespace Slayer
{
  class SlayerApplication
  {
    private List<string> arguments = new List<string>();
    private string processName = null;
    private Process[] ProcessesArray = null;
    private bool alwaysPreview = false;

    public string ProcessName { get; set; }
    public List<string> Arguments { get { return arguments; } }

    public void Execute()
    {
      var Application = new Application();

      var JumpList = new JumpList();
      var SlayableSection = (SlayableConfigurationSection)ConfigurationManager.GetSection("slayableSection");
      
      if (SlayableSection != null)
      {
        var ApplicationFilePath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;

        foreach (SlayableApplicationElement Element in SlayableSection.Applications)
        {
          var Task = new JumpTask();
          JumpList.JumpItems.Add(Task);
          Task.CustomCategory = "Slay";
          Task.ApplicationPath = ApplicationFilePath;
          Task.Title = Element.Name;
          Task.Arguments = Element.ProcessName;
          if (Element.Preview)
            Task.Arguments += @" \AlwaysPreview";
        }
      }

      // The entire JumpList is replaced each time
      JumpList.SetJumpList(Application, JumpList);
          
      foreach (string argument in Arguments)
      {
        if (argument.StartsWith("-") || argument.StartsWith("\\"))
        {
          // arguments with a switch indicator character at the front are assumed to be switches
          var switchArgument = argument.Remove(0, 1);

          if (switchArgument.ToUpper() == "AlwaysPreview".ToUpper())
          {
            alwaysPreview = true;
          }
          else
          {
            MessageBox.Show(String.Format("The switch parameter \"{0}\" is not recognised.", switchArgument));
            return; // do not continue running the application if there is something wrong with the parameters
          }
        }
        else
        {
          // arguments without switch indicator are assumed to be process names.
          // TO DO: Currently only the last process name listed will be used.
          processName = argument;
        }
      }

      Debug.Assert(ProcessName != String.Empty, "ProcessName cannot be an empty string");

      string sanitisedProcessName = ProcessName;

      if (sanitisedProcessName.EndsWith(".exe"))
        sanitisedProcessName = sanitisedProcessName.Remove(sanitisedProcessName.Length - ".exe".Length);
      
      // To Do: How to handle the user passing in a path instead of 'friendly' process name?

      ProcessesArray = Process.GetProcessesByName(sanitisedProcessName);

      if (ProcessesArray.Length > 0) 
      {
        if ((ProcessesArray.Length == 1) && !alwaysPreview)
        {
          //kill the process
          ProcessesArray[0].Kill();
        }
        else
        {
          List<Process> ProcessList = new List<Process>();

          foreach (Process process in ProcessesArray)
            ProcessList.Add(process);
          
          var MainWindow = new Window();
          MainWindow.FontFamily = new FontFamily("Calibri");
          MainWindow.FontSize = 13;
          MainWindow.Width = 400;
          MainWindow.Height = 470;
          MainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
          MainWindow.Title = String.Format("Processes ({0})", ProcessList.Count);
          MainWindow.Background = Brushes.White;
          MainWindow.KeyUp += (object sender, System.Windows.Input.KeyEventArgs Event) =>
          {
            if (Event.Key == Key.Escape)
              MainWindow.Close();
          };

          var MainBorder = new Border();
          MainWindow.Content = MainBorder;
          MainBorder.Margin = new Thickness(4);

          var VisualEngine = new SlayerVisualEngine();
          VisualEngine.MainBorder = MainBorder;
          VisualEngine.ProcessList = ProcessList;
          VisualEngine.Application = Application;
          VisualEngine.Install();

          MainWindow.Show();

          Application.DispatcherUnhandledException += (Sender, Event) =>
          {
            MessageBox.Show(Event.Exception.Message, "Exception");
            Event.Handled = true;
          };
          Application.MainWindow = MainWindow;
          Application.ShutdownMode = ShutdownMode.OnMainWindowClose;
          Application.Run();
        }   
      }
    }
  }
}
