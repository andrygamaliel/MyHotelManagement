using MyHotelManagement.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyHotelManagement.User_Control
{
    public partial class UserControlDashboard : UserControl
    {
        public UserControlDashboard()
        {
            InitializeComponent();
        }

        private int GetUserCount()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT COUNT(*) FROM Users";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while counting users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0; // Retorna 0 en caso de error
            }
        }

        private int GetRoomCount()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT COUNT(*) FROM Rooms";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while counting rooms: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0; // Retorna 0 en caso de error
            }
        }

        private int GetClientCount()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT COUNT(*) FROM Clients";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while counting clients: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0; // Retorna 0 en caso de error
            }
        }
        private void UpdateDashboardCounts()
        {
            labelUserCount.Text = GetUserCount().ToString();
            labelRoomCount.Text = GetRoomCount().ToString();
            labelClientCount.Text = GetClientCount().ToString();
        }

        public void RefreshCounts()
        {
            UpdateDashboardCounts(); // Método ya existente que actualiza los labels
        }

    }
}
