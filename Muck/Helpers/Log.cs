using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Muck
{
    public class Log
    {
        public static Log Instance { get; } = new Log();
        private readonly List<LogEntry> log = new List<LogEntry>();
        private readonly Dictionary<int, StringBuilder> storage = new Dictionary<int, StringBuilder>();

        private StringBuilder this[int lvl]
        {
            get
            {
                if (!storage.ContainsKey(lvl))
                {
                    storage.Add(lvl, new StringBuilder());
                }
                return storage[lvl];
            }
            set
            {
                if (!storage.ContainsKey(lvl))
                {
                    storage.Add(lvl, value);
                }
                storage[lvl] = value;
            }
        }

        public void Append(string content, int lvl = 0)
        {
            this[lvl].Append(content);
        }
        public void AppendLine(string content, int lvl = 0, ConsoleColor color = ConsoleColor.Magenta)
        {
            log.Add(new LogEntry
            {
                Level = lvl,
                Message = this[lvl]+content,
                Color = color == ConsoleColor.Magenta?Console.ForegroundColor:color,
                Timestamp = DateTime.Now
            });
            this[lvl].Clear();
        }

        private Log()
        {
        }

        public override string ToString()
        {
            return ToString(100);
        }

        public string ToString(int lvl)
        {
            return string.Join("\r\n", log.Where(x=>x.Level<=lvl).OrderByDescending(x => x.Timestamp).Select(x => x.ToString()));
        }

        public static void Print(int lvl)
        {
            var temp = Console.ForegroundColor;
            foreach (var entry in Instance.log.Where(x=>x.Level<=lvl))
            {
                Console.ForegroundColor = entry.Color;

                Console.WriteLine(entry.ToString(Instance.log.Where(x => x.Level <= lvl).SelectMany(x=>x.Message.Split(new[] {"\r\n"}, StringSplitOptions.None)).Max(x=>x.Length)));
                
                Console.ForegroundColor = temp;
            }
        }

        private class LogEntry
        {
            public int Level { get; set; }
            public string Message { get; set; }
            public ConsoleColor Color { get; set; }
            public DateTime Timestamp { get; set; }

            public override string ToString()
            {
                return ToString(150);
            }

            public string ToString(int pad)
            {
                return $"[{Level}]{Message.PadRight(pad)} @ {Timestamp:yyyy MM dd hh mm ss fff}";
            }
        }
    }
}