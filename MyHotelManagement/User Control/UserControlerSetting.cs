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


namespace MyHotelManagement.User_Control
{
    public partial class UserControlerSetting : UserControl
    {

        
        public UserControlerSetting()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                string connectionString = DatabaseHelper.GetConnectionString();
                
            }

        }

        private int selectedUserId;

        private void ClearTextBoxes()
        {
            textBoxUsername1.Clear();
            textBoxPassword1.Clear();
            selectedUserId = 0;
        }

        private void RefreshDataGridView()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT UserId, Username, Password, Role FROM Users"; // Asegúrate de que coincide con tu tabla

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            dataGridViewUser.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while refreshing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void Clear()
        {
            textBoxUsername.Clear();
            textBoxPassword.Clear();
            tabControlUser.SelectedTab = tabPageAddUser;
        }

        public void Clear1()
        {
            textBoxUsername1.Clear();
            textBoxPassword1.Clear();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // Validar que los campos de usuario y contraseña no estén vacíos
            if (string.IsNullOrWhiteSpace(textBoxUsername.Text) || string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                MessageBox.Show("Please fill out all fields.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Obtener los valores de los campos
                string username = textBoxUsername.Text.Trim();
                string password = textBoxPassword.Text.Trim();
                string role = "guest"; // Rol predeterminado para nuevos usuarios

                // Conexión a la base de datos
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();

                    // Verificar si el usuario ya existe
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", username);
                        int userExists = (int)checkCommand.ExecuteScalar();

                        if (userExists > 0)
                        {
                            MessageBox.Show("Username already exists. Please choose a different one.", "Duplicate User", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Query para insertar un usuario
                    string query = "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, @Role)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Agregar parámetros para evitar inyección SQL
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password); // Para mayor seguridad, encripta las contraseñas
                        command.Parameters.AddWithValue("@Role", role); // Rol predeterminado

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User added successfully as guest!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Clear(); // Limpiar los campos después de añadir
                        }
                        else
                        {
                            MessageBox.Show("Failed to add user. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void tabPageSearchUser_Enter(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT * FROM Users"; // Consulta SQL para obtener todos los usuarios

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        // Llenar un DataTable con los datos de la consulta
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            // Mostrar los datos en el DataGridView
                            dataGridViewUser.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabPageAddUser_Leave_1(object sender, EventArgs e)
        {
            Clear();
        }

        private void tabControlUser_Leave_1(object sender, EventArgs e)
        {
            textBoxSearchUsername.Clear();
        }

        private void tabPageUpdateAndDeleteUser_Leave_1(object sender, EventArgs e)
        {
            Clear1();
        }

        


        private void dataGridViewUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            // Verificar que se haya seleccionado una fila válida
            if (e.RowIndex >= 0)
            {
                // Obtener la fila seleccionada
                DataGridViewRow selectedRow = dataGridViewUser.Rows[e.RowIndex];

                // Guardar el UserId seleccionado en la variable
                selectedUserId = Convert.ToInt32(selectedRow.Cells["UserId"].Value);

                // Asignar los valores de las celdas a los TextBox correspondientes
                textBoxUsername1.Text = selectedRow.Cells["Username"].Value?.ToString();
                textBoxPassword1.Text = selectedRow.Cells["Password"].Value?.ToString();
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            // Validar que haya un usuario seleccionado
            if (string.IsNullOrWhiteSpace(textBoxUsername1.Text))
            {
                MessageBox.Show("Please select a user to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string username = textBoxUsername1.Text.Trim();

                // Confirmación antes de eliminar
                DialogResult confirm = MessageBox.Show($"Are you sure you want to delete the user '{username}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                    {
                        // Consulta para eliminar el usuario
                        string query = "DELETE FROM Users WHERE Username = @Username";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Username", username);

                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("User deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                RefreshDataGridView(); // Actualizar el DataGridView
                                ClearTextBoxes(); // Limpiar los campos
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete the user. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            // Validar que haya un UserId seleccionado y datos en los TextBox
            if (selectedUserId == 0 || string.IsNullOrWhiteSpace(textBoxUsername1.Text) || string.IsNullOrWhiteSpace(textBoxPassword1.Text))
            {
                MessageBox.Show("Please select a user and fill in all fields.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string newUsername = textBoxUsername1.Text.Trim();
                string newPassword = textBoxPassword1.Text.Trim();

                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta para actualizar el usuario basado en el UserId
                    string query = "UPDATE Users SET Username = @Username, Password = @Password WHERE UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", selectedUserId); // Usar el UserId almacenado
                        command.Parameters.AddWithValue("@Username", newUsername);
                        command.Parameters.AddWithValue("@Password", newPassword);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshDataGridView(); // Actualizar el DataGridView
                            ClearTextBoxes(); // Limpiar los campos
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the user. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxSearchUsername_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxSearchUsername.Text.Trim();

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta SQL con filtro dinámico
                    string query = "SELECT * FROM Users WHERE Username LIKE @SearchText + '%'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SearchText", searchText);

                        connection.Open();

                        // Llenar un DataTable con los datos filtrados
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            // Mostrar los datos filtrados en el DataGridView
                            dataGridViewUser.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while filtering data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
