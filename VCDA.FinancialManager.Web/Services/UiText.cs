using System.Globalization;

namespace VCDA.FinancialManager.Web.Services;

public sealed partial class UiText
{
    private static readonly Dictionary<string, Dictionary<string, string>> Texts = BuildTexts();

    private static Dictionary<string, Dictionary<string, string>> BuildTexts()
    {
        var es = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["App.BrandName"] = "VCDA-Financial-Manager",
            ["App.Tagline"] = "Control financiero claro, visual y multiusuario.",
            ["App.BrandShort"] = "VCDA",
            ["App.BrandProduct"] = "Financial Manager",
            ["App.Version"] = "Versión",
            ["Common.Close"] = "Cerrar",
            ["Common.Save"] = "Guardar",
            ["Common.Cancel"] = "Cancelar",
            ["Common.Delete"] = "Eliminar",
            ["Common.Loading"] = "Cargando...",
            ["Common.Yes"] = "Sí",
            ["Common.No"] = "No",
            ["Common.Back"] = "Volver",
            ["Common.Continue"] = "Continuar",
            ["Common.Download"] = "Descargar",
            ["Common.Error"] = "Error",
            ["Nav.Menu"] = "Menú de navegación",
            ["Nav.Home"] = "Inicio",
            ["Nav.Accounts"] = "Mis Cuentas",
            ["Nav.Transactions"] = "Transacciones",
            ["Nav.Budgets"] = "Presupuestos",
            ["Nav.Reports"] = "Reportes",
            ["Nav.ImportCsv"] = "Importar CSV",
            ["Nav.Admin"] = "Admin",
            ["Nav.Register"] = "Registrarse",
            ["Nav.Login"] = "Iniciar sesión",
            ["Nav.Logout"] = "Cerrar sesión",
            ["Nav.Profile"] = "Mi cuenta",
            ["Theme.Light"] = "Modo Claro",
            ["Theme.Dark"] = "Modo Oscuro",
            ["Language.Toggle"] = "Cambiar idioma",
            ["Guide.Open"] = "Guía",
            ["Guide.Kicker"] = "Ayuda permanente",
            ["Guide.Title"] = "Tu mapa de VCDA-Financial-Manager",
            ["Guide.Subtitle"] = "Usala como ayuda memoria. Modo normal para operar; modo detallado para seguir el flujo paso a paso.",
            ["Guide.Normal"] = "Normal",
            ["Guide.Detail"] = "Detallado",
            ["Guide.Normal.1"] = "<strong>Mis Cuentas:</strong> crea caja, banco, billetera o tarjeta antes de cargar movimientos.",
            ["Guide.Normal.2"] = "<strong>Transacciones:</strong> registrá ingresos y egresos con categoría, cuenta, fecha y descripción clara.",
            ["Guide.Normal.3"] = "<strong>Presupuestos:</strong> define limites mensuales para anticipar desbalances.",
            ["Guide.Normal.4"] = "<strong>Reportes e Importar CSV:</strong> analiza tendencias o carga movimientos masivos desde el template.",
            ["Guide.Detail.1"] = "<strong>1. Base financiera:</strong> crea tus cuentas reales y revisa que el saldo inicial sea correcto.",
            ["Guide.Detail.2"] = "<strong>2. Clasificacion:</strong> usa categorias consistentes para que reportes y presupuestos sean comparables.",
            ["Guide.Detail.3"] = "<strong>3. Movimientos:</strong> cada movimiento requiere cuenta, categoria, monto, descripcion y fecha.",
            ["Guide.Detail.4"] = "<strong>4. Control mensual:</strong> configura presupuestos por categoria para detectar excesos antes de fin de mes.",
            ["Guide.Detail.5"] = "<strong>5. Analisis:</strong> filtra reportes por periodo, cuenta o categoria y exporta CSV cuando necesites compartir datos.",
            ["Guide.Detail.6"] = "<strong>6. Carga masiva:</strong> descarga el template, respeta nombres exactos de cuenta/categoria y confirma solo luego de previsualizar.",
            ["Guide.CreateFirstAccount"] = "Crear primera cuenta",
            ["Guide.Seen"] = "Marcar como visto",
            ["Account.ManageTitle"] = "Gestionar mi cuenta",
            ["Account.ManageSubtitle"] = "Configuración de cuenta y seguridad",
            ["Account.Profile"] = "Perfil",
            ["Account.Email"] = "Email",
            ["Account.Password"] = "Contrasena",
            ["Account.TwoFactor"] = "Autenticación de dos factores",
            ["Account.PersonalData"] = "Datos personales",
            ["Account.RecoveryCodes"] = "Códigos de recuperación",
            ["Account.RecoveryCodesIntro"] = "Guardá estos códigos en un lugar seguro.",
            ["Account.RecoveryCodesWarning"] = "Si perdés tu dispositivo y no tenés estos códigos, podés perder acceso a tu cuenta.",
            ["Admin.UsersTitle"] = "Gestión de usuarios",
            ["Admin.User"] = "Usuario",
            ["Admin.Email"] = "Email",
            ["Admin.Roles"] = "Roles",
            ["Admin.EmailConfirmed"] = "Email confirmado",
            ["Admin.Status"] = "Estado",
            ["Admin.Actions"] = "Acciones",
            ["Admin.Active"] = "Activo",
            ["Admin.Inactive"] = "Inactivo",
            ["Admin.MakeAdmin"] = "Hacer Admin",
            ["Admin.RemoveAdmin"] = "Quitar Admin",
            ["Admin.Deactivate"] = "Desactivar",
            ["Admin.Reactivate"] = "Reactivar",
            ["Import.Title"] = "Importación masiva CSV",
            ["Import.DownloadTemplate"] = "Descargar template de ejemplo",
            ["Transactions.NoAccount"] = "Antes de registrar movimientos tenés que crear al menos una cuenta.",
            ["Transactions.CreateAccount"] = "Crear cuenta"
        };

