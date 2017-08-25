using System.Windows.Media;

namespace Slayer
{
  sealed class Theme
  {
    public Theme(ApplicationTheme ApplicationTheme, ProcessTheme ProcessTheme)
    {
      this.Application = ApplicationTheme;
      this.Process = ProcessTheme;
    }

    public readonly ApplicationTheme Application;
    public readonly ProcessTheme Process;
  }

  sealed class ApplicationTheme
  {
    public ApplicationTheme(
      Brush ProcessNameHeaderBackground,
      Brush Background,
      Brush ButtonBorder,
      Brush ButtonToolbarBackground,
      Brush ButtonBackground,
      Brush ButtonForeground)
    {
      this.ProcessNameHeaderBackground = ProcessNameHeaderBackground;
      this.Background = Background;
      this.ButtonBorder = ButtonBorder;
      this.ButtonToolbarBackground = ButtonToolbarBackground;
      this.ButtonBackground = ButtonBackground;
      this.ButtonForeground = ButtonForeground;
    }

    public readonly Brush ProcessNameHeaderBackground;
    public readonly Brush Background;
    public readonly Brush ButtonBorder;
    public readonly Brush ButtonToolbarBackground;
    public readonly Brush ButtonBackground;
    public readonly Brush ButtonForeground;
  }

  sealed class ProcessTheme
  {
    public ProcessTheme(
      Brush HeadingForeground,
      Brush BorderBackground,
      Brush Border,
      Brush ButtonBorder,
      Brush ButtonBackground,
      Brush ButtonForeground,
      Brush CaptionForeground,
      Brush DataForeground)
    {
      this.HeadingForeground = HeadingForeground;
      this.BorderBackground = BorderBackground;
      this.Border = Border;
      this.ButtonBorder = ButtonBorder;
      this.ButtonBackground = ButtonBackground;
      this.ButtonForeground = ButtonForeground;
      this.CaptionForeground = CaptionForeground;
      this.DataForeground = DataForeground;
    }

    public readonly Brush HeadingForeground;
    public readonly Brush BorderBackground;
    public readonly Brush Border;
    public readonly Brush ButtonBorder;
    public readonly Brush ButtonBackground;
    public readonly Brush ButtonForeground;
    public readonly Brush CaptionForeground;
    public readonly Brush DataForeground;
  }

  static class ThemeHelper
  {
    public static Theme Default()
    {
      return new Theme
      (
        ApplicationTheme: new ApplicationTheme(
          ProcessNameHeaderBackground: Brushes.DarkGray,
          Background: Brushes.White,
          ButtonBorder: Brushes.DimGray,
          ButtonToolbarBackground: Brushes.Transparent,
          ButtonBackground: Brushes.WhiteSmoke,
          ButtonForeground: Brushes.OrangeRed),

        ProcessTheme: new ProcessTheme(
          HeadingForeground: Brushes.Black,
          BorderBackground: Brushes.WhiteSmoke,
          Border: Brushes.DimGray,
          ButtonBorder: Brushes.Transparent,
          ButtonBackground: Brushes.Transparent,
          ButtonForeground: Brushes.OrangeRed,
          CaptionForeground: Brushes.Black,
          DataForeground: Brushes.Black)
      );
    }
  }
}