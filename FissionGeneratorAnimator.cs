/// FissionGeneratorAnimator
/// ---------------------------------------------------
/// Module defining an animation module for a fission generator

/// TODO: Add heat animation for deployed, retracted states

using System;
using System.Linq;
using UnityEngine;

namespace NearFuture
{
    public class FissionGeneratorAnimator : PartModule, IFissionGeneratorAnimator
    {
        //
        [KSPField(isPersistant = false)]
        public string DeployAnimation;

        [KSPField(isPersistant = true)]
        public string State;

        private AnimationState[] deployStates;

        public override void OnStart(PartModule.StartState state)
        {
            deployStates =  Utils.SetUpAnimation(DeployAnimation, this.part);


            if (CurrentState == RadiatorState.Deploying) 
            { 
                CurrentState = RadiatorState.Retracted; 
            }
            else if (CurrentState == RadiatorState.Retracting) 
            { 
                CurrentState = RadiatorState.Deployed; 
            }

            if (CurrentState == RadiatorState.Deployed)
            {
                foreach (AnimationState deployState in deployStates)
                {
                    deployState.normalizedTime = 1;
                }
            }


        }

        public RadiatorState CurrentState
        {
            get
            {
                try
                {
                    return (RadiatorState)Enum.Parse(typeof(RadiatorState), State);
                }
                catch
                {
                    CurrentState = RadiatorState.Retracted;
                    return CurrentState;
                }
            }
            private set
            {
                
                State = Enum.GetName(typeof(RadiatorState), value);
            }
        }

        public void Deploy()
        {
            if (CurrentState != RadiatorState.Retracted)
            { 
                return; 
            }
            CurrentState = RadiatorState.Deploying;

            foreach (var state in deployStates)
            {
                state.speed = 1;
            }
        }

        public void Retract()
        {
            if (CurrentState != RadiatorState.Deployed) 
            { 
                return; 
            }
            CurrentState = RadiatorState.Retracting;

            foreach (var state in deployStates)
            {
                state.speed = -1;
            }
        }

        public override void OnUpdate()
        {
            foreach (var deployState in deployStates)
            {
                deployState.normalizedTime = Mathf.Clamp01(deployState.normalizedTime);
            }

            if (CurrentState == RadiatorState.Deploying && deployStates[0].normalizedTime >= 1)
            {
                CurrentState = RadiatorState.Deployed;

            }
            else if (CurrentState == RadiatorState.Retracting && deployStates[0].normalizedTime >= 0)
            {
                CurrentState = RadiatorState.Retracted;
            }
        }
    }
}
