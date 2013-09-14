using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngineContentDefs;

namespace BumpSetSpikeContentDefs
{
    public class EffectEmitterDefinition : BehaviourDefinition
    {
        /// <summary>
        /// The GameObject to emit from this emitter.
        /// </summary>
        public String mEffectToEmit;

        /// <summary>
        /// How many effects to emit every cycle (as defined by mDelayBetweenEmissions).
        /// </summary>
        public Int32 mEffectsPerEmission;

        /// <summary>
        /// How many frames should pass between emissions.
        /// </summary>
        public Int32 mDelayBetweenEmissions;

        /// <summary>
        /// The amount of diviation from the direction of effect.
        /// </summary>
        public Single mAngleDiviation;

        /// <summary>
        /// The direction to shoot the effect in.
        /// </summary>
        public Single mDirection;

        /// <summary>
        /// How many frames should this emitter live.
        /// </summary>
        public Single mLifeTime;

        /// <summary>
        /// The max speed that objects get emitted with. They will get a random speed between 0 and
        /// this value.
        /// </summary>
        public Single mMaxSpeed;
    }
}
