namespace Slayer
{
  static class AssemblyHelper
  {
    public static System.Reflection.AssemblyName ExecutingAssemblyName()
    {
      return System.Reflection.Assembly.GetExecutingAssembly().GetName();
    }
    public static string ExecutingAssemblyVersion()
    {
      return ExecutingAssemblyName().Version.ToString();
    }
    public static string CompactExecutingAssemblyVersion()
    {
      return ExecutingAssemblyName().Version.ToString().TrimEnd('.', '0');
    }
  }
}