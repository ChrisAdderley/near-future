using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFuture
{
    class PowerTransmitter: PartModule
    {
        // Is transmitter on
        [KSPField(isPersistant = true)]
        public bool Enabled;

        // Number of physics frames per power update
        [KSPField(isPersistant = true)]
        public int PowerGenerationUpdateRate;


        [KSPEvent(guiActive = true, guiName = "Activate Transmitter", active = true)]
        public void ActivateTransmission()
        {
            Enabled = true;
        }

        [KSPEvent(guiActive = true, guiName = "Deactivate Transmitter", active = false)]
        public void DeactivateTransmission()
        {
            Enabled = false;
        }


        private int ticker;

        [KSPAction("Activate Transmitter")]
        public void ActivateTransmissionAction(KSPActionParam param)
        {
            ActivateTransmission();
        }

        [KSPAction("Deactivate Transmitter")]
        public void DeactivateTransmissionAction(KSPActionParam param)
        {
            DeactivateTransmission();
        }


        public override void OnStart(PartModule.StartState state)
        {
            Actions["ActivateTransmissionAction"].guiName = Events["ActivateTransmission"].guiName = "Activate Transmitter";
            Actions["DeactivateTransmissionAction"].guiName = Events["DeactivateTransmission"].guiName = "Deactivate Transmitter";
            
        }
        public override void OnUpdate()
        {
            Events["ActivateTransmission"].active = !Enabled;
            Events["DeactivateTransmission"].active = Enabled;
            if (Enabled)
            {
            }
        }

        public override void OnFixedUpdate()
        {
            Vector2 powerToTransmit = Vector2.zero;
            if (Enabled)
            {
                ticker++;
                // Update power generation
                if (ticker >= PowerGenerationUpdateRate)
                {
                    ticker = 0;
                    // Find power generation for vessel
                   powerToTransmit = SumVesselPowerGeneration();

                   SaveVesselPowerTransmission(powerToTransmit);
                }
            }
        }

        // Saves the vessel power transmission
        private void SaveVesselPowerTransmission(Vector2 amt)
        {
            ConfigNode cfg = Utils.GetConfigFile();

            string vesselIDTransmitter = vessel.id.ToString();

       
            if (!cfg.HasValue(vesselIDTransmitter))
            {
                cfg.AddValue(vesselIDTransmitter, amt[0].ToString());
            }
            else
            {
                cfg.SetValue(vesselIDTransmitter, amt[0].ToString());
            }

            if (!cfg.HasValue(vesselIDTransmitter + "_solar"))
            {
                cfg.AddValue(vesselIDTransmitter + "_solar", amt[1].ToString());
            }
            else
            {
                cfg.SetValue(vesselIDTransmitter + "_solar", amt[1].ToString());
            }

            cfg.Save(Utils.GetConfigFilePath());

        }

        // Sums the power generation of the vessel
        private Vector2 SumVesselPowerGeneration()
        {
            float totalGenerationStatic = 0f;
            float totalGenerationSolar = 0f;

            List<Part> partList = vessel.parts;
            foreach (Part curPart in partList)
            {
                PartModuleList pml = curPart.Modules;
                for (int i = 0; i < pml.Count; i++)
                {
                    // try to get any power generation parts
                    FissionGenerator curReactor = (FissionGenerator) pml.GetModule(i);
                    ModuleDeployableSolarPanel curPanel = (ModuleDeployableSolarPanel)pml.GetModule(i);

                    // Reactors are easy
                    if (curReactor != null)
                    {
                        totalGenerationStatic = 0f;// totalGenerationStatic + curReactor.GetCurrentPower();
                    }

                    if (curPanel != null)
                    {
                        // Panels must be extended to count as generating
                        if (curPanel.panelState == ModuleDeployableSolarPanel.panelStates.EXTENDED)
                        {
                            totalGenerationSolar = totalGenerationSolar + curPanel.chargeRate;
                        }
                    }

                }
            }
            print("NFPP: Vessel reactor power = " + totalGenerationStatic.ToString());
            print("NFPP: Vessel solar power = " + totalGenerationSolar.ToString());
            return new Vector2(totalGenerationStatic,totalGenerationSolar);
        }

    }
}
