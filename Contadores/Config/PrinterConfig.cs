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
        /// Obtiene o establece la comunidad de SNMP.
        /// </summary>
        public string SnmpCommunity { get; set; }
    }
}
