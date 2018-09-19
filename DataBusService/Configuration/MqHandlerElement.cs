//Copyright 2018 Damir Garipov
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Configuration;
using DataBusService.Interfaces;

namespace DataBusService.Configuration
{
    public class MqHandlerElement: ConfigurationElement
    {
        /// <summary>
        /// Тип обработчика сообщений.
        /// </summary>
        /// <remarks>
        /// Должен быть унаследован от <see cref="BaseMessageHandler{TMessage}"/>
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
            var handlerType = Type.GetType(TypeName);
            if (handlerType == null)
                throw new ArgumentException($"Type {TypeName} not found");

            return handlerType;
        }
    }
}