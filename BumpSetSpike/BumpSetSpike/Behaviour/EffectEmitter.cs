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
    class EffectEmitter : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Tracks how long it has been since the last emission.
        /// </summary>
        private Int32 mCurDelay;

        /// <summary>
        /// Tracks the lifetime of the emitter.
        /// </summary>
        private StopWatch mLifetime;

        /// <summary>
        /// The definition of this emitter.
        /// </summary>
        private EffectEmitterDefinition mDef;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public EffectEmitter(GameObject parentGOH, String fileName)
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

            mDef = GameObjectManager.pInstance.pContentManager.Load<EffectEmitterDefinition>(fileName);
            mCurDelay = mDef.mDelayBetweenEmissions;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void OnAdd()
        {
            base.OnAdd();

            mLifetime = StopWatchManager.pInstance.GetNewStopWatch();
            mLifetime.pLifeTime = mDef.mLifeTime;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void OnRemove()
        {
            base.OnRemove();

            StopWatchManager.pInstance.RecycleStopWatch(mLifetime);
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            mCurDelay = mDef.mDelayBetweenEmissions;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            mCurDelay++;

            // If the lifetime has passed, remove the object.
            if (mLifetime != null && mLifetime.IsExpired())
            {
                GameObjectManager.pInstance.Remove(mParentGOH);

                return;
            }

            // If not enough time has passed since the last emission, don't do anything.
            if (mCurDelay < mDef.mDelayBetweenEmissions)
            {
                return;
            }

            // Time to emit additional particles.
            for (Int32 i = 0; i < mDef.mEffectsPerEmission; i++)
            {
                GameObject fx = GameObjectFactory.pInstance.GetTemplate(mDef.mEffectToEmit);
                Single angle = (Single)RandomManager.pInstance.RandomPercent() * mDef.mAngleDiviation;
                angle -= mDef.mAngleDiviation * 0.5f;
                angle -= mDef.mDirection;
                Single randAngleRad = MathHelper.ToRadians(angle);
                fx.pDirection.mForward = new Vector2((Single)Math.Cos(randAngleRad), (Single)Math.Sin(randAngleRad));
                fx.pDirection.mSpeed = mDef.mMaxSpeed + (Single)RandomManager.pInstance.RandomPercent();
                fx.pPosition = mParentGOH.pPosition;
                fx.pRotation = randAngleRad;
                GameObjectManager.pInstance.Add(fx);
            }

            mCurDelay = 0;
        }
    }
}
