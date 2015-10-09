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
    public Brush ApplicationProcessNameHeaderBackground { get; set; }
    public Brush ApplicationBackground { get;  set; }
    public Brush ApplicationButtonBorder { get;  set; }
    public Brush ApplicationButtonToolbarBackground { get; set; }
    public Brush ApplicationButtonBackground { get;  set; }
    public Brush ApplicationButtonForeground { get;  set; }
    public Brush ProcessHeadingForeground { get;  set; }
    public Brush ProcessBorderBackground { get;  set; }
    public Brush ProcessBorder { get;  set; }
    public Brush ProcessButtonBorder { get;  set; }
    public Brush ProcessButtonBackground { get;  set; }
    public Brush ProcessButtonForeground { get;  set; }
    public Brush ProcessCaptionForeground { get;  set; }
    public Brush ProcessDataForeground { get;  set; }
  }

  static class ThemeHelper
  {
    public static Theme Default()
    {
      return new Theme
      {
        ApplicationProcessNameHeaderBackground = Brushes.DarkGray,
        ApplicationBackground = Brushes.White,
        ApplicationButtonBorder = Brushes.DarkGray,
        ApplicationButtonBackground = Brushes.WhiteSmoke,
        ApplicationButtonForeground = Brushes.OrangeRed,
        ProcessHeadingForeground = Brushes.Black,
        ProcessBorderBackground = Brushes.WhiteSmoke,
        ProcessBorder = Brushes.DarkGray,
        ProcessButtonBorder = Brushes.Transparent,
        ProcessButtonBackground = Brushes.Transparent,
        ProcessButtonForeground = Brushes.OrangeRed,
        ProcessCaptionForeground = Brushes.Black,
        ProcessDataForeground = Brushes.Black
      };
    }
  }
}