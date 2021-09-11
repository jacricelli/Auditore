namespace Contadores.Config
{
    /// <summary>
    /// Configuración de la aplicación.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// La configuración de la base de datos.
        /// </summary>
        public DatabaseConfig Database { get; set; }

        /// <summary>
        /// La configuración del registro de mensajes.
        /// </summary>
        public LogConfig Log { get; set; }

        /// <summary>
        /// La configuración de la impresora.
        /// </summary>
        public PrinterConfig Printer { get; set; }
    }
}
