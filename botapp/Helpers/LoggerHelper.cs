using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Helpers
{
    public static class LoggerHelper
    {
        private static readonly Dictionary<string, StreamWriter> _writers = new Dictionary<string, StreamWriter>();
        private static readonly object _lock = new object();

        public static void Log(string logFilePath, string message)
        {
            lock (_lock)
            {
                if (!_writers.ContainsKey(logFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
                    _writers[logFilePath] = new StreamWriter(logFilePath, true) { AutoFlush = true };
                    _writers[logFilePath].WriteLine($"[{DateTime.Now}] Logger iniciado");
                }

                _writers[logFilePath].WriteLine($"[{DateTime.Now}] {message}");
            }
        }

        public static void Close(string logFilePath)
        {
            lock (_lock)
            {
                if (_writers.ContainsKey(logFilePath))
                {
                    _writers[logFilePath].WriteLine($"[{DateTime.Now}] Logger finalizado");
                    _writers[logFilePath].Close();
                    _writers.Remove(logFilePath);
                }
            }
        }
    }
}
