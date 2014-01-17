/// VariableIspEngine
/// ---------------------------------------------------
/// A module that allows the Isp and thrust of an engine to be varied via a GUI
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace NearFuture
{
    class VariableISPEngine:PartModule
    {

        // Use the direct throttle method
        [KSPField(isPersistant = false)]
        public bool UseDirectThrottle = false;

        // Link all engines
        [KSPField(isPersistant = true)]
        public bool LinkAllEngines = false;
       
        // Maximum thrust
        [KSPField(isPersistant = false)]
        public float MaxThrust;

        // Isp at maximum thrust
        [KSPField(isPersistant = false)]
        public float MaxThrustIsp;

        // Minimum thrust
        [KSPField(isPersistant = false)]
        public float MinThrust;

        // Isp at minimum thrust
        [KSPField(isPersistant = false)]
        public float MinThrustIsp;

        // Name of the fuel
        [KSPField(isPersistant = false)]
        public string FuelName;

        // Ec to use
        [KSPField(isPersistant = false)]
        public float EnergyUsage = 100f;

       // [KSPField(isPersistant = false, guiActive = true, guiName = "Variable Isp Setting")]
       // public string CurThrustSettingGUI = "0%";

        // Current thrust setting
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Variable Thrust Level"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 0.1f)]
        public float CurThrustSetting = 0f;

        [KSPEvent(guiActive = true, guiName = "Link all Variable Engines", active = true)]
        public void LinkEngines()
        {
            LinkAllEngines = true;
        }
        // Retract all radiators attached to this reactor
        [KSPEvent(guiActive = false, guiName = "Unlink all Variable Engines", active = false)]
        public void UnlinkEngines()
        {
            LinkAllEngines = false;
        }

        // Actions
        [KSPAction("Link Engines")]
        public void LinkEnginesAction(KSPActionParam param)
        {
            LinkEngines();
        }

        [KSPAction("Unlink Engines")]
        public void UnlinkEnginesAction(KSPActionParam param)
        {
            UnlinkEngines();
        }

        [KSPAction("Toggle Link Engines")]
        public void ToggleLinkEnginesAction(KSPActionParam param)
        {
            LinkAllEngines = !LinkAllEngines;
        }


        public override string GetInfo()
        {
            return String.Format("Maximum Thrust: {0:F1} kN", MaxThrust) + "\n" +
                String.Format("Isp at Maximum Thrust: {0:F0} s", MaxThrustIsp) + "\n";
        }

        private float minThrust= 0f;

        private ModuleEnginesFX engine;
        private Propellant ecPropellant;
        private Propellant fuelPropellant;

        private FloatCurve thrustAtmoCurve;
       

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            this.moduleName = "Variable ISP Engine";
        }

        public void ChangeIspAndThrust(float level)
        {
            engine.atmosphereCurve = new FloatCurve();
            engine.atmosphereCurve.Add(0f, Mathf.Lerp(MinThrustIsp ,MaxThrustIsp,level));

            thrustAtmoCurve = new FloatCurve();
            thrustAtmoCurve.Add(0f, Mathf.Lerp(MinThrust, MaxThrust, level));
            thrustAtmoCurve.Add(1f, 0f);

            engine.maxThrust = Mathf.Lerp(MinThrust, MaxThrust, level);

            RecalculateRatios(engine.maxThrust, Mathf.Lerp(MinThrustIsp, MaxThrustIsp, level));
        }

        private void RecalculateRatios(float desiredthrust, float desiredisp)
        {
            double fuelDensity = PartResourceLibrary.Instance.GetDefinition(fuelPropellant.name).density;
            double fuelRate = ((desiredthrust * 1000f) / (desiredisp * 9.82d)) / (fuelDensity*1000f);
            float ecRate = EnergyUsage / (float)fuelRate;

            fuelPropellant.ratio = 0.1f;
            ecPropellant.ratio = fuelPropellant.ratio * ecRate;
        }



        public override void OnStart(PartModule.StartState state)
        {
            
            // Get moduleEngines
            PartModuleList pml = this.part.Modules;
            for (int i = 0; i < pml.Count; i++)
            {
                PartModule curModule = pml.GetModule(i);
                engine  = curModule.GetComponent<ModuleEnginesFX>();
            }

            if (engine != null)
                Debug.Log("NFPP: Engine Check Passed");

            foreach (Propellant prop in engine.propellants)
            {
                if (prop.name == FuelName)
                    fuelPropellant = prop;
                if (prop.name == "ElectricCharge")
                    ecPropellant = prop;
            }


            if (UseDirectThrottle)
                ChangeIspAndThrust(engine.requestedThrottle);
            else
                ChangeIspAndThrust(CurThrustSetting / 100f);

            Debug.Log("NFPP: Variable ISP engine setup complete");
            if (UseDirectThrottle)
            {
                Debug.Log("NFPP: Using direct throttle method");
            }
        }

        int frameCounter = 0;
        float lastThrottle = -1f;

        public void ResetFrameCount()
        {
            frameCounter = 0;
        }

        public void Update()
        {
            if ((LinkAllEngines && Events["LinkEngines"].active) || (!LinkAllEngines && Events["UnlinkEngines"].active))
            {
                Events["LinkEngines"].active = !LinkAllEngines;
                Events["UnlinkEngines"].active = LinkAllEngines;
            }
        }

        public void FixedUpdate()
        {
            if (engine != null)
            {
                if (UseDirectThrottle)
                {
                    float throttleAmt = engine.requestedThrottle;
                    Debug.Log(throttleAmt);
                     if (throttleAmt != lastThrottle)
                     {
                         ChangeIspAndThrust(throttleAmt);
                         engine.maxThrust = thrustAtmoCurve.Evaluate((float)FlightGlobals.getStaticPressure(vessel.transform.position));
                         lastThrottle = throttleAmt;
                     }
                     CurThrustSetting = engine.requestedThrottle * 100f;
                }
                else
                {
                    frameCounter++;
                    if (frameCounter >= 10)
                    {
                        
                        if (LinkAllEngines)
                        {
                            VariableISPEngine[] allVariableEngines = part.vessel.GetComponentsInChildren<VariableISPEngine>();
                            foreach (VariableISPEngine variableEngine in allVariableEngines)
                            {
                                variableEngine.ChangeIspAndThrust(CurThrustSetting / 100f);
                                variableEngine.ResetFrameCount();
                            }
                        } else 
                        {
                            ChangeIspAndThrust(CurThrustSetting / 100f);
                        }
                        engine.maxThrust = thrustAtmoCurve.Evaluate((float)FlightGlobals.getStaticPressure(vessel.transform.position));
                        frameCounter = 0;
                    }
                }
            }
            
        }

    }
}
