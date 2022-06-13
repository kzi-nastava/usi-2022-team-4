﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;

using Hospital.Appointments.Service;
using Hospital.Users.Service;
using Hospital.Appointments.Model;
using Hospital.Users.Model;
using Hospital.Rooms.Service;
using Hospital.Appointments.View;
using Hospital.Drugs.View;

namespace Hospital.Users.View
{
    public class Doctor : IMenuView
    {
        AppointmentService appointmentService = new AppointmentService();
        UserService userService = new UserService();
        HealthRecordService healthRecordService = new HealthRecordService();
        List<HealthRecord> healthRecords;
        List<Appointment> allMyAppointments;
        User currentRegisteredDoctor;
        RoomService roomService = new RoomService(); 
        public Doctor(User currentRegisteredDoctor)
        {
            this.currentRegisteredDoctor = currentRegisteredDoctor;
            allMyAppointments = appointmentService.GetDoctorAppointment(currentRegisteredDoctor);
            healthRecords = healthRecordService.HealthRecords;

        }
        public void DoctorMenu()
        {
            string choice;
            do
            {
                DoctorEnter.PrintDoctorMenu();
                choice = Console.ReadLine();
                if (choice.Equals("1")) {
                    this.ReadOwnAppointment();}
                else if (choice.Equals("2")){
                    this.CreateOwnAppointment();}
                else if (choice.Equals("3")){
                    this.UpdateOwnAppointment();}
                else if (choice.Equals("4")){
                    this.DeleteOwnAppointment();}
                else if (choice.Equals("5")){
                    this.ExaminingOwnSchedule();}
                else if (choice.Equals("6")){
                    DoctorPerformingAppointment doctorPerforming = new DoctorPerformingAppointment(appointmentService, healthRecords, healthRecordService, currentRegisteredDoctor, allMyAppointments, userService);
                    doctorPerforming.PerformingAppointment();}
                else if (choice.Equals("7")){
                    DrugVerification drugVerification = new DrugVerification();
                    drugVerification.DisplayDrugsForVerification();}
                else if (choice.Equals("8")){
                    DoctorDaysOff doctorDaysOff = new DoctorDaysOff(currentRegisteredDoctor);
                    doctorDaysOff.MenuForDaysOff();}
                else if (choice.Equals("9")){
                    this.LogOut();}
                else{Console.WriteLine("Unesite validnu radnju!");}
            } while (true);
        }
        private void ExaminingOwnSchedule()
        {
            string dateAppointment;
            do {
                Console.WriteLine("Unesite željeni datum: ");
                dateAppointment = Console.ReadLine();
            } while (!Utils.IsDateFormValid(dateAppointment));
            DoctorSchedule doctorSchedule = new DoctorSchedule(appointmentService, healthRecords, allMyAppointments, currentRegisteredDoctor);
            doctorSchedule.ReadOwnAppointmentSpecificDate(DateTime.ParseExact(dateAppointment, "MM/dd/yyyy", CultureInfo.InvariantCulture));
        }
        private void ReadOwnAppointment()
        {
            if (this.allMyAppointments.Count == 0){
                Console.WriteLine("\nJos uvek nemate zakazan termin!");
                return; }
            Console.WriteLine("\n\tPREGLEDI I OPERACIJE\n");
            Console.WriteLine(String.Format("|{0,5}|{1,10}|{2,10}|{3,10}|{4,10}|{5,10}|{6,10}", "Br.", "Pacijent", "Datum", "Pocetak", "Kraj", "Soba", "Vrsta termina"));
            int serialNumberAppointment = 1;
            foreach (Appointment appointment in this.allMyAppointments){
                if (CheckOwnAppointment(appointment)){
                    Console.WriteLine(appointment.ToStringDisplayForDoctor(serialNumberAppointment));
                    serialNumberAppointment += 1;}
            }
        }
        private bool CheckOwnAppointment(Appointment appointment)
        {
            if ((appointment.AppointmentState == Appointment.State.Created ||
                    appointment.AppointmentState == Appointment.State.Updated) &&
                    appointment.DateAppointment > DateTime.Now){
                return true;}
            return false;
        }
        public void CreateOwnAppointment()
        {
            string typeOfTerm = DoctorEnter.EnterTypeOfTerm();
            Appointment newAppointment;  
            if (typeOfTerm.Equals("1")){
                do{
                    newAppointment = PrintItemsToEnterExamination(typeOfTerm);  
                } while (!appointmentService.IsAppointmentFreeForDoctor(newAppointment));}
            else if (typeOfTerm.Equals("2")){
                do{    
                    newAppointment = PrintItemsToEnterOperation(typeOfTerm);
                } while (!appointmentService.IsAppointmentFreeForDoctor(newAppointment));}
            else{
                Console.WriteLine("Unesite validnu radnju!");
                return; }
            appointmentService.AddAppointment(newAppointment);
            this.allMyAppointments = appointmentService.GetDoctorAppointment(this.currentRegisteredDoctor);
            Console.WriteLine("Uspešno ste zakazali termin.");
        }
        private Appointment PrintItemsToEnterExamination(string typeOfTerm)
        {
            DateTime dateOfAppointment, startTime, newEndTime;
            int roomNumber;
            string patientEmail = DoctorEnter.EnterPatientEmail(userService);
            string newDate = DoctorEnter.EnterDate();
            string newStartTime = DoctorEnter.EnterStartTime();
            string newRoomNumber = DoctorEnter.EnterRoomNumber(roomService);   
            dateOfAppointment = DateTime.ParseExact(newDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            startTime = DateTime.ParseExact(newStartTime, "HH:mm", CultureInfo.InvariantCulture);
            newEndTime = startTime.AddMinutes(15);
            roomNumber = Int32.Parse(newRoomNumber);
            int id = appointmentService.GetNewAppointmentId();
            return new Appointment(id.ToString(), patientEmail, currentRegisteredDoctor.Email, dateOfAppointment, startTime, newEndTime, Appointment.State.Created, roomNumber, (Appointment.Type)int.Parse(typeOfTerm), false, false);
        }
        private Appointment PrintItemsToEnterOperation(string typeOfTerm)
        {
            DateTime dateOfAppointment, startTime, newEndTime;
            int roomNumber;
            string patientEmail = DoctorEnter.EnterPatientEmail(userService);
            string newDate = DoctorEnter.EnterDate();
            string newStartTime = DoctorEnter.EnterStartTime();
            string newDurationOperation = DoctorEnter.EnterDurationOperation();
            string newRoomNumber = DoctorEnter.EnterRoomNumber(roomService);
            dateOfAppointment = DateTime.ParseExact(newDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            startTime = DateTime.ParseExact(newStartTime, "HH:mm", CultureInfo.InvariantCulture);
            newEndTime = startTime.AddMinutes(Int32.Parse(newDurationOperation));
            roomNumber = Int32.Parse(newRoomNumber);
            int id = appointmentService.GetNewAppointmentId();
            return new Appointment(id.ToString(), patientEmail, currentRegisteredDoctor.Email, dateOfAppointment, startTime, newEndTime, Appointment.State.Created, roomNumber, (Appointment.Type)int.Parse(typeOfTerm), false, false);
        }
        private Appointment PrintItemsToChangeOperation(Appointment appontment)
        {
            DateTime dateOfAppointment, startTime, newEndTime;
            int roomNumber;
            string patientEmail = DoctorEnter.EnterPatientEmail(userService);
            string newDate = DoctorEnter.EnterDate();
            string newStartTime = DoctorEnter.EnterStartTime();
            string newDurationOperation = DoctorEnter.EnterDurationOperation();
            string newRoomNumber = DoctorEnter.EnterRoomNumber(roomService);
            dateOfAppointment = DateTime.ParseExact(newDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            startTime = DateTime.ParseExact(newStartTime, "HH:mm", CultureInfo.InvariantCulture);
            newEndTime = startTime.AddMinutes(Int32.Parse(newDurationOperation));
            roomNumber = Int32.Parse(newRoomNumber);
            return new Appointment(appontment.AppointmentId, patientEmail, currentRegisteredDoctor.Email, dateOfAppointment, startTime, newEndTime, Appointment.State.Updated, roomNumber, appontment.TypeOfTerm, false, appontment.Urgent);
        }
        private Appointment PrintItemsToChangeExamination(Appointment appointment)
        {
            DateTime dateOfAppointment, startTime, newEndTime;
            int roomNumber;
            string patientEmail = DoctorEnter.EnterPatientEmail(this.userService);
            string newDate = DoctorEnter.EnterDate();
            string newStartTime = DoctorEnter.EnterStartTime();
            string newRoomNumber = DoctorEnter.EnterRoomNumber(this.roomService);
            dateOfAppointment = DateTime.ParseExact(newDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            startTime = DateTime.ParseExact(newStartTime, "HH:mm", CultureInfo.InvariantCulture);
            newEndTime = startTime.AddMinutes(15);
            roomNumber = Int32.Parse(newRoomNumber);
            return new Appointment(appointment.AppointmentId, patientEmail, currentRegisteredDoctor.Email, dateOfAppointment, startTime, newEndTime, Appointment.State.Updated, roomNumber, appointment.TypeOfTerm, false, appointment.Urgent);
        }
        private void DeleteOwnAppointment()
        {
            if (this.allMyAppointments.Count != 0) {
                this.ReadOwnAppointment();
                string numberAppointment = DoctorEnter.EnterNumberAppointment(this.allMyAppointments);
                Appointment appointmentForDelete = this.allMyAppointments[Int32.Parse(numberAppointment) - 1];
                appointmentForDelete.AppointmentState = Appointment.State.Deleted;
                appointmentService.UpdateAppointment(appointmentForDelete);
                this.allMyAppointments = appointmentService.GetDoctorAppointment(this.currentRegisteredDoctor);
                Console.WriteLine("Uspesno ste obrisali termin!"); }
            else{ Console.WriteLine("\nJos uvek nemate zakazan termin!"); }
        }
        private void UpdateOwnAppointment()
        {
            if (this.allMyAppointments.Count != 0){
                this.ReadOwnAppointment();
                string numberAppointment = DoctorEnter.EnterUpdateNumberAppointment(this.allMyAppointments);
                Appointment appointmentForUpdate = this.allMyAppointments[Int32.Parse(numberAppointment) - 1];
                Appointment newAppointment;
                if (appointmentForUpdate.TypeOfTerm == Appointment.Type.Examination){newAppointment = this.PrintItemsToChangeExamination(appointmentForUpdate);}
                else{ newAppointment = this.PrintItemsToChangeOperation(appointmentForUpdate); }               
                appointmentService.UpdateAppointment(newAppointment);
                this.allMyAppointments = appointmentService.GetDoctorAppointment(this.currentRegisteredDoctor);
                Console.WriteLine("Uspesno ste izmenili termin!");}
            else{Console.WriteLine("\nJos uvek nemate zakazan termin!");}
        }
    }
}