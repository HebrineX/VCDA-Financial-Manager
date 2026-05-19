using System.Globalization;
using System.Text;
using VCDA.FinancialManager.Domain.Entities;
using VCDA.FinancialManager.Domain.Enums;
using VCDA.FinancialManager.Web.Models;

namespace VCDA.FinancialManager.Web.Services;

public static class CsvImportParser
{
    public static List<CsvImportRow> Parse(string content, IReadOnlyList<Cuenta> cuentas, IReadOnlyList<Categoria> categorias)
    {
        var rows = new List<CsvImportRow>();
        var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (i == 0 && line.StartsWith("Fecha", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var row = new CsvImportRow
            {
                LineNumber = i + 1,
                RawLine = line
            };

            var fields = SplitCsvLine(line);
            if (fields.Count < 6)
            {
                row.ErrorMessage = "Se esperan 6 columnas: Fecha, Descripcion, Monto, Tipo, Categoria, Cuenta.";
                rows.Add(row);
                continue;
            }

            if (!DateTime.TryParseExact(fields[0].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            {
                row.ErrorMessage = "Fecha inválida. Use formato YYYY-MM-DD.";
                rows.Add(row);
                continue;
            }

            row.Fecha = fecha;
            row.Descripcion = fields[1].Trim();

            if (!decimal.TryParse(fields[2].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var monto) || monto <= 0)
            {
                row.ErrorMessage = "Monto inválido. Debe ser un decimal positivo.";
                rows.Add(row);
                continue;
            }

            row.Monto = monto;

            var tipoText = fields[3].Trim();
            if (tipoText.Equals("Ingreso", StringComparison.OrdinalIgnoreCase))
            {
                row.Tipo = TipoTransaccion.Ingreso;
            }
            else if (tipoText.Equals("Egreso", StringComparison.OrdinalIgnoreCase))
            {
                row.Tipo = TipoTransaccion.Egreso;
            }
            else
            {
                row.ErrorMessage = "Tipo inválido. Use Ingreso o Egreso.";
                rows.Add(row);
                continue;
            }

            row.CategoriaNombre = fields[4].Trim();
            row.CuentaNombre = fields[5].Trim();

            var categoria = categorias.FirstOrDefault(c =>
                c.Nombre.Equals(row.CategoriaNombre, StringComparison.OrdinalIgnoreCase) &&
                c.Tipo == row.Tipo);

            if (categoria is null)
            {
                row.ErrorMessage = $"Categoría '{row.CategoriaNombre}' no encontrada para el tipo {tipoText}.";
                rows.Add(row);
                continue;
            }

            var cuenta = cuentas.FirstOrDefault(c =>
                c.Nombre.Equals(row.CuentaNombre, StringComparison.OrdinalIgnoreCase));

            if (cuenta is null)
            {
                row.ErrorMessage = $"Cuenta '{row.CuentaNombre}' no encontrada.";
                rows.Add(row);
                continue;
            }

            row.CategoriaId = categoria.Id;
            row.CuentaId = cuenta.Id;
            row.IsValid = true;
            rows.Add(row);
        }

        return rows;
    }

    public static string BuildExportCsv(IEnumerable<Transaccion> transacciones)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Fecha,Descripcion,Monto,Tipo,Categoria,Cuenta");

        foreach (var tx in transacciones)
        {
            var fecha = tx.Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var descripcion = EscapeCsv(tx.Descripcion);
            var monto = tx.Monto.ToString(CultureInfo.InvariantCulture);
            var tipo = tx.Tipo == TipoTransaccion.Ingreso ? "Ingreso" : "Egreso";
            var categoria = EscapeCsv(tx.Categoria?.Nombre ?? "");
            var cuenta = EscapeCsv(tx.Cuenta?.Nombre ?? "");
            sb.AppendLine($"{fecha},{descripcion},{monto},{tipo},{categoria},{cuenta}");
        }

        return sb.ToString();
    }

    private static List<string> SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        fields.Add(current.ToString());
        return fields;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
