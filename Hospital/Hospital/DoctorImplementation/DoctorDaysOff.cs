﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Model;
using Hospital.Repository;
using Hospital.Service;

namespace Hospital.DoctorImplementation
{
    class DoctorDaysOff
    {
        RequestForDaysOffService requestForDaysOffService;
        List<RequestForDaysOff> requestsForDaysOff;
        User currentRegisteredDoctor;


        public DoctorDaysOff(User doctor)
        {
            requestForDaysOffService = new RequestForDaysOffService();
            requestsForDaysOff = requestForDaysOffService.RequestsForDaysOff;
            currentRegisteredDoctor = doctor;

        }

        public void MenuForDaysOff()
        {
            string choice;
            int tryIntConvert;
            do
            {
                do
                {
                    Console.WriteLine("Izaberite zeljanu radnju: \n1) Pregled zahteva za slobodne dane\n2) Podnesite zahtev za slobodne dane\n>>");
                    choice = Console.ReadLine();
                } while (!int.TryParse(choice, out tryIntConvert));
            } while (!choice.Equals("1") && !choice.Equals("2"));

            if (choice.Equals("1")){
                this.PrintRequestsForDaysOff();
            }
            else{
                this.SubmitRequestForDaysOff();
            }

        }

        private void SubmitRequestForDaysOff()
        {
            string desiredDate, numberOfDays;
            DateTime startDate, endDate;
            bool urgent;
            do
            {
                desiredDate = this.EnterDate();
                urgent = this.UrgencyCheckRequired();
                do
                {
                    numberOfDays = this.EnterNumberOfDays();
                } while (!this.CheckNumberOFDaysForUrgency(int.Parse(numberOfDays), urgent));
                startDate = DateTime.ParseExact(desiredDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                endDate = startDate.AddDays(int.Parse(numberOfDays));
            } while (!requestForDaysOffService.CheckingAvailabilityOfDoctor(startDate, endDate, currentRegisteredDoctor));

            RequestForDaysOff.State state = this.GetState(urgent);
            RequestForDaysOff newRequest = new RequestForDaysOff(requestForDaysOffService.GetNewRequestId(), currentRegisteredDoctor.Email, startDate, endDate, EnteringReasonsForDaysOff(), state, urgent);
            requestForDaysOffService.AddRequest(newRequest);
        }

        private string EnterNumberOfDays()
        {
            string numberOfDays;
            int tryIntConvert;
            do
            {
                Console.WriteLine("Unesite broj dana: ");
                numberOfDays = Console.ReadLine();
            } while (!int.TryParse(numberOfDays, out tryIntConvert));
            return numberOfDays;

        }
        private string EnterDate()
        {
            string desiredDate;
            do
            {
                Console.WriteLine("Unesite datum: ");
                desiredDate = Console.ReadLine();
            } while (!requestForDaysOffService.IsDateValid(desiredDate));
            return desiredDate;
        }

        private bool CheckNumberOFDaysForUrgency(int numberOfDays, bool urgent)
        {
            if (urgent)
            {
                if (numberOfDays > 5)
                {
                    Console.WriteLine("Za hitne zahteve ne moze vise od 5 dana.");
                    return false;
                }
            }
            return true;
        }

        private RequestForDaysOff.State GetState(bool urgent)
        {
            if (urgent)
            {
                return RequestForDaysOff.State.Accepted;
            }
            return RequestForDaysOff.State.Waiting;

        }

       
        private string EnteringReasonsForDaysOff()
        {
            Console.WriteLine("Unesite razlog za slobodne dane: ");
            return Console.ReadLine();
        }

        private bool UrgencyCheckRequired()
        {
            int tryIntConvert;
            string choice;
            do
            {
                do
                {
                    Console.WriteLine("Da li je zahtev hitan: \n1) DA\n2) NE\n>>");
                    choice = Console.ReadLine();
                } while (!int.TryParse(choice, out tryIntConvert));
            } while (!choice.Equals("1") && !choice.Equals("2"));
            if (choice.Equals("1"))
            {
                return true;
            }
            return false;

        }
        private void PrintRequestsForDaysOff()
        {
            int serialNumber = 1;
            Console.WriteLine(String.Format("|{0,5}|{1,15}|{2,15}|{3,10}|{4,10}|","Br","Pocetak","Kraj", "Stanje","Hitno"));
            foreach(RequestForDaysOff request in requestsForDaysOff)
            {
                if (request.EmailDoctor.Equals(currentRegisteredDoctor.Email))
                {
                    Console.WriteLine(String.Format("|{0,5}|{1,15}|{2,15}|{3,10}|{4,10}|", serialNumber, request.StartDate.ToString("MM/dd/yyyy"), request.EndDate.ToString("MM/dd/yyyy"), request.StateRequired, request.Urgen));
                    serialNumber++;
                }
            }
        }
    }
}
