using RabbitMQ.Client;
using System.Runtime.CompilerServices;
using System.Text;

namespace NutritionWarriorAuthentication.Api.Queues
{
    public static class Send
    {
        public static void CreateUser(string id, string email, string phone_number, string name)
        {
           
            string queueName = "create_user_queue";
            string queueName2 = "update_user_queue";
            var factory = new ConnectionFactory() { Uri =new Uri("amqps://vqhjbecl:HbrY80nH1aBsmJ8oyFBsf43TeY5NM3Za@turkey.rmq.cloudamqp.com/vqhjbecl") };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var message = new
                {
                    id = id,
                    email = email,
                    phone_number = phone_number,
                    name = name
                };
                var message2 = new
                {
                    id = id,
                    email = email,
                    name = name,
                    image = ""
                };

                // Chuyển đổi dữ liệu sang dạng byte để gửi lên queue
                var body = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message));
                var body2 = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message2));
                // Đẩy message vào queue
                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName2,
                                     basicProperties: null,
                                     body: body2);

                Console.WriteLine($"[x] Sent message: {message}");
            }

        }
    }
}
