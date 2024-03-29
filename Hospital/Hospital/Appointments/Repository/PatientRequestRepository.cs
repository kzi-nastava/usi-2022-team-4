﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

using Hospital.Appointments.Model;

namespace Hospital.Appointments.Repository
{
	public class PatientRequestRepository : IPatientRequestRepository
	{
        public List<Appointment> Load()
        {
            List<Appointment> allRequests = new List<Appointment>();

            using (TextFieldParser parser = new TextFieldParser(@"..\..\Data\patientRequests.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    string id = fields[0];
                    string patientEmail = fields[1];
                    string doctorEmail = fields[2];
                    DateTime dateAppointment = DateTime.ParseExact(fields[3], "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    DateTime startExamination = DateTime.ParseExact(fields[4], "HH:mm", CultureInfo.InvariantCulture);
                    DateTime endExamination = DateTime.ParseExact(fields[5], "HH:mm", CultureInfo.InvariantCulture);
                    Appointment.State state = (Appointment.State)int.Parse(fields[6]);
                    int roomNumber = Int32.Parse(fields[7]);
                    Appointment.Type term = (Appointment.Type)int.Parse(fields[8]);
                    bool appointmentPerformed = Convert.ToBoolean(fields[9]);
                    bool urgent = Convert.ToBoolean(fields[10]);

                    Appointment appointment = new Appointment(id, patientEmail, doctorEmail, dateAppointment,
                        startExamination, endExamination, state, roomNumber, term, appointmentPerformed, urgent) ;
                    allRequests.Add(appointment);
                }
            }

            return allRequests;
        }

		public void Save(List<Appointment> requests)
		{
            string filePath = @"..\..\Data\patientRequests.csv";

            if (requests.Count == 0)
            {
                File.WriteAllText(filePath, string.Empty);

            }
            else
            {
                List<string> lines = new List<String>();

                string line;
                foreach (Appointment request in requests)
                {
                    line = request.AppointmentId + "," + request.PatientEmail + "," + request.DoctorEmail + "," + request.DateAppointment.ToString("MM/dd/yyyy") +
                        "," + request.StartTime.ToString("HH:mm") + "," + request.EndTime.ToString("HH:mm") + "," + (int)request.AppointmentState + ","
                        + request.RoomNumber + "," + (int)request.TypeOfTerm + "," + "false";
                    lines.Add(line);
                }
                File.WriteAllLines(filePath, lines.ToArray());
            }
        }
	}
}
