namespace VCDA.FinancialManager.Web.Services;

public sealed partial class UiText
{
    static partial void AddAdminImportTexts(Dictionary<string, string> es, Dictionary<string, string> en)
    {
        es["Admin.PageTitle"] = "Administración de usuarios";
        es["Admin.Kicker"] = "Panel Admin";
        es["Admin.Subtitle"] = "Administra roles y accesos sin borrar los datos financieros de cada usuario.";
        es["Admin.Users"] = "usuarios";
        es["Admin.TwoFactorRequiredTitle"] = "2FA obligatorio para administradores";
        es["Admin.TwoFactorRequiredBody"] = "Tu cuenta tiene permisos de administrador. Para operar el panel de usuarios, primero configurá una app autenticadora desde la seguridad de tu cuenta.";
        es["Admin.Configure2FA"] = "Configurar 2FA";
        es["Admin.Deactivated"] = "Desactivado";
        es["Admin.MakeAdminTitle"] = "Asignar rol Admin";
        es["Admin.RemoveAdminTitle"] = "Quitar rol Admin";
        es["Admin.ConfirmDeactivate"] = "¿Confirmás la desactivación de {0}? El usuario no podrá acceder, pero sus transacciones y datos se conservan.";
        es["Admin.StatusDeactivated"] = "Usuario desactivado. Sus transacciones y datos se conservaron.";
        es["Admin.StatusReactivated"] = "Usuario reactivado.";
        es["Admin.StatusAdminAdded"] = "Rol de administrador asignado.";
        es["Admin.StatusAdminRemoved"] = "Rol de administrador removido.";
        es["Admin.StateChangeError"] = "No se pudo cambiar el estado del usuario.";
        es["Import.Kicker"] = "Carga masiva";
        es["Import.Subtitle"] = "Previsualizá el archivo, corregí errores por fila y confirmá solo los movimientos válidos.";
        es["Import.Accounts"] = "cuentas disponibles";
        es["Import.AccountsHelp"] = "Los nombres deben coincidir con el CSV.";
        es["Import.Categories"] = "categorías disponibles";
        es["Import.CategoriesHelp"] = "Se valida también el tipo Ingreso/Egreso.";
        es["Import.MaxSize"] = "CSV hasta 5 MB";
        es["Import.DecimalHelp"] = "Monto con punto decimal: 1500.50.";
        es["Import.Format"] = "Formato:";
        es["Import.FormatColumns"] = "Fecha (YYYY-MM-DD), Descripción, Monto, Tipo (Ingreso/Egreso), Categoría, Cuenta";
        es["Import.TipNames"] = "Editá los nombres de categoría y cuenta para que coincidan exactamente con los que ya cargaste.";
        es["Import.TipType"] = "Usá Tipo como Ingreso o Egreso; la categoría debe pertenecer a ese mismo tipo.";
        es["Import.TipQuotes"] = "Si la descripción contiene comas, encerrala entre comillas dobles.";
        es["Import.MissingSetup"] = "Antes de importar necesitás al menos una cuenta y una categoría cargadas.";
        es["Import.Preview"] = "Vista previa";
        es["Import.Valid"] = "válidas";
        es["Import.Total"] = "total";
        es["Import.Row"] = "Fila";
        es["Import.Status"] = "Estado";
        es["Import.Confirm"] = "Confirmar importación";
        es["Import.Rows"] = "filas";
        es["Import.Success"] = "Se importaron {0} transacciones correctamente.";
        es["Import.Error"] = "Error en la importación: {0}";

        en["Admin.PageTitle"] = "User Administration";
        en["Admin.Kicker"] = "Admin Panel";
        en["Admin.Subtitle"] = "Manage roles and access without deleting each user's financial data.";
        en["Admin.Users"] = "users";
        en["Admin.TwoFactorRequiredTitle"] = "2FA required for administrators";
        en["Admin.TwoFactorRequiredBody"] = "Your account has administrator permissions. To use the user panel, first configure an authenticator app from your account security settings.";
        en["Admin.Configure2FA"] = "Configure 2FA";
        en["Admin.Deactivated"] = "Deactivated";
        en["Admin.MakeAdminTitle"] = "Assign Admin role";
        en["Admin.RemoveAdminTitle"] = "Remove Admin role";
        en["Admin.ConfirmDeactivate"] = "Confirm deactivation of {0}? The user will not be able to access the app, but transactions and data will be preserved.";
        en["Admin.StatusDeactivated"] = "User deactivated. Transactions and data were preserved.";
        en["Admin.StatusReactivated"] = "User reactivated.";
        en["Admin.StatusAdminAdded"] = "Administrator role assigned.";
        en["Admin.StatusAdminRemoved"] = "Administrator role removed.";
        en["Admin.StateChangeError"] = "Could not change the user status.";
        en["Import.Kicker"] = "Bulk upload";
        en["Import.Subtitle"] = "Preview the file, fix row errors, and confirm only valid movements.";
        en["Import.Accounts"] = "accounts available";
        en["Import.AccountsHelp"] = "Names must match the CSV.";
        en["Import.Categories"] = "categories available";
        en["Import.CategoriesHelp"] = "Income/Expense type is also validated.";
        en["Import.MaxSize"] = "CSV up to 5 MB";
        en["Import.DecimalHelp"] = "Amount with decimal point: 1500.50.";
        en["Import.Format"] = "Format:";
        en["Import.FormatColumns"] = "Date (YYYY-MM-DD), Description, Amount, Type (Income/Expense), Category, Account";
        en["Import.TipNames"] = "Edit category and account names so they exactly match the ones already loaded.";
        en["Import.TipType"] = "Use Type as Income or Expense; the category must belong to the same type.";
        en["Import.TipQuotes"] = "If the description contains commas, wrap it in double quotes.";
        en["Import.MissingSetup"] = "Before importing, you need at least one account and one category.";
        en["Import.Preview"] = "Preview";
        en["Import.Valid"] = "valid";
        en["Import.Total"] = "total";
        en["Import.Row"] = "Row";
        en["Import.Status"] = "Status";
        en["Import.Confirm"] = "Confirm import";
        en["Import.Rows"] = "rows";
        en["Import.Success"] = "{0} transactions were imported successfully.";
        en["Import.Error"] = "Import error: {0}";
    }
}
