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
    public partial class UserControlReservation : UserControl
    {
        public UserControlReservation()
        {
            InitializeComponent();
        }

        private void LoadReservations()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta para obtener todas las reservaciones con información del cliente
                    string query = @"
                SELECT 
                    Reservations.ReservationId,
                    Reservations.RoomType,
                    Reservations.RoomNumber,
                    Reservations.DateIn,
                    Reservations.DateOut,
                    Clients.ClientId,
                    Clients.FirstName,
                    Clients.LastName
                FROM Reservations
                INNER JOIN Clients ON Reservations.ClientId = Clients.ClientId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dataGridViewReservation.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading reservations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int selectedReservationId = 0;


        private void ClearReservationControls()
        {
            comboBoxType.SelectedIndex = -1;
            comboBoxNo.Items.Clear();
            textBoxClientID.Clear();
            dateTimePickerIn.Value = DateTime.Now;
            dateTimePickerOut.Value = DateTime.Now.AddDays(1);
        }

        private void ClearReservationControls1()
        {
            comboBoxType1.SelectedIndex = -1;
            comboBoxNo1.SelectedIndex = -1;
            textBoxClientID1.Clear();
            dateTimePickerIn1.Value = DateTime.Now;
            dateTimePickerOut1.Value = DateTime.Now.AddDays(1);
            selectedReservationId = 0; // Reiniciar el ReservationId
        }

        public void Clear()
        {
            comboBoxType1.SelectedIndex = -1;
            comboBoxNo1.Items.Clear();
            textBoxClientID1.Clear();
            dateTimePickerIn1.Value = DateTime.Now;
            dateTimePickerOut1.Value = DateTime.Now.AddDays(1);
        }

        private void RefreshReservationDataGridView()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT * FROM Reservations";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dataGridViewReservation.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while refreshing reservations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FillComboBoxNo1(string roomType, string selectedRoomNumber)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta para obtener todas las habitaciones del tipo seleccionado que están disponibles
                    string query = "SELECT RoomId FROM Rooms WHERE Type = @Type AND Free = 'Yes'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Type", roomType);

                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            comboBoxNo1.Items.Clear(); // Limpiar el ComboBox antes de llenarlo

                            while (reader.Read())
                            {
                                comboBoxNo1.Items.Add(reader["RoomId"].ToString());
                            }
                        }
                    }

                    // Seleccionar el número de habitación actual si está disponible
                    if (!string.IsNullOrWhiteSpace(selectedRoomNumber) && comboBoxNo1.Items.Contains(selectedRoomNumber))
                    {
                        comboBoxNo1.SelectedItem = selectedRoomNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while filling room numbers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsRoomAvailable(int roomId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query = "SELECT Free FROM Rooms WHERE RoomId = @RoomId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomId", roomId);

                        connection.Open();
                        string freeStatus = command.ExecuteScalar()?.ToString();

                        return freeStatus == "Yes"; // Retorna true si la habitación está disponible
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while checking room availability: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void UpdateReservation(int newRoomId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();

                    // Obtener el RoomId actual de la reservación seleccionada
                    string queryGetOldRoom = "SELECT RoomNumber FROM Reservations WHERE ReservationId = @ReservationId";
                    int oldRoomId;
                    using (SqlCommand commandGetOldRoom = new SqlCommand(queryGetOldRoom, connection))
                    {
                        commandGetOldRoom.Parameters.AddWithValue("@ReservationId", selectedReservationId);
                        oldRoomId = Convert.ToInt32(commandGetOldRoom.ExecuteScalar());
                    }

                    // Actualizar el estado de la habitación anterior a 'Yes'
                    string queryUpdateOldRoom = "UPDATE Rooms SET Free = 'Yes' WHERE RoomId = @OldRoomId";
                    using (SqlCommand commandUpdateOldRoom = new SqlCommand(queryUpdateOldRoom, connection))
                    {
                        commandUpdateOldRoom.Parameters.AddWithValue("@OldRoomId", oldRoomId);
                        commandUpdateOldRoom.ExecuteNonQuery();
                    }

                    // Actualizar la reservación con la nueva habitación
                    string queryUpdateReservation = @"
                UPDATE Reservations
                SET RoomNumber = @NewRoomId, DateIn = @DateIn, DateOut = @DateOut
                WHERE ReservationId = @ReservationId";
                    using (SqlCommand commandUpdateReservation = new SqlCommand(queryUpdateReservation, connection))
                    {
                        commandUpdateReservation.Parameters.AddWithValue("@NewRoomId", newRoomId);
                        commandUpdateReservation.Parameters.AddWithValue("@DateIn", dateTimePickerIn1.Value.Date);
                        commandUpdateReservation.Parameters.AddWithValue("@DateOut", dateTimePickerOut1.Value.Date);
                        commandUpdateReservation.Parameters.AddWithValue("@ReservationId", selectedReservationId);

                        int rowsAffected = commandUpdateReservation.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Cambiar el estado de la nueva habitación a 'No'
                            string queryUpdateNewRoom = "UPDATE Rooms SET Free = 'No' WHERE RoomId = @NewRoomId";
                            using (SqlCommand commandUpdateNewRoom = new SqlCommand(queryUpdateNewRoom, connection))
                            {
                                commandUpdateNewRoom.Parameters.AddWithValue("@NewRoomId", newRoomId);
                                commandUpdateNewRoom.ExecuteNonQuery();
                            }

                            MessageBox.Show("Reservation updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshReservationDataGridView(); // Refrescar DataGridView
                            ClearReservationControls1(); // Limpiar los controles
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the reservation. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxType.SelectedIndex == -1)
            {
                return; // Si no hay nada seleccionado, no hacer nada
            }

            string selectedType = comboBoxType.SelectedItem.ToString();

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta para obtener las habitaciones disponibles del tipo seleccionado
                    string query = "SELECT RoomId FROM Rooms WHERE Type = @Type AND Free = 'Yes'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Type", selectedType);

                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            comboBoxNo.Items.Clear(); // Limpiar el ComboBox antes de llenarlo

                            while (reader.Read())
                            {
                                comboBoxNo.Items.Add(reader["RoomId"].ToString());
                            }
                        }
                    }
                }

                // Mostrar mensaje si no hay habitaciones disponibles
                if (comboBoxNo.Items.Count == 0)
                {
                    MessageBox.Show("No rooms available for the selected type.", "No Availability", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }   
        }

        private void buttonAddReservation_Click(object sender, EventArgs e)
        {
            // Validar que los campos estén completos
            if (comboBoxType.SelectedIndex == -1 || comboBoxNo.SelectedIndex == -1 ||
                string.IsNullOrWhiteSpace(textBoxClientID.Text) ||
                dateTimePickerIn.Value >= dateTimePickerOut.Value)
            {
                MessageBox.Show("Please fill out all fields and ensure the check-in date is before the check-out date.",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Obtener los valores de los controles
                string roomType = comboBoxType.SelectedItem.ToString();
                int roomId = Convert.ToInt32(comboBoxNo.SelectedItem.ToString());
                int clientId = Convert.ToInt32(textBoxClientID.Text.Trim());
                DateTime dateIn = dateTimePickerIn.Value.Date;
                DateTime dateOut = dateTimePickerOut.Value.Date;

                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();

                    // Consulta para insertar la reservación
                    string queryInsert = "INSERT INTO Reservations (RoomType, RoomNumber, ClientId, DateIn, DateOut) VALUES (@RoomType, @RoomNumber, @ClientId, @DateIn, @DateOut)";
                    using (SqlCommand commandInsert = new SqlCommand(queryInsert, connection))
                    {
                        commandInsert.Parameters.AddWithValue("@RoomType", roomType);
                        commandInsert.Parameters.AddWithValue("@RoomNumber", roomId);
                        commandInsert.Parameters.AddWithValue("@ClientId", clientId);
                        commandInsert.Parameters.AddWithValue("@DateIn", dateIn);
                        commandInsert.Parameters.AddWithValue("@DateOut", dateOut);

                        int rowsAffected = commandInsert.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Actualizar el estado de disponibilidad de la habitación a 'No'
                            string queryUpdate = "UPDATE Rooms SET Free = 'No' WHERE RoomId = @RoomId";
                            using (SqlCommand commandUpdate = new SqlCommand(queryUpdate, connection))
                            {
                                commandUpdate.Parameters.AddWithValue("@RoomId", roomId);
                                commandUpdate.ExecuteNonQuery();
                            }

                            MessageBox.Show("Reservation added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshReservationDataGridView(); // Refrescar el DataGridView de reservaciones
                            ClearReservationControls(); // Limpiar los controles
                        }
                        else
                        {
                            MessageBox.Show("Failed to add reservation. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"A database error occurred: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxSearchClientID_TextChanged(object sender, EventArgs e)
        {
            string searchClientId = textBoxSearchClientID.Text.Trim();

            // Validar que solo se ingresen números
            if (!Regex.IsMatch(searchClientId, @"^\d*$"))
            {
                MessageBox.Show("Please enter only numeric values for Client ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    string query;

                    // Si el campo está vacío, mostrar todos los registros
                    if (string.IsNullOrWhiteSpace(searchClientId))
                    {
                        query = @"
                    SELECT 
                        Reservations.ReservationId,
                        Reservations.RoomType,
                        Reservations.RoomNumber,
                        Reservations.DateIn,
                        Reservations.DateOut,
                        Clients.ClientId,
                        Clients.FirstName,
                        Clients.LastName
                    FROM Reservations
                    INNER JOIN Clients ON Reservations.ClientId = Clients.ClientId";
                    }
                    else
                    {
                        // Consulta para buscar reservaciones por ClientId
                        query = @"
                    SELECT 
                        Reservations.ReservationId,
                        Reservations.RoomType,
                        Reservations.RoomNumber,
                        Reservations.DateIn,
                        Reservations.DateOut,
                        Clients.ClientId,
                        Clients.FirstName,
                        Clients.LastName
                    FROM Reservations
                    INNER JOIN Clients ON Reservations.ClientId = Clients.ClientId
                    WHERE Clients.ClientId = @ClientId";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(searchClientId))
                        {
                            command.Parameters.AddWithValue("@ClientId", Convert.ToInt32(searchClientId));
                        }

                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dataGridViewReservation.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while filtering reservations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabPageSearchReservation_Enter(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    // Consulta para obtener todas las reservaciones con información del cliente
                    string query = @"
                SELECT 
                    Reservations.ReservationId,
                    Reservations.RoomType,
                    Reservations.RoomNumber,
                    Reservations.DateIn,
                    Reservations.DateOut,
                    Clients.ClientId,
                    Clients.FirstName,
                    Clients.LastName
                FROM Reservations
                INNER JOIN Clients ON Reservations.ClientId = Clients.ClientId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dataGridViewReservation.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading reservations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridViewReservation_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridViewReservation.Rows[e.RowIndex];

                // Guardar el ReservationId
                selectedReservationId = Convert.ToInt32(selectedRow.Cells["ReservationId"].Value);

                // Autocompletar los valores en los controles
                comboBoxType1.SelectedItem = selectedRow.Cells["RoomType"].Value?.ToString();
                textBoxClientID1.Text = selectedRow.Cells["ClientId"].Value?.ToString();
                dateTimePickerIn1.Value = Convert.ToDateTime(selectedRow.Cells["DateIn"].Value);
                dateTimePickerOut1.Value = Convert.ToDateTime(selectedRow.Cells["DateOut"].Value);

                // Llenar comboBoxNo1 con las habitaciones del tipo seleccionado
                string roomType = selectedRow.Cells["RoomType"].Value?.ToString();
                string roomNumber = selectedRow.Cells["RoomNumber"].Value?.ToString();
                FillComboBoxNo1(roomType, roomNumber);
            }
        }

        private void buttonUpdateReservation_Click(object sender, EventArgs e)
        {
            if (selectedReservationId == 0 || comboBoxType1.SelectedIndex == -1 || comboBoxNo1.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a reservation and fill out all fields.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int roomId = Convert.ToInt32(comboBoxNo1.SelectedItem.ToString());

            // Verificar si la habitación está disponible
            if (!IsRoomAvailable(roomId))
            {
                MessageBox.Show("The selected room is already reserved by another client.", "Room Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Actualizar la reservación
            UpdateReservation(roomId);
        }

        private void buttonCancelReservation_Click(object sender, EventArgs e)
        {
            if (selectedReservationId == 0)
            {
                MessageBox.Show("Please select a reservation to cancel.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();

                    // Obtener el RoomId de la reservación seleccionada
                    string queryGetRoom = "SELECT RoomNumber FROM Reservations WHERE ReservationId = @ReservationId";
                    int roomId;
                    using (SqlCommand commandGetRoom = new SqlCommand(queryGetRoom, connection))
                    {
                        commandGetRoom.Parameters.AddWithValue("@ReservationId", selectedReservationId);
                        roomId = Convert.ToInt32(commandGetRoom.ExecuteScalar());
                    }

                    // Cancelar la reservación
                    string queryCancel = "DELETE FROM Reservations WHERE ReservationId = @ReservationId";
                    using (SqlCommand commandCancel = new SqlCommand(queryCancel, connection))
                    {
                        commandCancel.Parameters.AddWithValue("@ReservationId", selectedReservationId);

                        int rowsAffected = commandCancel.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Cambiar el estado de la habitación a "Yes"
                            string queryRoomUpdate = "UPDATE Rooms SET Free = 'Yes' WHERE RoomId = @RoomId";
                            using (SqlCommand commandRoomUpdate = new SqlCommand(queryRoomUpdate, connection))
                            {
                                commandRoomUpdate.Parameters.AddWithValue("@RoomId", roomId);
                                commandRoomUpdate.ExecuteNonQuery();
                            }

                            MessageBox.Show("Reservation canceled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshReservationDataGridView(); // Refrescar el DataGridView
                            ClearReservationControls1(); // Limpiar los controles
                        }
                        else
                        {
                            MessageBox.Show("Failed to cancel the reservation. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBoxType1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxType1.SelectedIndex != -1)
            {
                // Obtener el tipo de habitación seleccionado
                string selectedType = comboBoxType1.SelectedItem.ToString();

                // Llenar comboBoxNo1 con las habitaciones disponibles del tipo seleccionado
                FillComboBoxNo1(selectedType, null);
            }
        }
    }
}
