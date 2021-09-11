namespace Contadores.Config
{
    /// <summary>
    /// Configuración del registro de mensajes.
    /// </summary>
    public class LogConfig
    {
        /// <summary>
        /// Obtiene o establece un valor que indica si los mensajes deben escribirse en la salida estándar.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Obtiene o establece la configuración del envío de correo electrónico.
        /// </summary>
        public LogMailConfig Mail { get; set; }
    }
}
