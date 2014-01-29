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
        public float EnergyCost = 50f;

        // Refining rate per second
        [KSPField(isPersistant = false)]
        public float ReprocessRate = 0.0001f;

        // How much waste we recycle
        [KSPField(isPersistant = false)]
        public float RecycleEfficiency = 0.5f;

        // Animation
        [KSPField(isPersistant = false)]
        public string RefineAnimation;

        // Start Reprocessing Fuel
        [KSPEvent(guiActive = true, guiName = "Start Reprocessing", active = false)]
        public void StartReprocessing()
        {
            foreach (AnimationState workState in workStates)
            {
                workState.normalizedSpeed = 1.0f;
            }
        }
        // Stop Reprocessing Fuel
        [KSPEvent(guiActive = true, guiName = "Stop Reprocessing", active = false)]
        public void StopReprocessing()
        {
            Status = "Shutdown";
            foreach (AnimationState workState in workStates)
            {
                workState.normalizedSpeed = 0.0f;
            }
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


        // STATUS STRINGS
        ///--------------------
        // Fuel Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reprocessor Status")]
        public string Status;
        // Info for ui
        public override string GetInfo()
        {
            return String.Format("Power Required: {0:F1} Ec/s", EnergyCost) + "\n" +
                String.Format("Depleted Fuel Processing Rate: {0:F4} U/s", ReprocessRate) + "\n" +
                String.Format("Efficiency: {0:F0} U/s", RecycleEfficiency*100f);
        }

        private FissionContainer workContainer;
        private AnimationState[] workStates;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            this.moduleName = "Nuclear Fuel Reprocessor";
        }

        public override void OnStart(PartModule.StartState state)
        {
            workStates = Utils.SetUpAnimation(RefineAnimation, part);
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
            if (Enabled)
            {
                if (workContainer == null)
                {
                    // try to get a container, turn us off if we can't find any
                    workContainer = FindValidFissionContainer();
                    if (workContainer == null)
                    {

                        StopReprocessing();
                        Status = "No waste found";
                        return;
                    }
                }

                

                // consume power
                double power = this.part.RequestResource("ElectricCharge", EnergyCost * TimeWarp.fixedDeltaTime);

                if (power <= 0d)
                {
                    Status = "Not enough Electric Charge!";
                }
                else
                {
                    Status = String.Format("Processing at: {0:F0} U/s",ReprocessRate);
                    double wasteRefined = workContainer.part.RequestResource("DepletedUranium", ReprocessRate * TimeWarp.fixedDeltaTime);
                    if (wasteRefined >= 0d)
                    {
                        workContainer.part.RequestResource("EnrichedUranium", wasteRefined * RecycleEfficiency);

                    }
                }
            }
        }


        // Finds a container to refuel
        FissionContainer FindValidFissionContainer()
        {
            List<FissionContainer> candidates = FindFissionContainers();

            foreach (FissionContainer cont in candidates)
            {
                // check for fuel space
                if (cont.CheckFuelSpace(this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition("DepletedUranium").id).amount))
                {
                    Debug.Log("NFPP: Found valid container.");
                    return cont;
                }
            }
            ScreenMessages.PostScreenMessage(new ScreenMessage("No fuel containers with any Depleted Uranium Found!", 4f, ScreenMessageStyle.UPPER_CENTER));
            return null;
        }

        // Finds a list of all fission containers
        List<FissionContainer> FindFissionContainers()
        {
            List<FissionContainer> fissionContainers = new List<FissionContainer>();
            List<Part> allParts = this.vessel.parts;
            foreach (Part pt in allParts)
            {

                PartModuleList pml = pt.Modules;
                for (int i = 0; i < pml.Count; i++)
                {
                    PartModule curModule = pml.GetModule(i);
                    FissionContainer candidate = curModule.GetComponent<FissionContainer>();

                    if (candidate != null)
                        fissionContainers.Add(candidate);
                }

            }
            if (fissionContainers.Count == 0)
                ScreenMessages.PostScreenMessage(new ScreenMessage("No nuclear fuel containers attached to this ship.", 4f, ScreenMessageStyle.UPPER_CENTER));
            return fissionContainers;
        }

    }
}
