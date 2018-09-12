using System.Configuration;

namespace DataBus.Configuration
{
    public class Config
    {
        public static RabbitMqConfigSection GetRabbitMqConfigSection()
        {
            return (RabbitMqConfigSection)ConfigurationManager.GetSection("rabbitmq");
        }
    }
}
