using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TFP_FunnyGuns.Systems
{
    internal class MutatorSystem
    {
        private static List<IMutator> registeredMutators = new List<IMutator>();

        private static List<IMutator> activeMutators = new List<IMutator>();

        public static List<IMutator> activeMutatorsReplica { get
            {
                List<IMutator> list = new List<IMutator>();
                list.AddRange(activeMutators);
                return list;
            } 
        }

        private static IMutator mutatorToAnnounce;

        public static string mutatorDescriptionHUD { get
            {
                if (mutatorToAnnounce != null)
                    return mutatorToAnnounce.shortExplanation;
                return "";
            } 
        }

        public static void RegisterMutators()
        {
            registeredMutators.Clear();
            activeMutators.Clear();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(typ => typ.GetInterface(nameof(IMutator)) == typeof(IMutator)); //now we have mutators!

            foreach (var typ in types)
                registeredMutators.Add(Activator.CreateInstance(typ) as IMutator);
        }

        public static bool CheckIfThereIsAMutatorActive(Type mutType)
        {
            return activeMutators.Any(mut => mut.GetType().Name == mutType.Name);
        }

        public static void EngageRandomMutator()
        {
            List<IMutator> validMutators = new List<IMutator>();
            int weightsum = 0;

            foreach (var candidate in registeredMutators)
            {
                if (!candidate.StartCheck() || activeMutators.Any(mut => mut.GetType().Name == candidate.GetType().Name))
                {
                    continue;
                }

                weightsum += candidate.mutatorWeight;
                validMutators.Add(candidate);
            }

            if (validMutators.Count == 0)
            {
                Exiled.API.Features.Log.Warn("There are no valid mutators to engage. This is bad, skipping mutator assignment.");
                return;
            }

            int randomSel = UnityEngine.Random.Range(1, weightsum + 1);
            foreach (var candidate in validMutators)
            {
                randomSel -= candidate.mutatorWeight;
                if (randomSel <= 0)
                {
                    candidate.Engage();
                    activeMutators.Add(candidate);

                    mutatorToAnnounce = candidate;
                    Timing.CallDelayed(7.5f, () => mutatorToAnnounce = null);
                    return;
                }
            }
        }

        public static void DisengageAllMutators(bool terminateOnlyNonInstantDeath = false)
        {
            List<IMutator> markedForDeletion = new List<IMutator>();

            foreach (var mut in activeMutators)
            {
                if (!mut.instantDeathTermination && terminateOnlyNonInstantDeath)
                    continue;

                mut.DisEngage();
                markedForDeletion.Add(mut);
            }
            activeMutators.RemoveAll(active => markedForDeletion.Any(marked => marked.GetType().Name == active.GetType().Name));
        }
    }
}
