SELECT TOP 1 * FROM AppointmentSlots WHERE DoctorId = 1 AND IsBooked = 0

INSERT INTO Appointments (PatientId, DoctorId, SlotId, Status, CreatedAt)
VALUES (1, 1, 5, 'Pending', GETUTCDATE())

UPDATE AppointmentSlots SET IsBooked = 1 WHERE SlotId = 5
