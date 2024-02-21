namespace BagiraWebApi.Services.Loggers.FileLogger
{
    public class FileLogger : ILogger, IDisposable
    {
        string filePath;
        static object _lock = new object();
        public FileLogger(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            filePath = $"{folder}/{DateTime.UtcNow.AddHours(5):dd-MM-yy}.txt";
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose() { }

        public bool IsEnabled(LogLevel logLevel)
        {
            //return logLevel == LogLevel.Trace;
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId,
                    TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (_lock)
            {
                File.AppendAllText(filePath, formatter(state, exception) + Environment.NewLine);
            }
        }
    }
}
