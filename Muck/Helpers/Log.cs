using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Muck
{
    public class Log
    {
        public static Log Instance { get; } = new Log();
        private readonly Dictionary<int, StringBuilder> storage = new Dictionary<int, StringBuilder>();

        public StringBuilder this[int lvl]
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
        public void AppendLine(string content, int lvl = 0)
        {
            this[lvl].AppendLine(content);
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
            return string.Join("\r\n", storage.Where(x=>x.Key<=lvl).OrderByDescending(x => x.Key).Select(x => x.Value.ToString()));
        }
    }
}