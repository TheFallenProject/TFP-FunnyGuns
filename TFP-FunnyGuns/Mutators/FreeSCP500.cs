using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFP_FunnyGuns.Mutators
{
    internal class FreeSCP500 : IMutator
    {
        public string displayName { get; set; } = "<color=green>Раздача хилок</color>";
        public string shortExplanation { get; set; } = "Если у вас было место в инвентаре, вы получили SCP-500!";
        public int mutatorWeight { get; set; } = 12;
        public bool instantDeathTermination { get; set; } = false;

        public void DisEngage()
        {
            // do nothing
        }

        public void Engage()
        {
            foreach (var pl in Exiled.API.Features.Player.List)
            {
                if (pl.IsAlive)
                    pl.AddItem(ItemType.SCP500);
            }
        }

        public bool StartCheck()
        {
            return true;
        }
    }
}
