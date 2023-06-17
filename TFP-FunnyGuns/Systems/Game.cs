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
            Timing.RunCoroutine(endgameCheckerCoroutine(), "tfp_fg_ec");
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



                    if (advanceStage())
                    {
                        Exiled.API.Features.Cassie.Message(".g4", false, false, false);
                        CountdownSW.Start();
                    }
                    else
                    {
                        Exiled.API.Features.Cassie.Message(".g4 .g4", false, false, false);
                    }

                    Exiled.API.Features.Cassie.Clear();
                    
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        private static bool advanceStage()
        {
            switch (TimedEvents.LockdownStatus)
            {
                case TimedEvents.ZoneLockdownStatus.None:
                    TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.Surface);
                    MutatorSystem.EngageRandomMutator();
                    CountdownLimitSeconds = 150d;
                    break;
                case TimedEvents.ZoneLockdownStatus.Surface:
                    TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.LCZ);
                    MutatorSystem.EngageRandomMutator();
                    CountdownLimitSeconds = 120d;
                    break;
                case TimedEvents.ZoneLockdownStatus.LCZ:
                    TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.HCZ);
                    MutatorSystem.EngageRandomMutator();
                    CountdownLimitSeconds = 90d;
                    break;
                case TimedEvents.ZoneLockdownStatus.HCZ:
                    TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.InstantDeath);
                    MutatorSystem.DisengageAllMutators(true);
                    CountdownLimitSeconds = -1d; //basically disabling anything
                    Stage++;
                    return false;
            }
            Stage++;
            return true;
        }

        private static IEnumerator<float> endgameCheckerCoroutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);

                if (Exiled.API.Features.Player.List.Count() == 1)
                {
                    continue;
                }

                int CICount, NTFCount;
                NTFCount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.FoundationForces);
                CICount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.ChaosInsurgency);

                if (CICount == 0 || NTFCount == 0)
                {
                    if (CICount == 0)
                        Exiled.API.Features.Map.Broadcast(10, $"Победа <color=blue>MTF</color>.");
                    else if (NTFCount == 0)
                        Exiled.API.Features.Map.Broadcast(10, $"Победа <color=green>Хаоса</color>.");
                    Plugin.instance.DisEngage();
                }
            }
        }
    }
}
