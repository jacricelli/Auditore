namespace Contadores
{
    using CommandLine;

    /// <summary>
    /// Opciones.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Obtiene o establece la dirección IP de la impresora.
        /// </summary>
        [Option('i', "ip", Required = true, HelpText = "La dirección IP de la impresora.")]
        public string Address { get; set; }

        /// <summary>
        /// Obtiene o establece un valor que indica si debe forzarse la recopilación de contadores.
        /// </summary>
        [Option('f', "forzar", Required = false, Default = false, HelpText = "Fuerza la recopilación de contadores.")]
        public bool Force { get; set; }

        /// <summary>
        /// Obtiene o establece un valor que indica si debe mostrarse el detalle de las tareas llevadas a cabo.
        /// </summary>
        [Option('v', "verbose", Required = false, Default = false, HelpText = "Muestra el detalle de las tareas llevadas a cabo.")]
        public bool Verbose { get; set; }
    }
}
