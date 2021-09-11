namespace Contadores
{
    using Contadores.Config;
    using Newtonsoft.Json;
    using System;
    using System.IO;

    /// <summary>
    /// Programa.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        /// <param name="args">Los argumentos de la línea de comandos.</param>
        public static void Main(string[] args)
        {
        }

        /// <summary>
        /// Carga el archivo de configuración de la aplicación.
        /// </summary>
        /// <returns>Una instancia de <see cref="AppConfig"/> o null en caso de error.</returns>
        public static AppConfig LoadConfigFile()
        {
            try
            {
                var path = Util.GetPath(file: "Contadores.json");
                var json = File.ReadAllText(path);

                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Excepción '{ex.GetType().Name}' al cargar la configuración: {ex.Message}");
            }

            return null;
        }
    }
}
