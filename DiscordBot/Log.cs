using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WowBot
{
    class Log
    {
        private const string path = "bot.log";

        enum LOGLEVEL
        {
            INFO,
            DEBUG,
            WARN,
            ERROR
        }

        static Log()
        {

        }

        private static void WriteLog(LOGLEVEL level, string tag, string text)
        {
            string date = DateTime.Now.ToString("HH:mm");
            string fullText = $"[{date}][{tag}][{level}]{text}";
            Console.WriteLine(fullText);
            
            using (StreamWriter writer = File.AppendText(path))
            {
                writer.WriteLine($"{fullText}");
            }
        }

        public static void Any(string text)
        {
            WriteLog(LOGLEVEL.ERROR, "", text);
        }

        public static void Info(string tag, string text)
        {
            WriteLog(LOGLEVEL.INFO, tag, text);
        }

        public static void Debug(string tag, string text)
        {
            WriteLog(LOGLEVEL.DEBUG, tag, text);
        }

        public static void Warn(string tag, string text)
        {
            WriteLog(LOGLEVEL.WARN, tag, text);
        }

        public static void Error(string tag, string text)
        {
            WriteLog(LOGLEVEL.ERROR, tag, text);
        }
    }
}
