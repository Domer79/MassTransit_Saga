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

using System.Configuration;

namespace DataBusService.Configuration
{
    public class MqQueueElement: ConfigurationElement
    {
        /// <summary>
        /// Имя очереди, для которой необходимо сделать настройку
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get => (string) base["name"];
            set => base["name"] = value;
        }

        /// <summary>
        /// Количество потоков, которое необходимо задействовать для обработки сообщений
        /// </summary>
        [ConfigurationProperty("threadCount", DefaultValue = 1)]
        public int ThreadCount
        {
            get => (int)base["threadCount"];
            set => base["threadCount"] = value;
        }

        /// <summary>
        /// Количество предварительно выбранных сообщений для одного потока, которые шина забирает из очереди за один раз перед их обработкой
        /// </summary>
        [ConfigurationProperty("prefetchCountToThread", DefaultValue = 1)]
        public int PrefetchCountToThread
        {
            get => (int)base["prefetchCountToThread"];
            set => base["prefetchCountToThread"] = value;
        }

        /// <summary>
        /// Флаг, указывающий, что количество потоков должно равняться количеству ядер процессора системы
        /// </summary>
        /// <remarks>
        /// Этот параметр переопределяет параметр <see cref="ThreadCount"/>
        /// </remarks>
        [ConfigurationProperty("threadsByCoreCount")]
        public bool ThreadsByCoreCount
        {
            get => (bool)base["threadsByCoreCount"];
            set => base["threadsByCoreCount"] = value;
        }
    }
}