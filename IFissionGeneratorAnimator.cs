/// IFissionGeneratorAnimator
/// ---------------------------------------------------
/// Interface for the animator of a fission generator
/// Implement to make these things look right
/// 
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NearFuture
{
    // State defining whether radiators are retracted
    public enum RadiatorState
    {
        Deployed,
        Deploying,
        Retracted,
        Retracting,
    }

    public interface IFissionGeneratorAnimator
    {
        RadiatorState CurrentState { get; }
        void Deploy();
        void Retract();
    }
}
    