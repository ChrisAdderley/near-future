/// FissionContainer
/// ---------------------------------------------------
/// A container of fission fuel 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace NearFuture
{
    public class FissionContainer: PartModule
    {

        public bool Expended = false;


        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            this.moduleName = "Fission Fuel Container";
        }

        // Check to see if this module has both fuel and space for waste
        public bool CheckFuelSpace(double amt)
        {
            if (Expended)
            {
                return false;
            }

            double fuelAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition("EnrichedUranium").id).amount;
            double wasteSpaceAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition("DepletedUranium").id).maxAmount - this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition("DepletedUranium").id).amount;

            if (fuelAvailable > amt && wasteSpaceAvailable > amt)
            {
                Debug.Log("NFPP: Container has enough fuel");
                return true;
            }
            Debug.Log("NFPP: Container has insufficient fuel");
            return false;
            
        }

        // Refuel from this module
        public void RefuelReactorFromContainer(FissionGenerator reactor, double amt)
        {

            //Debug.Log("NFPP: FissionContainer has enough fuel and waste space");
            this.part.RequestResource("EnrichedUranium",amt);
            this.part.RequestResource("DepletedUranium", -amt);

            reactor.part.RequestResource("EnrichedUranium", -amt);
            reactor.part.RequestResource("DepletedUranium", amt);

            if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition("EnrichedUranium").id).amount <= 0 ||
                      ((this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition("DepletedUranium").id).maxAmount - this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition("DepletedUranium").id).amount) <= 0))
            {
                Expended = true;
                Debug.Log("NFPP: FissionContainer is now expended");
            }
        }
    }    
}
