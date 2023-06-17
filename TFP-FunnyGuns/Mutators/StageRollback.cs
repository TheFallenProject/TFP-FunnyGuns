using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFP_FunnyGuns.Systems;

namespace TFP_FunnyGuns.Mutators
{
    internal class StageRollback : IMutator
    {
        public string displayName { get; set; } = "<color=#ff00ea>Задержка стадии</color>";
        public string shortExplanation { get; set; } = "Зона не закрылась, радуйтесь";
        public int mutatorWeight { get; set; } = 420690;
        public bool instantDeathTermination { get; set; } = true;

        public void DisEngage()
        {
            
        }

        public void Engage()
        {
            TimedEvents.ZoneLockdownStatus newLD = (TimedEvents.ZoneLockdownStatus)((int)TimedEvents.LockdownStatus - 1);

            TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.None);
            TimedEvents.UpdateZoneLockdown(newLD);
        }

        public bool StartCheck()
        {
            return true;
        }
    }
}
