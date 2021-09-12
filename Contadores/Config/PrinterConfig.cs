namespace Contadores.Config
{
    /// <summary>
    /// Configuración para la impresora.
    /// </summary>
    public class PrinterConfig
    {
        /// <summary>
        /// Obtiene o establece el nombre de usuario.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Obtiene o establece la contraseña.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Obtiene o establece el tiempo máximo de espera (en milisegundos) para el cliente HTTP.
        /// </summary>
        public int HttpClientTimeout { get; set; }

        /// <summary>
        /// Obtiene o establece el tiempo máximo de espera (en milisegundos) para la utilidad Ping.
        /// </summary>
        public int PingTimeout { get; set; }
    }
}
