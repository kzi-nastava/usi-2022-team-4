﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital;
using Autofac;
using Hospital.Users.Model;
using Hospital.Appointments.Service;
using Hospital.Users.View;

namespace Hospital.Users.Service
{
	public class PatientAccountService
	{
		private List<User> _patients;
		private IUserService _userService;
		private IHealthRecordService _healthRecordService;

		public PatientAccountService()
		{
			this._userService = Globals.container.Resolve<IUserService>();
			this._patients = FilterPatients(_userService.Users);
			this._healthRecordService = Globals.container.Resolve<IHealthRecordService>();
		}

		public List<User> Patients { get { return _patients; } }

		public List<User> FilterPatients(List<User> allUsers)
		{
			List<User> patients = new List<User>();
			foreach (User user in allUsers)
			{
				if (user.UserRole == User.Role.Patient)
				{
					patients.Add(user);
				}
			}
			return patients;
		}

		public List<User> FilterActivePatients()
		{
			List<User> activePatients = new List<User>();

			foreach (User user in this._patients)
			{
				if (user.UserState == User.State.Active)
				{
					activePatients.Add(user);
				}
			}
			return activePatients;
		}

		public List<User> FilterBlockedPatients()
		{
			List<User> blockedPatients = new List<User>();
			foreach (User user in this._patients)
			{
				if (user.UserState == User.State.BlockedBySecretary || user.UserState == User.State.BlockedBySystem)
				{
					blockedPatients.Add(user);
				}
			}
			return blockedPatients;
		}

		public void BlockPatient(User patient)
		{
			_userService.BlockOrUnblockUser(patient, true);
			this._patients = FilterPatients(_userService.Users);
		}

		public void UnblockPatient(User patient)
		{
			_userService.BlockOrUnblockUser(patient, false);
			this._patients = FilterPatients(_userService.Users);

		}

		public void CreatePatientAccount(User newPatient)
		{
			this._userService.Add(newPatient);
			this._patients.Add(newPatient);


			this._healthRecordService.CreateHealthRecord(newPatient);
		}
	}
}
