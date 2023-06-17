using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFP_FunnyGuns.Mutators
{
    internal class GunFailure : IMutator
    {
        public string displayName { get; set; } = "<color=red>Холостые патроны</color>";
        public string shortExplanation { get; set; } = "Некоторые попадания могут не пройти";
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
            if (ev.DamageHandler.Type == Exiled.API.Enums.DamageType.Firearm)
            {
                if (UnityEngine.Random.Range(0, 10) < 3)
                {
                    ev.IsAllowed = false;
                }
            }
        }

        public bool StartCheck()
        {
            return true;
        }
    }
}
