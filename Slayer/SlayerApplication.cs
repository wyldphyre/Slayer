using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Shell;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;

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

  class SlayerApplication
  {
    private Application Application;
    private string ApplicationFilePath;
    private Configuration Configuration;
    private SlayableConfigurationSection SlayableSection;
    private string ConfigurationFilePath;

    private bool AlwaysPreview = false;

    public string ProcessName { get; private set; }
    public List<string> Arguments { get; private set; }

    public SlayerApplication()
    {
      this.Arguments = new List<string>();
      this.Application = new Application();

      var Assembly = System.Reflection.Assembly.GetExecutingAssembly();
      var DefaultConfigurationFileName = "Slayer.exe.Config";
      var v1ConfigurationFileName = "Slayer_v1.exe.Config";
      var v2ConfigurationFileName = "Slayer_v2.exe.Config";
      var UserDataFolder = Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
      var ApplicationDataFolder = Path.Combine(UserDataFolder, "Slayer");
      this.ConfigurationFilePath = Path.Combine(ApplicationDataFolder, DefaultConfigurationFileName);

      if (!File.Exists(ConfigurationFilePath))
      {
        if (!Directory.Exists(ApplicationDataFolder))
          Directory.CreateDirectory(ApplicationDataFolder);

        using (var StreamReader = new StreamReader(Assembly.GetManifestResourceStream(Assembly.GetName().Name + "." + DefaultConfigurationFileName)))
        using (var FileWriter = new StreamWriter(ConfigurationFilePath, false))
        {
          FileWriter.Write(StreamReader.ReadToEnd());
          FileWriter.Flush();
        }
      } else
      {
        var ReplaceConfigFile = false;

        using (var v1ConfigStreamReader = new StreamReader(Assembly.GetManifestResourceStream(Assembly.GetName().Name + "." + v1ConfigurationFileName)))
        using (var v2ConfigStreamReader = new StreamReader(Assembly.GetManifestResourceStream(Assembly.GetName().Name + "." + v2ConfigurationFileName)))
        using (var LocalConfigStreamReader = new StreamReader(ConfigurationFilePath))
        {
          var Sha1 = new SHA1CryptoServiceProvider();
          var v1Hash = BitConverter.ToString(Sha1.ComputeHash(Encoding.UTF8.GetBytes(v1ConfigStreamReader.ReadToEnd())));
          var v2Hash = BitConverter.ToString(Sha1.ComputeHash(Encoding.UTF8.GetBytes(v2ConfigStreamReader.ReadToEnd())));
          var LocalHash = BitConverter.ToString(Sha1.ComputeHash(Encoding.UTF8.GetBytes(LocalConfigStreamReader.ReadToEnd())));
          
          ReplaceConfigFile = LocalHash == v1Hash;
        }

        if (ReplaceConfigFile)
        {
          File.Delete(ConfigurationFilePath);

          using (var CurrentStreamReader = new StreamReader(Assembly.GetManifestResourceStream(Assembly.GetName().Name + "." + DefaultConfigurationFileName)))
          using (var FileWriter = new StreamWriter(ConfigurationFilePath, false))
          {
            FileWriter.Write(CurrentStreamReader.ReadToEnd());
            FileWriter.Flush();
          }
        }
      }

      this.ApplicationFilePath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
      var ConfigurationMap = new ExeConfigurationFileMap();
      ConfigurationMap.ExeConfigFilename = ConfigurationFilePath;
      this.Configuration = ConfigurationManager.OpenMappedExeConfiguration(ConfigurationMap, ConfigurationUserLevel.None);
      this.SlayableSection = (SlayableConfigurationSection)Configuration.GetSection("slayableSection");
    }

    public void Execute()
    {
      ConfigureJumpList();

      if (Arguments.Count > 0)
      {
        foreach (string argument in Arguments)
        {
          if (argument.StartsWith(@"\"))
          {
            MessageBox.Show($"Incorrect switch {argument}. Use '-' or '/' instead of '\\'.");
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
                MessageBox.Show($"The switch parameter \"{SwitchParameter}\" must be of the form <Switch>:<Argument>");
                return;
              }
            }
            
            if (SwitchParameter.ToUpper() == "AlwaysPreview".ToUpper())
            {
              AlwaysPreview = true;
            }
            else
            {
              MessageBox.Show($"The switch parameter \"{SwitchParameter}\" is not recognised.");
              return; // do not continue running the application if there is something wrong with the parameters
            }
          }
          else
          {
            // arguments without switch indicator are assumed to be process names.
            // TODO: Currently only the last process name listed will be used.
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
            AlwaysPreview = Element.Preview;
            break;
          }
        }
      }

      Debug.Assert(ProcessName != String.Empty, $"{nameof(ProcessName)} cannot be an empty string");

      string sanitisedProcessName = ProcessName;

      if (sanitisedProcessName.EndsWith(".exe"))
        sanitisedProcessName = sanitisedProcessName.Remove(sanitisedProcessName.Length - ".exe".Length);

      // TODO: Should the app also handle the user passing in a path instead of 'friendly' process name?

      var ProcessesArray = Process.GetProcessesByName(sanitisedProcessName);

      if (ProcessesArray.Length > 0)
      {
        if ((ProcessesArray.Length == 1) && !AlwaysPreview)
        {
          //kill the process
          ProcessesArray[0].Kill();
        }
        else
        {
          Application.ShutdownMode = ShutdownMode.OnMainWindowClose;
          Application.DispatcherUnhandledException += (Sender, Event) =>
          {
            MessageBox.Show(Event.Exception.Message, "Exception");
            Event.Handled = true;
          };

          var MainWindow = new Window();
          Application.MainWindow = MainWindow;

          var VisualEngine = new SlayerVisualEngine()
          {
            Theme = ThemeHelper.Default(),
            ProcessList = new List<Process>(ProcessesArray),
            Application = Application
          };
          VisualEngine.Install(MainWindow);
          VisualEngine.KillAllEvent += () =>
          {
            VisualEngine.ProcessList.ForEach(P => P.Kill());
            Application.Shutdown();
          };
          VisualEngine.KillOldestEvent += () =>
          {
            var OldestProcess = VisualEngine.ProcessList.OrderBy(P => P.StartTime).First();

            OldestProcess.Kill();
            Application.Shutdown();
          };
          VisualEngine.KillYoungestEvent += () =>
          {
            var YoungestProcess = VisualEngine.ProcessList.OrderBy(P => P.StartTime).Last();

            YoungestProcess.Kill();
            Application.Shutdown();
          };
          VisualEngine.ProcessShowMeEvent += (Context) =>
          {
            NativeMethods.SetForegroundWindow(Context.MainWindowHandle);
          };
          VisualEngine.ProcessKillMeEvent += (Context) =>
          {
            Context.Kill();
            VisualEngine.RemoveProcess(Context);

            if (VisualEngine.ProcessList.Count < 1)
              Application.Shutdown();
          };
          VisualEngine.ProcessKillOthersEvent += (Context) =>
          {
            foreach (Process KillableProcess in VisualEngine.ProcessList.Where(searchprocess => searchprocess != Context))
              KillableProcess.Kill();

            Application.Shutdown();
          };

          MainWindow.Show();

          Application.Run();
        }
      }
    }

    private void ConfigureJumpList()
    {
      var JumpList = new JumpList();

      JumpList.JumpItems.Add(new JumpTask
      {
        CustomCategory = "Configuration",
        Title = "Edit configuration",
        ApplicationPath = ConfigurationFilePath,
        IconResourcePath = NativeMethodHelper.AssociatedApplicationPathForExtension(NativeMethods.AssocStr.Executable, Path.GetExtension(ConfigurationFilePath))
      });

      JumpList.JumpItems.Add(new JumpTask
      {
        CustomCategory = "Configuration",
        Title = "Open configuration location",
        ApplicationPath = "explorer.exe",
        Arguments = $"/select,\"{ConfigurationFilePath}\"",
        IconResourcePath = "explorer.exe",
        IconResourceIndex = 0
      });

      if (SlayableSection != null)
      {
        foreach (SlayableApplicationElement Element in SlayableSection.Applications)
        {
          var Task = new JumpTask
          {
            CustomCategory = "Slay",
            ApplicationPath = ApplicationFilePath,
            Arguments = Element.ProcessName,
            Title = Element.Name
          };
          JumpList.JumpItems.Add(Task);

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
