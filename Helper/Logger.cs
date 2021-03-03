using log4net;

namespace Helper
{
    public static class Logger
    {
        public static ILog _Logger;

        public static void SetLogger(ILog logger)
        {
            _Logger = logger;
        }

        public static void Debug(object data)
        {
            _Logger.Debug(data);
        }

        public static void Error(object data)
        {
            _Logger.Error(data);
        }

        public static void Fatal(object data)
        {
            _Logger.Fatal(data);
        }
    }
}
