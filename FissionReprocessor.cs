// Fission Reprocessor
/// ---------------------------------------------------
/// A part that slowly recycles fission fuel in a fission Container

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFuture
{
    class FissionReprocessor: PartModule
    {

        // Is processor online?
        [KSPField(isPersistant = true)]
        public bool Enabled;

        // Energy cost per second
        [KSPField(isPersistant = false)]
        public float EnergyCost = 10f;

        // Refining rate per second
        [KSPField(isPersistant = false)]
        public float ReprocessRate = 0.0001f;

        // Animation
        [KSPField(isPersistant = false)]
        public string RefineAnimation;

        // Start Reprocessing Fuel
        [KSPEvent(guiActive = true, guiName = "Start Reprocessing", active = false)]
        public void StartReprocessing()
        {
        }
        // Stop Reprocessing Fuel
        [KSPEvent(guiActive = true, guiName = "Stop Reprocessing", active = false)]
        public void StopReprocessing()
        {
        }
        // Toggle Reprocessing
        [KSPEvent(guiActive = true, guiName = "Toggle Reprocessing", active = true)]
        public void ToggleReprocessing()
        {
        }

        // Actions
        [KSPAction("Start Reprocessing")]
        public void StartReprocessingAction(KSPActionParam param)
        {
            StartReprocessing();
        }

        [KSPAction("Stop Reprocessing")]
        public void StopReprocessingAction(KSPActionParam param)
        {
            StopReprocessing();
        }

        [KSPAction("Toggle Reprocessing")]
        public void ToggleReprocessingAction(KSPActionParam param)
        {
            ToggleReprocessing();
        }


        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            this.moduleName = "Nuclear Fuel Reprocessor";
        }

        public override void OnUpdate()
        {
            // Update events
            if ((Enabled && Events["StartReprocessing"].active) || (!Enabled && Events["StopReprocessing"].active))
            {
                Events["StartReprocessing"].active = !Enabled;
                Events["StopReprocessing"].active = Enabled;
            }
        }

        public override void OnFixedUpdate()
        {
            
        }

    }
}
