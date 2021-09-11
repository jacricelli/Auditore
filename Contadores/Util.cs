namespace Contadores
{
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Colección de métodos comunes.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Obtiene la ruta de acceso al directorio desde donde se ejecuta el ensamblado.
        /// </summary>
        /// <param name="subdir">Un subdirectorio a anexar a la ruta.</param>
        /// <param name="file">Un archivo a anexar a la ruta.</param>
        /// <returns>La ruta de acceso al directorio, subdirectorio o archivo.</returns>
        public static string GetPath(string subdir = "", string file = "")
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (subdir != string.Empty)
            {
                path += "\\" + subdir;
                Directory.CreateDirectory(path);
            }

            if (file != string.Empty)
            {
                path += "\\" + file;
            }

            return path;
        }
    }
}
