using System.Collections;
/*! 1 !*/
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

  class SlayerColourThemeSection : ConfigurationSection
  {
    [ConfigurationProperty("theme", IsKey = true, IsRequired = false)]
    public string Theme
    {
      get { return (string)this["theme"]; }
      set { this["theme"] = value; }
    }
    [ConfigurationProperty("themes")]
    public SlayerColourThemeElementCollection Themes
    {
      get { return (SlayerColourThemeElementCollection)this["themes"]; }
    }
  }
  class SlayerColourThemeElementCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new SlayerColourThemeElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((SlayerColourThemeElement)element).Name;
    }
  }

  class SlayerColourThemeElement : ConfigurationElement
  {
    [ConfigurationProperty("Name", IsKey = true, IsRequired = true)]
    public string Name
    {
      get { return (string)this["Name"]; }
    }
    [ConfigurationProperty("ApplicationBackground", IsKey = true, IsRequired = false)]
    public string ApplicationBackground
    {
      get { return (string)this["ApplicationBackground"]; }
    }
    [ConfigurationProperty("ApplicationButtonToolbarBackground", IsKey = true, IsRequired = false)]
    public string ApplicationButtonToolbarBackground
    {
      get { return (string)this["ApplicationButtonToolbarBackground"]; }
    }
    [ConfigurationProperty("ApplicationButtonBorder", IsKey = true, IsRequired = false)]
    public string ApplicationButtonBorder
    {
      get { return (string)this["ApplicationButtonBorder"]; }
    }
    [ConfigurationProperty("ApplicationButtonBackground", IsKey = true, IsRequired = false)]
    public string ApplicationButtonBackground
    {
      get { return (string)this["ApplicationButtonBackground"]; }
    }
    [ConfigurationProperty("ApplicationButtonForeground", IsKey = true, IsRequired = false)]
    public string ApplicationButtonForeground
    {
      get { return (string)this["ApplicationButtonForeground"]; }
    }
    [ConfigurationProperty("ProcessHeadingForeground", IsKey = true, IsRequired = false)]
    public string ProcessHeadingForeground
    {
      get { return (string)this["ProcessHeadingForeground"]; }
    }
    [ConfigurationProperty("ProcessBorderBackground", IsKey = true, IsRequired = false)]
    public string ProcessBorderBackground
    {
      get { return (string)this["ProcessBorderBackground"]; }
    }
    [ConfigurationProperty("ProcessBorder", IsKey = true, IsRequired = false)]
    public string ProcessBorder
    {
      get { return (string)this["ProcessBorder"]; }
    }
    [ConfigurationProperty("ProcessButtonBorder", IsKey = true, IsRequired = false)]
    public string ProcessButtonBorder
    {
      get { return (string)this["ProcessButtonBorder"]; }
    }
    [ConfigurationProperty("ProcessButtonBackground", IsKey = true, IsRequired = false)]
    public string ProcessButtonBackground
    {
      get { return (string)this["ProcessButtonBackground"]; }
    }
    [ConfigurationProperty("ProcessButtonForeground", IsKey = true, IsRequired = false)]
    public string ProcessButtonForeground
    {
      get { return (string)this["ProcessButtonForeground"]; }
    }
    [ConfigurationProperty("ProcessCaptionForeground", IsKey = true, IsRequired = false)]
    public string ProcessCaptionForeground
    {
      get { return (string)this["ProcessCaptionForeground"]; }
    }
    [ConfigurationProperty("ProcessDataForeground", IsKey = true, IsRequired = false)]
    public string ProcessDataForeground
    {
      get { return (string)this["ProcessDataForeground"]; }
    }
  }

  sealed class Theme
  {
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
    public static Theme Load(SlayerColourThemeElement ThemeElement)
    {
      var Theme = new Theme();
      var Converter = new System.Windows.Media.BrushConverter();

      if (ThemeElement.ApplicationBackground != "")
        Theme.ApplicationBackground = (Brush)Converter.ConvertFromString(ThemeElement.ApplicationBackground);

      if (ThemeElement.ApplicationButtonToolbarBackground != "")
        Theme.ApplicationButtonToolbarBackground = (Brush)Converter.ConvertFromString(ThemeElement.ApplicationButtonToolbarBackground);

      if (ThemeElement.ApplicationButtonBorder != "")
        Theme.ApplicationButtonBorder = (Brush)Converter.ConvertFromString(ThemeElement.ApplicationButtonBorder);

      if (ThemeElement.ApplicationButtonBackground != "")
        Theme.ApplicationButtonBackground = (Brush)Converter.ConvertFromString(ThemeElement.ApplicationButtonBackground);

      if (ThemeElement.ApplicationButtonForeground != "")
        Theme.ApplicationButtonForeground = (Brush)Converter.ConvertFromString(ThemeElement.ApplicationButtonForeground);

      if (ThemeElement.ProcessHeadingForeground != "")
        Theme.ProcessHeadingForeground = (Brush)Converter.ConvertFromString(ThemeElement.ProcessHeadingForeground);

      if (ThemeElement.ProcessBorderBackground != "")
        Theme.ProcessBorderBackground = (Brush)Converter.ConvertFromString(ThemeElement.ProcessBorderBackground);

      if (ThemeElement.ProcessBorder != "")
        Theme.ProcessBorder = (Brush)Converter.ConvertFromString(ThemeElement.ProcessBorder);

      if (ThemeElement.ProcessButtonBorder != "")
        Theme.ProcessButtonBorder = (Brush)Converter.ConvertFromString(ThemeElement.ProcessButtonBorder);

      if (ThemeElement.ProcessButtonBackground != "")
        Theme.ProcessButtonBackground = (Brush)Converter.ConvertFromString(ThemeElement.ProcessButtonBackground);

      if (ThemeElement.ProcessButtonForeground != "")
        Theme.ProcessButtonForeground = (Brush)Converter.ConvertFromString(ThemeElement.ProcessButtonForeground);

      if (ThemeElement.ProcessCaptionForeground != "")
        Theme.ProcessCaptionForeground = (Brush)Converter.ConvertFromString(ThemeElement.ProcessCaptionForeground);

      if (ThemeElement.ProcessDataForeground != "")
        Theme.ProcessDataForeground = (Brush)Converter.ConvertFromString(ThemeElement.ProcessDataForeground);

      return Theme;
    }
  }
}