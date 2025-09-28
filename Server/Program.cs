using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Server
{
    private const int Port = 27015;

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "SERVER";
        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(new IPEndPoint(IPAddress.Any, Port));
            listener.Listen(100);
            Console.WriteLine($"Сервер слухає на 0.0.0.0:{Port}");

            while (true)
            {
                Socket client = listener.Accept();
                Task.Run(() => HandleClient(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка сервера: {ex.Message}");
        }
        finally
        {
            try { listener.Close(); } catch {}
        }
    }

    private static void HandleClient(Socket client)
    {
        Console.WriteLine($"Клієнт підключився: {client.RemoteEndPoint}");

        try
        {
            using var ns = new NetworkStream(client, ownsSocket: false);
            using var reader = new StreamReader(ns, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            using var writer = new StreamWriter(ns, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true)
            {
                AutoFlush = true
            };

            writer.WriteLine("Вітаю на сервері. Надішліть 'help' для списку команд.");

            while (true)
            {
                string? line = reader.ReadLine(); 
                if (line == null)
                {
                    Console.WriteLine("Клієнт розірвав з’єднання.");
                    break;
                }

                line = line.Trim();
                Console.WriteLine($"<-- {client.RemoteEndPoint}: \"{line}\"");

                if (line.Equals("help", StringComparison.OrdinalIgnoreCase) ||
                    line.Equals("довідка", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteLine("Команди: hello | як справи | time | date | inc <n> або просто ціле число | quit");
                    continue;
                }

                if (line.Equals("hello", StringComparison.OrdinalIgnoreCase) ||
                    line.Equals("привіт", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteLine("Вітаю!");
                    continue;
                }

                if (line.Equals("як справи", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteLine("Чудово!");
                    continue;
                }

                if (line.Equals("time", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff"));
                    continue;
                }
                if (line.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd"));
                    continue;
                }

                if (line.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
                    line.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    writer.WriteLine("З'єднання закривається сервером. До побачення!");
                    break; 
                }

                if (line.StartsWith("inc ", StringComparison.OrdinalIgnoreCase))
                {
                    string arg = line.Substring(4).Trim();
                    if (int.TryParse(arg, out int n))
                        writer.WriteLine(n + 1);
                    else
                        writer.WriteLine("Помилка: очікувалось ціле число після 'inc'.");
                    continue;
                }

                if (int.TryParse(line, out int numberOnly))
                {
                    writer.WriteLine(numberOnly + 1);
                    continue;
                }

                writer.WriteLine("Невідома команда. Спробуйте 'help'.");
            }
        }
        catch (IOException)
        {
            Console.WriteLine("З’єднання перервано під час обміну.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка обробки клієнта: {ex.Message}");
        }
        finally
        {
            try
            {
                client.Shutdown(SocketShutdown.Both);
            }
            catch {}
            finally
            {
                client.Close();
                Console.WriteLine("Сесію з клієнтом закрито сервером.");
            }
        }
    }
}
