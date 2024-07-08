using NutritionWarriorAuthentication.Application.Common;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionWarriorAuthentication.Application.System.Cache
{
    public class ConnectionHelper
    {
        static ConnectionHelper()
        {
            //ConnectionHelper.lazyConnection = new Lazy<ConnectionMultiplexer>(() => {
            //    return ConnectionMultiplexer.Connect(ConfigurationManager.AppSetting["RedisURL"]);
            //});
            ConnectionHelper.lazyConnection = new Lazy<ConnectionMultiplexer>(() => {
                return ConnectionMultiplexer.Connect("redis-19734.c56.east-us.azure.redns.redis-cloud.com:19734,password=dXZtWYpaSKpnFBMeVk5i8nWRp7QXnjp9");
            });
        }
        private static Lazy<ConnectionMultiplexer> lazyConnection;
        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}
