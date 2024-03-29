﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Hospital.Drugs.Model
{
    public class DrugNotification
    {
        private string _patientEmail;
        private DateTime _timeNotification;

        public string PatientEmail { get { return _patientEmail; } }
         public DateTime TimeNotification { get { return _timeNotification; } set { _timeNotification = value; } }

        public DrugNotification(string patientEmail, DateTime timeNotification)
        {
            this._patientEmail = patientEmail;
            this._timeNotification = timeNotification;
        }
    }
}
