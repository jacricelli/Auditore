namespace Contadores
{
    using Contadores.Config;
    using Contadores.Entity;
    using Ricoh;
    using Ricoh.Models;
    using SnmpSharpNet;
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;

    /// <summary>
    /// Impresora.
    /// </summary>
    internal class Printer
    {
        /// <summary>
        /// La dirección IP de la impresora.
        /// </summary>
        private readonly string address;

        /// <summary>
        /// La configuración de la impresora.
        /// </summary>
        private readonly PrinterConfig config;

        /// <summary>
        /// Obtiene el registro (en la base de datos) de la impresora.
        /// </summary>
        internal ImpresoraEntity Entity { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">La dirección IP.</param>
        /// <param name="config">La configuración.</param>
        internal Printer(string address, PrinterConfig config)
        {
            this.address = address;
            this.config = config;

            SetEntity();
        }

        /// <summary>
        /// Comprueba si la impresora está en línea.
        /// </summary>
        /// <returns>true si hay una respuesta o false en caso contrario.</returns>
        internal bool IsOnline()
        {
            try
            {
                using (var ping = new Ping())
                {
                    return ping.Send(address, config.PingTimeout).Status == IPStatus.Success;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

            return false;
        }

        /// <summary>
        /// Obtiene el contador actual.
        /// </summary>
        /// <returns>El contador actual o -1 si no fue posible obtenerlo.</returns>
        internal int GetCurrentCounter()
        {
            try
            {
                var client = new SimpleSnmp(address, config.SnmpCommunity);
                var result = client.Get(SnmpVersion.Ver1, new string[] { "1.3.6.1.2.1.43.10.2.1.4.1.1" });
                if (result?.Count == 1)
                {
                    foreach (var kvp in result)
                    {
                        return int.Parse(kvp.Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

            return -1;
        }

        /// <summary>
        /// Obtiene los contadores de los usuarios.
        /// </summary>
        /// <returns>Una lista de <see cref="ContadorEntity"/> o null si no fue posible obtener los contadores.</returns>
        internal List<ContadorEntity> GetUserCounters()
        {
            if (Entity != null)
            {
                try
                {
                    using (var deviceManager = new DeviceManagement(address, config.UserName, config.Password) { TimeLimit = 300 })
                    {
                        var userCounters = new List<ContadorEntity>();

                        var counters = deviceManager.GetUserCounters();
                        foreach (var counter in counters)
                        {
                            UserCounter last = null;

                            if (counter != null)
                            {
                                if (counter.Copier.Total > 0 || counter.Printer.Total > 0)
                                {
                                    last = counter;

                                    var user = EnsureUserExists(counter.Authentication, counter.Username);
                                    if (user != null)
                                    {
                                        userCounters.Add(new ContadorEntity
                                        {
                                            Impresora = Entity,
                                            Usuario = user,
                                            Fecha = DateTime.Now,
                                            CopiadoraBn = counter.Copier.Black,
                                            CopiadoraColor = counter.Copier.Color,
                                            ImpresoraBn = counter.Printer.Black,
                                            ImpresoraColor = counter.Printer.Color
                                        });
                                    }
                                    else
                                    {
                                        Logger.Write($"No se pudo asegurar que el registro de '{counter.Authentication}/{counter.Username}' exista en la base de datos.", Logger.MessageType.Debug);

                                        return null;
                                    }
                                }
                            }
                            else
                            {
                                if (last == null)
                                {
                                    Logger.Write("El primer contador es nulo.", Logger.MessageType.Debug);
                                }
                                else
                                {
                                    Logger.Write($"El contador posterior a '{last.Authentication}/{last.Username}' es nulo.", Logger.MessageType.Debug);
                                }
                            }
                        }

                        return userCounters;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Establece la propiedad <see cref="Entity"/> con el registro de la impresora.
        /// </summary>
        private void SetEntity()
        {
            try
            {
                var commandText = "SELECT `id`, `ip`, `contador` FROM `impresoras` WHERE `ip` = @ip LIMIT 1";
                using (var command = Database.GetSQLiteCommand(commandText))
                {
                    command.Parameters.AddWithValue("@ip", address);

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        if (reader.HasRows)
                        {
                            Entity = new ImpresoraEntity
                            {
                                Id = reader.GetInt32(0),
                                Ip = reader.GetString(1),
                                Contador = reader.GetInt32(2)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        /// <summary>
        /// Asegura que el registro un usuario existe en la base de datos.
        /// </summary>
        /// <param name="code">El código.</param>
        /// <param name="user">El usuario.</param>
        /// <returns>Una instancia de <see cref="UsuarioEntity"/> o null si no fue posible obtener o crear el registro.</returns>
        private UsuarioEntity EnsureUserExists(string code, string user)
        {
            var realCode = code != "\"other\"" ? int.Parse(code) : 999999;
            var entity = GetUser(realCode);
            if (entity == null)
            {
                if (CreateUser(realCode, user))
                {
                    entity = GetUser(realCode);
                    if (entity != null)
                    {
                        Logger.Write($"Se ha creado el usuario '{realCode}/{user}'.", Logger.MessageType.Info);
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Obtiene el registro de un usuario.
        /// </summary>
        /// <param name="code">El código.</param>
        /// <returns>Una instancia de <see cref="UsuarioEntity"/> o null si no fue posible obtener el registro.</returns>
        private UsuarioEntity GetUser(int code)
        {
            try
            {
                var commandText = "SELECT `id`, `usuario` FROM `usuarios` WHERE `codigo` = @codigo LIMIT 1";
                using (var command = Database.GetSQLiteCommand(commandText))
                {
                    command.Parameters.AddWithValue("@codigo", code);

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        if (reader.HasRows)
                        {
                            return new UsuarioEntity
                            {
                                Id = reader.GetInt32(0),
                                Codigo = code,
                                Usuario = reader.GetString(1)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

            return null;
        }

        /// <summary>
        /// Crea un usuario.
        /// </summary>
        /// <param name="code">El código.</param>
        /// <param name="user">El usuario.</param>
        /// <returns>true si se ha creado el usuario o false en caso contrario.</returns>
        private bool CreateUser(int code, string user)
        {
            try
            {
                var commandText = "INSERT INTO `usuarios` (`codigo`, `usuario`, `timestamp`) VALUES (@codigo, @usuario, @timestamp)";
                using (var command = Database.GetSQLiteCommand(commandText))
                {
                    command.Parameters.AddWithValue("@codigo", code);
                    command.Parameters.AddWithValue("@usuario", user);
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
