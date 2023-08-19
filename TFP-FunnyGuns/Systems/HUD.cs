using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace TFP_FunnyGuns.Systems
{
    internal class HUD
    {
        private static string getStageColor()
        {
            switch (Game.Stage)
            {
                case 1:
                    return "#ffffff";
                case 2:
                    return "#89ff57";
                case 3:
                    return "#e6ff57";
                case 4:
                    return "#ffcd57";
                case 5:
                    return "#ff5757";
                default:
                    return "#ee57ff";
            }
        }

        private static string getSecondsWord(int secs)
        {
            string secondsWord = "";
            if (secs == 1)
                secondsWord = "секунда";
            if (secs > 1 && secs < 5)
                secondsWord = "секунды";
            if (secs >= 5 && secs < 21)
                secondsWord = "секунд";
            else
            {
                int rem10 = secs % 10;
                if (rem10 == 1)
                    secondsWord = "секунда";
                else if (rem10 > 1 && rem10 < 5)
                    secondsWord = "секунды";
                else
                    secondsWord = "секунд";
            }

            return secondsWord;
        }

        private static string getColorFlickering()
        {
            int timeRemaining = (int)(Game.CountdownLimitSeconds - (double)Game.CountdownSW.Elapsed.TotalSeconds);
            if (timeRemaining % 2 == 0)
                return "red";
            else
                return "white";
        }

        private static string getAdditionalInformation(PlayerRoles.Team team)
        {
            string additionalInfo = "";
            if (Game.Stage < 5)
            {
                additionalInfo = $"\n<color={getColorFlickering()}>%message</color>";
                switch (TimedEvents.LockdownStatus)
                {
                    case TimedEvents.ZoneLockdownStatus.None:
                        additionalInfo = additionalInfo.Replace("%message", "Закрытие поверхности в начале следующий стадии.");
                        break;
                    case TimedEvents.ZoneLockdownStatus.Surface:
                        additionalInfo = additionalInfo.Replace("%message", "Закрытие лайтов в начале следующий стадии.");
                        break;
                    case TimedEvents.ZoneLockdownStatus.LCZ:
                        additionalInfo = additionalInfo.Replace("%message", "Закрытие хардов в начале следующий стадии.");
                        break;
                    case TimedEvents.ZoneLockdownStatus.HCZ:
                        additionalInfo = additionalInfo.Replace("%message", "Отключение лечения и постоянный урон в начале следующей стадии.");
                        break;
                    case TimedEvents.ZoneLockdownStatus.InstantDeath:
                        additionalInfo = additionalInfo.Replace("%message", "Время на исходе...");
                        break;
                }
            }
            if (MutatorSystem.activeMutatorsReplica.Count > 0)
            {
                var mutators = MutatorSystem.activeMutatorsReplica;
                additionalInfo += "\n\n";
                if (MutatorSystem.mutatorDescriptionHUD != "")
                {
                    additionalInfo += $"<b>Новый мутатор: {MutatorSystem.mutatorDescriptionHUD}</b>\n";
                }

                additionalInfo += $"Активны{(mutators.Count == 1 ? "й" : "е")} мутатор{(mutators.Count == 1 ? "" : "ы")}: ";

                int c = 1;
                foreach (var mut in mutators)
                {
                    additionalInfo += $"{mut.displayName}{(c == mutators.Count ? "." : ", ")}";
                    c++;
                }
            }
            if (InterconiumCore.HudStatus != InterconiumCore.EquilaserHudStatus.None)
            {
                additionalInfo += "\n\n<color=#b7f553>";
                switch (InterconiumCore.HudStatus)
                {
                    case InterconiumCore.EquilaserHudStatus.Charging:
                        additionalInfo += "Запрос подкрепления подготавливается. Готовьтесь защищать или захватывать интерком"; 
                        break;
                    case InterconiumCore.EquilaserHudStatus.ActiveMTF:
                        additionalInfo += $"{(team == Team.FoundationForces ? $"Скажите что-нибудь в интерком для сравнения количиства живых игроков. (+{InterconiumCore.RespawnCount})" : $"Защитите интерком от MTF, чтобы они не сравняли счёт живых игроков с вами (+{InterconiumCore.RespawnCount}).")}";
                        break;
                    case InterconiumCore.EquilaserHudStatus.ActiveCI:
                        additionalInfo += $"{(team == Team.ChaosInsurgency ? $"Скажите что-нибудь в интерком для сравнения количиства живых игроков. (+{InterconiumCore.RespawnCount})" : $"Защитите интерком от хаоса, чтобы они не сравняли счёт живых игроков с вами (+{InterconiumCore.RespawnCount}).")}";
                        break;
                    case InterconiumCore.EquilaserHudStatus.Secured:
                        additionalInfo += "Интерком был захвачен. Подкрепление на месте.";
                        break;
                    case InterconiumCore.EquilaserHudStatus.Lost:
                        additionalInfo += "Интерком был защищён. Подкрепление не прибыло.";
                        break;
                    case InterconiumCore.EquilaserHudStatus.Recalled:
                        additionalInfo += "Счёт игроков равен, нет нужды в подкреплении.";
                        break;
                }
                additionalInfo += "</color>";
            }
            return additionalInfo;
        }

        public static IEnumerator<float> UpdateCoroutine()
        {
            while (true)
            {
                int timeRemaining = (int)(Game.CountdownLimitSeconds - Game.CountdownSW.Elapsed.TotalSeconds);
                foreach (var pl in Exiled.API.Features.Player.List)
                {
                    pl.ShowHint($"\n\n\n\n\n\n\n\n\nСтадия <color={getStageColor()}>{Game.Stage}</color>\n{(timeRemaining != -1 ? $"До следующей стадии: {timeRemaining} {getSecondsWord(timeRemaining)}" : "<color=red><b>Внезапная смерть</b></color>")}{getAdditionalInformation(pl.Role.Team)}", 2);
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}
