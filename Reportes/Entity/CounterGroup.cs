namespace Reportes.Entity
{
    using System;

    /// <summary>
    /// Grupo de contadores.
    /// </summary>
    internal class CounterGroup
    {
        /// <summary>
        /// Obtiene o establece la fecha.
        /// </summary>
        internal DateTime Date { get; set; }

        /// <summary>
        /// Obtiene o establece el set de contadores de la copiadora.
        /// </summary>
        internal CounterSet Copier { get; set; }

        /// <summary>
        /// Obtiene o establece el set de contadores de la impresora.
        /// </summary>
        internal CounterSet Printer { get; set; }

        /// <summary>
        /// Obtiene la sumatoria de los contadores.
        /// </summary>
        internal int Total => (int)Copier?.BlanckAndWhite + (int)Copier?.Color + (int)Printer?.BlanckAndWhite + (int)Printer?.Color;

        /// <summary>
        /// Genera un nuevo objeto con los mismos valores de la instancia actual.
        /// </summary>
        /// <returns>Una nueva instancia del objeto con los valores de la instancia actual.</returns>
        internal CounterGroup Clone()
        {
            return new CounterGroup
            {
                Date = Date,
                Copier = new CounterSet
                {
                    BlanckAndWhite = Copier.BlanckAndWhite,
                    Color = Copier.Color
                },
                Printer = new CounterSet
                {
                    BlanckAndWhite = Printer.BlanckAndWhite,
                    Color = Printer.Color
                }
            };
        }
    }
}
