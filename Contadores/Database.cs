namespace Contadores
{
    using Contadores.Entity;
    using System;
    using System.Collections.Generic;
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

        /// <summary>
        /// Almacena los contadores de los usuarios.
        /// </summary>
        /// <param name="userCounters">Una lista de <see cref="ContadorEntity"/>.</param>
        /// <returns>true si se han almacenado los contadores o false en caso contrario.</returns>
        internal static bool StoreUserCounters(List<ContadorEntity> userCounters)
        {
            try
            {
                var commandText = "INSERT INTO `contadores` " +
                    "(`impresora_id`, `usuario_id`, `fecha`, `hora`, `copiadora_bn`, `copiadora_color`, `impresora_bn`, `impresora_color`)" +
                    $"VALUES (@impresora_id, @usuario_id, @fecha, @hora, @copiadora_bn, @copiadora_color, @impresora_bn, @impresora_color) " +
                    "ON CONFLICT DO UPDATE SET " +
                    $"`hora` = @hora, `copiadora_bn` = @copiadora_bn, `copiadora_color` = @copiadora_color, `impresora_bn` = @impresora_bn, `impresora_color` = @impresora_color";

                using (var command = GetSQLiteCommand(commandText))
                {
                    foreach (var userCounter in userCounters)
                    {
                        command.Parameters.AddWithValue("@impresora_id", userCounter.Impresora.Id);
                        command.Parameters.AddWithValue("@usuario_id", userCounter.Usuario.Id);
                        command.Parameters.AddWithValue("@fecha", userCounter.Fecha.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@hora", userCounter.Fecha.ToString("HH:mm:ss"));
                        command.Parameters.AddWithValue("@copiadora_bn", userCounter.CopiadoraBn);
                        command.Parameters.AddWithValue("@copiadora_color", userCounter.CopiadoraColor);
                        command.Parameters.AddWithValue("@impresora_bn", userCounter.ImpresoraBn);
                        command.Parameters.AddWithValue("@impresora_color", userCounter.ImpresoraColor);

                        if (command.ExecuteNonQuery() != 1)
                        {
                            Logger.Write("La consulta de inserción/actualización de contador no produjo cambios.", Logger.MessageType.Debug);

                            return false;
                        }
                        
                        command.Parameters.Clear();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

            return false;
        }

        /// <summary>
        /// Establece el contador de la impresora.
        /// </summary>
        /// <param name="printer">Una instancia de <see cref="ImpresoraEntity"/>.</param>
        /// <param name="counter">El contador.</param>
        /// <returns>true si se ha establecido el contador o false en caso contrario.</returns>
        internal static bool SetPrinterCounter(ImpresoraEntity printer, int counter)
        {
            try
            {
                var commandText = "UPDATE `impresoras` SET `contador` = @contador, `timestamp` = @timestamp WHERE `id` = @id";
                using (var command = GetSQLiteCommand(commandText))
                {
                    command.Parameters.AddWithValue("@contador", counter);
                    command.Parameters.AddWithValue("@id", printer.Id);
                    command.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    return command.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

            return false;
        }
    }
}
