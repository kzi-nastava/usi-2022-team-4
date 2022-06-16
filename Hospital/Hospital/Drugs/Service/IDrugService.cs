﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Drugs.Model;

namespace Hospital.Drugs.Service
{
    public interface  IDrugService
    {
        List<Drug> Drugs { get; }
        int GetNewDrugId();
        void AddDrug(Drug drug);
    }
}
