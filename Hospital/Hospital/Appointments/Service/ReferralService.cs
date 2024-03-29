﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital;
using Autofac;
using Hospital.Users.Service;
using Hospital.Appointments.Repository;
using Hospital.Appointments.Model;
using Hospital.Users.Model;
using Hospital.Users.View;

namespace Hospital.Appointments.Service
{
	public class ReferralService : IReferralService
	{
		private IReferralRepository _referralRepository;
		private List<Referral> _referrals;
		private IAppointmentService _appointmentService;
		private IUserService _userService;

		public List<Referral> Referrals { get { return this._referrals; } }

		public ReferralService()
		{
			this._referralRepository = Globals.container.Resolve<IReferralRepository>();
			this._userService = Globals.container.Resolve<IUserService>();
			_referrals = this._referralRepository.Load();
		}

		public int GetNewReferralId()
		{
			return this._referrals.Count + 1;
		}

		public List<Referral> FilterUnused()
		{
			List<Referral> unusedReferrals = new List<Referral>();

			foreach (Referral referral in _referrals)
			{
				if (!referral.Used)
				{
					unusedReferrals.Add(referral);
				}
			}
			return unusedReferrals;
		}

		public void UseReferral(Referral usedReferral)
		{
			foreach (Referral referral in _referrals)
			{
				if (referral.Id.Equals(usedReferral.Id))
				{
					referral.Used = true;
					_referralRepository.Save(this._referrals);
					break;
				}
			}
		}

		public void Add(Referral referral)
		{
			this._referrals.Add(referral);
			this._referralRepository.Save(this._referrals);
		}

	}
}
