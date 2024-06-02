using NLog;

namespace RV32I
{
    public static class Log
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void Trace(string message)
        {
            logger.Trace(message);
        }

        public static void Info(string message)
        {
            logger.Info(message);
        }

        public static void Warn(string message)
        {
            logger.Warn(message);
        }

        public static void Error(string message)
        {
            logger.Error(message);
        }
    }
}
