using System.Configuration;
using System.Windows.Forms;

namespace MyHotelManagement.Helpers
{
    public static class DatabaseHelper
    {
        public static string GetConnectionString()
        {
            // Evitar que se ejecute en tiempo de diseño
            if (IsInDesignMode())
            {
                return string.Empty;
            }

            // Retorna la cadena de conexión en tiempo de ejecución
            return ConfigurationManager.ConnectionStrings["Hotel_System"]?.ConnectionString ??
                   throw new ConfigurationErrorsException("Connection string 'Hotel_System' is missing in App.config.");
        }

        private static bool IsInDesignMode()
        {
            return Application.ExecutablePath.Contains("devenv.exe");
        }
    }
}

