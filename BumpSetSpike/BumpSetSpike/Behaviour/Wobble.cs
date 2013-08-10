using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;
using MBHEngine.Math;
using BumpSetSpikeContentDefs;

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// Rotates and scales the object on a looping tween.
    /// </summary>
    class Wobble : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Defines how the scale of the object changes during the wobble. This is applied
        /// uniformly to both X and Y.
        /// </summary>
        private Tween mScaleTween;

        /// <summary>
        /// Defines how rotation of the object changes during the wobble.
        /// </summary>
        private Tween mRotationTween;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public Wobble(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// Call this to initialize a Behaviour with data supplied in a file.
        /// </summary>
        /// <param name="fileName">The file to load from.</param>
        public override void LoadContent(String fileName)
        {
            base.LoadContent(fileName);

            WobbleDefinition def = GameObjectManager.pInstance.pContentManager.Load<WobbleDefinition>(fileName);

            if (def.mScaleTween.mLength > 0.0f)
            {
                StopWatch watch = StopWatchManager.pInstance.GetNewStopWatch();
                watch.pLifeTime = def.mScaleTween.mLength;
                mScaleTween = new Tween(watch, def.mScaleTween.mStartValue, def.mScaleTween.mEndValue);
            }

            if (def.mRotationTween.mLength > 0.0f)
            {
                StopWatch watch = StopWatchManager.pInstance.GetNewStopWatch();
                watch.pLifeTime = def.mRotationTween.mLength;
                mRotationTween = new Tween(watch, def.mRotationTween.mStartValue, def.mRotationTween.mEndValue);
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Wobble()
        {
            if (mScaleTween.mWatch != null)
            {
                StopWatchManager.pInstance.RecycleStopWatch(mScaleTween.mWatch);
            }

            if (mRotationTween.mWatch != null)
            {
                StopWatchManager.pInstance.RecycleStopWatch(mRotationTween.mWatch);
            }
        }

        /// <summary>
        /// Called once per frame by the game object.
        /// </summary>
        /// <param name="gameTime">The amount of time that has passed this frame.</param>
        public override void Update(GameTime gameTime)
        {
            if (mScaleTween.mWatch != null)
            {
                mScaleTween.Update();
                mParentGOH.pScaleXY = mScaleTween.mCurrentValue;
            }

            if (mRotationTween.mWatch != null)
            {
                mRotationTween.Update();
                mParentGOH.pRotation = MathHelper.ToRadians(mRotationTween.mCurrentValue);
            }
        }
    }
}
