using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Core
{
	public sealed class Logger
	{
		public readonly string LogFilePath = AppSettings.GetLocalAppDataPath() + Path.DirectorySeparatorChar + "errors.log";

		private static readonly Lazy<Logger> Lazy = new Lazy<Logger>(() => new Logger());

        public static Logger Instance => Lazy.Value;

        public static ILogger CoreInstance { get; set; }

        private Logger()
		{
		}

		public void Log(Exception e)
		{
            var logger = CoreInstance;
            if (logger == null)
            {
                // Ignore "normal" XmlSerializers constructor exceptions
                if (e.Message.Contains("ControlzEx.XmlSerializers"))
                    return;

                using (StreamWriter sw = File.AppendText(LogFilePath))
                {
                    sw.WriteLine("{0} {1}: {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), e.Message);
                    sw.WriteLine("Type: {0}", e.GetType());
                    sw.WriteLine("StackTrace:");
                    sw.WriteLine(e.StackTrace);
                    if (e.InnerException != null)
                    {
                        sw.WriteLine("InnerException:");
                        sw.WriteLine(e.InnerException);
                    }
                    sw.WriteLine("-----------------------------------");
                    sw.Close();
                }
            }
            else
            {
                logger.LogError(new EventId(1), e, "Error");
            }

		}
	}
}