        var en = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["App.BrandName"] = "VCDA-Financial-Manager",
            ["App.Tagline"] = "Clear, visual, multi-user financial control.",
            ["App.BrandShort"] = "VCDA",
            ["App.BrandProduct"] = "Financial Manager",
            ["App.Version"] = "Version",
            ["Common.Close"] = "Close",
            ["Common.Save"] = "Save",
            ["Common.Cancel"] = "Cancel",
            ["Common.Delete"] = "Delete",
            ["Common.Loading"] = "Loading...",
            ["Common.Yes"] = "Yes",
            ["Common.No"] = "No",
            ["Common.Back"] = "Back",
            ["Common.Continue"] = "Continue",
            ["Common.Download"] = "Download",
            ["Common.Error"] = "Error",
            ["Nav.Menu"] = "Navigation menu",
            ["Nav.Home"] = "Home",
            ["Nav.Accounts"] = "My Accounts",
            ["Nav.Transactions"] = "Transactions",
            ["Nav.Budgets"] = "Budgets",
            ["Nav.Reports"] = "Reports",
            ["Nav.ImportCsv"] = "Import CSV",
            ["Nav.Admin"] = "Admin",
            ["Nav.Register"] = "Register",
            ["Nav.Login"] = "Login",
            ["Nav.Logout"] = "Sign out",
            ["Nav.Profile"] = "My account",
            ["Theme.Light"] = "Light Mode",
            ["Theme.Dark"] = "Dark Mode",
            ["Language.Toggle"] = "Change language",
            ["Guide.Open"] = "Guide",
            ["Guide.Kicker"] = "Persistent help",
            ["Guide.Title"] = "Your VCDA-Financial-Manager map",
            ["Guide.Subtitle"] = "Use it as a reference. Normal mode gets you moving; detailed mode walks the full flow step by step.",
            ["Guide.Normal"] = "Normal",
            ["Guide.Detail"] = "Detailed",
            ["Guide.Normal.1"] = "<strong>My Accounts:</strong> create cash, bank, wallet, or card accounts before adding movements.",
            ["Guide.Normal.2"] = "<strong>Transactions:</strong> record income and expenses with category, account, date, and clear description.",
            ["Guide.Normal.3"] = "<strong>Budgets:</strong> set monthly limits to anticipate imbalances.",
            ["Guide.Normal.4"] = "<strong>Reports and Import CSV:</strong> analyze trends or bulk-load movements from the template.",
            ["Guide.Detail.1"] = "<strong>1. Financial base:</strong> create your real accounts and verify the opening balance.",
            ["Guide.Detail.2"] = "<strong>2. Classification:</strong> use consistent categories so reports and budgets stay comparable.",
            ["Guide.Detail.3"] = "<strong>3. Movements:</strong> each movement requires account, category, amount, description, and date.",
            ["Guide.Detail.4"] = "<strong>4. Monthly control:</strong> configure category budgets to detect overspending early.",
            ["Guide.Detail.5"] = "<strong>5. Analysis:</strong> filter reports by period, account, or category and export CSV when needed.",
            ["Guide.Detail.6"] = "<strong>6. Bulk import:</strong> download the template, keep exact account/category names, and confirm only after previewing.",
            ["Guide.CreateFirstAccount"] = "Create first account",
            ["Guide.Seen"] = "Mark as seen",
            ["Account.ManageTitle"] = "Manage my account",
            ["Account.ManageSubtitle"] = "Account and security settings",
            ["Account.Profile"] = "Profile",
            ["Account.Email"] = "Email",
            ["Account.Password"] = "Password",
            ["Account.TwoFactor"] = "Two-factor authentication",
            ["Account.PersonalData"] = "Personal data",
            ["Account.RecoveryCodes"] = "Recovery codes",
            ["Account.RecoveryCodesIntro"] = "Keep these codes in a safe place.",
            ["Account.RecoveryCodesWarning"] = "If you lose your device and do not have these codes, you may lose access to your account.",
            ["Admin.UsersTitle"] = "User Management",
            ["Admin.User"] = "User",
            ["Admin.Email"] = "Email",
            ["Admin.Roles"] = "Roles",
            ["Admin.EmailConfirmed"] = "Email confirmed",
            ["Admin.Status"] = "Status",
            ["Admin.Actions"] = "Actions",
            ["Admin.Active"] = "Active",
            ["Admin.Inactive"] = "Inactive",
            ["Admin.MakeAdmin"] = "Make Admin",
            ["Admin.RemoveAdmin"] = "Remove Admin",
            ["Admin.Deactivate"] = "Deactivate",
            ["Admin.Reactivate"] = "Reactivate",
            ["Import.Title"] = "Bulk CSV Import",
            ["Import.DownloadTemplate"] = "Download sample template",
            ["Transactions.NoAccount"] = "Before adding movements, you need to create at least one account.",
            ["Transactions.CreateAccount"] = "Create account"
        };

        AddShellTexts(es, en);
        AddIdentityTexts(es, en);
        AddFinancialTexts(es, en);
        AddAdminImportTexts(es, en);

        return new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["es"] = es,
            ["en"] = en
        };
    }

    static partial void AddShellTexts(Dictionary<string, string> es, Dictionary<string, string> en);
    static partial void AddIdentityTexts(Dictionary<string, string> es, Dictionary<string, string> en);
    static partial void AddFinancialTexts(Dictionary<string, string> es, Dictionary<string, string> en);
    static partial void AddAdminImportTexts(Dictionary<string, string> es, Dictionary<string, string> en);

    public string Language => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase)
        ? "en"
        : "es";

    public string NextCulture => Language == "en" ? "es-AR" : "en-US";

    public string CurrentLanguageLabel => Language.ToUpperInvariant();

    public string this[string key] => Texts.TryGetValue(Language, out var cultureTexts) && cultureTexts.TryGetValue(key, out var value)
        ? value
        : Texts["es"].TryGetValue(key, out var fallback)
            ? fallback
            : key;
}
