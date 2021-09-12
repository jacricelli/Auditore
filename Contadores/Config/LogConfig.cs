namespace Contadores.Config
{
    /// <summary>
    /// Configuración para el registro de mensajes.
    /// </summary>
    public class LogConfig
    {
        /// <summary>
        /// Obtiene o establece un valor que indica si los mensajes deben escribirse en la salida estándar.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Obtiene o establece la configuración para el envío de correo electrónico.
        /// </summary>
        public LogMailConfig Mail { get; set; }
    }
}
