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

    [ConfigurationProperty("ProcessBorderBackground", IsKey = true, IsRequired = false)]
    public string ProcessBorderBackground
    {
      get { return (string)this["ProcessBorderBackground"]; }
    }
    [ConfigurationProperty("ProcessBorderBorder", IsKey = true, IsRequired = false)]
    public string ProcessBorderBorder
    {
      get { return (string)this["ProcessBorderBorder"]; }
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

  sealed class ColourTheme
  {
    public Brush ApplicationBackground { get; private set; }
    public Brush ApplicationButtonBorder { get; private set; }
    public Brush ApplicationButtonBackground { get; private set; }
    public Brush ApplicationButtonForeground { get; private set; }

    public Brush ProcessBorderBackground { get; private set; }
    public Brush ProcessBorderBorder { get; private set; }
    public Brush ProcessButtonBorder { get; private set; }
    public Brush ProcessButtonBackground { get; private set; }
    public Brush ProcessButtonForeground { get; private set; }
    public Brush ProcessCaptionForeground { get; private set; }
    public Brush ProcessDataForeground { get; private set; }

    public void Default()
    {
      ApplicationBackground = Brushes.White;
      ApplicationButtonBorder = Brushes.DarkGray;
      ApplicationButtonBackground = Brushes.WhiteSmoke;
      ApplicationButtonForeground = Brushes.OrangeRed;
    
      ProcessBorderBackground = Brushes.WhiteSmoke;
      ProcessBorderBorder = Brushes.DarkGray;
      ProcessButtonBorder = Brushes.Transparent;
      ProcessButtonBackground = Brushes.Transparent;
      ProcessButtonForeground = Brushes.OrangeRed;
      ProcessCaptionForeground = Brushes.Black;
      ProcessDataForeground = Brushes.Black;
    }
    public void Load(SlayerColourThemeElement ThemeElement)
    {
      var Converter = new System.Windows.Media.BrushConverter();

      if (ThemeElement.ApplicationBackground != "")
        ApplicationBackground = (Brush)Converter.ConvertFromString(ThemeElement.ApplicationBackground);
      
      //ApplicationButtonBorder;
      //ApplicationButtonBackground;
      //ApplicationButtonForeground;

      //ProcessBorderBackground;
      //ProcessBorderBorder;
      //ProcessButtonBorder;
      //ProcessButtonBackground;
      //ProcessButtonForeground;

      if (ThemeElement.ProcessCaptionForeground != "")
        ProcessCaptionForeground = (Brush)Converter.ConvertFromString(ThemeElement.ProcessCaptionForeground);

      if (ThemeElement.ProcessDataForeground != "")
        ProcessDataForeground = (Brush)Converter.ConvertFromString(ThemeElement.ProcessDataForeground);
    }
  }
}