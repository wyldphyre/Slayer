using System.Configuration;
using System.Windows.Media;

namespace Slayer
{
  class SlayableApplicationElement : ConfigurationElement
  {

    [ConfigurationProperty("name", IsKey=true, IsRequired=true)]
    public string Name
    {
      get { return (string)this["name"]; }
    }

    [ConfigurationProperty("processName", IsKey = true, IsRequired = true)]
    public string ProcessName
    {
      get { return (string)this["processName"]; }
    }

    [ConfigurationProperty("preview", IsKey=true, IsRequired=false)]
    public bool Preview
    {
      get { return (bool)this["preview"]; }
    }

    [ConfigurationProperty("default", IsKey=true, IsRequired=false)]
    public bool Default
    {
      get { return (bool)this["default"]; }
    }
  }

  class SlayableApplicationElementCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new SlayableApplicationElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((SlayableApplicationElement)element).Name;
    }
  }

  class SlayableConfigurationSection : ConfigurationSection
  {
    [ConfigurationProperty("applications")]
    public SlayableApplicationElementCollection Applications
    {
      get { return (SlayableApplicationElementCollection)this["applications"]; }
    }
  }

  sealed class Theme
  {
    public Theme(
      Brush ApplicationProcessNameHeaderBackground,
      Brush ApplicationBackground,
      Brush ApplicationButtonBorder,
      Brush ApplicationButtonToolbarBackground,
      Brush ApplicationButtonBackground,
      Brush ApplicationButtonForeground,
      Brush ProcessHeadingForeground,
      Brush ProcessBorderBackground,
      Brush ProcessBorder,
      Brush ProcessButtonBorder,
      Brush ProcessButtonBackground,
      Brush ProcessButtonForeground,
      Brush ProcessCaptionForeground,
      Brush ProcessDataForeground)
    {
      this.ApplicationProcessNameHeaderBackground = ApplicationProcessNameHeaderBackground;
      this.ApplicationBackground = ApplicationBackground;
      this.ApplicationButtonBorder = ApplicationButtonBorder;
      this.ApplicationButtonToolbarBackground = ApplicationButtonToolbarBackground;
      this.ApplicationButtonBackground = ApplicationButtonBackground;
      this.ApplicationButtonForeground = ApplicationButtonForeground;
      this.ProcessHeadingForeground = ProcessHeadingForeground;
      this.ProcessBorderBackground = ProcessBorderBackground;
      this.ProcessBorder = ProcessBorder;
      this.ProcessButtonBorder = ProcessButtonBorder;
      this.ProcessButtonBackground = ProcessButtonBackground;
      this.ProcessButtonForeground = ProcessButtonForeground;
      this.ProcessCaptionForeground = ProcessCaptionForeground;
      this.ProcessDataForeground = ProcessDataForeground;
    }

    public readonly Brush ApplicationProcessNameHeaderBackground;
    public readonly Brush ApplicationBackground;
    public readonly Brush ApplicationButtonBorder;
    public readonly Brush ApplicationButtonToolbarBackground;
    public readonly Brush ApplicationButtonBackground;
    public readonly Brush ApplicationButtonForeground;
    public readonly Brush ProcessHeadingForeground;
    public readonly Brush ProcessBorderBackground;
    public readonly Brush ProcessBorder;
    public readonly Brush ProcessButtonBorder;
    public readonly Brush ProcessButtonBackground;
    public readonly Brush ProcessButtonForeground;
    public readonly Brush ProcessCaptionForeground;
    public readonly Brush ProcessDataForeground;
  }

  static class ThemeHelper
  {
    public static Theme Default()
    {
      return new Theme
      (
        ApplicationProcessNameHeaderBackground: Brushes.DarkGray,
        ApplicationBackground: Brushes.White,
        ApplicationButtonBorder: Brushes.DarkGray,
        ApplicationButtonToolbarBackground: Brushes.Transparent,
        ApplicationButtonBackground: Brushes.WhiteSmoke,
        ApplicationButtonForeground: Brushes.OrangeRed,
        ProcessHeadingForeground: Brushes.Black,
        ProcessBorderBackground: Brushes.WhiteSmoke,
        ProcessBorder: Brushes.DarkGray,
        ProcessButtonBorder: Brushes.Transparent,
        ProcessButtonBackground: Brushes.Transparent,
        ProcessButtonForeground: Brushes.OrangeRed,
        ProcessCaptionForeground: Brushes.Black,
        ProcessDataForeground: Brushes.Black
      );
    }
  }
}