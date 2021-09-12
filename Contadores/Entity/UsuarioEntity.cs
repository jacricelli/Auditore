namespace Contadores.Entity
{
    /// <summary>
    /// Representa un registro de la tabla usuarios.
    /// </summary>
    internal class UsuarioEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador.
        /// </summary>
        internal int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el código.
        /// </summary>
        internal int Codigo { get; set; }

        /// <summary>
        /// Obtiene o establece el usuario.
        /// </summary>
        internal string Usuario { get; set; }
    }
}