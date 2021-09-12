namespace Contadores.Config
{
    /// <summary>
    /// Configuración para la aplicación.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Obtiene o establece la configuración para la base de datos.
        /// </summary>
        public DatabaseConfig Database { get; set; }

        /// <summary>
        /// Obtiene o establece la configuración para el registro de mensajes.
        /// </summary>
        public LogConfig Log { get; set; }

        /// <summary>
        /// Obtiene o establece la configuración para la impresora.
        /// </summary>
        public PrinterConfig Printer { get; set; }
    }
}
