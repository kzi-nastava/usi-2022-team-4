﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Drugs.Model;

namespace Hospital.Drugs.Service
{
    public interface  IDrugService: IService<Drug>
    {
        void AddDrug(Drug drug);
        int GetNewDrugId();
    }
}
