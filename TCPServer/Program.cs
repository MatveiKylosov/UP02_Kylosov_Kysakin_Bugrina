using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServerExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 1337; // Порт для прослушивания
            IPAddress ipAddress = IPAddress.Any; // Принимаем подключения со всех интерфейсов

            TcpListener listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine($"Сервер запущен на порту {port}. Ожидание подключений...");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Клиент подключился!");

                // Обработка подключения в отдельном асинхронном потоке
                _ = HandleClientAsync(client);
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                {
                    NetworkStream stream = client.GetStream();

                    // Чтение данных от клиента
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Получено от клиента: " + receivedMessage);

                    // Подготовка ответа
                    string responseMessage = "Привет от сервера!";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);

                    // Отправка ответа клиенту
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine("Ответ отправлен клиенту.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обработке клиента: " + ex.Message);
            }
        }
    }
}
