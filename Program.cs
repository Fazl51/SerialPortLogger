using System;
using System.IO;
using System.IO.Ports;
using Newtonsoft.Json;


class SerialPortLogger
{
    private static SerialPort _serialPort;
    private static string _logFile = "log.json";

    static void Main()
    {
        Console.WriteLine("=== Настройка COM-порта ===");

        string portName = Prompt("Имя порта", "COM9");
        int baudRate = int.Parse(Prompt("BaudRate", "9600"));
        int dataBits = int.Parse(Prompt("DataBits", "8"));

        Parity parity = Enum.TryParse(Prompt("Parity (None, Even, Odd, Mark, Space)", "None"), out Parity p) ? p : Parity.None;
        StopBits stopBits = Enum.TryParse(Prompt("StopBits (None, One, Two, OnePointFive)", "One"), out StopBits sb) ? sb : StopBits.One;

        bool dtrEnable = Prompt("DTR Enable (true/false)", "false").ToLower() == "true";
        bool rtsEnable = Prompt("RTS Enable (true/false)", "false").ToLower() == "true";

        try
        {
            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits,
                DtrEnable = dtrEnable,
                RtsEnable = rtsEnable,
                NewLine = "\r\n",
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };

            _serialPort.DataReceived += SerialDataReceived;
            _serialPort.Open();

            Console.WriteLine($"[{DateTime.Now}] Мониторинг {portName} начат. Нажмите Enter для остановки.");
            Console.ReadLine();

            _serialPort.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при открытии порта: {ex.Message}");
        }
    }

    private static void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string data = _serialPort.ReadLine().Trim();
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = data
            };

            AppendLog(logEntry);
            Console.WriteLine($"[{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss}] {logEntry.Message}");
        }
        catch (TimeoutException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка чтения: {ex.Message}");
        }
    }

    private static void AppendLog(LogEntry entry)
    {
        string jsonLine = JsonConvert.SerializeObject(entry);
        File.AppendAllText(_logFile, jsonLine + Environment.NewLine);
    }

    private static string Prompt(string label, string defaultValue)
    {
        Console.Write($"{label} [{defaultValue}]: ");
        var input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }
}
