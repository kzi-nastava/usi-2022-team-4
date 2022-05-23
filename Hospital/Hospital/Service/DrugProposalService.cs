﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Repository;
using Hospital.Model;
using System.IO;

namespace Hospital.Service
{
    public class DrugProposalService
    {
        private DrugProposalRepository _drugProposalRepository;
        private List<DrugProposal> _drugProposals;


        public DrugProposalService()
        {
            this._drugProposalRepository = new DrugProposalRepository();
            this._drugProposals = _drugProposalRepository.Load();
        }

        public List<DrugProposal> DrugProposals { get { return _drugProposals; }set { _drugProposals = value; } }

        public void UpdateDrugProposalFile()
        {
            string filePath = @"..\..\Data\drugProposals.csv";

            List<string> lines = new List<String>();

            string line;
            foreach (DrugProposal drugProposal in this._drugProposals)
            {
                line = drugProposal.ToString();
                lines.Add(line);
            }
            File.WriteAllLines(filePath, lines.ToArray());
        }

        public List<DrugProposal> GetDrugProposalsByStatus(DrugProposal.Status status)
        {
            List<DrugProposal> proposals = new List<DrugProposal>();
            foreach (DrugProposal drugProposal in this._drugProposals)
            {
                if (drugProposal.ProposalStatus == status)
                {
                    proposals.Add(drugProposal);
                }
            }
            return proposals;
        }

        public List<DrugProposal> WaitingStatusDrugProposals()
        {
            return GetDrugProposalsByStatus(DrugProposal.Status.Waiting);
        }

        public List<DrugProposal> GetRejectedDrugProposals() 
        {
            return GetDrugProposalsByStatus(DrugProposal.Status.Rejected);
        }

        public void UpdateDrugProposal(DrugProposal drugProposalForChange)
        {
            foreach(DrugProposal drugProposal in this._drugProposals)
            {
                if (drugProposal.Id.Equals(drugProposalForChange.Id))
                {
                    drugProposal.ProposalStatus = drugProposalForChange.ProposalStatus;
                    drugProposal.Comment = drugProposalForChange.Comment;
                }
            }
        }

        public DrugProposal Get(string id)
        {
            foreach (DrugProposal drugProposal in _drugProposals)
            {
                if (drugProposal.Id.Equals(id))
                    return drugProposal;
            }
            return null;
        }
        public bool IdExists(string id)
        {
            return Get(id) != null;
        }

        public bool CreateDrugProposal(string id, string drugName, List<Ingredient> ingredients)
        {
            if (IdExists(id))
                return false;
            DrugProposal drugProposal = new DrugProposal(id, drugName, ingredients, DrugProposal.Status.Waiting, "");
            _drugProposals.Add(drugProposal);
            UpdateDrugProposalFile();
            return true;
        }

        public bool ReviewDrugProposal(string id, string drugName, List<Ingredient> ingredients)
        {
            if (!IdExists(id) || Get(id).ProposalStatus != DrugProposal.Status.Rejected)
                return false;
            foreach (DrugProposal proposal in _drugProposals) 
            {
                if (proposal.Id.Equals(id))
                {
                    proposal.DrugName = drugName;
                    proposal.Ingredients = ingredients;
                    proposal.ProposalStatus = DrugProposal.Status.Waiting;
                }
            }
            UpdateDrugProposalFile();
            return true;
        }
    }
}
