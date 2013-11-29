using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearFuture
{
    class ElectricEngineGUIOverride:PartModule
    {

        [KSPField(isPersistant = false)]
        public string EnergyUsage;


        public override string GetInfo()
        {
            return String.Format("Maximum Required Power: {0:F1} Ec/s", EnergyUsage);
        }
    }
}
