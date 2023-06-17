using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFP_FunnyGuns.Mutators
{
    internal class ReloadSpeedIncrease : IMutator
    {
        public string displayName { get; set; } = "<color=green>Быстрая перезарядка</color>";
        public string shortExplanation { get; set; } = "Скорость перезарядки увеличена";
        public int mutatorWeight { get; set; } = 10;
        public bool instantDeathTermination { get; set; } = false;

        public void DisEngage()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= Player_ChangingRole;
            foreach (var pl in Exiled.API.Features.Player.List)
            {
                pl.DisableEffect(Exiled.API.Enums.EffectType.Scp1853);
            }
        }

        public void Engage()
        {
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
            foreach (var pl in Exiled.API.Features.Player.List)
            {
                pl.EnableEffect(Exiled.API.Enums.EffectType.Scp1853);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.Player.ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(5f, () =>
            {
                ev.Player.EnableEffect(Exiled.API.Enums.EffectType.Scp1853);
            });
        }

        public bool StartCheck()
        {
            return true;
        }
    }
}
