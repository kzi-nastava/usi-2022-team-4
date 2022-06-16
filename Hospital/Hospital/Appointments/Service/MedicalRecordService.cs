﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Hospital.Appointments.Repository;
using Hospital.Appointments.Model;

namespace Hospital.Appointments.Service
{
    public class MedicalRecordService: IMedicalRecordService
    {
        private MedicalRecordRepository _medicalRecordRepository;
        private List<MedicalRecord> _medicalRecords;

        public MedicalRecordService()
        {
            this._medicalRecordRepository = new MedicalRecordRepository();
            this._medicalRecords = _medicalRecordRepository.Load();

        }

        public List<MedicalRecord> MedicalRecords { get { return _medicalRecords; } }

        public void Add(MedicalRecord medicalRecord)
        {
            this._medicalRecords.Add(medicalRecord);
            this._medicalRecordRepository.Save(this._medicalRecords);
        }

    }
}
