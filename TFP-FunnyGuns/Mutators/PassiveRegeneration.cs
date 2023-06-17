using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFP_FunnyGuns.Mutators
{
    internal class PassiveRegeneration : IMutator
    {
        public string displayName { get; set; } = "<color=green>Пассивная регенерация</color>";
        public string shortExplanation { get; set; } = "ХП медленно восполняется";
        public int mutatorWeight { get; set; } = 10;
        public bool instantDeathTermination { get; set; } = true;

        public void DisEngage()
        {
            Timing.KillCoroutines("fg_healing");
        }

        public void Engage()
        {
            Timing.RunCoroutine(HealingCoroutine(), "fg_healing");
        }

        public bool StartCheck()
        {
            return true;
        }

        private static IEnumerator<float> HealingCoroutine()
        {
            while (true)
            {
                foreach (var pl in Exiled.API.Features.Player.List)
                {
                    pl.Heal(1f);
                }

                yield return Timing.WaitForSeconds(0.75f);
            }
        }
    }
}
