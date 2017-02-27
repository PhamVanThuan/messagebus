using System;

namespace YmatouMQNet4.Logs
{
    [Serializable]
    public class NullLog : ILog
    {
        public void Debug(string s)
        {

        }

        public void Debug(string format, params object[] args)
        {

        }

        public void Info(string s)
        {

        }

        public void Info(string format, params object[] args)
        {

        }

        public void Warning(string format, params object[] args)
        {

        }

        public void Warning(string s)
        {

        }

        public void Warning(string s, Exception ex)
        {

        }

        public void Error(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(s);
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public void Error(string message, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Format("{0},{1}", message, ex.ToString()));
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public void Error(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(format, args);
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public void Fatal(string s, Exception ex)
        {

        }

        public void Fatal(string s)
        {

        }

        public void Fatal(string format, object[] args)
        {

        }
    }
}
