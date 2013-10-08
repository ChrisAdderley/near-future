/// FissionGenerator
/// ---------------------------------------------------
/// FissionGeenrator part module

/// TODO: Figure out how to refresh UI widgets for deploy/retract radiators

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFuture
{
    public class FissionGenerator: PartModule
    {
        // Is generator online
        [KSPField(isPersistant = true)]
        public bool Enabled;

        // Power generation when closed and open
        [KSPField(isPersistant = false)]
        public float PowerGenerationDeployed;
        [KSPField(isPersistant = false)]
        public float PowerGenerationRetracted;
        [KSPField(isPersistant = false)]
        public float PowerGenerationResponseRate;

        // current generation
        public float currentGeneration;

        private IFissionGeneratorAnimator animator;
        
        // Reactor activation actions
        [KSPEvent(guiActive = true, guiName = "Enable Reactor", active = true)]
        public void Enable()
        {
    
            Enabled = true;
        }
        [KSPEvent(guiActive = true, guiName = "Disable Reactor", active = false)]
        public void Disable()
        {
            GeneratorStatus = "Reactor Offline";
            Enabled = false;
        }
        [KSPAction("Enable Reactor")]
        public void EnableAction(KSPActionParam param) { Enable(); }

        [KSPAction("Disable Reactor")]
        public void DisableAction(KSPActionParam param) { Disable(); }

        [KSPAction("Toggle Reactor")]
        public void ToggleAction(KSPActionParam param)
        {
            Enabled = !Enabled;
        }

        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Output")]
        public string GeneratorStatus;

        // Radiator Actions
        [KSPEvent(guiActive = true, guiName = "Deploy Radiators", active = true)]
        public void DeployRadiators()
        {
           animator.Deploy();
        }

        [KSPEvent(guiActive = true, guiName = "Retract Radiators", active = false)]
        public void RetractRadiators()
        {
           animator.Retract();
        }

        [KSPAction("Deploy Radiators")]
        public void DeployRadiatorsAction(KSPActionParam param)
        {
            DeployRadiators();
        }

        [KSPAction("Retract Radiators")]
        public void RetractRadiatorsAction(KSPActionParam param)
        {
            RetractRadiators();
        }

        [KSPAction("Toggle Radiators")]
        public void ToggleRadiatorsAction(KSPActionParam param)
        {
            if (animator.CurrentState == RadiatorState.Deployed || animator.CurrentState == RadiatorState.Deploying)
            {
                RetractRadiators();
            }
            else if (animator.CurrentState == RadiatorState.Retracted || animator.CurrentState == RadiatorState.Retracting)
            {
                DeployRadiators();
            }
        }

        // Radiator Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Radiator Status")]
        public string RadiatorStatus;


        // Info for ui
        public override string GetInfo()
        {
            return String.Format("Maximum Power: {0:F2}/s", currentGeneration);
        }

        // Implement the fissiongeneratoranimator class
        private class DefaultFissionGeneratorAnimator : IFissionGeneratorAnimator
        {
            public RadiatorState CurrentState { get; private set; }
            public void Deploy() { CurrentState = RadiatorState.Deployed; }
            public void Retract() { CurrentState = RadiatorState.Retracted; }

            public DefaultFissionGeneratorAnimator()
            {
                CurrentState = RadiatorState.Retracted;
            }
        }


        public override void OnStart(PartModule.StartState state)
        {
            this.part.force_activate();
            animator = part.Modules.OfType<IFissionGeneratorAnimator>().SingleOrDefault() ?? new FissionGeneratorAnimator();

            // Figure out what the current production should be
            if (Enabled)
            {
                if (animator.CurrentState != RadiatorState.Deployed)
                {
                    currentGeneration = PowerGenerationRetracted;
                }
                else
                {
                    currentGeneration = PowerGenerationDeployed;
                }
            }
            else
            {
                currentGeneration = 0f;
            }

        }

        public override void OnLoad(ConfigNode node)
        {}

        // Update function for animation, UI
        public override void OnUpdate()
        {
            Events["Enable"].active = !Enabled;
            Events["Disable"].active = Enabled;

            var retracted = (animator.CurrentState == RadiatorState.Retracted);
            var deployed = (animator.CurrentState == RadiatorState.Deployed);

            if (Events["DeployRadiators"].active != retracted || Events["RetractRadiators"].active != deployed)
            {
                Events["DeployRadiators"].active = retracted;
                Events["RetractRadiators"].active = deployed;
            }
            RadiatorStatus = animator.CurrentState.ToString();

            // Update GUI 
            GeneratorStatus = String.Format("Generation rate: {0:F2}/s", currentGeneration);

        }

        // Fixed update function. Actually does the gameplay stuff
        public override void OnFixedUpdate()
        {
            if (Enabled)
            {
                 // if radiators are not open, move towards closed generation at response rate
                if (animator.CurrentState != RadiatorState.Deployed)
                {
                    currentGeneration = Mathf.MoveTowards(currentGeneration, PowerGenerationRetracted, TimeWarp.fixedDeltaTime * PowerGenerationResponseRate);
                    this.part.RequestResource("ElectricCharge", -TimeWarp.fixedDeltaTime * currentGeneration);
                }
                else
                {
                    currentGeneration = Mathf.MoveTowards(currentGeneration, PowerGenerationDeployed, TimeWarp.fixedDeltaTime * PowerGenerationResponseRate);
                    this.part.RequestResource("ElectricCharge", -TimeWarp.fixedDeltaTime * currentGeneration);
                }
                
            }
            else {
                currentGeneration = Mathf.MoveTowards(currentGeneration, 0f, TimeWarp.fixedDeltaTime * PowerGenerationResponseRate);
                this.part.RequestResource("ElectricCharge", -TimeWarp.fixedDeltaTime * currentGeneration);
                
             }
        }
    }
}
