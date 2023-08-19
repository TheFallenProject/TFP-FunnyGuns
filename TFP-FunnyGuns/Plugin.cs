using Exiled.API.Enums;
using InventorySystem;
using LightContainmentZoneDecontamination;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFP_AutoEvent;
using TFP_FunnyGuns.Systems;

namespace TFP_FunnyGuns
{
    /// <summary>
    /// This is still raw, because mutators have not been implemented yet. I will do it, i swear, but not now.
    /// </summary>
    public class Plugin : IEvent //moving to internal events to test the class assignment bug being fixed (spoiler alert: it didn't work =()
    {
        public string CommandName { get; set; } = "funnyguns";
        public string DisplayName { get; set; } = "<color=#fcf98b>Funny Guns</color>";
        public bool LowPlayerEvent { get; set; } = false;
        public int EventWeighting { get; set; } = 10;
        public string ShortRulesDescription { get; set; } = "Уничтожьте <color=red>вражескую команду</color>, пока не уничтожили <color=green>вас</color>! <color=#fcf98b>Зоны закрываются, мутаторы выбираются</color>!";
        public string ExpandedRules { get; set; } = "Вы - моговец или хаосит.\nВаша задача - <color=yellow>уничтожить вражескую команду</color>. Вы можете, <color=yellow>и должны</color>, <color=yellowспускаться в комплекс</color>, если не хотите умереть сразу на поверхности.\n\n" +
            "Во время игры будут продвигаться стадии с 1 до 5, где 5 - внезапная смерть. С каждым увеличением стадии будет запускаться новый мутатор." +
            " Они могут поменять как работают некоторые механики. Они работают со 2 по 4 стадию включительно, но выключаются на 5.\n\nА, и да, пока не забыл. " +
            "<color=red>Не тимьтесь во время ивента</color>, иначе админ может и должен постучать вам по голове.";
        public int PreLaunchTimeSeconds { get; set; } = 30;

        public event EventDefaults.EndedDelegate Ended;

        // used to balance CI spawn in case of unfair NTF spawn
        private static bool CISpawnedUnfairly;

        public static Plugin instance;

        public void DisEngage()
        {
            foreach (var door in Exiled.API.Features.Door.List)
            {
                if (door.Zone == ZoneType.HeavyContainment)
                {
                    if (((int)door.IgnoredDamageTypes & (int)Interactables.Interobjects.DoorUtils.DoorDamageType.Grenade) != 0)
                        door.IgnoredDamageTypes ^= Interactables.Interobjects.DoorUtils.DoorDamageType.Grenade;
                }
            }
            Exiled.API.Features.Log.Info("disengaged");
            MutatorSystem.DisengageAllMutators();
            TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.None);
            Timing.KillCoroutines("tfp_fg_hud");
            Timing.KillCoroutines("tfp_fg_ld");
            Timing.KillCoroutines("tfp_fg_ec");
            Game.CountdownSW.Stop();
            Game.CountdownSW.Reset();
            InterconiumCore.Finish(false);
            instance = null;
            Exiled.Events.Handlers.Player.Dying -= Player_Dying;
            Exiled.Events.Handlers.Server.RespawningTeam -= Server_RespawningTeam;
            try
            {
                Ended.Invoke();
            }
            catch (Exception ex) { Exiled.API.Features.Log.Warn($"Failed to invoke Ended event. This is bad, go check this pretty nifty error: {ex.Message}"); }
        }

        public void Engage()
        {
            Exiled.Events.Handlers.Player.Dying += Player_Dying;
            Exiled.Events.Handlers.Server.RespawningTeam += Server_RespawningTeam;
            TimedEvents.UpdateZoneLockdown(TimedEvents.ZoneLockdownStatus.None);
            foreach (var door in Exiled.API.Features.Door.List)
            {
                if (door.Zone == ZoneType.HeavyContainment)
                {
                    door.IgnoredDamageTypes |= Interactables.Interobjects.DoorUtils.DoorDamageType.Grenade; // OH MY GOD BITWISE LOGIC NO WAAAY
                }
            }
            DecontaminationController.Singleton.NetworkDecontaminationOverride = DecontaminationController.DecontaminationStatus.Disabled; //trying to disable it
            Game.StartEvent();
            Timing.RunCoroutine(TimedEvents.LockdownCoroutine(), "tfp_fg_ld");
            Timing.RunCoroutine(HUD.UpdateCoroutine(), "tfp_fg_hud");
        }

