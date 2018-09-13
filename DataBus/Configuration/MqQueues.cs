using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBus.Configuration
{
    [ConfigurationCollection(typeof(MqQueueElement), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
    public class MqQueues: ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MqQueueElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MqQueueElement) element).Name;
        }

        public MqQueueElement this[int index] => (MqQueueElement)BaseGet(index);

        public new MqQueueElement this[string name] => (MqQueueElement)BaseGet(name);
    }
}
