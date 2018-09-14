using System.Configuration;

namespace DataBus.Configuration
{
    public class RabbitMqConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("mqhandlers")]
        public MqHandlers MqHandlers => (MqHandlers)base["mqhandlers"];

        [ConfigurationProperty("connections"), ConfigurationCollection(typeof(MqConnections), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public MqConnections Connections => (MqConnections) base["connections"];

        [ConfigurationProperty("queues")]
        public MqQueues Queues => (MqQueues) base["queues"];

        [ConfigurationProperty("busSettings")]
        public BusSettings BusSettings => (BusSettings) base["busSettings"];
    }
}
