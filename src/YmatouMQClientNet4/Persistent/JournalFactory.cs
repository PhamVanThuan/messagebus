using System;

namespace YmatouMessageBusClientNet4.Persistent
{
    public class JournalFactory
    {
        private static readonly Lazy<MessageLocalJournal> localJournal = new Lazy<MessageLocalJournal>(() => new MessageLocalJournal());
        private static readonly Lazy<MessageSendLog> messageLog = new Lazy<MessageSendLog>(() => new MessageSendLog());

        public static MessageLocalJournal MessageLocalJournalBuilder { get { return localJournal.Value; } }
        public static MessageSendLog MessageSendLogBuilder { get { return messageLog.Value; } }
    }
}
