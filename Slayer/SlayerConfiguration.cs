using System.Collections;
using System.Configuration;
using System.Windows.Media;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

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

  [DataContract]
  internal sealed class ThemeColour
  {
    private Brush Brush;

    public ThemeColour(Color Colour)
    {
      this.Colour = Colour;
    }

    private Color Colour { get; set; }

    [DataMember]
    public string ColourText
    {
      get
      {
        return "#" + Colour.A.ToString("X2") + Colour.R.ToString("X2") + Colour.G.ToString("X2") + Colour.B.ToString("X2");
      }
      set
      {
        Colour = (Color)System.Windows.Media.ColorConverter.ConvertFromString(value);
      }
    }
    public Brush AsBrush
    {
      get
      {
        if (Brush == null)
        {
          var Converter = new System.Windows.Media.BrushConverter();

          this.Brush = new SolidColorBrush(Colour);
        }

        return this.Brush;
      }
    }
  }

  [DataContract]
  sealed class Theme
  {
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public ThemeColour ApplicationBackground { get;  set; }
    [DataMember]
    public ThemeColour ApplicationButtonBorder { get; set; }
    [DataMember]
    public ThemeColour ApplicationButtonToolbarBackground { get; set; }
    [DataMember]
    public ThemeColour ApplicationButtonBackground { get; set; }
    [DataMember]
    public ThemeColour ApplicationButtonForeground { get; set; }
    [DataMember]
    public ThemeColour ProcessHeadingForeground { get; set; }
    [DataMember]
    public ThemeColour ProcessBorderBackground { get; set; }
    [DataMember]
    public ThemeColour ProcessBorder { get; set; }
    [DataMember]
    public ThemeColour ProcessButtonBorder { get; set; }
    [DataMember]
    public ThemeColour ProcessButtonBackground { get; set; }
    [DataMember]
    public ThemeColour ProcessButtonForeground { get; set; }
    [DataMember]
    public ThemeColour ProcessCaptionForeground { get; set; }
    [DataMember]
    public ThemeColour ProcessDataForeground { get; set; }
  }

  static class ThemeHelper
  {
    public static Theme Default()
    {
      return new Theme
      {
        Name = "Default",
        ApplicationBackground = new ThemeColour(Colors.White),
        ApplicationButtonBorder = new ThemeColour(Colors.DarkGray),
        ApplicationButtonBackground = new ThemeColour(Colors.WhiteSmoke),
        ApplicationButtonForeground = new ThemeColour(Colors.OrangeRed),
        ProcessHeadingForeground = new ThemeColour(Colors.Black),
        ProcessBorderBackground = new ThemeColour(Colors.WhiteSmoke),
        ProcessBorder = new ThemeColour(Colors.DarkGray),
        ProcessButtonBorder = new ThemeColour(Colors.Transparent),
        ProcessButtonBackground = new ThemeColour(Colors.Transparent),
        ProcessButtonForeground = new ThemeColour(Colors.OrangeRed),
        ProcessCaptionForeground = new ThemeColour(Colors.Black),
        ProcessDataForeground = new ThemeColour(Colors.Black)
      };
    }
    public static Theme Load(SlayerColourThemeElement ThemeElement)
    {
      var Theme = new Theme();

      if (ThemeElement.Name != "")
        Theme.Name = ThemeElement.Name;

      if (ThemeElement.ApplicationBackground != "")
        Theme.ApplicationBackground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ApplicationBackground));
      
      if (ThemeElement.ApplicationButtonToolbarBackground != "")
        Theme.ApplicationButtonToolbarBackground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ApplicationButtonToolbarBackground));

      if (ThemeElement.ApplicationButtonBorder != "")
        Theme.ApplicationButtonBorder = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ApplicationButtonBorder));

      if (ThemeElement.ApplicationButtonBackground != "")
        Theme.ApplicationButtonBackground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ApplicationButtonBackground));

      if (ThemeElement.ApplicationButtonForeground != "")
        Theme.ApplicationButtonForeground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ApplicationButtonForeground));

      if (ThemeElement.ProcessHeadingForeground != "")
        Theme.ProcessHeadingForeground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ProcessHeadingForeground));

      if (ThemeElement.ProcessBorderBackground != "")
        Theme.ProcessBorderBackground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ProcessBorderBackground));

      if (ThemeElement.ProcessBorder != "")
        Theme.ProcessBorder = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ProcessBorder));

      if (ThemeElement.ProcessButtonBorder != "")
        Theme.ProcessButtonBorder = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ProcessButtonBorder));

      if (ThemeElement.ProcessButtonBackground != "")
        Theme.ProcessButtonBackground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ProcessButtonBackground));

      if (ThemeElement.ProcessButtonForeground != "")
        Theme.ProcessButtonForeground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ProcessButtonForeground));

      if (ThemeElement.ProcessCaptionForeground != "")
        Theme.ProcessCaptionForeground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ProcessCaptionForeground));

      if (ThemeElement.ProcessDataForeground != "")
        Theme.ProcessDataForeground = new ThemeColour((Color)System.Windows.Media.ColorConverter.ConvertFromString(ThemeElement.ProcessDataForeground));

      return Theme;
    }
  }

  public static class JsonHelper
  {
    public static void SaveToFile<T>(T Source, string Filename)
    {
      using (var Writer = new StreamWriter(Filename, false, Encoding.UTF8))
      {
        Writer.Write(ToJson(Source));
        Writer.Flush();
        Writer.Close();
      }
    }
    public static string ToJson<T>(T Source)
    {
      return Newtonsoft.Json.JsonConvert.SerializeObject(Source, Newtonsoft.Json.Formatting.Indented);
      //string Result = null;
      
      //var Serialiser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(Source.GetType());
      //using (var Stream = new MemoryStream())
      //{
      //  //Serialiser.WriteObject(Stream, Source);
      //  Serialiser.Serialize(Source);
      //  Result = Encoding.Default.GetString(Stream.ToArray());
      //}

      //return Result;
    }
  }
}