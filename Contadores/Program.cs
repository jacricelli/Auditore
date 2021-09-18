namespace Contadores
{
    using CommandLine;
    using CommandLine.Text;
    using Contadores.Command;
    using Contadores.Config;
    using Newtonsoft.Json;
    using System;
    using System.IO;

    /// <summary>
    /// Programa.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        /// <param name="args">Los argumentos de la línea de comandos.</param>
        public static void Main(string[] args)
        {
            SentenceBuilder.Factory = () => new LocalizableSentenceBuilder();
            var result = Parser.Default.ParseArguments<Options>(args);
            if (result.Tag == ParserResultType.Parsed)
            {
                var config = LoadConfigFile();
                if (config != null)
                {
                    var options = (Options)result.GetType().GetProperty("Value").GetValue(result, null);

                    Logger.Config.Mail = config.Log.Mail;
                    Logger.Config.Mail.Subject = $"Problemas con la recopilación de contadores ({options.Address}).";
                    Logger.Config.Verbose = options.Verbose;

                    Run(config, options);

                    Logger.Persist(options.Address);
                }
            }
        }

        /// <summary>
        /// Carga el archivo de configuración de la aplicación.
        /// </summary>
        /// <returns>Una instancia de <see cref="AppConfig"/> o null en caso de error.</returns>
        public static AppConfig LoadConfigFile()
        {
            try
            {
                var path = Util.GetPath(file: "Contadores.json");
                var json = File.ReadAllText(path);

                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Excepción '{ex.GetType().Name}' al cargar la configuración: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Recopila y almacena los contadores de los usuarios.
        /// </summary>
        /// <param name="config">Una instancia de <see cref="AppConfig"/>.</param>
        /// <param name="options">Una instancia de <see cref="Options"/>.</param>
        private static void Run(AppConfig config, Options options)
        {
            if (Database.Open(config.Database.ConnectionString))
            {
                var printer = new Printer(options.Address, config.Printer);
                if (printer.Entity != null)
                {
                    var currentCounter = printer.GetCurrentCounter();
                    if (currentCounter > 0)
                    {
                        Logger.Write($"El contador de la impresora es: {currentCounter}", Logger.MessageType.Info);

                        var storedCounter = !options.Force ? printer.Entity.Contador : -1;
                        if (currentCounter > storedCounter)
                        {
                            Logger.Write("Recopilando contadores...", Logger.MessageType.Info);

                            var userCounters = printer.GetUserCounters();
                            if (userCounters?.Count > 0)
                            {
                                Logger.Write("Almacenando contadores...", Logger.MessageType.Info);

                                Database.Begin();
                                if (Database.StoreUserCounters(userCounters))
                                {
                                    if (Database.SetPrinterCounter(printer.Entity, currentCounter))
                                    {
                                        Database.Commit();

                                        Logger.Write("Se han actualizado los contadores.", Logger.MessageType.Info);

                                        return;
                                    }
                                }
                                Database.Rollback();

                                Logger.Write("No se han realizado cambios debido a un error interno.", Logger.MessageType.Info);
                            }
                            else
                            {
                                Logger.Write("Se produjo un error al obtener los contadores o no hay usuarios registrados en la impresora.", Logger.MessageType.Info);
                            }
                        }
                        else
                        {
                            Logger.Write("No es necesario actualizar los contadores.", Logger.MessageType.Info);
                        }
                    }
                    else
                    {
                        Logger.Write("No fue posible obtener el contador de la impresora o el valor obtenido es menor o igual a cero.", Logger.MessageType.Info);
                    }
                }
                else
                {
                    Logger.Write($"La impresora '{options.Address}' no se encuentra en la base de datos.", Logger.MessageType.Info);
                }
            }
        }
    }
}
