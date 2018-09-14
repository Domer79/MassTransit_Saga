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