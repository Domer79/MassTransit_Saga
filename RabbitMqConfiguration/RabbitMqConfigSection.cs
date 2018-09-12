using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqConfiguration
{
    public class RabbitMqConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("mqhandlers")]
        public MqHandlers MqHandlers => (MqHandlers)base["mqhandlers"];

        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get => (string)base["host"];
            set => base["host"] = value;
        }
    }

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

    public class MqHandlerElement: ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true, IsKey = true)]
        public string TypeName
        {
            get => (string) base["type"];
            set => base["type"] = value;
        }

        [ConfigurationProperty("queue", IsRequired = true)]
        public string Queue
        {
            get => (string)base["queue"];
            set => base["queue"] = value;
        }

        public Type GetHandlerType()
        {
            return System.Type.GetType(TypeName);
        }
    }
}
