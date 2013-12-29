// Utils
// ---------------------------------
// Static functions that are useful for the NearFuture pack

using System;
using UnityEngine;
using System.Collections.Generic;

namespace NearFuture
{
    internal static class Utils
    {
        private static string cfgName = "NFPP.cfg";

        // Gets the cfg file path
        public static string GetConfigFilePath()
        {
            string path = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/" + cfgName;
            
            return path;
        }

        // Gets the cfg file itself
        public static ConfigNode GetConfigFile()
        {
            ConfigNode config = new ConfigNode();
            return config;
        }

        // This function loads up some animationstates
        public static AnimationState[] SetUpAnimation(string animationName, Part part)
        {
            var states = new List<AnimationState>();
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.speed = 0;
                animationState.enabled = true;
                // Clamp this or else weird things happen
                animationState.wrapMode = WrapMode.ClampForever;
                animation.Blend(animationName);
                states.Add(animationState);
            }
            // Convert 
            return states.ToArray();
        }

        // Returns true if ship is it atmoshpere
        public static bool VesselInAtmosphere(Vessel vessel)
        {
           return vessel.heightFromSurface < vessel.mainBody.maxAtmosphereAltitude;
        }

    }

    public enum RadiatorState
    {
        Deployed,
        Deploying,
        Retracted,
        Retracting,
        Broken,
    }
}
