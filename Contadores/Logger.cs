namespace Contadores
{
    using Contadores.Config;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Mail;
    using System.Text;

    /// <summary>
    /// Registro de mensajes.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Los tipos de mensajes.
        /// </summary>
        public enum MessageType : ushort
        {
            /// <summary>
            /// Un mensaje de depuración.
            /// </summary>
            Debug,

            /// <summary>
            /// Un mensaje de error.
            /// </summary>
            Error,

            /// <summary>
            /// Un mensaje de información.
            /// </summary>
            Info
        }

        /// <summary>
        /// Los mensajes.
        /// </summary>
        private static readonly StringBuilder messages = new StringBuilder();

        /// <summary>
        /// Indica si se ha imprimido la cabecera.
        /// </summary>
        private static bool headerWrited = false;

        /// <summary>
        /// Una instancia de <see cref="LogConfig"/>.
        /// </summary>
        public static LogConfig Config { get; private set; } = new LogConfig();

        /// <summary>
        /// Escribe un mensaje en el registro.
        /// </summary>
        /// <param name="message">El mensaje.</param>
        /// <param name="type">El tipo de mensaje.</param>
        public static void Write(string message, MessageType type = MessageType.Error)
        {
            var frame = new StackFrame(1, true);
            var method = frame.GetMethod();
            message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {method.DeclaringType.Name}.{method.Name} - {message}";

            if (Config.Verbose)
            {
                if (!headerWrited)
                {
                    WriteHeader();
                }

                if (type == MessageType.Error)
                {
                    Console.Error.WriteLine(message);
                }
                else
                {
                    Console.WriteLine(message);
                }
            }

            if (type != MessageType.Info)
            {
                messages.AppendLine(message);
            }
        }

        /// <summary>
        /// Escribe una excepción en el registro.
        /// </summary>
        /// <param name="ex">La excepción.</param>
        public static void Write(Exception ex)
        {
            if (ex is AggregateException)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
            }

            var frame = new StackFrame(1, true);
            var method = frame.GetMethod();
            var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {method.DeclaringType.Name}.{method.Name} - {ex.GetType().Name}: {ex.Message}";

            if (Config.Verbose)
            {
                if (!headerWrited)
                {
                    WriteHeader();
                }

                Console.Error.WriteLine(message);
            }

            messages.AppendLine(message);
        }

        /// <summary>
        /// Guarda los mensajes en un archivo y, si está habilitado, envía los mensajes por correo electrónico.
        /// </summary>
        /// <param name="fileName">El nombre del archivo sin extensión. Si no se especifica, se utilizará el nombre del proceso.</param>
        public static void Persist(string fileName = "")
        {
            if (messages.Length > 0)
            {
                if (Config.Mail.Enabled)
                {
                    SendMail();
                }

                SaveFile(fileName);
            }
        }

        /// <summary>
        /// Envía los mensajes por correo electrónico.
        /// </summary>
        private static void SendMail()
        {
            using (var client = new SmtpClient(Config.Mail.Server, Config.Mail.Port))
            {
                using (var message = new MailMessage(Config.Mail.From, Config.Mail.To))
                {
                    message.Subject = Config.Mail.Subject;
                    message.Body = messages.ToString();

                    try
                    {
                        client.Send(message);
                    }
                    catch (Exception ex)
                    {
                        Write(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Guarda los mensajes en un archivo.
        /// </summary>
        /// <param name="fileName">El nombre del archivo sin extensión. Si no se especifica, se utilizará el nombre del proceso.</param>
        private static void SaveFile(string fileName = "")
        {
            if (fileName.Length == 0)
            {
                fileName = Process.GetCurrentProcess().ProcessName;
            }

            try
            {
                var path = Util.GetPath("logs", $"{fileName}.log");
                using (var writer = new StreamWriter(path, true, Encoding.UTF8))
                {
                    writer.WriteLine(messages.ToString());
                }
            }
            catch (Exception ex)
            {
                if (Config.Verbose == false)
                {
                    Config.Verbose = true;
                }

                Write(ex);
            }
        }

        /// <summary>
        /// Escribe el nombre y versión del producto en el flujo de salida estándar.
        /// </summary>
        private static void WriteHeader()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName);
            Console.WriteLine($"{versionInfo.ProductName} v{versionInfo.ProductVersion}\n");

            headerWrited = true;
        }
    }
}
