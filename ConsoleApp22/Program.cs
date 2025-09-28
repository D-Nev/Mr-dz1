using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    private const int Port = 27015;

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.Title = "CLIENT";
        PrintMenu();

        var ip = IPAddress.Loopback; 
        var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            client.Connect(new IPEndPoint(ip, Port));
            Console.WriteLine("Підключення встановлено.");

            using var ns = new NetworkStream(client, ownsSocket: false);
            using var reader = new StreamReader(ns, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            using var writer = new StreamWriter(ns, new UTF8Encoding(false), bufferSize: 1024, leaveOpen: true) { AutoFlush = true };

            string? greet = reader.ReadLine();
            if (greet != null)
                Console.WriteLine($"Сервер: {greet}");

            while (true)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                if (line is null) continue;

                writer.WriteLine(line);

                string? response = reader.ReadLine();

                if (response == null)
                {
                    Console.WriteLine("Сервер закрив з’єднання.");
                    break;
                }

                Console.WriteLine($"Сервер: {response}");

                if (line.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
                    line.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    string? maybeMore = reader.ReadLine();
                    Console.WriteLine("Сервер закрив з’єднання.");
                    break;
                }
            }
        }
        catch (SocketException se)
        {
            Console.WriteLine($"Помилка сокета: {se.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка клієнта: {ex.Message}");
        }
        finally
        {
            try { client.Close(); } catch {}
            Console.WriteLine("Клієнт завершив роботу.");
        }
    }

    private static void PrintMenu()
    {
        Console.WriteLine("Доступні команди до сервера:");
        Console.WriteLine("  help            — список команд");
        Console.WriteLine("  hello / привіт  — вітання");
        Console.WriteLine("  як справи       — відповідь: \"Чудово!\"");
        Console.WriteLine("  time / date     — поточний час / дата");
        Console.WriteLine("  inc <n>         — повертає n+1");
        Console.WriteLine("  <ціле число>    — також повертає число+1");
        Console.WriteLine("  quit            — попросити сервер закрити з’єднання");
        Console.WriteLine(new string('-', 50));
    }
}
