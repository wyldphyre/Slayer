/*! 1 !*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Windows.Media;
using System.Diagnostics;

namespace Slayer
{
  static class Program
  {
    [System.STAThread]
    static void Main(string[] Arguments)
    {
      SlayerApplication slayerApplication = new SlayerApplication();
      slayerApplication.SetupJumpList();

      slayerApplication.Arguments.AddRange(Arguments);

      slayerApplication.Execute();
    }
  }
}
