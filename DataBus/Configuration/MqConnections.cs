using System.Configuration;

namespace DataBus.Configuration
{
    [ConfigurationCollection(typeof(MqConnectionElement))]
    public class MqConnections: ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MqConnectionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MqConnectionElement)element).Name;
        }

        public MqConnectionElement this[int index] => (MqConnectionElement)BaseGet(index);

        public new MqConnectionElement this[string name] => (MqConnectionElement)BaseGet(name);
    }
}