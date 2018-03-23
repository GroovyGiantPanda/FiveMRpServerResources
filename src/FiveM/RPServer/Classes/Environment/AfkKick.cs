using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Server.Classes.Environment
{
    class AfkKick : BaseScript
    {
        public AfkKick()
        {
            EventHandlers["AfkKick.RequestDrop"] += new Func<CitizenFX.Core.Player, Task>(DropRequest);
        }

        public Task DropRequest([FromSource] CitizenFX.Core.Player source)
        {
            source.Drop("Afk");
            return Task.FromResult(0);
        }
    }
}
