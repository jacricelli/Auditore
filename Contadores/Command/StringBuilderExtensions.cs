namespace Contadores.Command
{
    using System.Text;

    /// <summary>
    /// Extensión de <see cref="StringBuilder"/>.
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Anexa uno o más valores cuando se cumple la condición especificada.
        /// </summary>
        /// <param name="builder">Una instancia de <see cref="StringBuilder"/>.</param>
        /// <param name="condition">La condición.</param>
        /// <param name="values">Un arreglo de valores.</param>
        /// <returns>Una instancia de <see cref="StringBuilder"/>.</returns>
        public static StringBuilder AppendWhen(this StringBuilder builder, bool condition, params string[] values)
        {
            if (condition)
            {
                foreach (var value in values)
                {
                    builder.Append(value);
                }
            }

            return builder;
        }

        /// <summary>
        /// Anexa un valor de acuerdo a la condición especificada.
        /// </summary>
        /// <param name="builder">Una instancia de <see cref="StringBuilder"/>.</param>
        /// <param name="condition">La condición.</param>
        /// <param name="ifTrue">Un valor a anexar si la condición es verdadera.</param>
        /// <param name="ifFalse">Un valor a anexar si la condición es falsa.</param>
        /// <returns>Una instancia de <see cref="StringBuilder"/>.</returns>
        public static StringBuilder AppendIf(this StringBuilder builder, bool condition, string ifTrue, string ifFalse)
        {
            return condition
                ? builder.Append(ifTrue)
                : builder.Append(ifFalse);
        }
    }
}
