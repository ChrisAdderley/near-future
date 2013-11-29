/// FissionGeneratorRadiatorAnimator
/// ---------------------------------------------------
/// Handles animation and fx for radiators

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFuture
{
    class FissionRadiator: PartModule
    {

        // animation for deploying and retracting of radiators
        [KSPField(isPersistant = false)]
        public string DeployAnimation;

        // animation for radiator heat
        [KSPField(isPersistant = false)]
        public string HeatAnimation;

        // Current radiator state
        [KSPField(isPersistant = true)]
        public string SavedState;

        // Temperature lost by radiators
        [KSPField(isPersistant = false)]
        public float HeatRadiated;

        // Temperature lost by radiators when closed
        [KSPField(isPersistant = false)]
        public float HeatRadiatedClosed;

        // Amount of power dissipated w/ pressure in ideal conditions
        [KSPField(isPersistant = false)]
        public FloatCurve PressureCurve;

        // animations for retracting
        private AnimationState[] deployStates;
        // and for heating
        private AnimationState[] heatStates;

        private float requestedHeatRejection = 0f;

        // STATUS STRINGS
        ///--------------------
        // Fuel Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Current Heat Rejection")]
        public string HeatRejectionGUI = "0K";


        

        // ACTIONS
        // -----------------
        // Deploy radiator
        [KSPEvent(guiActive = true, guiName = "Deploy Radiator", active = true)]
        public void DeployRadiator()
        {
            Deploy();
 
        }
        // Retract radiator
        [KSPEvent(guiActive = true, guiName = "Retract Radiator", active = false)]
        public void RetractRadiator()
        {
            Retract();

        }
        // Toggle radiator
        [KSPEvent(guiActive = true, guiName = "Toggle Radiator", active = true)]
        public void ToggleRadiator()
        {

            Toggle();
        }



        [KSPAction("Deploy Radiators")]
        public void DeployRadiatorsAction(KSPActionParam param)
        {
            DeployRadiator();
        }

        [KSPAction("Retract Radiators")]
        public void RetractRadiatorsAction(KSPActionParam param)
        {
            RetractRadiator();
        }

        [KSPAction("Toggle Radiators")]
        public void ToggleRadiatorsAction(KSPActionParam param)
        {
            ToggleRadiator();
        }

        // Info for ui
        public override string GetInfo()
        {
            return String.Format("Heat Rejection (Retracted): {0:F1} kW", HeatRadiatedClosed) + "\n" +
                String.Format("Heat Rejection (Deployed): {0:F1} kW", HeatRadiated);
        }

        // Deploy Radiators
        public void Deploy()
        {
            foreach (AnimationState deployState in deployStates)
            {
                deployState.speed = 1;
            }
            State = RadiatorState.Deploying;
        }

        // Retract Radiators
        public void Retract()
        {
            foreach (AnimationState deployState in deployStates)
            {
                deployState.speed = -1;
            }
            State = RadiatorState.Retracting;
        }
        // Toggle Radiators
        public void Toggle()
        {
            if (State == RadiatorState.Deployed)
                Retract();
            else if (State == RadiatorState.Retracted)
                Deploy();
            else
                return;
        }


        private float availableHeatRejection = 0f;

        public float HeatRejection(float request)
        {
            requestedHeatRejection = request;
            return availableHeatRejection; 
        }

        // Get the state
        public RadiatorState State
        {
            get
            {
                try
                {
                    return (RadiatorState)Enum.Parse(typeof(RadiatorState), SavedState);
                }
                catch
                {
                    State = RadiatorState.Retracted;
                    return State;
                }
            }
            set
            {
                SavedState = value.ToString();
            }
        }


        public override void OnStart(PartModule.StartState state)
        {
            deployStates = Utils.SetUpAnimation(DeployAnimation, part);
            heatStates = Utils.SetUpAnimation(HeatAnimation, part);
            

            PressureCurve = new FloatCurve();
            PressureCurve.Add(0f, 0f);
            PressureCurve.Add(1f, 1f);

            if (State == RadiatorState.Deployed || State == RadiatorState.Deploying)
            {
                foreach (AnimationState deployState in deployStates)
                {
                    deployState.normalizedTime = 1f;
                }
            }
            else if (State == RadiatorState.Retracted || State == RadiatorState.Retracting)
            {
                foreach (AnimationState deployState in deployStates)
                {
                    deployState.normalizedTime = 0f;
                }
            }
            else
            {
                // broken! none for you!
            }

            this.part.force_activate();

        }

        
        public override void OnUpdate()
        {
            foreach (AnimationState deployState in deployStates)
            {
                deployState.normalizedTime = Mathf.Clamp01(deployState.normalizedTime);
            }
            if (State == RadiatorState.Retracting)
            {
               if (EvalAnimationCompletionReversed(deployStates) == 0f)
                   State = RadiatorState.Retracted;
            }
            
            if (State == RadiatorState.Deploying)
            {
                if (EvalAnimationCompletion(deployStates) == 1f)
                    State = RadiatorState.Deployed;
            }
            
            if ((State == RadiatorState.Deployed && Events["DeployRadiator"].active)  || (State == RadiatorState.Retracted && Events["RetractRadiator"].active))
            {
                Events["DeployRadiator"].active = !Events["DeployRadiator"].active;
                Events["RetractRadiator"].active = !Events["RetractRadiator"].active;
            }

        }


        public override void  OnFixedUpdate()
        {
            
            // convect
            if (Utils.VesselInAtmosphere(this.vessel))
            {
                double pressure = FlightGlobals.getStaticPressure(vessel.transform.position);
                availableHeatRejection = PressureCurve.Evaluate((float)pressure);

            }
            else
            {
                availableHeatRejection = 0f;
            }
            if (State != RadiatorState.Deployed && State != RadiatorState.Broken)
            {
                availableHeatRejection += HeatRadiatedClosed;
            }
            else if (State == RadiatorState.Broken)
            {
                availableHeatRejection = 0f;
            }
            else
            {
                availableHeatRejection += HeatRadiated;
            }
            foreach (AnimationState state in heatStates)
            {
                state.normalizedTime = Mathf.MoveTowards(state.normalizedTime,Mathf.Clamp01(requestedHeatRejection/availableHeatRejection),0.1f*TimeWarp.fixedDeltaTime);
            }
            HeatRejectionGUI = String.Format("{0:F1} kW", availableHeatRejection);
            
        }

        

        private float EvalAnimationCompletion(AnimationState[] states)
        {
            float checker = 0f;
            foreach (AnimationState state in states)
            {
                checker = Mathf.Max(state.normalizedTime, checker);
            }
            return checker;
        }
        private float EvalAnimationCompletionReversed(AnimationState[] states)
        {
            float checker = 1f;
            foreach (AnimationState state in states)
            {
                checker = Mathf.Min(state.normalizedTime, checker);
            }
            return checker;
        }

    }
}
