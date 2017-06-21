namespace Slayer
{
  static class Program
  {
    [System.STAThread]
    static void Main(string[] Arguments)
    {
      var slayerApplication = new SlayerApplication();

      slayerApplication.Arguments.AddRange(Arguments);

      slayerApplication.Execute();
    }
  }
}
