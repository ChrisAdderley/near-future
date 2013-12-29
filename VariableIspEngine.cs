/// VariableIspEngine
/// ---------------------------------------------------
/// A module that allows the Isp and thrust of an engine to be varied
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

        [KSPField(isPersistant = false)]
        public float MaxThrustFuelRatio;
        [KSPField(isPersistant = false)]
        public float MaxThrustEcRatio;
        [KSPField(isPersistant = false)]
        public string FuelName;



        [KSPField(isPersistant = false, guiActive = true, guiName = "Variable Isp Setting")]
        public string CurThrustSettingGUI = "0%";

        [KSPField(isPersistant = true)]
        public float CurThrustSetting = 0f;


        public override string GetInfo()
        {
            return String.Format("Maximum Thrust: {0:F1} kN", MaxThrust) + "\n" +
                String.Format("Isp at Maximum Thrust: {0:F0} s", MaxThrustIsp) + "\n";
        }

        // Step for adjustments
        [KSPField(isPersistant = false)]
        public float VariableStep = 0.2f;


        private ModuleEngines engine;
        private Propellant ecPropellant;
        private Propellant fuelPropellant;
        private FloatCurve thrustCurve;
        private FloatCurve ispCurve;
        private FloatCurve ecCurve;
        private FloatCurve fuelCurve;

        private FloatCurve thrustAtmoCurve;
        // Actions
        [KSPEvent(guiActive = true, guiName = "Increase Thrust", active = true)]
        public void IncreaseThrustGUI()
        {
            IncreaseVariableThrust();
        }

        [KSPEvent(guiActive = true, guiName = "Decrease Thrust", active = true)]
        public void DecreaseThrustGUI()
        {
            DecreaseVariableThrust();
        }


        [KSPAction("Increase Thrust")]
        public void IncreaseThrustAction(KSPActionParam param) 
        {
            IncreaseVariableThrust();
        }

        [KSPAction("Decrease Thrust")]
        public void DecreaseThrustAction(KSPActionParam param)
        {
            DecreaseVariableThrust();
        }



        public void IncreaseVariableThrust()
        {
            CurThrustSetting = Mathf.Clamp01(CurThrustSetting + VariableStep);
            // adjust isp
            engine.atmosphereCurve = new FloatCurve();
            engine.atmosphereCurve.Add(0f, ispCurve.Evaluate(CurThrustSetting));
            engine.atmosphereCurve.Add(1f, 200f);

            fuelPropellant.ratio = fuelCurve.Evaluate(CurThrustSetting);
            ecPropellant.ratio = ecCurve.Evaluate(CurThrustSetting);

            // adjust thrust
            engine.maxThrust = thrustCurve.Evaluate(CurThrustSetting);

            CurThrustSettingGUI = String.Format("{0:F0}%", CurThrustSetting*100f);
        }

        public void DecreaseVariableThrust()
        {
            CurThrustSetting = Mathf.Clamp01(CurThrustSetting - VariableStep);
            // adjust isp
            engine.atmosphereCurve = new FloatCurve();
            engine.atmosphereCurve.Add(0f, ispCurve.Evaluate(CurThrustSetting));
            engine.atmosphereCurve.Add(1f, 200f);

            fuelPropellant.ratio = fuelCurve.Evaluate(CurThrustSetting);
            ecPropellant.ratio = ecCurve.Evaluate(CurThrustSetting);

            // adjust thrust
            engine.maxThrust = thrustCurve.Evaluate(CurThrustSetting);
            CurThrustSettingGUI = String.Format("{0:F0}%", CurThrustSetting * 100f);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            this.moduleName = "Variable ISP Engine";
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

            thrustCurve = new FloatCurve();
            ispCurve = new FloatCurve();
            ecCurve = new FloatCurve();
            fuelCurve = new FloatCurve();
            thrustAtmoCurve = new FloatCurve();

            // get thrust at setting 0 from ModuleEngines
            thrustCurve.Add(0f,engine.maxThrust);
            thrustCurve.Add(1f,MaxThrust);
            
            // get isp at setting 0 from ModuleEngines
            ispCurve.Add(0f,engine.atmosphereCurve.Evaluate(0f));
            ispCurve.Add(1f,MaxThrustIsp);
            
            foreach (Propellant prop in engine.propellants)
            {
                if (prop.name == FuelName)
                    fuelPropellant = prop;
                if (prop.name == "ElectricCharge")
                    ecPropellant = prop;    
            }

            fuelCurve.Add(0f, fuelPropellant.ratio);
            fuelCurve.Add(1f, MaxThrustFuelRatio);

            ecCurve.Add(0f, ecPropellant.ratio);
            ecCurve.Add(1f, MaxThrustEcRatio);

            fuelPropellant.ratio = fuelCurve.Evaluate(CurThrustSetting);
            ecPropellant.ratio = ecCurve.Evaluate(CurThrustSetting);

            // adjust isp
            engine.atmosphereCurve = new FloatCurve();
            engine.atmosphereCurve.Add(0f, ispCurve.Evaluate(CurThrustSetting));
            engine.atmosphereCurve.Add(1f, 200f);
            // adjust thrust
            engine.maxThrust = thrustCurve.Evaluate(CurThrustSetting);

            CurThrustSettingGUI = String.Format("{0:F0}%", CurThrustSetting * 100f);

            Debug.Log("NFPP: Variable ISP engine setup complete");
        }

    }
}
