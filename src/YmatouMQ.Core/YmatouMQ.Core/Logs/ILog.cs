using System;
namespace YmatouMQNet4
{
    public interface ILog
    {
        void Debug(string s);
        void Debug(string format, params object[] args);
        void Info(string s);
        void Info(string format, params object[] args);
        void Warning(string format, params object[] args);
        void Warning(string s);
        void Warning(string s, Exception ex);
        void Error(string s);
        void Error(string message, Exception ex);
        void Error(string format, params object[] args);
        void Fatal(string s, Exception ex);
        void Fatal(string s);
        void Fatal(string format, object[] args);      
    }
}
