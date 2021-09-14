namespace Reportes
{
    using ClosedXML.Excel;
    using Contadores;
    using FluentDateTime;
    using Reportes.Entity;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Generador de reportes.
    /// </summary>
    internal class Report
    {
        /// <summary>
        /// Construye un reporte de contadores.
        /// </summary>
        /// <param name="from">La fecha desde la cual buscar registros.</param>
        /// <param name="thru">La fecha hasta la cual buscar registros.</param>
        internal static void Build(DateTime from, DateTime thru)
        {
            var records = GatherRecords(from, thru);
            if (records.Count > 0)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        Generate(workbook, records);
                        Format(workbook);

                        string fileName = string.Empty;
                        if (from == from.FirstDayOfMonth() && thru == thru.LastDayOfMonth())
                        {
                            fileName = $"Contadores_{from:yyyy-MM}.xlsx";
                        }
                        else
                        {
                            fileName = $"Contadores_{from:yyyy-MM-dd}_{from:yyyy-MM-dd}.xlsx";
                        }

                        workbook.SaveAs(Util.GetPath("reports", fileName));

                        Logger.Write($"Se ha guardado el reporte en {fileName}", Logger.MessageType.Info);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
            else
            {
                Logger.Write("No se han encontrado registros para el intervalo especificado.", Logger.MessageType.Info);
            }
        }

        /// <summary>
        /// Recopila los registros para el reporte.
        /// </summary>
        /// <param name="from">La fecha desde la cual buscar registros.</param>
        /// <param name="thru">La fecha hasta la cual buscar registros.</param>
        private static List<Record> GatherRecords(DateTime from, DateTime thru)
        {
            var sql = "SELECT `impresora_id`, `usuario_id`, `empresas`.`id`, `areas`.`id` FROM `contadores` " +
                "JOIN `impresoras` ON (`contadores`.`impresora_id` = `impresoras`.`id`) " +
                "JOIN `usuarios` ON (`contadores`.`usuario_id` = `usuarios`.`id`) " +
                "JOIN `empresas` ON (`impresoras`.`empresa_id` = `empresas`.`id`) " +
                "JOIN `areas` ON (`impresoras`.`area_id` = `areas`.`id`) " +
                "WHERE (`fecha` >= @from AND `fecha` <= @thru) " +
                "GROUP BY `impresora_id`, `usuario_id`" +
                "ORDER BY `empresas`.`orden`, `areas`.`orden`, `impresoras`.`orden`, `usuarios`.`usuario`";

            var records = new List<Record>();
            var enterprises = GetKeyValuePairs("empresas", "nombre");
            var areas = GetKeyValuePairs("areas", "nombre");
            var users = GetKeyValuePairs("usuarios", "usuario");

            using (var command = Database.GetSQLiteCommand(sql))
            {
                command.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@thru", thru.ToString("yyyy-MM-dd"));

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var printerId = reader.GetInt32(0);
                            var userId = reader.GetInt32(1);
                            var first = GetCounter(printerId, userId, from.AddDays(-1));
                            var last = GetCounter(printerId, userId, thru);

                            records.Add(new Record(first, last)
                            {
                                Enterprise = enterprises[reader.GetInt32(2)],
                                Area = areas[reader.GetInt32(3)],
                                User = users[reader.GetInt32(1)],
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }

            return records;
        }

        /// <summary>
        /// Obtiene un conjunto de contadores.
        /// </summary>
        /// <param name="printerId">El identificador de la impresora.</param>
        /// <param name="userId">El identificador del usuario.</param>
        /// <param name="date">La fecha.</param>
        /// <returns>Una instancia de <see cref="CounterGroup"/> o null si no fue posible obtener el registro.</returns>
        private static CounterGroup GetCounter(int printerId, int userId, DateTime date)
        {
            var sql = "SELECT `fecha`, `hora`, `copiadora_bn`, `copiadora_color`, `impresora_bn`, `impresora_color` FROM `contadores` " +
                "WHERE (`impresora_id` = @printerId AND `usuario_id` = @userId AND `fecha` <= @date) " +
                "ORDER BY `fecha` DESC LIMIT 1";

            using (var command = Database.GetSQLiteCommand(sql))
            {
                command.Parameters.AddWithValue("@printerId", printerId);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        if (reader.HasRows)
                        {
                            return new CounterGroup
                            {
                                Date = Convert.ToDateTime($"{reader.GetString(0)} {reader.GetString(1)}"),
                                Copier = new CounterSet
                                {
                                    BlanckAndWhite = reader.GetInt32(2),
                                    Color = reader.GetInt32(3)
                                },
                                Printer = new CounterSet
                                {
                                    BlanckAndWhite = reader.GetInt32(4),
                                    Color = reader.GetInt32(5)
                                }
                            };
                        }
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
        /// Obtiene una colección de valores.
        /// </summary>
        /// <param name="tableName">El nombre de la tabla.</param>
        /// <param name="valueField">El nombre del campo para la clave.</param>
        /// <param name="keyField">El nombre del campo para el valor.</param>
        /// <returns>Una colección de valores o null en caso de error.</returns>
        private static Dictionary<int, string> GetKeyValuePairs(string tableName, string valueField, string keyField = "id")
        {
            var sql = $"SELECT `{keyField}`, `{valueField}` FROM `{tableName}`";

            using (var command = Database.GetSQLiteCommand(sql))
            {
                try
                {
                    var rows = new Dictionary<int, string>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(reader.GetInt32(0), reader.GetString(1));
                        }
                    }

                    return rows;
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Genera el reporte de contadores.
        /// </summary>
        /// <param name="workbook">Una instancia de <see cref="IXLWorkbook"/>.</param>
        /// <param name="records">La lista de registros.</param>
        private static void Generate(IXLWorkbook workbook, List<Record> records)
        {
            var totalSheet = workbook.AddWorksheet("Resumen");
            var counterSheet = workbook.AddWorksheet("Contadores");
            var detailSheet = workbook.AddWorksheet("Detalles");
            var enterpriseAreaMap = new Dictionary<string, List<string>>();

            var counterRow = 1;
            var detailRow = 2;
            foreach (var record in records)
            {
                detailRow++;

                if (!enterpriseAreaMap.ContainsKey(record.Enterprise))
                {
                    enterpriseAreaMap.Add(record.Enterprise, new List<string>());
                }
                if (!enterpriseAreaMap[record.Enterprise].Contains(record.Area))
                {
                    enterpriseAreaMap[record.Enterprise].Add(record.Area);
                }

                detailSheet.Cell($"A{detailRow}").Value = record.Enterprise;
                detailSheet.Cell($"B{detailRow}").Value = record.Area;
                detailSheet.Cell($"C{detailRow}").Value = record.User;

                if (record.First != null)
                {
                    detailSheet.Cell($"D{detailRow}").Value = record.First.Date.ToString("dd/MM/yyyy");
                    detailSheet.Cell($"E{detailRow}").Value = record.First.Copier.BlanckAndWhite;
                    detailSheet.Cell($"F{detailRow}").Value = record.First.Copier.Color;
                    detailSheet.Cell($"G{detailRow}").Value = record.First.Printer.BlanckAndWhite;
                    detailSheet.Cell($"H{detailRow}").Value = record.First.Printer.Color;
                }

                detailSheet.Cell($"I{detailRow}").Value = record.Last.Date.ToString("dd/MM/yyyy");
                detailSheet.Cell($"J{detailRow}").Value = record.Last.Copier.BlanckAndWhite;
                detailSheet.Cell($"K{detailRow}").Value = record.Last.Copier.Color;
                detailSheet.Cell($"L{detailRow}").Value = record.Last.Printer.BlanckAndWhite;
                detailSheet.Cell($"M{detailRow}").Value = record.Last.Printer.Color;

                detailSheet.Cell($"N{detailRow}").Value = record.Final.Copier.BlanckAndWhite;
                detailSheet.Cell($"O{detailRow}").Value = record.Final.Copier.Color;
                detailSheet.Cell($"P{detailRow}").Value = record.Final.Printer.BlanckAndWhite;
                detailSheet.Cell($"Q{detailRow}").Value = record.Final.Printer.Color;

                if (record.Final.Total > 0)
                {
                    counterRow++;

                    counterSheet.Cell($"A{counterRow}").Value = record.Enterprise;
                    counterSheet.Cell($"B{counterRow}").Value = record.Area;
                    counterSheet.Cell($"C{counterRow}").Value = record.User;

                    counterSheet.Cell($"D{counterRow}").Value = record.Final.Copier.BlanckAndWhite;
                    counterSheet.Cell($"E{counterRow}").Value = record.Final.Copier.Color;
                    counterSheet.Cell($"F{counterRow}").Value = record.Final.Printer.BlanckAndWhite;
                    counterSheet.Cell($"G{counterRow}").Value = record.Final.Printer.Color;
                }
            }

            var enterpriseRow = 1;
            var areaRow = 1;
            foreach (var enterprise in enterpriseAreaMap)
            {
                enterpriseRow++;

                foreach (var area in enterprise.Value)
                {
                    areaRow++;

                    totalSheet.Cell($"A{areaRow}").Value = enterprise.Key;
                    totalSheet.Cell($"B{areaRow}").Value = area;

                    totalSheet.Cell($"C{areaRow}").FormulaA1 = $"=SUMIFS(Contadores!$D$2:$D${counterRow},Contadores!$A$2:$A${counterRow},A{areaRow},Contadores!$B$2:$B${counterRow},B{areaRow})+" +
                        $"SUMIFS(Contadores!$F$2:$F${counterRow},Contadores!$A$2:$A${counterRow},A{areaRow},Contadores!$B$2:$B${counterRow},B{areaRow})";

                    totalSheet.Cell($"D{areaRow}").FormulaA1 = $"=SUMIFS(Contadores!$E$2:$E${counterRow},Contadores!$A$2:$A${counterRow},A{areaRow},Contadores!$B$2:$B${counterRow},B{areaRow})+" +
                        $"SUMIFS(Contadores!$G$2:$G${counterRow},Contadores!$A$2:$A${counterRow},A{areaRow},Contadores!$B$2:$B${counterRow},B{areaRow})";
                }

                totalSheet.Cell($"H{enterpriseRow}").Value = enterprise.Key;

                totalSheet.Cell($"I{enterpriseRow}").FormulaA1 = $"=SUMIF($A$2:A{areaRow},H{enterpriseRow},$C$2:C{areaRow})";
                totalSheet.Cell($"J{enterpriseRow}").FormulaA1 = $"=SUMIF($A$2:A{areaRow},H{enterpriseRow},$D$2:D{areaRow})";
            }
        }

        /// <summary>
        /// Formatea el reporte de contadores.
        /// </summary>
        /// <param name="workbook">Una instancia de <see cref="IXLWorkbook"/>.</param>
        private static void Format(IXLWorkbook workbook)
        {
            var totalSheet = workbook.Worksheets.Worksheet("Resumen");
            var counterSheet = workbook.Worksheets.Worksheet("Contadores");
            var detailSheet = workbook.Worksheets.Worksheet("Detalles");

            totalSheet.Cell("A1").Value = "Empresa";
            totalSheet.Cell("B1").Value = "Área";
            totalSheet.Cell("C1").Value = "B/N";
            totalSheet.Cell("D1").Value = "Color";

            totalSheet.Column("A").Width = 15;
            totalSheet.Column("B").Width = 30;
            totalSheet.Column("C").Width = 12;
            totalSheet.Column("D").Width = 12;

            totalSheet.Range("A1:D1").Style.Font.Bold = true;
            totalSheet.Range("C1:D1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            totalSheet.Range("A1:D1").Style.Border.BottomBorder = XLBorderStyleValues.Medium;

            totalSheet.Cell("H1").Value = "Empresa";
            totalSheet.Cell("I1").Value = "B/N";
            totalSheet.Cell("J1").Value = "Color";

            totalSheet.Column("H").Width = 15;
            totalSheet.Column("I").Width = 12;
            totalSheet.Column("J").Width = 12;

            totalSheet.Range("H1:J1").Style.Font.Bold = true;
            totalSheet.Range("I1:J1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            totalSheet.Range("H1:J1").Style.Border.BottomBorder = XLBorderStyleValues.Medium;


            counterSheet.Cell("A1").Value = "Empresa";
            counterSheet.Cell("B1").Value = "Área";
            counterSheet.Cell("C1").Value = "Usuario";
            counterSheet.Cell("D1").Value = "Copiadora (B/N)";
            counterSheet.Cell("E1").Value = "Copiadora (Color)";
            counterSheet.Cell("F1").Value = "Impresora (B/N)";
            counterSheet.Cell("G1").Value = "Impresora (Color)";

            counterSheet.Column("A").Width = 15;
            counterSheet.Column("B").Width = 30;
            counterSheet.Column("C").Width = 30;
            counterSheet.Column("D").Width = 18;
            counterSheet.Column("E").Width = 18;
            counterSheet.Column("F").Width = 18;
            counterSheet.Column("G").Width = 18;

            counterSheet.Range("A1:G1").Style.Font.Bold = true;
            counterSheet.Range("D1:G1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            counterSheet.Range("A1:G1").Style.Border.BottomBorder = XLBorderStyleValues.Medium;

            counterSheet.Range("A:C").SetAutoFilter();


            detailSheet.Range("D1:H1").Merge();
            detailSheet.Range("I1:M1").Merge();
            detailSheet.Range("N1:Q1").Merge();
            detailSheet.Range("A1:A2").Merge();
            detailSheet.Range("B1:B2").Merge();
            detailSheet.Range("C1:C2").Merge();

            detailSheet.Cell("D1").Value = "Primer contador";
            detailSheet.Cell("I1").Value = "Segundo contador";
            detailSheet.Cell("N1").Value = "Contador en reporte";

            detailSheet.Cell("A1").Value = "Empresa";
            detailSheet.Cell("B1").Value = "Área";
            detailSheet.Cell("C1").Value = "Usuario";
            detailSheet.Cell("D2").Value = "Fecha";
            detailSheet.Cell("E2").Value = "Copiadora (B/N)";
            detailSheet.Cell("F2").Value = "Copiadora (Color)";
            detailSheet.Cell("G2").Value = "Impresora (B/N)";
            detailSheet.Cell("H2").Value = "Impresora (Color)";
            detailSheet.Cell("I2").Value = "Fecha";
            detailSheet.Cell("J2").Value = "Copiadora (B/N)";
            detailSheet.Cell("K2").Value = "Copiadora (Color)";
            detailSheet.Cell("L2").Value = "Impresora (B/N)";
            detailSheet.Cell("M2").Value = "Impresora (Color)";
            detailSheet.Cell("N2").Value = "Copiadora (B/N)";
            detailSheet.Cell("O2").Value = "Copiadora (Color)";
            detailSheet.Cell("P2").Value = "Impresora (B/N)";
            detailSheet.Cell("Q2").Value = "Impresora (Color)";

            detailSheet.Column("A").Width = 15;
            detailSheet.Column("B").Width = 30;
            detailSheet.Column("C").Width = 30;
            detailSheet.Column("D").Width = 12;
            detailSheet.Column("E").Width = 18;
            detailSheet.Column("F").Width = 18;
            detailSheet.Column("G").Width = 18;
            detailSheet.Column("H").Width = 18;
            detailSheet.Column("I").Width = 12;
            detailSheet.Column("J").Width = 18;
            detailSheet.Column("K").Width = 18;
            detailSheet.Column("L").Width = 18;
            detailSheet.Column("M").Width = 18;
            detailSheet.Column("N").Width = 18;
            detailSheet.Column("O").Width = 18;
            detailSheet.Column("P").Width = 18;
            detailSheet.Column("Q").Width = 18;

            detailSheet.Range("A1:Q2").Style.Font.Bold = true;
            detailSheet.Range("D1:Q1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            detailSheet.Range("D2:Q2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            detailSheet.Range("D1:Q1").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            detailSheet.Range("A2:Q2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;

            var lastRow = detailSheet.LastRowUsed().RowNumber();
            detailSheet.Range($"D1:D{lastRow}").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
            detailSheet.Range($"I1:I{lastRow}").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
            detailSheet.Range($"N1:N{lastRow}").Style.Border.LeftBorder = XLBorderStyleValues.Medium;

            detailSheet.Range("A:C").SetAutoFilter();
        }
    }
}
