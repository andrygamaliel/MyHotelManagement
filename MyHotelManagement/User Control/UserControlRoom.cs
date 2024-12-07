using MyHotelManagement.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MyHotelManagement.User_Control
{
    public partial class UserControlRoom : UserControl
    {
        public UserControlRoom()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                string connectionString = DatabaseHelper.GetConnectionString();

            }
        }

        private int selectedRoomId = 0;

        private void ClearRoomControls()
        {
            comboBoxType.SelectedIndex = -1;
            textBoxPhoneNo.Clear();
            radioButtonYes.Checked = false;
            radioButtonNo.Checked = false;
        }

        public void Clear()
        {
            // Limpia el ComboBox y los TextBox
            comboBoxType1.SelectedIndex = -1;
            textBoxPhoneNo1.Clear();

            // Deselecciona los RadioButtons
            radioButtonYes1.Checked = false;
            radioButtonNo1.Checked = false;
        }

        private void ClearRoomControls1()
        {
            comboBoxType1.SelectedIndex = -1;
            textBoxPhoneNo1.Clear();
            radioButtonYes1.Checked = false;
            radioButtonNo1.Checked = false;
            selectedRoomId = 0; // Reiniciar RoomId
        }

        private void RefreshRoomDataGridView()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT * FROM Rooms";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dataGridViewRoom.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while refreshing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // Validar que los campos estén completos
            if (comboBoxType.SelectedIndex == -1 || string.IsNullOrWhiteSpace(textBoxPhoneNo.Text) || (!radioButtonYes.Checked && !radioButtonNo.Checked))
            {
                MessageBox.Show("Please fill out all fields.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar el formato del número de teléfono
            string roomPhone = textBoxPhoneNo.Text.Trim();
            if (!Regex.IsMatch(roomPhone, @"^\d{10}$"))
            {
                MessageBox.Show("Please enter a valid phone number (10 digits).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Obtener los valores de los controles
                string type = comboBoxType.SelectedItem.ToString();
                string free = radioButtonYes.Checked ? "Yes" : "No";

                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta para insertar la habitación
                    string query = "INSERT INTO Rooms (Type, RoomPhone, Free) VALUES (@Type, @RoomPhone, @Free)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Type", type);
                        command.Parameters.AddWithValue("@RoomPhone", roomPhone);
                        command.Parameters.AddWithValue("@Free", free);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Room added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshRoomDataGridView(); // Refrescar el DataGridView
                            ClearRoomControls(); // Limpiar los controles
                        }
                        else
                        {
                            MessageBox.Show("Failed to add the room. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejar errores de unicidad
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    MessageBox.Show("A room with this phone number already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void tabPageSearchRoom_Enter(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT * FROM Rooms"; // Consulta SQL para obtener todos los datos de la tabla Rooms

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        // Llenar un DataTable con los datos de la consulta
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            // Validar si hay datos en el DataTable
                            if (dataTable.Rows.Count > 0)
                            {
                                dataGridViewRoom.DataSource = dataTable; // Mostrar los datos
                            }
                            else
                            {
                                MessageBox.Show("No rooms found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                dataGridViewRoom.DataSource = null; // Asegurarse de que no quede basura visual
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading rooms: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxSearchRoomNo_TextChanged(object sender, EventArgs e)
        {
            string searchRoomId = textBoxSearchRoomNo.Text.Trim();

            // Validar que solo se ingresen números
            if (!Regex.IsMatch(searchRoomId, @"^\d*$"))
            {
                MessageBox.Show("Please enter only numeric values.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Detener ejecución si el valor no es válido
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query;

                    // Si el campo está vacío, mostrar todos los registros
                    if (string.IsNullOrWhiteSpace(searchRoomId))
                    {
                        query = "SELECT * FROM Rooms";
                    }
                    else
                    {
                        // Consulta SQL para buscar habitaciones por RoomId
                        query = "SELECT * FROM Rooms WHERE RoomId = @SearchRoomId";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(searchRoomId))
                        {
                            command.Parameters.AddWithValue("@SearchRoomId", searchRoomId);
                        }

                        connection.Open();

                        // Llenar un DataTable con los datos filtrados
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            // Mostrar los datos filtrados en el DataGridView
                            dataGridViewRoom.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while filtering data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridViewRoom_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridViewRoom.Rows[e.RowIndex];

                // Guardar el RoomId en la variable
                selectedRoomId = Convert.ToInt32(selectedRow.Cells["RoomId"].Value);

                // Autocompletar los controles con los valores seleccionados
                comboBoxType1.SelectedItem = selectedRow.Cells["Type"].Value?.ToString();
                textBoxPhoneNo1.Text = selectedRow.Cells["RoomPhone"].Value?.ToString();

                string free = selectedRow.Cells["Free"].Value?.ToString();
                if (free == "Yes")
                {
                    radioButtonYes1.Checked = true;
                    radioButtonNo1.Checked = false;
                }
                else
                {
                    radioButtonYes1.Checked = false;
                    radioButtonNo1.Checked = true;
                }
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            // Validar que haya un RoomId seleccionado y que los campos no estén vacíos
            if (selectedRoomId == 0 || comboBoxType1.SelectedIndex == -1 ||
                string.IsNullOrWhiteSpace(textBoxPhoneNo1.Text) ||
                (!radioButtonYes1.Checked && !radioButtonNo1.Checked))
            {
                MessageBox.Show("Please select a room and fill in all fields.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar el formato del número de teléfono
            string roomPhone = textBoxPhoneNo1.Text.Trim();
            if (!Regex.IsMatch(roomPhone, @"^\d{10}$"))
            {
                MessageBox.Show("Please enter a valid phone number (10 digits).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Obtener los valores actualizados de los controles
                string type = comboBoxType1.SelectedItem.ToString();
                string free = radioButtonYes1.Checked ? "Yes" : "No";

                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta para actualizar la habitación
                    string query = "UPDATE Rooms SET Type = @Type, RoomPhone = @RoomPhone, Free = @Free WHERE RoomId = @RoomId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomId", selectedRoomId);
                        command.Parameters.AddWithValue("@Type", type);
                        command.Parameters.AddWithValue("@RoomPhone", roomPhone);
                        command.Parameters.AddWithValue("@Free", free);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Room updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshRoomDataGridView(); // Refrescar el DataGridView
                            ClearRoomControls1(); // Limpiar los controles
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the room. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            // Validar que haya un RoomId seleccionado
            if (selectedRoomId == 0)
            {
                MessageBox.Show("Please select a room to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Confirmar antes de eliminar
                    DialogResult confirm = MessageBox.Show("Are you sure you want to delete this room?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (confirm == DialogResult.Yes)
                    {
                        // Consulta para eliminar la habitación
                        string query = "DELETE FROM Rooms WHERE RoomId = @RoomId";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@RoomId", selectedRoomId);

                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Room deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                RefreshRoomDataGridView(); // Refrescar el DataGridView
                                ClearRoomControls1(); // Limpiar los controles
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete the room. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
