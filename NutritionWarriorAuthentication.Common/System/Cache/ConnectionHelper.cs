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
                return ConnectionMultiplexer.Connect("127.0.0.1:6379");
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
