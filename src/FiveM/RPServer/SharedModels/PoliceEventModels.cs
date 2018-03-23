using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.SharedModels.PoliceEventModels
{
    class TackleEvent
    {
        public int SourceId { get; set; } // We don't need more information than this
        public List<int> Targets { get; set; } // Will be verified on other clients
        public bool IsSourcePoliceDog { get; set; } // Could be verified on server too

        public TackleEvent() { }

        public TackleEvent(List<int> Targets, bool IsSourcePoliceDog)
        {
            this.Targets = Targets;
            this.IsSourcePoliceDog = IsSourcePoliceDog;
        }
    }
}
