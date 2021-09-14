namespace Reportes
{
    using CommandLine;
    using CommandLine.Text;
    using Contadores;
    using Contadores.Command;

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
                var config = Contadores.Program.LoadConfigFile();
                if (config != null)
                {
                    var options = (Options)result.GetType().GetProperty("Value").GetValue(result, null);

                    Logger.Config.Verbose = true;

                    if (Database.Open(config.Database.ConnectionString))
                    {
                        Report.Build(options.From, options.Thru);
                    }

                    Logger.Persist();
                }
            }
        }
    }
}