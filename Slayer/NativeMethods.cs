using System;
using System.Runtime.InteropServices;
using System.Text;

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
      var ret = NativeMethods.AssocQueryString(NativeMethods.AssocF.NoUserSettings, association, extension, null, null, ref length);
      if (ret != S_FALSE)
        throw new InvalidOperationException("Could not determine associated string");

      var sb = new StringBuilder((int)length); // (length-1) will probably work too as the marshaller adds null termination
      ret = NativeMethods.AssocQueryString(NativeMethods.AssocF.NoUserSettings, association, extension, null, sb, ref length);
      if (ret != S_OK)
        throw new InvalidOperationException("Could not determine associated string");

      return sb.ToString();
    }
  }
}
