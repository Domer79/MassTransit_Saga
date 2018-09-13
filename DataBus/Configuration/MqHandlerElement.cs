using System;
using System.Configuration;
using DataBus.Interfaces;

namespace DataBus.Configuration
{
    public class MqHandlerElement: ConfigurationElement
    {
        /// <summary>
        /// Тип обработчика сообщений.
        /// </summary>
        /// <remarks>
        /// Должен быть унаследован от <see cref="MessageHandler{TMessage}"/>
        /// </remarks>
        [ConfigurationProperty("type", IsRequired = true, IsKey = true)]
        public string TypeName
        {
            get => (string) base["type"];
            set => base["type"] = value;
        }

        /// <summary>
        /// Имя очереди
        /// </summary>
        /// <remarks>
        /// Для дополнительной настройки параметров очереди нужно использовать раздел "queues" в конфигурационном файле
        /// </remarks>
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