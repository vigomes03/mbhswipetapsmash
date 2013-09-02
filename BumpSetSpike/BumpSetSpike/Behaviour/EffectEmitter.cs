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
        private String mEffectToEmit;

        private Int32 mEffectsPerEmission;

        private Int32 mDelayBetweenEmissions;

        private Int32 mCurDelay;

        private StopWatch mLifetime;

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

            //WobbleDefinition def = GameObjectManager.pInstance.pContentManager.Load<WobbleDefinition>(fileName);

            mEffectToEmit = "GameObjects\\Items\\Sparks\\Sparks";

            mEffectsPerEmission = 10;
            mDelayBetweenEmissions = 1000;
            mCurDelay = mDelayBetweenEmissions;
        }

        public override void OnAdd()
        {
            base.OnAdd();

            mLifetime = StopWatchManager.pInstance.GetNewStopWatch();
            mLifetime.pLifeTime = 2.0f;
        }

        public override void OnRemove()
        {
            base.OnRemove();

            StopWatchManager.pInstance.RecycleStopWatch(mLifetime);
        }

        public override void Reset()
        {
            base.Reset();

            mCurDelay = mDelayBetweenEmissions;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            mCurDelay++;

            if (mLifetime != null && mLifetime.IsExpired())
            {
                GameObjectManager.pInstance.Remove(mParentGOH);

                return;
            }

            if (mCurDelay < mDelayBetweenEmissions)
            {
                return;
            }

            for (Int32 i = 0; i < mEffectsPerEmission; i++)
            {
                GameObject fx = GameObjectFactory.pInstance.GetTemplate(mEffectToEmit);
                Single randAngleRad = MathHelper.ToRadians((Single)RandomManager.pInstance.RandomPercent() * 360.0f);
                fx.pDirection.mForward = new Vector2((Single)Math.Cos(randAngleRad), (Single)Math.Sin(randAngleRad));
                fx.pDirection.mSpeed = 1.0f;
                fx.pPosition = mParentGOH.pPosition;
                fx.pRotation = MathHelper.ToRadians((Single)RandomManager.pInstance.RandomPercent() * 360.0f);
                GameObjectManager.pInstance.Add(fx);
            }

            mCurDelay = 0;
        }
    }
}
