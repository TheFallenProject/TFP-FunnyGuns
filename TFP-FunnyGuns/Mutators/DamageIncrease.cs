using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFP_FunnyGuns.Mutators
{
    internal class DamageIncrease : IMutator
    {
        public string displayName { get; set; } = "Урон увеличен";
        public string shortExplanation { get; set; } = "Урон от оружия был увеличен в 1.8 раз";
        public int mutatorWeight { get; set; } = 5;
        public bool instantDeathTermination { get; set; } = false;

        public void DisEngage()
        {
            Exiled.Events.Handlers.Player.Hurting -= Player_Hurting;
        }

        public void Engage()
        {
            Exiled.Events.Handlers.Player.Hurting += Player_Hurting;
        }

        private void Player_Hurting(Exiled.Events.EventArgs.Player.HurtingEventArgs ev)
        {
            if (ev.IsAllowed && ev.DamageHandler.Type == Exiled.API.Enums.DamageType.Firearm)
            {
                ev.Amount *= 1.8f;
            }
        }

        public bool StartCheck()
        {
            return true;
        }
    }
}
