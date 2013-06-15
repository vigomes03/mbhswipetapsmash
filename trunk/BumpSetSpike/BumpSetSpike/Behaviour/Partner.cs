using System;
using MBHEngine.Behaviour;
using Microsoft.Xna.Framework;
using MBHEngine.GameObject;
using MBHEngine.Math;
using System.Diagnostics;
using BumpSetSpikeContentDefs;
using Microsoft.Xna.Framework.Input.Touch;
using MBHEngine.Render;
using MBHEngine.Debug;
using System.Collections.Generic;
using BumpSetSpike.Gameflow;

namespace BumpSetSpike.Behaviour
{
    class Partner : MBHEngine.Behaviour.Behaviour
    {
        private enum State
        {
            Idle = 0,
            Bump,
        }

        private State mCurrentState;

        private StopWatch mStateTimer;

        private List<MBHEngineContentDefs.GameObjectDefinition.Classifications> mBallClassifications;

        private List<GameObject> mCollisionResults;

        private Int32 mHitCount;

        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;
        private SpriteRender.SetSpriteEffectsMessage mSetSpriteEffectsMsg;
        private Player.GetCurrentStateMessage mGetCurrentStateMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public Partner(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// Call this to initialize a Behaviour with data supplied in a file.
        /// </summary>
        /// <param name="fileName">The file to load from.</param>
        public override void LoadContent(String fileName)
        {
            PartnerDefinition def = GameObjectManager.pInstance.pContentManager.Load<PartnerDefinition>(fileName);

            base.LoadContent(fileName);

            //DamageFlashDefinition def = GameObjectManager.pInstance.pContentManager.Load<DamageFlashDefinition>(fileName);

            mCurrentState = State.Idle;

            mBallClassifications = new List<MBHEngineContentDefs.GameObjectDefinition.Classifications>(1);
            mBallClassifications.Add(MBHEngineContentDefs.GameObjectDefinition.Classifications.SAFE_HOUSE);

            mCollisionResults = new List<GameObject>(16);

            mStateTimer = StopWatchManager.pInstance.GetNewStopWatch();

            mHitCount = 0;

            mSetActiveAnimationMsg = new SpriteRender.SetActiveAnimationMessage();
            mSetSpriteEffectsMsg = new SpriteRender.SetSpriteEffectsMessage();
            mGetCurrentStateMsg = new Player.GetCurrentStateMessage();
        }

        public override void OnAdd()
        {
            mSetSpriteEffectsMsg.mSpriteEffects_In = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            mParentGOH.OnMessage(mSetSpriteEffectsMsg);
        }

        /// <summary>
        /// Called once per frame by the game object.
        /// </summary>
        /// <param name="gameTime">The amount of time that has passed this frame.</param>
        public override void Update(GameTime gameTime)
        {
            mParentGOH.pDirection.mForward.Y += 0.2f;
            mParentGOH.pPosition += mParentGOH.pDirection.mForward;

            Vector2 topLeft = new Vector2(-90.0f, -80.0f);
            Vector2 bottomRight = new Vector2(90.0f, 0.0f);

            if (mParentGOH.pPosY > bottomRight.Y)
            {
                mParentGOH.pPosY = bottomRight.Y;
                mParentGOH.pDirection.mForward = Vector2.Zero;
            }
            else if (mParentGOH.pPosY < topLeft.Y)
            {
                mParentGOH.pPosY = topLeft.Y;
                mParentGOH.pDirection.mForward.Y = 0.0f;
            }

            if (mParentGOH.pPosX < topLeft.X)
            {
                mParentGOH.pPosX = topLeft.X;
                mParentGOH.pDirection.mForward.X = 0.0f;
            }
            else if (mParentGOH.pPosX > bottomRight.X)
            {
                mParentGOH.pPosX = bottomRight.X;
                mParentGOH.pDirection.mForward.X = 0.0f;
            }

            mCollisionResults.Clear();
            GameObjectManager.pInstance.GetGameObjectsInRange(mParentGOH.pPosition, 45.0f, ref mCollisionResults, mBallClassifications);

            Int32 mMaxHitCount = 1;

            mGetCurrentStateMsg.Reset();
            GameObjectManager.pInstance.pPlayer.OnMessage(mGetCurrentStateMsg, mParentGOH);

            if (mCollisionResults.Count > 0 && mHitCount < mMaxHitCount && mGetCurrentStateMsg.mState_In != Player.State.Receiving)
            {
                mSetActiveAnimationMsg.Reset();
                mSetActiveAnimationMsg.mAnimationSetName_In = "Bump";
                mSetActiveAnimationMsg.mDoNotRestartIfCompleted_In = true;
                mParentGOH.OnMessage(mSetActiveAnimationMsg);
               
            }
            else
            {

                if (GameflowManager.pInstance.pState == GameflowManager.State.Lose)
                {
                    mSetActiveAnimationMsg.Reset();
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Sad";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
                else
                {
                    mSetActiveAnimationMsg.Reset();
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Idle";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
            }

            mCollisionResults.Clear();
            GameObjectManager.pInstance.GetGameObjectsInRange(mParentGOH, ref mCollisionResults, mBallClassifications);

            if (mCollisionResults.Count > 0 && mHitCount < mMaxHitCount && mGetCurrentStateMsg.mState_In != Player.State.Receiving)
            {
                if (mGetCurrentStateMsg.mState_In != Player.State.Receiving)
                {
                    mHitCount++;

                    mCollisionResults[0].pDirection.mForward.X = 0.0f;
                    mCollisionResults[0].pDirection.mForward.Y = -5.0f;

                    mCurrentState = State.Bump;
                }
            }

            /*
            mParentGOH.pPosY = Math.Min(0.0f, mParentGOH.pPosY);
            mParentGOH.pPosY = Math.Max(-30.0f, mParentGOH.pPosY);
            mParentGOH.pPosX = Math.Max(-45.0f, mParentGOH.pPosX);
            mParentGOH.pPosX = Math.Min(45.0f, mParentGOH.pPosX);
            */

            DebugMessageDisplay.pInstance.AddDynamicMessage("Partner: " + mParentGOH.pPosition);
        }

        /// <summary>
        /// The main interface for communicating between behaviours.  Using polymorphism, we
        /// define a bunch of different messages deriving from BehaviourMessage.  Each behaviour
        /// can then check for particular upcasted messahe types, and either grab some data 
        /// from it (set message) or store some data in it (get message).
        /// </summary>
        /// <param name="msg">The message being communicated to the behaviour.</param>
        public override void OnMessage(ref BehaviourMessage msg)
        {
            if (msg is Player.OnMatchRestartMessage || msg is Player.OnGameRestartMessage)
            {
                mHitCount = 0;
            }
        }
    }
}
