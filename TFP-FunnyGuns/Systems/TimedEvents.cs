using Exiled.API.Enums;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFP_FunnyGuns.Systems
{
    internal class TimedEvents
    {
        internal enum ZoneLockdownStatus
        {
            /// <summary>
            /// No zones are currently locked down
            /// </summary>
            None = 0,

            /// <summary>
            /// Surface has been locked down
            /// </summary>
            Surface = 1,

            /// <summary>
            /// LCZ and surface are now locked down
            /// </summary>
            LCZ = 2,

            /// <summary>
            /// HCZ, LCZ and surface are now locked down
            /// </summary>
            HCZ = 3,

            /// <summary>
            /// HCZ, LCZ and surface are now locked down. Players are now constantly damaged and can no longer heal using healing items.
            /// </summary>
            InstantDeath = 4
        }

        internal static ZoneLockdownStatus LockdownStatus = ZoneLockdownStatus.None;

        public static void UpdateZoneLockdown(ZoneLockdownStatus _lds)
        {
            if (_lds == LockdownStatus)
                return;

            if ((int)_lds < (int)LockdownStatus && _lds != ZoneLockdownStatus.None)
            {
                throw new Exception("You cannot decrease lockdown severity, only reset it to None"); //because it will be messy real soon real fast
            }
            if ((int)_lds != (int)LockdownStatus + 1 && _lds != ZoneLockdownStatus.None)
            {
                UpdateZoneLockdown((ZoneLockdownStatus)((int)_lds - 1)); //Skipping ahead is allowed, but recursion is still required to not make this messy
            }

            ElevatorType[] suitableLifts;
            IEnumerable<Exiled.API.Features.Lift> elevs;
            IEnumerable<Exiled.API.Features.Door> doors;

            switch (_lds)
            {
                case ZoneLockdownStatus.None:
                    suitableLifts = new ElevatorType[] { ElevatorType.GateA, ElevatorType.GateB, ElevatorType.LczA, ElevatorType.LczB };
                    elevs = Exiled.API.Features.Lift.List.Where(el => suitableLifts.Contains(el.Type));
                    foreach (var elev in elevs)
                    {
                        if (elev.IsLocked)
                            elev.ChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.None);
                    }

                    doors = Exiled.API.Features.Door.List.Where(door => door.Type == DoorType.HeavyContainmentDoor);
                    foreach (var dr in doors)
                    {
                        dr.Unlock();
                    }
                    try { Exiled.Events.Handlers.Player.UsingItem -= Player_UsingItem; } catch { }
                    LockdownStatus = _lds;
                    break;
                case ZoneLockdownStatus.Surface:
                    suitableLifts = new ElevatorType[] { ElevatorType.GateA, ElevatorType.GateB };
                    elevs = Exiled.API.Features.Lift.List.Where(el => suitableLifts.Contains(el.Type));
                    foreach (var elev in elevs)
                    {
                        if (!elev.IsLocked)
                            elev.ChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.SpecialDoorFeature);
                    }
                    LockdownStatus = _lds;
                    break;
                case ZoneLockdownStatus.LCZ:
                    suitableLifts = new ElevatorType[] { ElevatorType.LczA, ElevatorType.LczB };
                    elevs = Exiled.API.Features.Lift.List.Where(el => suitableLifts.Contains(el.Type));
                    foreach (var elev in elevs)
                    {
                        if (!elev.IsLocked)
                            elev.ChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.SpecialDoorFeature);
                    }
                    LockdownStatus = _lds;
                    break;
                case ZoneLockdownStatus.HCZ:
                    doors = Exiled.API.Features.Door.List.Where(door => door.Type == DoorType.HeavyContainmentDoor);
                    foreach (var dr in doors)
                    {
                        dr.Lock(420690f, DoorLockType.SpecialDoorFeature);
                        dr.IsOpen = false;
                    }
                    LockdownStatus = _lds;
                    break;
                case ZoneLockdownStatus.InstantDeath:
                    Exiled.Events.Handlers.Player.UsingItem += Player_UsingItem;
                    LockdownStatus = _lds;
                    break;
            }
        }

        private static void Player_UsingItem(Exiled.Events.EventArgs.Player.UsingItemEventArgs ev)
        {
            if (ev.Item.Type == ItemType.Medkit || ev.Item.Type == ItemType.Adrenaline || ev.Item.Type == ItemType.SCP500 || ev.Item.Type == ItemType.Painkillers)
            {
                ev.IsAllowed = false;
                ev.Player.ClearBroadcasts();
                ev.Player.Broadcast(3, "<color=red>Что-то не то...</color>");
            }
        }

        public static IEnumerator<float> LockdownCoroutine()
        {
            Action<Exiled.API.Features.Player> hurtPlayer = new Action<Exiled.API.Features.Player>((pl) =>
            {
                pl.EnableEffect(Exiled.API.Enums.EffectType.Blinded, 5);
                pl.Hurt(15f, "<color=red>Зона была отсечена.</color>");
            });

            while (true)
            {
                int lockdownInteger = (int)LockdownStatus;
                foreach (var pl in Exiled.API.Features.Player.List)
                {
                    if (lockdownInteger >= 1)
                    {
                        if (pl.Zone == Exiled.API.Enums.ZoneType.Surface)
                        {
                            hurtPlayer.Invoke(pl);
                        }
                    }
                    if (lockdownInteger >= 2)
                    {
                        if (pl.Zone == Exiled.API.Enums.ZoneType.LightContainment)
                        {
                            hurtPlayer.Invoke(pl);
                        }
                    }
                    if (lockdownInteger >= 3)
                    {
                        if (pl.Zone == Exiled.API.Enums.ZoneType.HeavyContainment)
                        {
                            hurtPlayer.Invoke(pl);
                        }
                    }
                    if (lockdownInteger >= 4)
                    {
                        pl.Hurt(2.5f, $"<color=red>Внезапная смерть от инсульта{(UnityEngine.Random.Range(0, 10) == 0 ? " жопы =)" : "")}</color>"); //haha funni easter egg. Also, one whole minute of survival on full HP.
                    }
                }
                yield return Timing.WaitForSeconds(1.5f);
            }
        }
    }
}
