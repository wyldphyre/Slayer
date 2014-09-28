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
    private Application Application;
    private string ApplicationFilePath;
    private Configuration Configuration;
    private SlayableConfigurationSection SlayableSection;
    private SlayerColourThemeSection ColourThemeSection;

    private List<string> arguments = new List<string>();
    private Process[] ProcessesArray = null;
    private bool alwaysPreview = false;
    private Theme Theme;

    public string ProcessName { get; set; }
    public List<string> Arguments { get { return arguments; } }

    public SlayerApplication()
    {
      this.Application = new Application();
      this.ApplicationFilePath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
      this.Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      this.SlayableSection = (SlayableConfigurationSection)Configuration.GetSection("slayableSection");
      this.ColourThemeSection = (SlayerColourThemeSection)Configuration.GetSection("slayerColourThemeSection");

      Theme = ThemeHelper.Default();

      // colour themes

      if (ColourThemeSection != null && ColourThemeSection.Theme != "" && !ColourThemeSection.Theme.Equals("default", StringComparison.CurrentCultureIgnoreCase))
      {
        // load the specified theme from the list
        SlayerColourThemeElement ColourTheme = null;
        
        foreach (SlayerColourThemeElement ThemeElement in ColourThemeSection.Themes)
        {
          if (ThemeElement.Name.Equals(ColourThemeSection.Theme, StringComparison.CurrentCultureIgnoreCase))
          {
            ColourTheme = ThemeElement;
            break;
          }
        }

        if (ColourTheme == null)
          throw new ApplicationException(string.Format("Could not locate theme '{0}'", ColourThemeSection.Theme));

        Theme = ThemeHelper.Load(ColourTheme);
      }
    }

    public void Execute()
    {
      SetupJumpList();

      if (arguments.Count > 0)
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
            var SwitchParameter = argument.Remove(0, 1);
            var SwitchArgument = "";
            if (SwitchParameter.Contains(":"))
            {
              var Components = SwitchParameter.Split(':');
              SwitchParameter = Components[0];
              SwitchArgument = Components[1];

              if (SwitchArgument == "")
              {
                MessageBox.Show(string.Format("The switch parameter \"{0}\" must be of the form <Switch>:<Argument>", SwitchParameter));
                return;
              }
            }
            
            if (SwitchParameter.ToUpper() == "AlwaysPreview".ToUpper())
            {
              alwaysPreview = true;
            }
            else if (SwitchParameter.Equals("SetTheme", StringComparison.CurrentCultureIgnoreCase))
            {
              var ConfigurationFileInfo = new FileInfo(Configuration.FilePath);
              var IsReadOnly = ConfigurationFileInfo.IsReadOnly;

              if (IsReadOnly)
                ConfigurationFileInfo.IsReadOnly = false;

              ColourThemeSection.Theme = SwitchArgument;
              Configuration.Save(ConfigurationSaveMode.Modified);
              SetupJumpList(); //need to update the default marker next to the correct theme name.

              if (IsReadOnly)
                ConfigurationFileInfo.IsReadOnly = true;
              return;
            }
            else
            {
              MessageBox.Show(String.Format("The switch parameter \"{0}\" is not recognised.", SwitchParameter));
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
      }
      else
      {
        foreach (SlayableApplicationElement Element in SlayableSection.Applications)
        {
          if (Element.Default)
          {
            ProcessName = Element.ProcessName;
            alwaysPreview = Element.Preview;
            break;
          }
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

          var VisualEngine = new SlayerVisualEngine(MainWindow)
          {
            Theme = this.Theme,
            ProcessList = ProcessList,
            Application = Application
          };
          VisualEngine.Install();

          MainWindow.Show();

          Application.Run();
        }
      }
    }

    private void SetupJumpList()
    {
      var JumpList = new JumpList();

      if (ColourThemeSection != null)
      {
        var DefaultThemeTask = new JumpTask();
        JumpList.JumpItems.Add(DefaultThemeTask);
        DefaultThemeTask.CustomCategory = "Theme";
        DefaultThemeTask.Arguments = "-settheme:default";
        DefaultThemeTask.Title = "Default";
        if (ColourThemeSection.Theme.Equals("default", StringComparison.CurrentCultureIgnoreCase))
          DefaultThemeTask.Title += " \u2605";//★

        foreach (SlayerColourThemeElement ThemeElement in ColourThemeSection.Themes)
        {
          var ThemeTask = new JumpTask();
          JumpList.JumpItems.Add(ThemeTask);
          ThemeTask.CustomCategory = "Theme";
          ThemeTask.Arguments = "-settheme:" + ThemeElement.Name;
          ThemeTask.Title = ThemeElement.Name;
          if (ColourThemeSection.Theme.Equals(ThemeElement.Name, StringComparison.CurrentCultureIgnoreCase))
            ThemeTask.Title += " \u2605";//★
        }
      }

      if (SlayableSection != null)
      {
        foreach (SlayableApplicationElement Element in SlayableSection.Applications)
        {
          var Task = new JumpTask();
          JumpList.JumpItems.Add(Task);
          Task.CustomCategory = "Slay";
          Task.ApplicationPath = ApplicationFilePath;
          Task.Arguments = Element.ProcessName;
          Task.Title = Element.Name;
          if (Element.Default)
            Task.Title += " \u2605";//★
          if (Element.Preview)
            Task.Arguments += @" /AlwaysPreview";
        }
      }

      // The entire JumpList is replaced each time
      JumpList.SetJumpList(Application, JumpList);
    }
  }
}
