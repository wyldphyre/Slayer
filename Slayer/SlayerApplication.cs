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
    private Process[] ProcessesArray = null;
    private bool alwaysPreview = false;

    public string ProcessName { get; set; }
    public List<string> Arguments { get { return arguments; } }

    public SlayerApplication()
    {
      this.Application = new Application();
      this.ApplicationFilePath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
      this.SlayableSection = (SlayableConfigurationSection)ConfigurationManager.GetSection("slayableSection");
    }

    public void Execute()
    {     
      foreach (string argument in Arguments)
      {
        if (argument.StartsWith(@"\"))
        {
          MessageBox.Show(string.Format("Incorrect switch {0}. Use '-' or '/' instead of '\\'.", argument));
          return;
        }
        if (argument.StartsWith("-") || argument.StartsWith("/"))
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
          ProcessName = argument;
        }
      }

      Debug.Assert(ProcessName != String.Empty, "ProcessName cannot be an empty string");

      string sanitisedProcessName = ProcessName;

      if (sanitisedProcessName.EndsWith(".exe"))
        sanitisedProcessName = sanitisedProcessName.Remove(sanitisedProcessName.Length - ".exe".Length);
      
      // TODO: Should the app also handle the user passing in a path instead of 'friendly' process name?

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

          Application.ShutdownMode = ShutdownMode.OnMainWindowClose;
          Application.DispatcherUnhandledException += (Sender, Event) =>
          {
            MessageBox.Show(Event.Exception.Message, "Exception");
            Event.Handled = true;
          };
          
          var MainWindow = new Window();
          Application.MainWindow = MainWindow;
          
          var VisualEngine = new SlayerVisualEngine(MainWindow);
          VisualEngine.ProcessList = ProcessList;
          VisualEngine.Application = Application;
          VisualEngine.Install();

          MainWindow.Show();

          Application.Run();
        }   
      }
    }

    public void SetupJumpList()
    {
      var JumpList = new JumpList();

      if (SlayableSection != null)
      {
        foreach (SlayableApplicationElement Element in SlayableSection.Applications)
        {
          var Task = new JumpTask();
          JumpList.JumpItems.Add(Task);
          Task.CustomCategory = "Slay";
          Task.ApplicationPath = ApplicationFilePath;
          Task.Title = Element.Name;
          Task.Arguments = Element.ProcessName;
          if (Element.Preview)
            Task.Arguments += @" /AlwaysPreview";
        }
      }

      // The entire JumpList is replaced each time
      JumpList.SetJumpList(Application, JumpList);
    }

    private Application Application;
    private string ApplicationFilePath;
    private SlayableConfigurationSection SlayableSection;
  }
}
