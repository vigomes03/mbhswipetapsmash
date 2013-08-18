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
using MBHEngineContentDefs;
using Microsoft.Xna.Framework.Audio;

namespace BumpSetSpike.Behaviour
{
    class Partner : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Tracks how long we have been in a state.
        /// </summary>
        private StopWatch mStateTimer;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private List<MBHEngineContentDefs.GameObjectDefinition.Classifications> mBallClassifications;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private List<GameObject> mCollisionResults;

        /// <summary>
        /// How many times has this player hit the ball this round.
        /// </summary>
        private Int32 mHitCount;

        /// <summary>
        /// The sound that plays when the ball is bumped.
        /// </summary>
        private SoundEffect mFxBump;

        /// <summary>
        /// Keeps track of the rendering priority so that it can be restored.
        /// </summary>
        private Int32 mStartingRenderPriority;

        /// <summary>
        /// Preallocated messages to avoid GC.
        /// </summary>
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
            
            mBallClassifications = new List<MBHEngineContentDefs.GameObjectDefinition.Classifications>(1);
            mBallClassifications.Add(MBHEngineContentDefs.GameObjectDefinition.Classifications.VOLLEY_BALL);

            mCollisionResults = new List<GameObject>(16);

            mStateTimer = StopWatchManager.pInstance.GetNewStopWatch();

            mHitCount = 0;

            mFxBump = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\Bump");

            mStartingRenderPriority = mParentGOH.pRenderPriority;

            mSetActiveAnimationMsg = new SpriteRender.SetActiveAnimationMessage();
            mSetSpriteEffectsMsg = new SpriteRender.SetSpriteEffectsMessage();
            mGetCurrentStateMsg = new Player.GetCurrentStateMessage();
        }

        /// <summary>
        /// See parent.
        /// </summary>
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

            if (mCollisionResults.Count > 0 && mHitCount < mMaxHitCount && mGetCurrentStateMsg.mState_Out != Player.State.Receiving)
            {
                mSetActiveAnimationMsg.Reset();
                mSetActiveAnimationMsg.mAnimationSetName_In = "Bump";
                mSetActiveAnimationMsg.mDoNotRestartIfCompleted_In = true;
                mParentGOH.OnMessage(mSetActiveAnimationMsg);

                if (TutorialManager.pInstance.pCurState == TutorialManager.State.SET)
                {
                    TutorialManager.pInstance.pCurState = TutorialManager.State.SET_TXT;
                }
            }
            else
            {

                if (GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_OVER)
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

            if (mCollisionResults.Count > 0 && mHitCount < mMaxHitCount && mGetCurrentStateMsg.mState_Out != Player.State.Receiving)
            {
                if (mGetCurrentStateMsg.mState_Out != Player.State.Receiving)
                {
                    mHitCount++;

                    mCollisionResults[0].pDirection.mForward.X = 0.0f;
                    mCollisionResults[0].pDirection.mForward.Y = -5.0f;

                    mFxBump.Play();
                }
            }

            //DebugMessageDisplay.pInstance.AddDynamicMessage("Partner: " + mParentGOH.pPosition);
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
            else if (msg is TutorialManager.HighlightPartnerMessage)
            {
                TutorialManager.HighlightPartnerMessage temp = (TutorialManager.HighlightPartnerMessage)msg;

                if (temp.mEnable)
                {
                    mParentGOH.pRenderPriority = 100;
                }
                else
                {
                    mParentGOH.pRenderPriority = mStartingRenderPriority;
                }
            }
        }
    }
}
