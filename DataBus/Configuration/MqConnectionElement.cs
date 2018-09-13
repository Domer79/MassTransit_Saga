using System.Configuration;

namespace DataBus.Configuration
{
    public class MqConnectionElement: ConfigurationElement
    {
        /// <summary>
        /// Наименование строки подключения
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public string Name
        {
            get => (string)base["name"];
            set => base["name"] = value;
        }

        /// <summary>
        /// Наименование хоста RabbitMq
        /// </summary>
        [ConfigurationProperty("url", IsRequired = true)]
        public string Url
        {
            get => (string)base["url"];
            set => base["url"] = value;
        }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        [ConfigurationProperty("user", IsRequired = true)]
        public string UserName
        {
            get => (string)base["user"];
            set => base["user"] = value;
        }

        /// <summary>
        /// Пароль
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get => (string)base["password"];
            set => base["password"] = value;
        }
    }
}