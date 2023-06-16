using Exiled.API.Enums;
using MEC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TFP_FunnyGuns.Systems
{
    internal class Game
    {
        public static Stopwatch CountdownSW = new Stopwatch();
        public static double CountdownLimitSeconds = 60d;

        public static ushort Stage = 1;

        public static void StartEvent()
        {
            Stage = 1;
            CountdownSW.Reset();
            Exiled.API.Features.Lift.List.ToList().Where(elev => elev.Type == ElevatorType.GateA || elev.Type == ElevatorType.GateB).ToList().ForEach(elev =>
            {
                if (elev.IsLocked)
                    elev.ChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.None);
            });
            Exiled.API.Features.Door.List.First(dr => dr.Type == DoorType.SurfaceGate).Unlock();
            CountdownLimitSeconds = 90d;
            CountdownSW.Start();

            Timing.RunCoroutine(countdownCoroutine(), "tfp_fg_cd");
        }

        public static void StopEvent()
        {
            Stage = 1;
            CountdownSW.Reset();
            Exiled.API.Features.Lift.List.ToList().Where(elev => elev.Type == ElevatorType.GateA || elev.Type == ElevatorType.GateB || elev.Type == ElevatorType.LczA || elev.Type == ElevatorType.LczB).ToList().ForEach(elev =>
            {
                if (elev.IsLocked)
                    elev.ChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.None);
            });
            Exiled.API.Features.Door.List.First(dr => dr.Type == DoorType.SurfaceGate).Unlock();
        }

        private static IEnumerator<float> countdownCoroutine()
        {
            while (true)
            {
                if (CountdownLimitSeconds <= CountdownSW.Elapsed.TotalSeconds && CountdownLimitSeconds != -1)
                {
                    CountdownSW.Stop();
                    CountdownSW.Reset();

                    //TODO: Call a random mutator here

                    if (advanceStage())
                        CountdownSW.Start();
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        private static bool advanceStage()
        {
            switch (Stage)
            {
                case 1:
                    TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.Surface);
                    CountdownLimitSeconds = 150d;
                    break;
                case 2:
                    TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.LCZ);
                    CountdownLimitSeconds = 120d;
                    break;
                case 3:
                    TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.HCZ);
                    CountdownLimitSeconds = 90d;
                    break;
                case 4:
                    TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.InstantDeath);
                    CountdownLimitSeconds = -1d; //basically disabling anything
                    return false;
            }
            Stage++;
            return true;
        }
    }
}
