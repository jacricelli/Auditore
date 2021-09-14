namespace Reportes.Entity
{
    /// <summary>
    /// Set de contadores.
    /// </summary>
    internal class CounterSet
    {
        /// <summary>
        /// Obtiene o establece el contador blanco y negro.
        /// </summary>
        internal int BlanckAndWhite { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece el contador color.
        /// </summary>
        internal int Color { get; set; } = 0;
    }
}
