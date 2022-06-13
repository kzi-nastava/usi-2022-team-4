﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hospital.Users.Service;
using Hospital.Users.Model;
using Hospital.Rooms.Service;
using Hospital.Appointments.Service;

namespace Hospital.Users.View
{
    public class Login
    {
        UserService _userService = new UserService();
        User _registeredUser;

        public void LogIn()
        {
            Console.WriteLine("\nPrijava na sistem");
            Console.WriteLine("------------------");

            while (true)
            {
                Console.Write("Unesite email: ");
                string email = Console.ReadLine();

                if (!_userService.IsEmailValid(email))
                {
                    Console.WriteLine("Email nije validan!");
                }
                else if (_userService.IsUserBlocked(email))
                {
                    Console.WriteLine("Korisnik je blokiran. Prijava nije moguca!");
                }
                else
                {
                    Console.Write("Unesite lozinku: ");
                    string password = Console.ReadLine();

                    this._registeredUser = _userService.TryLogin(email, password);
                    if (this._registeredUser == null)
                    {
                        Console.WriteLine("Pogresna lozinka!");
                    }
                    else
                    {
                        break;
                    }
                }
                Console.Write("------------------\n");
            }

            Console.WriteLine("Uspesno ste se ulogovali!");
            Console.WriteLine($"\nDobrodosli {this._registeredUser.Name + " " + this._registeredUser.Surname}");

            // Move equipment if scheduled time has passed
            RoomService roomService = new RoomService();
            EquipmentService equipmentService = new EquipmentService(roomService);
            EquipmentMovingService equipmentMovingService = new EquipmentMovingService(equipmentService, roomService);
            equipmentMovingService.MoveEquipment();

            // Merge or split rooms if complex renovations are finished
            AppointmentService appointmentService = new AppointmentService();
            RenovationService renovationService = new RenovationService(roomService, appointmentService, equipmentService);
            renovationService.Renovate();

            if (this._registeredUser.UserRole == User.Role.Patient)
            { 
                // patient
                Patient registeredPatient = new Patient(this._registeredUser.Email);
                registeredPatient.PatientMenu();
            }
            else if (this._registeredUser.UserRole == User.Role.Doctor)
            {
                // doctor
                Doctor registeredDoctor = new Doctor(this._registeredUser);
                registeredDoctor.DoctorMenu();
            }
            else if (this._registeredUser.UserRole == User.Role.Secretary)
			{
                // secretary
                Secretary registeredSecretary = new Secretary(this._userService);
                registeredSecretary.SecretaryMenu();
			}
            else if (this._registeredUser.UserRole == User.Role.Manager)
            {
                // manager
                Manager registeredManager = new Manager(this._registeredUser);
                registeredManager.ManagerMenu();
            }
        }
    }
}