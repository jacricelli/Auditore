namespace Reportes
{
    using CommandLine;
    using FluentDateTime;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Opciones.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// El intervalo de tiempo para buscar registros.
        /// </summary>
        private string _interval;

        /// <summary>
        /// La fecha a partir de la cual buscar registros.
        /// </summary>
        private DateTime _from;

        /// <summary>
        /// La fecha hasta la cual buscar registros.
        /// </summary>
        private DateTime _thru;

        /// <summary>
        /// Obtiene o establece el intervalo de tiempo para buscar registros.
        /// </summary>
        [Option('i', "intervalo", SetName = "intervalo", Default = "mesPasado", HelpText = "El intervalo de tiempo para buscar registros.\nValores permitidos: hoy, ayer, estaSemana, semanaPasada, ultimasDosSemanas, esteMes, mesPasado.")]
        public string Interval
        {
            get
            {
                return _interval;
            }

            set
            {
                var intervals = new List<string> { "hoy", "ayer", "estaSemana", "semanaPasada", "ultimasDosSemanas", "esteMes", "mesPasado" };
                if (intervals.IndexOf(value) < 0)
                {
                    throw new Exception($"El intervalo '{value}' no es válido.");
                }
                else
                {
                    _interval = value;

                    if (_from == default && _thru == default)
                    {
                        switch (_interval)
                        {
                            case "hoy":
                                _from = DateTime.Today.Midnight();
                                _thru = DateTime.Today.EndOfDay();
                                break;

                            case "ayer":
                                _from = DateTime.Today.PreviousDay().Midnight();
                                _thru = DateTime.Today.PreviousDay().EndOfDay();
                                break;

                            case "estaSemana":
                                _from = DateTime.Today.FirstDayOfWeek().AddDays(1);
                                _thru = _from.LastDayOfWeek().AddDays(1).EndOfDay();
                                break;

                            case "semanaPasada":
                                _from = DateTime.Today.FirstDayOfWeek().AddDays(1).WeekEarlier();
                                _thru = _from.LastDayOfWeek().AddDays(1).EndOfDay();
                                break;

                            case "ultimasDosSemanas":
                                _from = DateTime.Today.FirstDayOfWeek().AddDays(-13);
                                _thru = DateTime.Today.WeekEarlier().LastDayOfWeek().AddDays(1).EndOfDay();
                                break;

                            case "esteMes":
                                _from = DateTime.Today.BeginningOfMonth();
                                _thru = DateTime.Today.EndOfMonth();
                                break;

                            case "mesPasado":
                                _from = DateTime.Today.PreviousMonth().BeginningOfMonth();
                                _thru = DateTime.Today.PreviousMonth().EndOfMonth();
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene o establece la fecha a partir de la cual buscar registros.
        /// </summary>
        [Option('d', "desde", SetName = "rango", HelpText = "Fecha (en formato yyyy-MM-dd) a partir de la cual buscar registros.\nNo disponbile si se especifica la opción 'i, intervalo'.")]
        public DateTime From
        {
            get
            {
                if (_from == default && _thru != default)
                {
                    _from = _thru.Midnight();
                }

                return _from;
            }

            set
            {
                if (Thru != default && value > Thru)
                {
                    throw new Exception("La fecha debe ser menor a la especificada en la opción 'h, hasta'.");
                }
                else
                {
                    _from = value;
                }

            }
        }

        /// <summary>
        /// Obtiene o establece la fecha hasta la cual buscar registros.
        /// </summary>
        [Option('h', "hasta", SetName = "rango", HelpText = "Fecha (en formato yyyy-MM-dd) hasta la cual buscar registros.\nNo disponbile si se especifica la opción 'i, intervalo'.")]
        public DateTime Thru
        {
            get
            {
                if (_thru == default && _from != default)
                {
                    _thru = _from.EndOfDay();
                }

                return _thru;
            }

            set
            {
                if (From != default && value < From)
                {
                    throw new Exception("La fecha debe ser mayor a la especificada en la opción 'd, desde'.");
                }
                else
                {
                    _thru = value;
                }
            }
        }
    }
}
