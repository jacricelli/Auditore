namespace Contadores.Entity
{
    using System;

    /// <summary>
    /// Representa un registro de la tabla contadores.
    /// </summary>
    internal class ContadorEntity
    {
        /// <summary>
        /// Obtiene o establece el registro de la impresora.
        /// </summary>
        internal ImpresoraEntity Impresora { get; set; }

        /// <summary>
        /// Obtiene o establece el registro del usuario.
        /// </summary>
        internal UsuarioEntity Usuario { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha.
        /// </summary>
        internal DateTime Fecha { get; set; }

        /// <summary>
        /// Obtiene o establece el contador blanco y negro de la copiadora.
        /// </summary>
        internal uint CopiadoraBn { get; set; }

        /// <summary>
        /// Obtiene o establece el contador color de la copiadora.
        /// </summary>
        internal uint CopiadoraColor { get; set; }

        /// <summary>
        /// Obtiene o establece el contador blanco y negro de la impresora.
        /// </summary>
        internal uint ImpresoraBn { get; set; }

        /// <summary>
        /// Obtiene o establece el contador color de la impresora.
        /// </summary>
        internal uint ImpresoraColor { get; set; }
    }
}