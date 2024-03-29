﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hospital;
using Hospital.Appointments.Repository;
using Hospital.Appointments.Model;

namespace Hospital.Appointments.Service
{
	public class PatientRequestService : IPatientRequestService
	{
		private IAppointmentService _appointmentService;
		private IPatientRequestRepository _requestRepository;
		private List<Appointment> _requests;

		public List<Appointment> Requests { get { return this._requests; } }

		public PatientRequestService()
		{
			_requestRepository = Globals.container.Resolve<IPatientRequestRepository>();
			_requests = _requestRepository.Load();
			this._appointmentService = Globals.container.Resolve<IAppointmentService>();
		}

		public Appointment FindInitialAppointment(string id)
		{
			Appointment initialAppointment = null;
			foreach (Appointment appointment in _appointmentService.Appointments)
			{
				if (id == appointment.AppointmentId)
				{
					initialAppointment = appointment;
					break;
				}
			}
			return initialAppointment;
		}

		public void DenyRequest(Appointment request)
		{
			_requests.Remove(request);
			UpdateFile();
			List<Appointment> allAppointments = _appointmentService.Appointments;
			foreach (Appointment appointment in allAppointments)
			{
				if (appointment.AppointmentId == request.AppointmentId)
				{
					appointment.AppointmentState = Appointment.State.Created;
					break;
				}

			}
			this._appointmentService.Save();

		}

		public void AcceptRequest(Appointment request)
		{
			_requests.Remove(request);
			UpdateFile();
			List<Appointment> allAppointments = _appointmentService.Appointments;
			foreach (Appointment appointment in allAppointments)
			{
				if (appointment.AppointmentId == request.AppointmentId)
				{
					if (request.AppointmentState == Appointment.State.DeleteRequest)
						appointment.AppointmentState = Appointment.State.Deleted;
					else
					{
						appointment.DoctorEmail = request.DoctorEmail;
						appointment.DateAppointment = request.DateAppointment;
						appointment.StartTime = request.StartTime;
						appointment.EndTime = request.EndTime;
						appointment.AppointmentState = Appointment.State.Updated;
					}

					break;
				}
			}

			this._appointmentService.Save();

		}

		public void ProcessRequest(Appointment request, int choice)
		{
			if (choice == 2)
			{
				DenyRequest(request);
			}
			else
			{
				AcceptRequest(request);
			}
		}

		public List<Appointment> FilterPending()
		{
			List<Appointment> pendingRequests = new List<Appointment>();
			foreach (Appointment request in _requests)
			{
				if (request.DateAppointment > DateTime.Now)
				{
					pendingRequests.Add(request);
				}
			}
			return pendingRequests;
		}

		public void UpdateFile()
		{
			this._requestRepository.Save(Requests);
		}
	}
}
