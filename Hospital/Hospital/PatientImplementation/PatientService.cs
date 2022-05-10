﻿// HEADER
/*In PatientService there are functions that enable the implementation of the patient's functionalities*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Model;
using Hospital.Service;
using System.Globalization;
using System.IO;
using Hospital.DoctorImplementation;

namespace Hospital.PatientImplementation
{
    class PatientService
    {
        RequestService _requestService;
        AppointmentService _appointmentService = new AppointmentService();  // loading all appointments
        List<Appointment> _allAppointments;
        List<User> _allUsers;
        User _currentRegisteredUser;

        // getters
        public List<Appointment> Appointments { get { return _allAppointments; } }

        public RequestService RequestService { get { return _requestService; } }

        public AppointmentService AppointmentService { get { return _appointmentService; } }

        public PatientService(User user, List<User> allUsers)
        {
            _requestService = new RequestService(_appointmentService);
            this._currentRegisteredUser = user;
            this._allUsers = allUsers;
            _allAppointments = _appointmentService.AppointmentRepository.Load();
        }

        // VRATI LISTU I ONDA U PATIENT SETUJ
        public void RefreshPatientAppointments(Patient currentRegisteredPatient) 
        {
             this._allAppointments = _appointmentService.AppointmentRepository.Load();

            // finding all appointments for the registered patient that have not been deleted and has not yet passed
            List<Appointment> patientCurrentAppointment = new List<Appointment>();
            foreach (Appointment appointment in _allAppointments)
            {
                if (appointment.PatientEmail.Equals(_currentRegisteredUser.Email) &&
                    appointment.AppointmentState != Appointment.State.Deleted &&
                    appointment.DateAppointment >= DateTime.Now.Date)
                {
                    // check if today's appointment has passed
                    if (appointment.DateAppointment == DateTime.Now.Date && appointment.StartTime.Hour <= DateTime.Now.Hour)
                        continue;
                    patientCurrentAppointment.Add(appointment);
                }
            }
            currentRegisteredPatient.PatientAppointments = patientCurrentAppointment;
        }

        public void TableHeader()
        {
            Console.WriteLine();
            Console.WriteLine(String.Format("{0,3}|{1,10}|{2,10}|{3,10}|{4,10}|{5,10}|{6,10}|{7,10}",
                "Br.", "Doktor", "Datum", "Pocetak", "Kraj", "Soba", "Tip", "Stanje"));
        }

        public List<Appointment> FindAppointmentsForDeleteAndUpdate(Patient currentlyRegisteredPatient)
        {
            int appointmentOrdinalNumber = 0;

            this.TableHeader();

            List<Appointment> appointmentsForChange = new List<Appointment>();
            foreach (Appointment appointment in currentlyRegisteredPatient.PatientAppointments) 
            {
                if (appointment.AppointmentState != Appointment.State.UpdateRequest &&
                    appointment.AppointmentState != Appointment.State.DeleteRequest && 
                    appointment.TypeOfTerm != Appointment.Type.Operation)
                {
                    appointmentsForChange.Add(appointment);
                    appointmentOrdinalNumber++;
                    Console.WriteLine(appointmentOrdinalNumber + ". " + appointment.DisplayOfPatientAppointment());
                }
            }
            Console.WriteLine();

            return appointmentsForChange;
        }

        public bool HasPatientAppointmen(List<Appointment> patientAppointments)
        {
            if (!(patientAppointments.Count == 0))
                return true;
            Console.WriteLine("\nNemate ni jedan pregled za prikaz!");
            return false;
        }

        public string[] InputValuesForAppointment()
        {
            string[] inputValues = new string[3];

            string doctorEmail;
            string newDate;
            string newStartTime;

            do
            {
                Console.Write("\nUnesite email doktora: ");
                doctorEmail = Console.ReadLine();
                Console.Write("Unesite datum (MM/dd/yyyy): ");
                newDate = Console.ReadLine();
                Console.Write("Unesite vreme pocetka pregleda (HH:mm): ");
                newStartTime = Console.ReadLine();
            } while (!this.IsValidInput(doctorEmail, newDate, newStartTime));

            inputValues[0] = doctorEmail;
            inputValues[1] = newDate;
            inputValues[2] = newStartTime;

            return inputValues;
        }

        public bool IsValidInput(string doctorEmail, string newDateAppointment, string newStartTime)
        {           
            return (_appointmentService.IsDateFormValid(newDateAppointment) &&
                _appointmentService.IsTimeFormValid(newStartTime) && _appointmentService.IsDoctorExist(doctorEmail)!=null);
        }

        //public bool IsAppointmentFree(string id, string[] inputValues)
        //{
        //    string doctorEmail = inputValues[0];
        //    string newDate = inputValues[1];
        //    string newStartTime = inputValues[2];

        //    DateTime dateExamination = DateTime.Parse(newDate);
        //    DateTime startTime = DateTime.Parse(newStartTime);

        //    foreach (Appointment appointment in _appointmentService.Appointments) {
        //        if (appointment.DoctorEmail.Equals(doctorEmail) && appointment.DateAppointment == dateExamination
        //            && appointment.StartTime <= startTime && appointment.EndTime > startTime)
        //        {
        //            //Console.WriteLine("Izabran doktor je zauzet u tom terminu!");
        //            return false;
        //        }
        //        else if (appointment.PatientEmail.Equals(_currentRegisteredUser.Email) && appointment.DateAppointment == dateExamination
        //            && appointment.StartTime <= startTime && appointment.EndTime > startTime && !appointment.AppointmentId.Equals(id))
        //        {
        //            //Console.WriteLine("Vec imate zakazan pregled u tom terminu!");
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        public List<UserAction> LoadMyCurrentActions(string registeredUserEmail)
        {
            UserActionService actionService = new UserActionService();  // loading all users actions
            List<UserAction> myCurrentActions = new List<UserAction>();

            foreach (UserAction action in actionService.Actions)
            {
                if (registeredUserEmail.Equals(action.PatientEmail) && (DateTime.Now - action.ActionDate).TotalDays <= 30)
                    myCurrentActions.Add(action);
            }
            return myCurrentActions;
        }

        public void BlockAccessApplication()
        {
            // read from file
            string filePath = @"..\..\Data\users.csv";
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split(new[] { ',' });
                string userEmailFromFile = fields[1];
                if (_currentRegisteredUser.Email.Equals(userEmailFromFile))
                    lines[i] = fields[0] + "," + fields[1] + "," + fields[2] + "," + fields[3] + "," + fields[4]
                        + "," + (int) User.State.BlockedBySystem;
            }
            // saving changes
            File.WriteAllLines(filePath, lines);
        }

        public void AppendToActionFile(string typeAction)
        {
            string filePath = @"..\..\Data\actions.csv";

            UserAction newAction;
            if (typeAction.Equals("create"))
                newAction = new UserAction(_currentRegisteredUser.Email, DateTime.Now, UserAction.State.Created);
            else if(typeAction.Equals("update"))
                newAction = new UserAction(_currentRegisteredUser.Email, DateTime.Now, UserAction.State.Modified);
            else
                newAction = new UserAction(_currentRegisteredUser.Email, DateTime.Now, UserAction.State.Deleted);

            File.AppendAllText(filePath, newAction.ToString());
        }

        public Appointment PickAppointmentForDeleteOrUpdate(Patient patient)
        {
            List<Appointment> appointmentsForChange = this.FindAppointmentsForDeleteAndUpdate(patient);

            if (appointmentsForChange.Count == 0)
                return null;

            string inputNumberAppointment;
            int numberAppointment;
            do
            {
                Console.WriteLine("Unesite broj pregleda za izabranu operaciju");
                Console.Write(">> ");
                inputNumberAppointment = Console.ReadLine();
            } while (!int.TryParse(inputNumberAppointment, out numberAppointment) || numberAppointment < 1
            || numberAppointment > appointmentsForChange.Count);

            return appointmentsForChange[numberAppointment - 1];
        }

        public void ReadFileForDeleteAppointment(Appointment appointmentForDelete)
        {
            string filePath = @"..\..\Data\appointments.csv";
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split(new[] { ',' });
                string id = fields[0];

                if (id.Equals(appointmentForDelete.AppointmentId))
                {

                    if ((appointmentForDelete.DateAppointment - DateTime.Now).TotalDays <= 2)
                    {
                        appointmentForDelete.AppointmentState = Appointment.State.DeleteRequest;
                        this._requestService.Requests.Add(appointmentForDelete);
                        this._requestService.UpdateFile();
                        Console.WriteLine("Zahtev za brisanje je poslat sekretaru!");
                    }
                    else
                    {
                        // logical deletion
                        appointmentForDelete.AppointmentState = Appointment.State.Deleted;
                        Console.WriteLine("Uspesno ste obrisali pregled!");
                    }
                    lines[i] = appointmentForDelete.ToString();
                }
            }
            // saving changes
            File.WriteAllLines(filePath, lines);
        }

        public void ReadFileForUpdateAppointment(Appointment appointmentForUpdate, string[] inputValues)
        {

            string doctorEmail = inputValues[0];
            string newDate = inputValues[1];
            string newStartTime = inputValues[2];

            string filePath = @"..\..\Data\appointments.csv";
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] fields = lines[i].Split(new[] { ',' });
                string id = fields[0];

                if (id.Equals(appointmentForUpdate.AppointmentId))
                {
                    Appointment newAppointment;
                    DateTime appointmentDate = DateTime.ParseExact(newDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    DateTime appointmentStartTime = DateTime.ParseExact(newStartTime, "HH:mm", CultureInfo.InvariantCulture);
                    DateTime appointmentEndTime = appointmentStartTime.AddMinutes(15);

                    if ((appointmentForUpdate.DateAppointment - DateTime.Now).TotalDays <= 2)
                    {
                        appointmentForUpdate.AppointmentState = Appointment.State.UpdateRequest;
                        newAppointment = new Appointment(id, this._currentRegisteredUser.Email, doctorEmail, appointmentDate,
                        appointmentStartTime, appointmentEndTime, Appointment.State.UpdateRequest, Int32.Parse(fields[7]),
                        Appointment.Type.Examination, false);

                        this._requestService.Requests.Add(newAppointment);
                        this._requestService.UpdateFile();
                        Console.WriteLine("Zahtev za izmenu je poslat sekretaru!");
                    }
                    else
                    {
                        appointmentForUpdate.AppointmentState = Appointment.State.Updated;
                        appointmentForUpdate.DoctorEmail = doctorEmail;
                        appointmentForUpdate.DateAppointment = appointmentDate;
                        appointmentForUpdate.StartTime = appointmentStartTime;
                        appointmentForUpdate.EndTime = appointmentEndTime;
                        Console.WriteLine("Uspesno ste izvrsili izmenu pregleda!");
                    }
                    lines[i] = appointmentForUpdate.ToString();
                }
                // saving changes
                File.WriteAllLines(filePath, lines);
            }
        }

        public bool CompareTwoTimes(string startTime, string endTime)
        {
            DateTime initialTime = DateTime.ParseExact(startTime, "HH:mm", CultureInfo.InvariantCulture);
            DateTime latestTime = DateTime.ParseExact(endTime, "HH:mm", CultureInfo.InvariantCulture);

            if (initialTime.AddMinutes(15) > latestTime)
                return false;

            return initialTime < latestTime;
        }

        public string[] InputValueForRecommendationAppointments()
        {
            string[] inputValues = new string[4];

            string doctorEmail;
            string latestDate;
            string startTime;
            string endTime;

            do
            {
                Console.Write("\nUnesite email doktora: ");
                doctorEmail = Console.ReadLine();
                Console.Write("Unesite datum do kog mora da bude pregled (MM/dd/yyyy): ");
                latestDate = Console.ReadLine();
                Console.Write("Unesite vreme najranije moguceg pregleda (HH:mm): ");
                startTime = Console.ReadLine();
                Console.Write("Unesite vreme najkasnijeg moguceg pregleda (HH:mm): ");
                endTime = Console.ReadLine();
            } while (!(this.IsValidInput(doctorEmail, latestDate, startTime) && _appointmentService.IsTimeFormValid(endTime)
            && this.CompareTwoTimes(startTime, endTime)));

            inputValues[0] = doctorEmail;
            inputValues[1] = latestDate;
            inputValues[2] = startTime;
            inputValues[3] = endTime;

            return inputValues;
        }

        //public string CheckPriority()
        //{
        //    string priority;
        //    do
        //    {
        //        Console.WriteLine("\nIzaberite prioritet pri zakazivanju");
        //        Console.WriteLine("1. Doktor");
        //        Console.WriteLine("2. Vremenski opseg");
        //        Console.Write(">> ");
        //        priority = Console.ReadLine();

        //        if (priority.Equals("1"))
        //            return "1";
        //        else if (priority.Equals("2"))
        //            return "2";
        //    } while (true);
        //}

        //public bool IsTimeBetweenTwoTimes(DateTime time)
        //{
        //    DateTime midnight = DateTime.ParseExact("00:00", "HH:mm", CultureInfo.InvariantCulture);
        //    DateTime earliestTime = DateTime.ParseExact("06:00", "HH:mm", CultureInfo.InvariantCulture);
        //    if (TimeSpan.Compare(time.TimeOfDay, midnight.TimeOfDay) != -1 &&
        //        TimeSpan.Compare(time.TimeOfDay, earliestTime.TimeOfDay) == -1) 
        //        return true;
        //    return false;
        //}

        //public Appointment FindAppointmentAtChosenDoctor(string[] inputValues)
        //{
        //    string doctorEmail = inputValues[0];
        //    DateTime latestDate = DateTime.ParseExact(inputValues[1], "MM/dd/yyyy", CultureInfo.InvariantCulture);
        //    DateTime startTime = DateTime.ParseExact(inputValues[2], "HH:mm", CultureInfo.InvariantCulture);

        //    DateTime earliestDate = DateTime.Now.AddDays(1);
        //    string[] dataForAppointment;
        //    do
        //    {
        //        if (this.IsTimeBetweenTwoTimes(startTime))
        //        {
        //            startTime = DateTime.ParseExact("06:00", "HH:mm", CultureInfo.InvariantCulture);
        //            earliestDate = earliestDate.AddDays(1);
        //        }
                
        //        if (earliestDate.Date > latestDate.Date)
        //            return null; // daj mu predloge

        //        dataForAppointment = new string[] { doctorEmail, earliestDate.ToString("MM/dd/yyyy"), startTime.ToString("HH:mm") };
        //        startTime = startTime.AddMinutes(15);
        //    } while (!this.IsAppointmentFree("0", dataForAppointment));

        //    return this.CreateAppointment(dataForAppointment);
        //}

        //public string AcceptAppointment(Appointment newAppointment)
        //{
        //    Console.WriteLine("\nPRONADJEN TERMIN PREGLEDA");
        //    this.TableHeader();
        //    Console.WriteLine("1. " + newAppointment.DisplayOfPatientAppointment());

        //    string choice;
        //    do
        //    {
        //        Console.WriteLine("\nIzaberite opciju");
        //        Console.WriteLine("1. Prihvatam");
        //        Console.WriteLine("2. Odbijam");
        //        Console.Write(">> ");
        //        choice = Console.ReadLine();

        //        if (choice.Equals("1"))
        //            return "1";
        //        else if (choice.Equals("2"))
        //            return "2";
        //    } while (true);
        //}

        //public Appointment FindAppointmentInTheSelectedRange(string[] inputValues)
        //{
        //    string[] dataForAppointment = new string[3];

        //    foreach (User doctor in this.AllDoctors())
        //    {
        //        dataForAppointment = this.IsDoctorAvailable(inputValues, doctor);
        //        if (dataForAppointment != null)
        //            break;
        //    }
        //    if (dataForAppointment == null)
        //        return null;  // daj mu opcije

        //    return this.CreateAppointment(dataForAppointment);
        //}

        //public string[] IsDoctorAvailable(string[] inputValues, User doctor)
        //{
        //    DateTime latestDate = DateTime.ParseExact(inputValues[1], "MM/dd/yyyy", CultureInfo.InvariantCulture);
        //    DateTime startTime = DateTime.ParseExact(inputValues[2], "HH:mm", CultureInfo.InvariantCulture);
        //    DateTime endTime = DateTime.ParseExact(inputValues[3], "HH:mm", CultureInfo.InvariantCulture);

        //    DateTime earliestDate = DateTime.Now.AddDays(1);
        //    string[] dataForAppointment;
        //    do
        //    {
        //        if (startTime.TimeOfDay >= endTime.TimeOfDay)
        //        {
        //            earliestDate = earliestDate.AddDays(1);
        //            startTime = DateTime.ParseExact(inputValues[2], "HH:mm", CultureInfo.InvariantCulture);
        //        }

        //        if (earliestDate.Date > latestDate.Date)
        //            return null;
               
        //        dataForAppointment = new string[] { doctor.Email, earliestDate.ToString("MM/dd/yyyy"), startTime.ToString("HH:mm") };
        //        startTime = startTime.AddMinutes(15);
        //    } while (!this.IsAppointmentFree("0", dataForAppointment));

        //    return dataForAppointment;
        //}

        //public void FindAppointmentsClosestPatientWishesForDoctor(string[] inputValues)
        //{
        //    List<Appointment> appointmentsForChoosing = new List<Appointment>();

        //    foreach (User doctor in this.AllDoctors())
        //    {
        //        inputValues[0] = doctor.Email;
        //        Appointment newAppointment = this.FindAppointmentInTheSelectedRange(inputValues);
        //        if (newAppointment == null) // ako ne moze nikako
        //        {
        //            appointmentsForChoosing = this.FindRandomAppointmentForScheduling(inputValues);
        //            Appointment selectedAppointment = this.PickAppointmentForScheduling(appointmentsForChoosing);
        //            // kreiraj?
        //            return;
        //        }
        //        appointmentsForChoosing.Add(newAppointment);

        //    }
        //}

        //public List<Appointment> FindRandomAppointmentForScheduling(string[] inputValues)
        //{
        //    List<Appointment> appointmentsForChoosing = new List<Appointment>();

        //    DateTime appointmentDate = DateTime.Now.AddDays(1);
        //    DateTime startTime = DateTime.ParseExact("06:00", "HH:mm", CultureInfo.InvariantCulture);

        //    int appointmentNumber = 0;
        //    string[] dataForAppointment;

        //    do
        //    {
        //        if (this.IsTimeBetweenTwoTimes(startTime))
        //        {
        //            startTime = DateTime.ParseExact("06:00", "HH:mm", CultureInfo.InvariantCulture);
        //            appointmentDate = appointmentDate.AddDays(1);
        //        }

        //        dataForAppointment = new string[] { inputValues[0], appointmentDate.ToString("MM/dd/yyyy"), startTime.ToString("HH:mm") };

        //        if (this.IsAppointmentFree("0", dataForAppointment))
        //        {
        //            appointmentsForChoosing.Add(this.CreateAppointment(dataForAppointment));
        //            appointmentNumber += 1;
        //        }
        //        startTime = startTime.AddMinutes(15);
        //    } while (appointmentNumber != 3);

        //    return appointmentsForChoosing;
        //}

        //public List<User> AllDoctors()
        //{
        //    List<User> allDoctors = new List<User>();
        //    foreach (User user in _allUsers)
        //        if (user.UserRole == User.Role.Doctor)
        //            allDoctors.Add(user);
        //    return allDoctors;
        //}

        //public Appointment CreateAppointment(string[] dataForAppointment)
        //{
        //    DateTime appointmentDate = DateTime.ParseExact(dataForAppointment[1], "MM/dd/yyyy", CultureInfo.InvariantCulture);
        //    DateTime startTime = DateTime.ParseExact(dataForAppointment[2], "HH:mm", CultureInfo.InvariantCulture);

        //    string id = this.AppointmentService.GetNewAppointmentId().ToString();
        //    Room freeRoom = this.FindFreeRoom(appointmentDate, startTime);
        //    int roomId = Int32.Parse(freeRoom.Id);

        //    Appointment newAppointment = new Appointment(id, this._currentRegisteredUser.Email, dataForAppointment[0],
        //        appointmentDate, startTime, startTime.AddMinutes(15), Appointment.State.Created, roomId,
        //        Appointment.Type.Examination, false);

        //    return newAppointment;
        //}

        //public void PrintAppointments(List<Appointment> appointments)
        //{
        //    int numAppointment = 1;
        //    Console.WriteLine("\nPREDLOZI TERMINA PREGLEDA");
        //    this.TableHeader();
        //    foreach (Appointment appointment in appointments)
        //        Console.WriteLine(numAppointment + ". " + appointment.DisplayOfPatientAppointment());
        //}

        //public Appointment PickAppointmentForScheduling(List<Appointment> appointments)
        //{
        //    this.PrintAppointments(appointments);
        //    int numAppointment;
        //    string choice;
        //    do
        //    {
        //        Console.WriteLine("Unesite broj pregleda koji zelite da zakazete");
        //        Console.Write(">> ");
        //        choice = Console.ReadLine();
        //    } while (!int.TryParse(choice, out numAppointment) || numAppointment < 1 || numAppointment > appointments.Count);

        //    return appointments[numAppointment - 1];
        //}
    }
}