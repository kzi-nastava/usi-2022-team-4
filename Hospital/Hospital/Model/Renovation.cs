﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Model
{
    public class Renovation
    {
        public enum Type 
        {
            SimpleRenovation = 1,
            SplitRenovation = 2,
            MergeRenovation = 3
        }

        private string _id;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _roomId;
        private bool _active;
        private Type _type;

        public string Id { get { return _id; } }
        public DateTime StartDate { get { return _startDate; } }
        public DateTime EndDate { get { return _endDate; } }
        public string RoomId { get { return _roomId; } }
        public bool IsActive { get { return _active; } set { _active = value; } }
        public Type RenovationType { get { return _type; } }

        public Renovation(string id, DateTime startDate, DateTime endDate, string roomId, bool active, Type type)
        {
            this._id = id;
            this._startDate = startDate;
            this._endDate = endDate;
            this._roomId = roomId;
            this._active = active;
            this._type = type;
        }
    }
}
