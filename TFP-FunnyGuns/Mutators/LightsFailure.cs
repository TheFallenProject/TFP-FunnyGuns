using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFP_FunnyGuns.Mutators
{
    internal class LightsFailure : IMutator
    {
        public string displayName { get; set; } = "<color=yellow>Поломки света</color>";
        public string shortExplanation { get; set; } = "Свет может пропадать";
        public int mutatorWeight { get; set; } = 10;
        public bool instantDeathTermination { get; set; } = false;

        public void DisEngage()
        {
            Timing.KillCoroutines("fg_lo");
        }

        public void Engage()
        {
            Timing.RunCoroutine(LightsOutCoroutine(), "fg_lo");
        }

        public bool StartCheck()
        {
            return true;
        }

        private static IEnumerator<float> LightsOutCoroutine()
        {
            int highFloor = 60;
            yield return Timing.WaitForSeconds(15f); //a little bit of guranteed downtime

            while (true)
            {
                if (UnityEngine.Random.Range(0, highFloor) < 10)
                {
                    float time = UnityEngine.Random.Range(7f, 24f);

                    Exiled.API.Features.Map.TurnOffAllLights(time);
                    highFloor = 60;
                    yield return Timing.WaitForSeconds(time + 4f);
                }
                else
                {
                    highFloor -= UnityEngine.Random.Range(5, 12);
                    if (highFloor < 1)
                        highFloor = 1;

                    yield return Timing.WaitForSeconds(4f);
                }
            }
        }
    }
}
