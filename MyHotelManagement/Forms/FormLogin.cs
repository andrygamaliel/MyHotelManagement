using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using MyHotelManagement.Helpers;

namespace MyHotelManagement
{
    public partial class FormLogin : Form
    {

        public FormLogin()
        {
            InitializeComponent();
        }

        private void pictureBoxMinimize_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBoxMinimize, "Minimize");
        }

        private void pictureBoxClose_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBoxClose, "Close");

        }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBoxMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBoxShow_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBoxShow, "Show Password");
        }

        private void pictureBoxHide_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBoxHide, "Hide Password");
        }

        private void pictureBoxShow_Click(object sender, EventArgs e)
        {
            pictureBoxShow.Hide();
            textBoxPassword.UseSystemPasswordChar = false;
            pictureBoxHide.Show();
        }

        private void pictureBoxHide_Click(object sender, EventArgs e)
        {
            pictureBoxHide.Hide();
            textBoxPassword.UseSystemPasswordChar = true;
            pictureBoxShow.Show();
        }

        private void buttonLogIn_Click(object sender, EventArgs e)
        {
            string username = textBoxUserName.Text.Trim();
            string password = textBoxPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your username and password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Obtener cadena de conexión desde App.config o appsettings.json
                string connectionString = DatabaseHelper.GetConnectionString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Consulta SQL para verificar las credenciales
                    string query = "SELECT Role FROM Users WHERE Username = @Username AND Password = @Password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            // Extraer el rol del usuario
                            string role = reader["Role"].ToString();
                            MessageBox.Show($"Welcome, {role}!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Abrir el formulario correspondiente según el rol
                            if (role == "Administrator")
                            {
                                FormDashboard adminDashboard = new FormDashboard();
                                adminDashboard.Username = textBoxUserName.Text;
                                textBoxUserName.Clear();
                                textBoxPassword.Clear();
                                adminDashboard.Show();
                            }
                            else
                            {
                                FormDashboard userDashboard = new FormDashboard();
                                userDashboard.Username = textBoxUserName.Text;
                                textBoxUserName.Clear();
                                textBoxPassword.Clear();
                                userDashboard.Show();
                            }
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabelJoke_Click(object sender, EventArgs e)
        {
            MessageBox.Show("What a shame, I'm very sorry.", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
