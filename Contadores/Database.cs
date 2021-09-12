namespace Contadores
{
    using System;
    using System.Data.SQLite;

    /// <summary>
    /// Base de datos.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Instancia de <see cref="SQLiteConnection"/>.
        /// </summary>
        private static SQLiteConnection connection;

        /// <summary>
        /// Instancia de <see cref="SQLiteTransaction"/>.
        /// </summary>
        private static SQLiteTransaction transaction;

        /// <summary>
        /// Obtiene un valor que indica si se ha establecido una conexión con la base de datos.
        /// </summary>
        public static bool IsConnected => connection?.State == System.Data.ConnectionState.Open;

        /// <summary>
        /// Establece una conexión con la base de datos.
        /// </summary>
        /// <param name="connectionString">La cadena de conexión.</param>
        /// <returns>true si se ha establecido la conexión o false en caso contrario.</returns>
        public static bool Open(string connectionString)
        {
            try
            {
                if (connection == null)
                {
                    connection = new SQLiteConnection(connectionString);
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

            return IsConnected;
        }

        /// <summary>
        /// Cierra la conexión con la base de datos.
        /// </summary>
        /// <param name="commitActiveTransaction">Si hay una transacción activa, indica si se confirma o cancela la transacción.</param>
        public static void Close(bool commitActiveTransaction = true)
        {
            if (connection != null)
            {
                if (transaction != null)
                {
                    if (commitActiveTransaction)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }

                    transaction.Dispose();
                    transaction = null;
                }

                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        /// <summary>
        /// Obtiene una nueva instancia de <see cref="SQLiteCommand"/>.
        /// </summary>
        /// <param name="commandText">El texto de la consulta.</param>
        /// <returns>Una instancia de <see cref="SQLiteCommand"/>.</returns>
        public static SQLiteCommand GetSQLiteCommand(string commandText)
        {
            return new SQLiteCommand(commandText, connection, transaction);
        }

        /// <summary>
        /// Comienza una transacción.
        /// </summary>
        public static void Begin()
        {
            if (IsConnected && transaction == null)
            {
                transaction = connection.BeginTransaction();
            }
        }

        /// <summary>
        /// Confirma la transacción activa.
        /// </summary>
        public static void Commit()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction.Dispose();
                transaction = null;
            }
        }

        /// <summary>
        /// Cancela la transacción activa.
        /// </summary>
        public static void Rollback()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction.Dispose();
                transaction = null;
            }
        }
    }
}
