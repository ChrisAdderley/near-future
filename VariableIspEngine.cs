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


        public override string GetInfo()
        {
            return String.Format("Maximum Thrust: {0:F1} kN", MaxThrust) + "\n" +
                String.Format("Isp at Maximum Thrust: {0:F0} s", MaxThrustIsp) + "\n";
        }

        private float minThrust= 0f;

        private ModuleEngines engine;
        private Propellant ecPropellant;
        private Propellant fuelPropellant;

        private FloatCurve thrustAtmoCurve;
       

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            this.moduleName = "Variable ISP Engine";
        }

        private void ChangeIspAndThrust()
        {
            engine.atmosphereCurve = new FloatCurve();
            engine.atmosphereCurve.Add(0f, Mathf.Lerp(MinThrustIsp ,MaxThrustIsp,CurThrustSetting/100f));

            thrustAtmoCurve = new FloatCurve();
            thrustAtmoCurve.Add(0f, Mathf.Lerp(MinThrust, MaxThrust, CurThrustSetting / 100f));
            thrustAtmoCurve.Add(1f, 0f);

            engine.maxThrust = Mathf.Lerp(MinThrust, MaxThrust, CurThrustSetting / 100f);

            RecalculateRatios(engine.maxThrust, Mathf.Lerp(MinThrustIsp, MaxThrustIsp, CurThrustSetting / 100f));
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
                engine  = curModule.GetComponent<ModuleEngines>();
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

    

            ChangeIspAndThrust();

            //CurThrustSettingGUI = String.Format("{0:F0}%", CurThrustSetting * 100f);

            Debug.Log("NFPP: Variable ISP engine setup complete");
        }

        int frameCounter = 0;

        public override void OnFixedUpdate()
        {
            frameCounter++;
            if (frameCounter >= 10)
            {
                ChangeIspAndThrust();

                engine.maxThrust = thrustAtmoCurve.Evaluate((float)FlightGlobals.getStaticPressure(vessel.transform.position));
                frameCounter = 0;

            }
            
        }

    }
}
