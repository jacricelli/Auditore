namespace Reportes.Entity
{
    /// <summary>
    /// Registro.
    /// </summary>
    internal class Record
    {
        /// <summary>
        /// Obtiene o establece la empresa.
        /// </summary>
        internal string Enterprise { get; set; }

        /// <summary>
        /// Obtiene o establece el área.
        /// </summary>
        internal string Area { get; set; }

        /// <summary>
        /// Obtiene o establece el usuario.
        /// </summary>
        internal string User { get; set; }

        /// <summary>
        /// Obtiene el primer conjunto de contadores.
        /// </summary>
        internal CounterGroup First { get; private set; }

        /// <summary>
        /// Obtiene el último conjunto de contadores.
        /// </summary>
        internal CounterGroup Last { get; private set; }

        /// <summary>
        /// Obtiene el conjunto de contadores que surge de la diferencia entre el primero y el último.
        /// </summary>
        internal CounterGroup Final { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="first">El primer conjunto de contadores.</param>
        /// <param name="last">El segundo conjunto de contadores.</param>
        internal Record(CounterGroup first, CounterGroup last)
        {
            First = first;
            Last = last;
            Final = last.Clone();

            if (First != null && First.Date < Last.Date)
            {
                /** Copier.BlanckAndWhite **/

                if (Last.Copier.BlanckAndWhite > First.Copier.BlanckAndWhite)
                {
                    Final.Copier.BlanckAndWhite -= First.Copier.BlanckAndWhite;
                }
                else if (Last.Copier.BlanckAndWhite < First.Copier.BlanckAndWhite)
                {
                    Final = Last.Clone();

                    return;
                }
                else
                {
                    Final.Copier.BlanckAndWhite = 0;
                }

                /** Copier.Color **/

                if (Last.Copier.Color > First.Copier.Color)
                {
                    Final.Copier.Color -= First.Copier.Color;
                }
                else if (Last.Copier.Color < First.Copier.Color)
                {
                    Final = Last.Clone();

                    return;
                }
                else
                {
                    Final.Copier.Color = 0;
                }

                /** Printer.BlanckAndWhite **/

                if (Last.Printer.BlanckAndWhite > First.Printer.BlanckAndWhite)
                {
                    Final.Printer.BlanckAndWhite -= First.Printer.BlanckAndWhite;
                }
                else if (Last.Printer.BlanckAndWhite < First.Printer.BlanckAndWhite)
                {
                    Final = Last.Clone();

                    return;
                }
                else
                {
                    Final.Printer.BlanckAndWhite = 0;
                }

                /** Printer.Color **/

                if (Last.Printer.Color > First.Printer.Color)
                {
                    Final.Printer.Color -= First.Printer.Color;
                }
                else if (Last.Printer.Color < First.Printer.Color)
                {
                    Final = Last.Clone();

                    return;
                }
                else
                {
                    Final.Printer.Color = 0;
                }
            }
        }
    }
}
