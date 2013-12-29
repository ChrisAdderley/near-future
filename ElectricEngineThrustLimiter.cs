using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private ModuleEngines engine;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            
        }
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            engine = part.GetComponent<ModuleEngines>();

            ThrustCurve = new FloatCurve();
            ThrustCurve.Add(0f, engine.maxThrust);
            ThrustCurve.Add(minPressure, minThrust);
        }
        public override void OnFixedUpdate()
        {
            engine.maxThrust = ThrustCurve.Evaluate((float)FlightGlobals.getStaticPressure(vessel.transform.position));
        }


    }
}
