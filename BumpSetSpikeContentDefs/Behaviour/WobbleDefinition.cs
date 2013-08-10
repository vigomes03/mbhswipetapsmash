using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngineContentDefs;

namespace BumpSetSpikeContentDefs
{
    public class WobbleDefinition : BehaviourDefinition
    {
        /// <summary>
        /// A wobble is just a collection of tweens, and how those tweens behave is encapsulated
        /// in this data structure.
        /// </summary>
        public class TweenInfo
        {
            /// <summary>
            /// How long should the tween take to complete, start to finish (eg. start -> end -> start).
            /// A length of 0 will result in no tween being applied in this case.
            /// </summary>
            public Single mLength;

            /// <summary>
            /// The starting value of this tween.
            /// </summary>
            public Single mStartValue;

            /// <summary>
            /// The ending value of this tween (it will then loop back to mStartValue).
            /// </summary>
            public Single mEndValue;
        }

        /// <summary>
        /// Tween used for rotation data.
        /// </summary>
        public TweenInfo mRotationTween;

        /// <summary>
        /// Tween used for scale data.
        /// </summary>
        public TweenInfo mScaleTween;
    }
}
