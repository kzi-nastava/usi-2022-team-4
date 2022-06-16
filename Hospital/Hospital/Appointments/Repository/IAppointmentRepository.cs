﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Appointments.Model;

namespace Hospital.Appointments.Repository
{
    public interface IAppointmentRepository: IRepository<Appointment>
    {
    }
}
