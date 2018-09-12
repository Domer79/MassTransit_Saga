using System;
using System.Configuration;

namespace DataBus.Configuration
{
    public class RabbitMqConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("mqhandlers")]
        public MqHandlers MqHandlers => (MqHandlers)base["mqhandlers"];

        [ConfigurationProperty("connections"), ConfigurationCollection(typeof(MqConnections), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public MqConnections Connections => (MqConnections) base["connections"];
    }

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

    public class MqConnectionElement: ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public string Name
        {
            get => (string)base["name"];
            set => base["name"] = value;
        }

        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get => (string)base["host"];
            set => base["host"] = value;
        }

        [ConfigurationProperty("user", IsRequired = true)]
        public string UserName
        {
            get => (string)base["user"];
            set => base["user"] = value;
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get => (string)base["password"];
            set => base["password"] = value;
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
            var handlerType = System.Type.GetType(TypeName);
            if (handlerType == null)
                throw new ArgumentException(TypeName);

            return handlerType;
        }
    }
}
