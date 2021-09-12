namespace Contadores.Config
{
    /// <summary>
    /// Configuración para el envío de correo electrónico.
    /// </summary>
    public class LogMailConfig
    {
        /// <summary>
        /// Obtiene o establece un valor que indica si está habilitado el envío de correo electrónico.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del host o la dirección IP del servidor SMTP.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Obtiene o establece el puerto del servidor SMTP.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Obtiene o establece la dirección de correo del remitente.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Obtiene o establece la dirección de correo del destinatario.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Obtiene o establece el asunto del mensaje.
        /// </summary>
        public string Subject { get; set; }
    }
}
