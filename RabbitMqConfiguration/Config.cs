using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqConfiguration
{
    public class Config
    {
        public static RabbitMqConfigSection GetRabbitMqConfigSection()
        {
            return (RabbitMqConfigSection)ConfigurationManager.GetSection("rabbitmq");
        }
    }
}
