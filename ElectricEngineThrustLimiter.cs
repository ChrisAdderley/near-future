/// ElectricEngineThrustLimiter
/// ---------------------------
/// Module that limits the thrust of an engine while in an atmosphere instead of changing ISP
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFuture
{
    class ElectricEngineThrustLimiter:PartModule
    {

        // Thrust
        [KSPField(isPersistant = false)]
        public float minThrust = 0f;
        [KSPField(isPersistant = false)]
        public float minPressure = 1f;


        private FloatCurve ThrustCurve;
        private ModuleEnginesFX engine;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            
        }
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            engine = part.GetComponent<ModuleEnginesFX>();

            ThrustCurve = new FloatCurve();
            ThrustCurve.Add(0f, engine.maxThrust);
            ThrustCurve.Add(minPressure, minThrust);
        }
        public void FixedUpdate()
        {
            //engine.finalThrust

            if (engine != null)
            {
                Transform engineVector = engine.thrustTransforms[0];
                Rigidbody partRB = part.Rigidbody;
                Debug.Log(engine.finalThrust);
                partRB.AddForceAtPosition(engineVector.forward*engine.finalThrust*ThrustCurve.Evaluate((float)FlightGlobals.getStaticPressure(vessel.transform.position)),engineVector.position);
                
            }
             //   engine.maxThrust =  ThrustCurve.Evaluate((float)FlightGlobals.getStaticPressure(vessel.transform.position));
        }


    }
}
