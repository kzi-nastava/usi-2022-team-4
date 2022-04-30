﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Model;
using Hospital.Repository;

namespace Hospital.Service
{
    class AppointmentService
    {
        private AppointmentRepository appointmentRepository;
        private UserRepository userRepository;
        private List<Appointment> appointments;
        private List<User> users;

        public AppointmentRepository AppointmentRepository { get { return appointmentRepository; } }
        public List<Appointment> Appointments { get { return appointments; } }



        public AppointmentService()
        {
            appointmentRepository = new AppointmentRepository();
            userRepository = new UserRepository();
            appointments = appointmentRepository.Load();
            users = userRepository.Load();
        }

        public List<Appointment> GetDoctorAppointment(User user)
        {

            //returns all appointments for a particular doctor
            List<Appointment> doctorAppointment = new List<Appointment>();
            foreach (Appointment appointment in this.appointments)
            {
                if (appointment.DoctorEmail.Equals(user.Email))
                    doctorAppointment.Add(appointment);
            }
            return doctorAppointment;
        }

        public bool IsAppointmentFreeForDoctor(string doctorEmail,string patientEmail,DateTime newDate,DateTime startTime, DateTime newEndTime, int newRoomNumber)
        {
            foreach (Appointment appointment in appointments)
            {
                if (appointment.DoctorEmail.Equals(doctorEmail) && appointment.DateAppointment == newDate
                    && ( appointment.StartTime <= startTime || appointment.EndTime > startTime) && appointment.RoomNumber == newRoomNumber)
                {
                    Console.WriteLine("Termin je vec zauzet!");
                    return false;
                }
                if (appointment.PatientEmail.Equals(patientEmail) && appointment.DateAppointment == newDate && (appointment.StartTime <= startTime || appointment.EndTime > startTime))
                {
                    Console.WriteLine("Pacijent vec ima zakazan drugi pregled u ovom terminu!");
                    return false;

                }
            }
            return true;
        }

 


        public bool IsPatientEmailValid(string patientEmail)
        {

            foreach(User user in users)
            {
                if((user.Email == patientEmail) && (user.UserRole == User.Role.Patient) && user.UserState == User.State.Active)
                {
                    return true;
                }
            }
            Console.WriteLine("Pacijent ne postoji!");
            return false;
        }

        public bool IsDateFormValid(string date)
        {
            DateTime checkDate;
            bool validDate = DateTime.TryParseExact(date, "MM/dd/yyyy", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out checkDate);
            if (!validDate)
            {
                Console.WriteLine("Nevalidan unos datuma");
                return false;
            }
            else if (checkDate < DateTime.Now)
            {
                Console.WriteLine("Uneli ste datum koji je prosao");
                return false;
            }
            return true;
        }

        public bool IsTimeFormValid(string time)
        {
            TimeSpan checkTime;
            bool validTime = TimeSpan.TryParse(time, out checkTime);
            if (!validTime)
            {
                Console.WriteLine("Nevalidan unos vremena");
                return false;
            }
            return true;
        }

        
        public bool IsRoomNumberValid(string roomNumber)
        {
            //add a check to see if the room exists
            return true;

        }

        public bool IsIntegerValid(string number)
        {
            bool isNumeric = true;
            foreach (char c in number)
            {
                if (!Char.IsNumber(c))
                {
                    isNumeric = false;
                    Console.WriteLine("Unesite validnu radnju!");
                    break;
                }
            }
            return isNumeric;
        }

        public void DeleteAppointment(Appointment appointment)
        {
            string filePath = @"..\..\Data\appointments.csv";
            string[] lines = File.ReadAllLines(filePath);
           
            for (int i = 0; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split(new[] { ',' });
                string id = fields[0];

                if (id.Equals(appointment.AppointmentId))
                {

                    lines[i] = id + "," + fields[1] + "," + fields[2] + "," + fields[3] + "," + fields[4] + "," + fields[5]
                        + "," + (int)Appointment.AppointmentState.Deleted + "," + fields[7] + "," + fields[8];
                    Console.WriteLine("Uspesno ste obrisali termin!");
                    
                }
            }

            // saving changes
            File.WriteAllLines(filePath, lines);
        }

        public void UpdateAppointment(Appointment appointmentChange)
        {
            string filePath = @"..\..\Data\appointments.csv";
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split(new[] { ',' });
                string id = fields[0];

                if (id.Equals(appointmentChange.AppointmentId))
                {

                    lines[i] = id + "," + fields[1] + "," + fields[2] + "," + fields[3] + "," + fields[4] + "," + fields[5]
                        + "," + (int)Appointment.AppointmentState.Updated + "," + fields[7] + "," + fields[8];
                    Console.WriteLine("Uspesno ste izmenili termin!");

                }
            }
            // saving changes
            File.WriteAllLines(filePath, lines);
        }

    }

 
}
