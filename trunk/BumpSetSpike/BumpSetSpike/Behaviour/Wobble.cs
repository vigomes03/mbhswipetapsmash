using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;
using MBHEngine.Math;

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

            //DamageWobbleDefinition def = GameObjectManager.pInstance.pContentManager.Load<DamageWobbleDefinition>(fileName);

            StopWatch watch = StopWatchManager.pInstance.GetNewStopWatch();
            watch.pLifeTime = 5.0f;
            mScaleTween = new Tween(watch, 0.95f, 1.05f);

            watch = StopWatchManager.pInstance.GetNewStopWatch();
            watch.pLifeTime = 15.0f;
            mRotationTween = new Tween(watch, -2, 2);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Wobble()
        {
            StopWatchManager.pInstance.RecycleStopWatch(mScaleTween.mWatch);
            StopWatchManager.pInstance.RecycleStopWatch(mRotationTween.mWatch);
        }

        /// <summary>
        /// Called once per frame by the game object.
        /// </summary>
        /// <param name="gameTime">The amount of time that has passed this frame.</param>
        public override void Update(GameTime gameTime)
        {
            mScaleTween.Update();
            mRotationTween.Update();

            mParentGOH.pRotation = MathHelper.ToRadians(mRotationTween.mCurrentValue);
            mParentGOH.pScaleXY = mScaleTween.mCurrentValue;
        }
    }
}
