namespace Contadores.Entity
{
    /// <summary>
    /// Representa un registro de la tabla impresoras.
    /// </summary>
    internal class ImpresoraEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador.
        /// </summary>
        internal int Id { get; set; }

        /// <summary>
        /// Obtiene o establece la dirección IP.
        /// </summary>
        internal string Ip { get; set; }

        /// <summary>
        /// Obtiene o establece el contador.
        /// </summary>
        internal int Contador { get; set; }
    }
}