        private void Server_RespawningTeam(Exiled.Events.EventArgs.Server.RespawningTeamEventArgs ev)
        {
            ev.IsAllowed = false; //no normal respawns
        }

        private void Player_Dying(Exiled.Events.EventArgs.Player.DyingEventArgs ev)
        {
            ev.Player.RemoveItem(itm => !itm.IsConsumable);
        }

        public bool LaunchCheck(out string reason)
        {
            if (Exiled.API.Features.Round.ElapsedTime.TotalMinutes > 5)
            {
                reason = "Round is longer than 5 minutes. Please restart the round";
                return false;
            }
            if (Exiled.API.Features.Door.List.Where(dr => dr.Zone == ZoneType.HeavyContainment && dr.Type != DoorType.Scp939Cryo).Any(dr => dr.IsBroken))
            {
                reason = "At least one door, that is not 939_CRYO was broken in HCZ. This might be the reason why HCZ lockdown will not work. Please restart the round";
                return false;
            }
            reason = "NA";
            return true;
        }

        public void PreLaunch()
        {
            instance = this;
            MutatorSystem.RegisterMutators();
            int NTF, CI;
            
            if (Exiled.API.Features.Player.List.Count() % 2 == 0)
            {
                NTF = Exiled.API.Features.Player.List.Count() / 2;
                CI = NTF;
            }
            else
            {
                NTF = (Exiled.API.Features.Player.List.Count() / 2) + 1;
                CI = NTF - 1;
            }

            foreach (var pl in Exiled.API.Features.Player.List)
            {
                int sel = UnityEngine.Random.Range(0, 2);

                if (sel == 0 && NTF > 0)
                {
                    pl.Role.Set(PlayerRoles.RoleTypeId.NtfSergeant);
                    pl.ClearInventory();
                    pl.AddItem(new ItemType[] { ItemType.KeycardNTFCommander, ItemType.ArmorHeavy, ItemType.Medkit, ItemType.Medkit, ItemType.Radio, ItemType.GunAK, ItemType.GunE11SR });
                    NTF--;
                }
                else if (sel == 0 && NTF <= 0)
                {
                    pl.Role.Set(PlayerRoles.RoleTypeId.ChaosRifleman);
                    pl.ClearInventory();
                    pl.AddItem(new ItemType[] { ItemType.KeycardChaosInsurgency, ItemType.ArmorHeavy, ItemType.Medkit, ItemType.Medkit, ItemType.Medkit, ItemType.GunAK, ItemType.GunE11SR });
                    CI--;
                }

                if (sel == 1 && CI > 0)
                {
                    pl.Role.Set(PlayerRoles.RoleTypeId.ChaosRifleman);
                    pl.ClearInventory();
                    pl.AddItem(new ItemType[] { ItemType.KeycardChaosInsurgency, ItemType.ArmorHeavy, ItemType.Medkit, ItemType.Medkit, ItemType.Medkit, ItemType.GunAK, ItemType.GunE11SR });
                    CI--;
                }
                else if (sel == 1 && CI <= 0)
                {
                    pl.Role.Set(PlayerRoles.RoleTypeId.NtfSergeant);
                    pl.ClearInventory();
                    pl.AddItem(new ItemType[] { ItemType.KeycardNTFCommander, ItemType.ArmorHeavy, ItemType.Medkit, ItemType.Medkit, ItemType.Radio, ItemType.GunAK, ItemType.GunE11SR });
                    NTF--;
                }
            }

            Exiled.API.Features.Lift.List.ToList().Where(elev => elev.Type == ElevatorType.GateA || elev.Type == ElevatorType.GateB).ToList().ForEach(elev =>
            {
                if (!elev.IsLocked)
                    elev.ChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.SpecialDoorFeature);
            });
            Exiled.API.Features.Door.List.First(dr => dr.Type == DoorType.SurfaceGate).Lock(420690f, DoorLockType.SpecialDoorFeature);
        }
    }
}
