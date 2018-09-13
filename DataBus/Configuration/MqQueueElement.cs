using System.Configuration;

namespace DataBus.Configuration
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
        /// Количество предварительно выбранных сообщений, которые шина забирает из очереди за один раз перед их обработкой
        /// </summary>
        [ConfigurationProperty("prefetchCountToCore", DefaultValue = 1)]
        public int PrefetchCountToCore
        {
            get => (int)base["prefetchCountToCore"];
            set => base["prefetchCountToCore"] = value;
        }

        /// <summary>
        /// Флаг, указывающий, что количество потоков должно равняться количеству ядер процессора системы
        /// </summary>
        [ConfigurationProperty("threadsOfCore")]
        public bool ThreadsOfCore
        {
            get => (bool)base["threadsOfCore"];
            set => base["threadsOfCore"] = value;
        }
    }
}