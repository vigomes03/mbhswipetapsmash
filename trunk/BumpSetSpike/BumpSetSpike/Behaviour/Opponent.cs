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
    class Opponent : MBHEngine.Behaviour.Behaviour
    {
        private enum State
        {
            Idle = 0,
            Knocked,
            Dead,
        }

        private State mCurrentState;

        private StopWatch mStateTimer;

        private List<MBHEngineContentDefs.GameObjectDefinition.Classifications> mBallClassifications;

        private List<GameObject> mCollisionResults;

        private Boolean mKabooomAvail;

        private Vector2 mStartPos;

        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;
        private SpriteRender.SetSpriteEffectsMessage mSetSpriteEffectsMsg;
        private SpriteRender.GetAttachmentPointMessage mGetAttachmentPointMsg;
        private Player.GetCurrentStateMessage mGetCurrentStateMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public Opponent(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// Call this to initialize a Behaviour with data supplied in a file.
        /// </summary>
        /// <param name="fileName">The file to load from.</param>
        public override void LoadContent(String fileName)
        {
            //PartnerDefinition def = GameObjectManager.pInstance.pContentManager.Load<PartnerDefinition>(fileName);

            base.LoadContent(fileName);

            //DamageFlashDefinition def = GameObjectManager.pInstance.pContentManager.Load<DamageFlashDefinition>(fileName);

            mCurrentState = State.Idle;

            mBallClassifications = new List<MBHEngineContentDefs.GameObjectDefinition.Classifications>(1);
            mBallClassifications.Add(MBHEngineContentDefs.GameObjectDefinition.Classifications.SAFE_HOUSE);

            mCollisionResults = new List<GameObject>(16);

            mStateTimer = StopWatchManager.pInstance.GetNewStopWatch();

            mKabooomAvail = true;
            
            mSetActiveAnimationMsg = new SpriteRender.SetActiveAnimationMessage();
            mSetSpriteEffectsMsg = new SpriteRender.SetSpriteEffectsMessage();
            mGetAttachmentPointMsg = new SpriteRender.GetAttachmentPointMessage();
            mGetCurrentStateMsg = new Player.GetCurrentStateMessage();
        }

        public override void OnAdd()
        {
            mSetSpriteEffectsMsg.mSpriteEffects_In = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            mParentGOH.OnMessage(mSetSpriteEffectsMsg);

            mStartPos = mParentGOH.pPosition;
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

                if (mCurrentState == State.Knocked)
                {
                    mCurrentState = State.Dead;
                }
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

            mGetCurrentStateMsg.Reset();
            GameObjectManager.pInstance.pPlayer.OnMessage(mGetCurrentStateMsg, mParentGOH);

            if (mCurrentState == State.Knocked)
            {
                mSetActiveAnimationMsg.Reset();
                mSetActiveAnimationMsg.mAnimationSetName_In = "Knocked";
                mSetActiveAnimationMsg.mDoNotRestartIfCompleted_In = true;
                mParentGOH.OnMessage(mSetActiveAnimationMsg);
            }
            else if (mCurrentState == State.Dead)
            {
                mSetActiveAnimationMsg.Reset();
                mSetActiveAnimationMsg.mAnimationSetName_In = "Dead";
                mSetActiveAnimationMsg.mDoNotRestartIfCompleted_In = true;
                mParentGOH.OnMessage(mSetActiveAnimationMsg);
            }
            else if (mCollisionResults.Count > 0 && mCollisionResults[0].pPosition.X > 0.0f && mGetCurrentStateMsg.mState_In != Player.State.Receiving)
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
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Happy";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
                else
                {
                    mSetActiveAnimationMsg.Reset();
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Idle";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
            }

            if (mGetCurrentStateMsg.mState_In != Player.State.Receiving)
            {
                mCollisionResults.Clear();
                GameObjectManager.pInstance.GetGameObjectsInRange(mParentGOH, ref mCollisionResults, mBallClassifications);

                if (mCollisionResults.Count > 0 && mCurrentState == State.Idle && mKabooomAvail)
                {
                    // Get hit here.
                    mParentGOH.pDirection.mForward = mCollisionResults[0].pDirection.mForward * 0.5f;
                    mParentGOH.pDirection.mForward.X *= 0.2f;

                    mParentGOH.pDirection.mForward.Y = -1.0f * Math.Abs(mParentGOH.pDirection.mForward.Y);

                    GameObject kabooom = GameObjectFactory.pInstance.GetTemplate("GameObjects\\Items\\Kabooom\\Kabooom");
                    kabooom.pPosition = mParentGOH.pPosition;
                    kabooom.pPosY -= 32.0f;
                    kabooom.pRotation = -25.0f;
                    GameObjectManager.pInstance.Add(kabooom);

                    mGetAttachmentPointMsg.Reset();
                    mGetAttachmentPointMsg.mName_In = "Blood";
                    mParentGOH.OnMessage(mGetAttachmentPointMsg);

                    GameObject blood = GameObjectFactory.pInstance.GetTemplate("GameObjects\\Items\\Blood\\Blood");
                    blood.pPosition = mGetAttachmentPointMsg.mPoisitionInWorld_Out;
                    GameObjectManager.pInstance.Add(blood);

                    ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Kabooom, mParentGOH.pPosition);

                    mCurrentState = State.Knocked;
                }
            }

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
            if (msg is Player.OnMatchRestartMessage)
            {
                //mCurrentState = State.Idle;
                mKabooomAvail = true;
            }
            else if (msg is Player.OnGameRestartMessage)
            {
                mKabooomAvail = true;
                mCurrentState = State.Idle;
                mParentGOH.pPosition = mStartPos;
            }
            else if (msg is Ball.OnPlayOverMessage)
            {
                mKabooomAvail = false;
            }
        }
    }
}
