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
using MyHotelManagement.Helpers;
using System.Text.RegularExpressions;


namespace MyHotelManagement.User_Control
{
    public partial class UserControlClient : UserControl
    {
        public UserControlClient()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                string connectionString = DatabaseHelper.GetConnectionString();

            }
        }

        public void Clear()
        {
            // Limpia los campos de entrada en el UserControlClient
            textBoxFirstName.Clear();
            textBoxLastName.Clear();
            textBoxPhoneNo.Clear();
            textBoxAddress.Clear();
        }


        

        private int selectedClientId = 0;
        private void ClearClientTextBoxes()
        {
            textBoxFirstName.Clear();
            textBoxLastName.Clear();
            textBoxPhoneNo.Clear();
            textBoxAddress.Clear();
            selectedClientId = 0;
        }
        private void ClearClientTextBoxes1()
        {
            textBoxFirstName1.Clear();
            textBoxLastName1.Clear();
            textBoxPhoneNo1.Clear();
            textBoxAddress1.Clear();
        }

        private void RefreshClientDataGridView()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT * FROM Clients";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dataGridViewClient.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while refreshing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void buttonAddClient_Click(object sender, EventArgs e)
        {
            // Validar que todos los campos estén llenos
            if (string.IsNullOrWhiteSpace(textBoxFirstName.Text) ||
                string.IsNullOrWhiteSpace(textBoxLastName.Text) ||
                string.IsNullOrWhiteSpace(textBoxPhoneNo.Text) ||
                string.IsNullOrWhiteSpace(textBoxAddress.Text))
            {
                MessageBox.Show("Please fill out all fields.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar que el número de teléfono tenga el formato correcto
            string phoneNo = textBoxPhoneNo.Text.Trim();
            if (!Regex.IsMatch(phoneNo, @"^\d{10}$")) // Asegurarse de que sean exactamente 10 dígitos
            {
                MessageBox.Show("Please enter a valid phone number (10 digits).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Obtener los valores de los demás campos
                string firstName = textBoxFirstName.Text.Trim();
                string lastName = textBoxLastName.Text.Trim();
                string address = textBoxAddress.Text.Trim();

                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta para insertar un cliente
                    string query = "INSERT INTO Clients (FirstName, LastName, PhoneNo, Address) VALUES (@FirstName, @LastName, @PhoneNo, @Address)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Agregar parámetros para evitar inyección SQL
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@PhoneNo", phoneNo);
                        command.Parameters.AddWithValue("@Address", address);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Client added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshClientDataGridView(); // Actualizar el DataGridView
                            ClearClientTextBoxes(); // Limpiar los campos
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejar errores relacionados con la base de datos
                if (ex.Number == 2627 || ex.Number == 2601) // Error por duplicado
                {
                    MessageBox.Show("A client with this phone number already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabPageSearchClient_Enter(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT * FROM Clients"; // Consulta SQL para obtener todos los clientes

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        // Llenar un DataTable con los datos de la consulta
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            // Validar si hay filas en el DataTable
                            if (dataTable.Rows.Count > 0)
                            {
                                dataGridViewClient.DataSource = dataTable; // Mostrar los datos
                            }
                            else
                            {
                                MessageBox.Show("No clients found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                dataGridViewClient.DataSource = null; // Asegurarse de que no quede basura visual
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading clients: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxSearchPhoneNo_TextChanged(object sender, EventArgs e)
        {
            string searchPhoneNo = textBoxSearchPhoneNo.Text.Trim();

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query;

                    // Si el campo está vacío, mostrar todos los registros
                    if (string.IsNullOrWhiteSpace(searchPhoneNo))
                    {
                        query = "SELECT * FROM Clients";
                    }
                    else
                    {
                        query = "SELECT * FROM Clients WHERE PhoneNo LIKE @SearchPhoneNo + '%'";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(searchPhoneNo))
                        {
                            command.Parameters.AddWithValue("@SearchPhoneNo", searchPhoneNo);
                        }

                        connection.Open();

                        // Llenar un DataTable con los datos filtrados
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            // Mostrar los datos filtrados en el DataGridView
                            dataGridViewClient.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while filtering data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonUpdateClient_Click(object sender, EventArgs e)
        {
            // Validar que haya un ClientId seleccionado y que los campos no estén vacíos
            if (selectedClientId == 0 ||
                string.IsNullOrWhiteSpace(textBoxFirstName1.Text) ||
                string.IsNullOrWhiteSpace(textBoxLastName1.Text) ||
                string.IsNullOrWhiteSpace(textBoxPhoneNo1.Text) ||
                string.IsNullOrWhiteSpace(textBoxAddress1.Text))
            {
                MessageBox.Show("Please select a client and fill in all fields.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar el formato del número de teléfono
            string phoneNo = textBoxPhoneNo1.Text.Trim();
            if (!Regex.IsMatch(phoneNo, @"^\d{10}$"))
            {
                MessageBox.Show("Please enter a valid phone number (10 digits).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Obtener los valores actualizados de los campos
                string firstName = textBoxFirstName1.Text.Trim();
                string lastName = textBoxLastName1.Text.Trim();
                string address = textBoxAddress1.Text.Trim();

                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta para actualizar al cliente
                    string query = "UPDATE Clients SET FirstName = @FirstName, LastName = @LastName, PhoneNo = @PhoneNo, Address = @Address WHERE ClientId = @ClientId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClientId", selectedClientId); // Usar el ClientId almacenado
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@PhoneNo", phoneNo);
                        command.Parameters.AddWithValue("@Address", address);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Client updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshClientDataGridView(); // Refrescar el DataGridView
                            ClearClientTextBoxes1(); // Limpiar los campos
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the client. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDeleteClient_Click(object sender, EventArgs e)
        {
            // Validar que haya un ClientId seleccionado
            if (selectedClientId == 0)
            {
                MessageBox.Show("Please select a client to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Confirmación antes de eliminar
                    DialogResult confirm = MessageBox.Show("Are you sure you want to delete this client?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (confirm == DialogResult.Yes)
                    {
                        // Consulta para eliminar al cliente
                        string query = "DELETE FROM Clients WHERE ClientId = @ClientId";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ClientId", selectedClientId); // Usar el ClientId almacenado

                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Client deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                RefreshClientDataGridView(); // Refrescar el DataGridView
                                ClearClientTextBoxes1(); // Limpiar los campos
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete the client. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void dataGridViewClient_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Obtener la fila seleccionada
                DataGridViewRow selectedRow = dataGridViewClient.Rows[e.RowIndex];

                // Almacenar el ClientId en la variable
                selectedClientId = Convert.ToInt32(selectedRow.Cells["ClientId"].Value);

                // Mostrar los datos seleccionados en los TextBox
                textBoxFirstName1.Text = selectedRow.Cells["FirstName"].Value?.ToString();
                textBoxLastName1.Text = selectedRow.Cells["LastName"].Value?.ToString();
                textBoxPhoneNo1.Text = selectedRow.Cells["PhoneNo"].Value?.ToString();
                textBoxAddress1.Text = selectedRow.Cells["Address"].Value?.ToString();
            }
        }
    }
}
