using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFP_FunnyGuns
{
    internal interface IMutator
    {
        /// <summary>
        /// Display name of the mutator
        /// </summary>
        string displayName { get; set; }

        /// <summary>
        /// In 4-5 words describe what it does
        /// </summary>
        string shortExplanation { get; set; }

        /// <summary>
        /// Increase or decrease mutator engage chance. Baseline is 10, lower it for annoying mutators.
        /// </summary>
        int mutatorWeight { get; set; }

        /// <summary>
        /// Should the mutator be terminated upon stage 5?
        /// </summary>
        bool instantDeathTermination { get; set; }

        /// <summary>
        /// This method is invoked when mutator is engaged.
        /// </summary>
        void Engage();

        /// <summary>
        /// This method is invoked when mutator is disengaged.
        /// </summary>
        void DisEngage();

        /// <summary>
        /// A method for the mutator to check if it is okay to start
        /// </summary>
        /// <returns>true if OK, false otherwise</returns>
        bool StartCheck();
    }
}
