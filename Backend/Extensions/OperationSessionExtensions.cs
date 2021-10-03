using System.Linq;
using Microsoft.Extensions.Logging;

namespace Helios
{
    public static class OperationSessionExtensions
    {
        public static bool IsInterrupted(this OperationSession session, ILogger logger, string topic, string expectation)
        {
            var message = session.LastSendMessage;
            var myMessages = session.MessageBuffer
                .Where(x => x.Topic == topic)
                .ToList();

            // we cant see our own message?
            if (session.MessageBuffer.Count == 0)
            {
                logger.LogCritical("Session MessageBuffer empty");
                return true;
            }

            // there is more than one status message available
            if (myMessages.Count > 1)
            {
                logger.LogDebug("myMessage>1");
                foreach (var (Topic, Message) in session.MessageBuffer)
                {
                    logger.LogDebug("Buffer - Topic {0} Message {1}", Topic, Message);
                }
                session.MessageBuffer.Clear();
                return true;
            }

            if (myMessages.Last().Message != expectation)
            {
                logger.LogDebug("myMessage != LastSendMessage; {0} != {1}", myMessages.Last().Message, message);
                foreach (var (Topic, Message) in session.MessageBuffer)
                {
                    logger.LogDebug("Buffer - Topic {0} Message {1}", Topic, Message);
                }
                session.MessageBuffer.Clear();
                return true;
            }

            session.MessageBuffer.Clear();
            return false;
        }
    }
}