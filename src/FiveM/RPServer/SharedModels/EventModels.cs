using FamilyRP.Roleplay.Enums.Character;
using FamilyRP.Roleplay.Enums.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.SharedModels
{
    /// <summary>
    /// This file to be used for generic small event models (I rather not have 20 files with them)
    /// </summary>

    class TriggerEventForPlayersModel
    {
        public int[] serverIds;
        public bool passFullSerializedModel;
        public int sourceServerId;
        public string eventName;
        public string Payload;

        public TriggerEventForPlayersModel(int serverId, string eventName, string Payload, bool passFullSerializedModel = false)
        {
            this.serverIds = new int[] { serverId };
            this.eventName = eventName;
            this.Payload = Payload;
            this.passFullSerializedModel = passFullSerializedModel;
        }

        public TriggerEventForPlayersModel(int[] serverIds, string eventName, string Payload, bool passFullSerializedModel = false)
        {
            this.serverIds = serverIds;
            this.eventName = eventName;
            this.Payload = Payload;
            this.passFullSerializedModel = passFullSerializedModel;
        }

        public TriggerEventForPlayersModel() { }
    }

    class TriggerEventForPrivilegeModel
    {
        public Privilege Privilege;
        public bool passFullSerializedModel;
        public int sourceServerId;
        public string eventName;
        public string Payload;

        public TriggerEventForPrivilegeModel(Privilege privilege, string eventName, string Payload, bool passFullSerializedModel = false)
        {
            this.Privilege = privilege;
            this.eventName = eventName;
            this.Payload = Payload;
            this.passFullSerializedModel = passFullSerializedModel;
        }

        public TriggerEventForPrivilegeModel() { }
    }

    class TriggerEventForDutyModel
    {
        public Duty Duty;
        public bool passFullSerializedModel;
        public int sourceServerId = -1;
        public string eventName;
        public string Payload;

        public TriggerEventForDutyModel(Duty duty, string eventName, string Payload, bool passFullSerializedModel = false)
        {
            this.Duty = duty;
            this.eventName = eventName;
            this.Payload = Payload;
            this.passFullSerializedModel = passFullSerializedModel;
        }

        public TriggerEventForDutyModel() { }
    }

    //class TriggerEventForJobModel
    //{
    //    public Job Job;
    //    public bool passFullSerializedModel;
    //    public int sourceServerId;
    //    public string eventName;
    //    public string Payload;

    //    public TriggerEventForJobModel(Job job, string eventName, string Payload, bool passFullSerializedModel = false)
    //    {
    //        this.Job = job;
    //        this.eventName = eventName;
    //        this.Payload = Payload;
    //        this.passFullSerializedModel = passFullSerializedModel;
    //    }

    //    public TriggerEventForJobModel() { }
    //}

    class TriggerEventForAllModel
    {
        public bool passFullSerializedModel;
        public int sourceServerId;
        public string eventName;
        public string Payload;

        public TriggerEventForAllModel(string eventName, string Payload, bool passFullSerializedModel = false)
        {
            this.eventName = eventName;
            this.Payload = Payload;
            this.passFullSerializedModel = passFullSerializedModel;
        }

        public TriggerEventForAllModel() { }
    }
}
