CREATE DATABASE Hotel_System;

---USE Hotel_System;

CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL
);

-- Usuario de prueba
INSERT INTO Users (Username, Password, Role)
VALUES
('admin', 'admin123', 'Administrator'),
('guest', 'guest123', 'User');  

select * from Users;

ALTER TABLE Users
ADD CONSTRAINT UQ_Users_Username UNIQUE (Username);

go

CREATE TABLE Clients (
    ClientId INT PRIMARY KEY IDENTITY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    PhoneNo NVARCHAR(15) NOT NULL UNIQUE, -- Restricción de unicidad
    Address NVARCHAR(255) NOT NULL
);

go

CREATE TABLE Rooms (
    RoomId INT PRIMARY KEY IDENTITY,         -- Identificador único
    Type NVARCHAR(50) NOT NULL,              -- Tipo de habitación
    RoomPhone NVARCHAR(15) NOT NULL UNIQUE,  -- Número de teléfono único
    Free NVARCHAR(3) NOT NULL                -- Disponibilidad (Yes/No)
);

SELECT RoomId, Type, Free FROM Rooms WHERE Type = 'Family' AND Free = 'Yes';

select * from Rooms;

INSERT INTO Rooms (Type, RoomPhone, Free) VALUES
('Single', '1001001001', 'Yes'),
('Single', '1001001002', 'No'),
('Single', '1001001003', 'Yes'),
('Single', '1001001004', 'Yes'),
('Single', '1001001005', 'No'),
('Double', '2002002001', 'Yes'),
('Double', '2002002002', 'No'),
('Double', '2002002003', 'Yes'),
('Double', '2002002004', 'Yes'),
('Double', '2002002005', 'No'),
('Family', '3003003001', 'Yes'),
('Family', '3003003002', 'No'),
('Family', '3003003003', 'Yes'),
('Family', '3003003004', 'Yes'),
('Family', '3003003005', 'No'),
('Suite', '4004004001', 'Yes'),
('Suite', '4004004002', 'No'),
('Suite', '4004004003', 'Yes'),
('Suite', '4004004004', 'Yes'),
('Suite', '4004004005', 'No');



go

CREATE TABLE Reservations (
    ReservationId INT PRIMARY KEY IDENTITY,
    RoomType NVARCHAR(50) NOT NULL,
	RoomNumber INT NOT NULL,
	ClientId INT NOT NULL,
    DateIn DATE NOT NULL,
    DateOut DATE NOT NULL,
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId),
);
