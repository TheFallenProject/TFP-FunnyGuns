using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFP_FunnyGuns.Mutators
{
    internal class JankyDoors : IMutator
    {
        public string displayName { get; set; } = "<color=orange>Беды с дверьми</color>";
        public string shortExplanation { get; set; } = "Двери открываются и закрываются сами по себе";
        public int mutatorWeight { get; set; } = 42042;
        public bool instantDeathTermination { get; set; } = false;

        public void DisEngage()
        {
            Timing.KillCoroutines("fg_dr");
        }

        public void Engage()
        {
            Timing.RunCoroutine(doorCoroutine(), "fg_dr");
        }

        public bool StartCheck()
        {
            return true;
        }

        private static IEnumerator<float> doorCoroutine()
        {
            while (true)
            {
            again: //This is a bad idea, yes. But it also works.
                var dr = Exiled.API.Features.Door.List.ElementAt(UnityEngine.Random.Range(0, Exiled.API.Features.Door.List.Count()));
                if (dr.IsLocked)
                    goto again;

                dr.IsOpen = !dr.IsOpen;

                yield return Timing.WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.1f));
            }
        }
    }
}
