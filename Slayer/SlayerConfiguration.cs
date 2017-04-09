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
}