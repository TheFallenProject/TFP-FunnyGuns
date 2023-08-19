using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;
using PlayerRoles;

namespace TFP_FunnyGuns.Systems
{
    internal class InterconiumCore // haha robocraft referenc
    {
        public static bool EquilaserActive = false;

        private static List<int> immunePlayers = new List<int>();

        public static EquilaserHudStatus HudStatus;

        public static int SecondsLeftToSecure = -1;

        public static int RespawnCount { get
            {
                int ciCount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.ChaosInsurgency);
                int ntfCount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.FoundationForces);
                int respawnCountMax;

                if (HudStatus == EquilaserHudStatus.ActiveCI)
                {
                    respawnCountMax = ntfCount - ciCount;
                    if (respawnCountMax < 0)
                    {
                        respawnCountMax = 0;
                    }
                }
                else if (HudStatus == EquilaserHudStatus.ActiveMTF)
                {
                    respawnCountMax = ciCount - ntfCount;
                    if (respawnCountMax < 0)
                    {
                        respawnCountMax = 0;
                    }
                }
                else return 0;

                var spectatorList = Exiled.API.Features.Player.List.Where(pl => pl.Role.Type == PlayerRoles.RoleTypeId.Spectator);
                return Math.Min(respawnCountMax, spectatorList.Count());
            } }

        public enum EquilaserHudStatus
        {
            /// <summary>
            /// Equialiser has not been started yet
            /// </summary>
            None,

            /// <summary>
            /// Equilaser is being prepaired for launch, grace period for winning team to react and time to buckle up for the losing team
            /// </summary>
            Charging,

            /// <summary>
            /// Equialiser is active, MTF needs to secure the intercom
            /// </summary>
            ActiveMTF,

            /// <summary>
            /// Equiliser is active, CI needs to secure the intercom
            /// </summary>
            ActiveCI,

            /// <summary>
            /// Equilaser is now secured by the losing team
            /// </summary>
            Secured,

            /// <summary>
            /// Equilaser was defended by dominating team
            /// </summary>
            Lost,

            /// <summary>
            /// No need to equalise
            /// </summary>
            Recalled
        }

        public static void Equalise()
        {
            Timing.RunCoroutine(ArmCoroutine());
        }

        private static IEnumerator<float> ArmCoroutine()
        {
            HudStatus = EquilaserHudStatus.Charging;

            yield return Timing.WaitForSeconds(UnityEngine.Random.Range(12f, 30f)); //The intercom will take from 12 to 30 seconds to charge up the equilaser

            int ciCount, ntfCount;
            ciCount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.ChaosInsurgency);
            ntfCount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.FoundationForces);

            if (ciCount == ntfCount)
            {
                HudStatus = EquilaserHudStatus.Recalled;

                Timing.CallDelayed(10f, () =>
                {
                    HudStatus = EquilaserHudStatus.None;
                });

                yield break;
            }
            else if (ciCount > ntfCount)
                HudStatus = EquilaserHudStatus.ActiveMTF;
            else
                HudStatus = EquilaserHudStatus.ActiveCI;

            EquilaserActive = true;

            Exiled.Events.Handlers.Player.IntercomSpeaking += Player_IntercomSpeaking;
        }

        public static void Finish(bool secured = false)
        {
            if (EquilaserActive)
            {
                if (secured)
                {
                    RespawnLosingTeam();
                    HudStatus = EquilaserHudStatus.Secured;
                }
                else
                {
                    HudStatus = EquilaserHudStatus.Lost;
                }
                EquilaserActive = false;
                try
                {
                    Exiled.Events.Handlers.Player.IntercomSpeaking -= Player_IntercomSpeaking;
                }
                catch { }
                Timing.CallDelayed(10f, () => { HudStatus = EquilaserHudStatus.None; });
            }
        }

        private static void RespawnLosingTeam()
        {
            Exiled.API.Features.Door.List.First(dr => dr.Type == Exiled.API.Enums.DoorType.Intercom).IsOpen = true;
            if (HudStatus == EquilaserHudStatus.ActiveMTF)
            {
                int ciCount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.ChaosInsurgency);
                int ntfCount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.FoundationForces);

                int respawnCountMax = ciCount - ntfCount;
                if (respawnCountMax < 0)
                {
                    respawnCountMax = 0;
                }

                respawnCountMax++;

                var spectatorList = Exiled.API.Features.Player.List.Where(pl => pl.Role.Type == PlayerRoles.RoleTypeId.Spectator);

                for (int i = 0; i <= Math.Min(respawnCountMax, spectatorList.Count()); i++)
                {
                    var pl = spectatorList.ElementAt(UnityEngine.Random.Range(0, spectatorList.Count() - 1));

                    pl.Role.Set(PlayerRoles.RoleTypeId.NtfSergeant);
                    pl.IsGodModeEnabled = true;
                    pl.ClearInventory();
                    pl.AddItem(new ItemType[] { ItemType.KeycardNTFCommander, ItemType.ArmorHeavy, ItemType.Medkit, ItemType.Medkit, ItemType.Radio, ItemType.GunAK, ItemType.GunE11SR });
                    Timing.CallDelayed(2f, () => {
                        pl.DisableEffect(Exiled.API.Enums.EffectType.Blinded);
                        pl.Teleport(Exiled.API.Features.Door.List.First(dr => dr.Type == Exiled.API.Enums.DoorType.Intercom).Position);
                        Timing.CallDelayed(5f, () => pl.IsGodModeEnabled = false);
                    });

                    spectatorList = Exiled.API.Features.Player.List.Where(ply => ply.Role.Type == PlayerRoles.RoleTypeId.Spectator);
                }
            }
            else if (HudStatus == EquilaserHudStatus.ActiveCI)
            {
                int ciCount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.ChaosInsurgency);
                int ntfCount = Exiled.API.Features.Player.List.Count(pl => pl.Role.Team == PlayerRoles.Team.FoundationForces);

                int respawnCountMax = ntfCount - ciCount;
                if (respawnCountMax < 0)
                {
                    respawnCountMax = 0;
                }

                respawnCountMax++;

                var spectatorList = Exiled.API.Features.Player.List.Where(pl => pl.Role.Type == PlayerRoles.RoleTypeId.Spectator);

                for (int i = 0; i <= Math.Min(respawnCountMax, spectatorList.Count()); i++)
                {
                    var pl = spectatorList.ElementAt(UnityEngine.Random.Range(0, spectatorList.Count() - 1));

                    pl.Role.Set(PlayerRoles.RoleTypeId.ChaosRifleman);
                    pl.IsGodModeEnabled = true;
                    pl.ClearInventory();
                    pl.AddItem(new ItemType[] { ItemType.KeycardNTFCommander, ItemType.ArmorHeavy, ItemType.Medkit, ItemType.Medkit, ItemType.Medkit, ItemType.GunAK, ItemType.GunE11SR });
                    Timing.CallDelayed(2f, () => {
                        pl.DisableEffect(Exiled.API.Enums.EffectType.Blinded);
                        pl.Teleport(Exiled.API.Features.Door.List.First(dr => dr.Type == Exiled.API.Enums.DoorType.Intercom).Position);
                        Timing.CallDelayed(5f, () => pl.IsGodModeEnabled = false);
                    });

                    spectatorList = Exiled.API.Features.Player.List.Where(ply => ply.Role.Type == PlayerRoles.RoleTypeId.Spectator);
                }
            }
        }

        private static void Player_IntercomSpeaking(Exiled.Events.EventArgs.Player.IntercomSpeakingEventArgs ev)
        {
            if (EquilaserActive)
            {
                bool OK = false;
                if (ev.Player.Role.Team == Team.FoundationForces && HudStatus == EquilaserHudStatus.ActiveMTF)
                    OK = true;
                if (ev.Player.Role.Team == Team.ChaosInsurgency && HudStatus == EquilaserHudStatus.ActiveCI)
                    OK = true;

                if (OK)
                    Finish(true);
            }
        }
    }
}
