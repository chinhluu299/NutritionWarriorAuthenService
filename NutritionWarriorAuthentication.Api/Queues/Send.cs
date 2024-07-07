using RabbitMQ.Client;
using System.Text;

namespace NutritionWarriorAuthentication.Api.Queues
{
    public static class Send
    {
        public static void CreateUser(Object data)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost", // Thay đổi thành địa chỉ RabbitMQ server của bạn nếu cần
                UserName = "guest",
                Password = "guest"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Đảm bảo queue tồn tại trước khi publish message
                channel.QueueDeclare(queue: "create_user_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // Tạo message
                string message = "Thông tin tạo user"; // Thay đổi thông tin message tùy theo yêu cầu của bạn
                var body = Encoding.UTF8.GetBytes(message);

                // Publish message vào queue
                channel.BasicPublish(exchange: "",
                                     routingKey: "create_user_queue",
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine($"Đã gửi message: {message}");
            }

            Console.WriteLine("Nhấn phím bất kỳ để thoát.");
            Console.ReadLine();

        }
    }
}
