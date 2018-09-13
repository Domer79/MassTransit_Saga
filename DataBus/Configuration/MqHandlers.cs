using System.Configuration;

namespace DataBus.Configuration
{
    [ConfigurationCollection(typeof(MqHandlerElement))]
    public class MqHandlers: ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MqHandlerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MqHandlerElement)element);
        }

        public MqHandlerElement this[int index] => (MqHandlerElement)BaseGet(index);
    }
}