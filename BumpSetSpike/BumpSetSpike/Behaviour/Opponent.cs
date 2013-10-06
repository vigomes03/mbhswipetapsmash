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
    /// <summary>
    /// The AI players we are playing against.
    /// </summary>
    class Opponent : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// The current state of this character.
        /// </summary>
        private enum State
        {
            Idle = 0,
            Knocked,
            Dead,
        }

        /// <summary>
        /// The current state of this character.
        /// </summary>
        private State mCurrentState;

        /// <summary>
        /// Tracks how long we should be in the current state.
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
        /// The opponent only gets knocked around during the initial hit. Once it hits the ground,
        /// it will not knock them around. This tracks when that happens.
        /// </summary>
        private Boolean mKabooomAvail;

        /// <summary>
        /// Tracks where the character started so that they can be reset when the game restarts.
        /// </summary>
        private Vector2 mStartPos;

        /// <summary>
        /// The sound that plays when the opponent gets smashed with the ball.
        /// </summary>
        private SoundEffect mFxHit;

        /// <summary>
        /// The sound that plays when they hit the ground.
        /// </summary>
        private SoundEffect mFxHitGround;

        /// <summary>
        /// Preallocated messages to avoid GC.
        /// </summary>
        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;
        private SpriteRender.SetSpriteEffectsMessage mSetSpriteEffectsMsg;
        private SpriteRender.GetAttachmentPointMessage mGetAttachmentPointMsg;
        private Player.GetCurrentStateMessage mGetCurrentStateMsg;
        private HitCountDisplay.GetCurrentHitCountMessage mGetCurrentHitCountMsg;

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
            base.LoadContent(fileName);

            mCurrentState = State.Idle;

            mBallClassifications = new List<MBHEngineContentDefs.GameObjectDefinition.Classifications>(1);
            mBallClassifications.Add(MBHEngineContentDefs.GameObjectDefinition.Classifications.VOLLEY_BALL);

            mCollisionResults = new List<GameObject>(16);

            mStateTimer = StopWatchManager.pInstance.GetNewStopWatch();

            mKabooomAvail = true;

            mFxHit = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\HitOpponent");
            mFxHitGround = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\HitOpponentLand");
            
            mSetActiveAnimationMsg = new SpriteRender.SetActiveAnimationMessage();
            mSetSpriteEffectsMsg = new SpriteRender.SetSpriteEffectsMessage();
            mGetAttachmentPointMsg = new SpriteRender.GetAttachmentPointMessage();
            mGetCurrentStateMsg = new Player.GetCurrentStateMessage();
            mGetCurrentHitCountMsg = new HitCountDisplay.GetCurrentHitCountMessage();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void OnAdd()
        {
            // The opponents just always face left.
            mSetSpriteEffectsMsg.mSpriteEffects_In = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            mParentGOH.OnMessage(mSetSpriteEffectsMsg);

            // Save this so that it can be restarted later.
            mStartPos = mParentGOH.pPosition;
        }

        /// <summary>
        /// Called once per frame by the game object.
        /// </summary>
        /// <param name="gameTime">The amount of time that has passed this frame.</param>
        public override void Update(GameTime gameTime)
        {
            // Apply momentum and gravity to the character.
            mParentGOH.pDirection.mForward.Y += 0.2f;
            mParentGOH.pPosition += mParentGOH.pDirection.mForward;

            // Define the area that this character can move.
            Vector2 topLeft = new Vector2(-90.0f, -80.0f);
            Vector2 bottomRight = new Vector2(90.0f, 0.0f);

            if (mParentGOH.pPosY > bottomRight.Y)
            {
                // Clamp to the ground.
                mParentGOH.pPosY = bottomRight.Y;
                mParentGOH.pDirection.mForward = Vector2.Zero;

                // If they were being knocked around, we don't want to switch to a dead state
                // until they hit the ground.
                if (mCurrentState == State.Knocked)
                {
                    mFxHitGround.Play();

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

            // Get the ball.
            mCollisionResults.Clear();
            GameObjectManager.pInstance.GetGameObjectsInRange(mParentGOH.pPosition, 45.0f, ref mCollisionResults, mBallClassifications);

            // Get the current state of the player.
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
            else if (mCollisionResults.Count > 0 && mCollisionResults[0].pPosition.X > 0.0f && mGetCurrentStateMsg.mState_Out != Player.State.Receiving)
            {
                mSetActiveAnimationMsg.Reset();
                mSetActiveAnimationMsg.mAnimationSetName_In = "Bump";
                mSetActiveAnimationMsg.mDoNotRestartIfCompleted_In = true;
                mParentGOH.OnMessage(mSetActiveAnimationMsg);
            }
            else
            {
                if (GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_OVER ||
                    GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_OVER_LOSS)
                {
                    mSetActiveAnimationMsg.Reset();

                    // Depending on if the player got a new high score or not, we want to play a different
                    // animation.
                    GameObjectManager.pInstance.BroadcastMessage(mGetCurrentHitCountMsg, mParentGOH);
                    if (mGetCurrentHitCountMsg.mCount_Out > LeaderBoardManager.pInstance.pTopHits &&
                        GameObjectManager.pInstance.pCurUpdatePass != BehaviourDefinition.Passes.GAME_OVER_LOSS)
                    {
                        mSetActiveAnimationMsg.mAnimationSetName_In = "Sad";
                    }
                    else
                    {
                        mSetActiveAnimationMsg.mAnimationSetName_In = "Happy";
                    }
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
                else
                {
                    mSetActiveAnimationMsg.Reset();
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Idle";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
            }

            // If the player is no longer recieving the ball, then we start checking for
            // KABOOOM hits. We wait so that the opponent is not hit by the serve itself.
            if (mGetCurrentStateMsg.mState_Out != Player.State.Receiving)
            {
                // Find the ball again, but this time only if it is colliding with us.
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
                    // Put it above them, but also offset a bit based on the velocity they are being knocked in. This gives
                    // enough variety that we shouldn't have z-sort issues.
                    kabooom.pPosY -= 32.0f - (mParentGOH.pDirection.mForward.Y * 2.0f);
                    GameObjectManager.pInstance.Add(kabooom);

                    DebugMessageDisplay.pInstance.AddConstantMessage("Kabooom Angle: " + MathHelper.ToDegrees(kabooom.pRotation));

                    mGetAttachmentPointMsg.Reset();
                    mGetAttachmentPointMsg.mName_In = "Blood";
                    mParentGOH.OnMessage(mGetAttachmentPointMsg);

                    GameObject blood = GameObjectFactory.pInstance.GetTemplate("GameObjects\\Items\\Blood\\Blood");
                    blood.pPosition = mGetAttachmentPointMsg.mPoisitionInWorld_Out;
                    GameObjectManager.pInstance.Add(blood);

                    ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Kabooom, mParentGOH.pPosition);

                    mFxHit.Play();
                    mFxHitGround.Play();

                    mCurrentState = State.Knocked;
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
            if (msg is Player.OnMatchRestartMessage)
            {
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
                // The ball has hit the ground. We don't want bouncing balls to KABOOOM the opponent.
                mKabooomAvail = false;
            }
        }
    }
}
