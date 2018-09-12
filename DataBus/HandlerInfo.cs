using System;

namespace DataBus
{
    public class HandlerInfo
    {
        public Type MessageHandlerType { get; set; }
        public Type MessageType { get; set; }
        public QueueGenerationType QueueGenerationType { get; set; }
        public string QueueName { get; set; }
    }
}