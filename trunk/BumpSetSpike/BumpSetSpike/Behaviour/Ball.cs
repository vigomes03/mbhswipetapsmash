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
    /// The Volleyball.
    /// </summary>
    class Ball : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// When the ball hits the ground the play is over. There will be some additional
        /// time before the game restarts, but this happens right away.
        /// </summary>
        public class OnPlayOverMessage : BehaviourMessage
        {
            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset()
            {
                
            }
        }

        /// <summary>
        /// Ask around and find out where we should be servering the ball to at the start of a new
        /// match.
        /// </summary>
        public class GetServeDestinationMessage : BehaviourMessage
        {
            /// <summary>
            /// The position to serve to.
            /// </summary>
            public Vector2 mDestination_Out;

            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset()
            {
                mDestination_Out = Vector2.Zero;
            }
        }

        /// <summary>
        /// Preallocate collision structs.
        /// </summary>
        private LineSegment mCollisionWall;
        private LineSegment mBallMovementLine;

        /// <summary>
        /// After the ball hits the gound, this is how much time should pass before the
        /// match is considered completed.
        /// </summary>
        private StopWatch mTimeOnGroundToEndPlay;

        /// <summary>
        /// The sound that plays when the ball hits the sand.
        /// </summary>
        private SoundEffect mFxSand;

        /// <summary>
        /// Tracks if the ball was on the ground last frame to prevent dupe sounds playing.
        /// </summary>
        private Boolean mOnGround;

        /// <summary>
        /// Keeps track of the rendering priority so that it can be restored.
        /// </summary>
        private Int32 mStartingRenderPriority;

        /// <summary>
        /// When the ball hits the ground, ending a match, we save where it landed. We need this data because
        /// the ball can continue to move, and we don't go to the game over screen right away.
        /// </summary>
        private Vector2 mLandPosition;

        /// <summary>
        /// Tracks if the ball has hit the ground yet.
        /// </summary>
        private Boolean mPlayOver;

        /// <summary>
        /// Tracks if the ball has hit the net this play. Used for achievement.
        /// </summary>
        private Boolean mHasHitNet;

        /// <summary>
        /// Preallocated messages to avoid GC.
        /// </summary>
        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;
        private SpriteRender.GetAttachmentPointMessage mGetAttachmentPointMsg;
        private OnPlayOverMessage mOnPlayOverMsg;
        private Player.OnMatchRestartMessage mOnMatchRestartMsg;
        private HitCountDisplay.IncrementHitCountMessage mIncrementHitCountMsg;
        private GetServeDestinationMessage mSetServeDestinationMsg;
        private Player.GetCurrentStateMessage mGetCurrentStateMsg;
        private Player.GetHasMultipleHitsBeforePartnerMessage mGetHasMultipleHitsBeforePartnerMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public Ball(GameObject parentGOH, String fileName)
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

            mCollisionWall = new LineSegment();
            mBallMovementLine = new LineSegment();

            mTimeOnGroundToEndPlay = StopWatchManager.pInstance.GetNewStopWatch();
            mTimeOnGroundToEndPlay.pLifeTime = 10.0f;
            mTimeOnGroundToEndPlay.pIsPaused = true;

            mFxSand = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\HitSand");

            mOnGround = false;
            mPlayOver = false;
            mHasHitNet = false;

            mStartingRenderPriority = mParentGOH.pRenderPriority;

            mSetActiveAnimationMsg = new SpriteRender.SetActiveAnimationMessage();
            mGetAttachmentPointMsg = new SpriteRender.GetAttachmentPointMessage();
            mOnPlayOverMsg = new OnPlayOverMessage();
            mOnMatchRestartMsg = new Player.OnMatchRestartMessage();
            mIncrementHitCountMsg = new HitCountDisplay.IncrementHitCountMessage();
            mSetServeDestinationMsg = new GetServeDestinationMessage();
            mGetCurrentStateMsg = new Player.GetCurrentStateMessage();
            mGetHasMultipleHitsBeforePartnerMsg = new Player.GetHasMultipleHitsBeforePartnerMessage();
        }

        /// <summary>
        /// Called once per frame by the game object.
        /// </summary>
        /// <param name="gameTime">The amount of time that has passed this frame.</param>
        public override void Update(GameTime gameTime)
        {
            // While in the main menu, the ball should not be rendered.
            // TODO: Move to render passes.
            if (GameObjectManager.pInstance.pCurUpdatePass == MBHEngineContentDefs.BehaviourDefinition.Passes.MAIN_MENU)
            {
                mParentGOH.pDoRender = false;

                return;
            }

            mParentGOH.pDoRender = true;

            // Apply gravity and directional movement.
            mParentGOH.pDirection.mForward.Y += 0.2f;
            mParentGOH.pPosition += mParentGOH.pDirection.mForward;

            // Define the extends of the playable area.
            Vector2 topLeft = new Vector2(-108.0f, -80.0f);
            Vector2 bottomRight = new Vector2(108.0f, 0.0f);

            // Has it hit the ground?
            if (mParentGOH.pPosY > bottomRight.Y)
            {
                // If the play is not already over, save the position that the ball hit the ground so 
                // that we can use that data later when figuring out who got the point.
                if (!mPlayOver)
                {
                    mPlayOver = true;
                    mLandPosition = mParentGOH.pPosition;
                }

                // Correct the position and dampen the movement so that it slows down a little
                // each time it hits the ground.
                mParentGOH.pPosY = bottomRight.Y;
                mParentGOH.pDirection.mForward.Y *= -0.6f;
                mParentGOH.pDirection.mForward.X *= 0.9f;

                if (mTimeOnGroundToEndPlay.pIsPaused)
                {
                    mTimeOnGroundToEndPlay.Restart();
                    mTimeOnGroundToEndPlay.pIsPaused = false;

                    GameObjectManager.pInstance.BroadcastMessage(mOnPlayOverMsg, mParentGOH);
                }

                if (!mOnGround)
                {
                    mOnGround = true;

                    mGetAttachmentPointMsg.mName_In = "Dust";
                    mParentGOH.OnMessage(mGetAttachmentPointMsg);

                    // Create a dust effect at the point of contact with the ground.
                    GameObject dust = GameObjectFactory.pInstance.GetTemplate("GameObjects\\Items\\Dust\\Dust");
                    dust.pPosition = mGetAttachmentPointMsg.mPoisitionInWorld_Out;
                    GameObjectManager.pInstance.Add(dust);

                    mFxSand.Play();
                }
            }
            else
            {
                mOnGround = false;
            }

            // Based on the velocity of the ball, play a different animation.
            if (mParentGOH.pDirection.mForward.Length() > 5.0f)
            {
                mSetActiveAnimationMsg.Reset();
                mSetActiveAnimationMsg.mAnimationSetName_In = "SpinFast";
            }
            else if (Math.Abs(mParentGOH.pDirection.mForward.X) <= 0.01f)
            {
                mSetActiveAnimationMsg.Reset();
                mSetActiveAnimationMsg.mAnimationSetName_In = "SpinNone";
            }
            else
            {
                mSetActiveAnimationMsg.Reset();
                mSetActiveAnimationMsg.mAnimationSetName_In = "SpinSlow";
            }

            if (mParentGOH.pPosY < -70.0f)
            {
                if (TutorialManager.pInstance.pCurState == TutorialManager.State.SWIPE)
                {
                    TutorialManager.pInstance.pCurState = TutorialManager.State.SWIPE_TXT_BALL_HIGH;
                }
            }

            mParentGOH.OnMessage(mSetActiveAnimationMsg);

            GameObjectManager.pInstance.pPlayer.OnMessage(mGetCurrentStateMsg, mParentGOH);

            // Has enough time passed since the play ended?
			// And is the player on the ground and standing in idle?
            if (mTimeOnGroundToEndPlay.IsExpired() && 
                GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_PLAY &&
                mGetCurrentStateMsg.mState_Out == Player.State.Idle)
            {

                if (GameModeManager.pInstance.pMode == GameModeManager.GameMode.TrickAttack)
                {
                    if (mLandPosition.X < 0.0f)
                    {
                        GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_OVER_LOSS;
                    }
                    else
                    {
                        GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_OVER;

                        if (mHasHitNet == true)
                        {
                            AchievementManager.pInstance.UnlockAchievement(AchievementManager.Achievements.BendTheRules);
                        }

                        if (ScoreManager.pInstance.IsMovePerformed(ScoreManager.ScoreType.HangTime))
                        {
                            AchievementManager.pInstance.UnlockAchievement(AchievementManager.Achievements.HangTime);
                        }

                        if (ScoreManager.pInstance.CalMultiplier() >= 8)
                        {
                            AchievementManager.pInstance.UnlockAchievement(AchievementManager.Achievements.IsThatEvenPossible);
                        }

                        mGetHasMultipleHitsBeforePartnerMsg.Reset();
                        GameObjectManager.pInstance.BroadcastMessage(mGetHasMultipleHitsBeforePartnerMsg, mParentGOH);

                        if (mGetHasMultipleHitsBeforePartnerMsg.mHasMultipleHits_Out)
                        {
                            AchievementManager.pInstance.UnlockAchievement(AchievementManager.Achievements.PuttingTheIInTeam);
                        }
                    }
                }
                else
                {
                    // Left of the net is a loss. Right of the net is win and requires the next play start.
                    if (mLandPosition.X < 0.0f)
                    {
                        if (TutorialManager.pInstance.pCurState == TutorialManager.State.PLAYER_TRYING)
                        {
                            GameObjectManager.pInstance.BroadcastMessage(mOnMatchRestartMsg, mParentGOH);
                            TutorialManager.pInstance.pCurState = TutorialManager.State.TRY_AGAIN;
                        }
                        else if (TutorialManager.pInstance.pCurState == TutorialManager.State.TRYING_AGAIN)
                        {
                            GameObjectManager.pInstance.BroadcastMessage(mOnMatchRestartMsg, mParentGOH);
                            TutorialManager.pInstance.pFailCount++;

                            if (TutorialManager.pInstance.pFailCount < 10)
                            {
                                TutorialManager.pInstance.pCurState = TutorialManager.State.TRY_AGAIN;
                            }
                            else
                            {
                                TutorialManager.pInstance.StopTutorial();
                                TutorialManager.pInstance.StartTutorial();
                            }
                        }
                        else if (TutorialManager.pInstance.pCurState == TutorialManager.State.TAP_END)
                        {
                            System.Diagnostics.Debug.Assert(false, "Tutorial failed to play winning move.");

                            GameObjectManager.pInstance.BroadcastMessage(mOnMatchRestartMsg, mParentGOH);
                            TutorialManager.pInstance.StopTutorial();
                            TutorialManager.pInstance.StartTutorial();
                        }
                        else
                        {
                            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_OVER;
                        }
                    }
                    else
                    {
                        if (TutorialManager.pInstance.pCurState == TutorialManager.State.PLAYER_TRYING)
                        {
                            TutorialManager.pInstance.pCurState = TutorialManager.State.COMPLETE_WELL_DONE;
                        }
                        else if (TutorialManager.pInstance.pCurState == TutorialManager.State.TRYING_AGAIN)
                        {
                            TutorialManager.pInstance.pCurState = TutorialManager.State.COMPLETE_WELL_DONE;
                        }
                        else if (TutorialManager.pInstance.pCurState == TutorialManager.State.TAP_END)
                        {
                            TutorialManager.pInstance.pCurState = TutorialManager.State.PLAYER_TRY;
                        }
                        else
                        {
                            GameObjectManager.pInstance.BroadcastMessage(mIncrementHitCountMsg, mParentGOH);

                            // We only award the achiement for actual gameplay. Not during the tutorial.
                            if (mHasHitNet == true)
                            {
                                AchievementManager.pInstance.UnlockAchievement(AchievementManager.Achievements.BendTheRules);
                            }

                            mGetHasMultipleHitsBeforePartnerMsg.Reset();
                            GameObjectManager.pInstance.BroadcastMessage(mGetHasMultipleHitsBeforePartnerMsg, mParentGOH);

                            if (mGetHasMultipleHitsBeforePartnerMsg.mHasMultipleHits_Out)
                            {
                                AchievementManager.pInstance.UnlockAchievement(AchievementManager.Achievements.PuttingTheIInTeam);
                            }
                        }

                        GameObjectManager.pInstance.BroadcastMessage(mOnMatchRestartMsg, mParentGOH);
                    }
                }

                mTimeOnGroundToEndPlay.pIsPaused = true;
            }

            //DebugMessageDisplay.pInstance.AddDynamicMessage("Ball: " + mParentGOH.pPosition);
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void PostUpdate(GameTime gameTime)
        {
            // After movement has finished, do corrections to the position if it is hitting the net.
            //

            List<GameObject> nets = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.WALL);

            System.Diagnostics.Debug.Assert(nets.Count == 1);

            if (nets.Count > 0)
            {
                GameObject net = nets[0];

                if (net.pCollisionRect.Intersects(mParentGOH.pCollisionRect))
                {
                    Vector2 ballChange = mParentGOH.pPosition - mParentGOH.pPrevPos;

                    mBallMovementLine.pPointA = mParentGOH.pPrevPos;
                    mBallMovementLine.pPointB = mParentGOH.pPosition;

                    if (mParentGOH.pDirection.mForward.X > 0.0f)
                    {
                        net.pCollisionRect.GetLeftEdge(ref mCollisionWall);

                        Vector2 intersect = new Vector2();
                        if (mCollisionWall.Intersects(mBallMovementLine, ref intersect))
                        {
                            mParentGOH.pDirection.mForward.X *= -0.1f;

                            mParentGOH.pPosX = intersect.X - mParentGOH.pCollisionRect.pDimensionsHalved.X;

                            ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Net, mParentGOH.pPosition);

                            mSetActiveAnimationMsg.Reset();
                            mSetActiveAnimationMsg.mAnimationSetName_In = "Bounce";
                            net.OnMessage(mSetActiveAnimationMsg, mParentGOH);

                            mHasHitNet = true;
                        }
                    }
                }
            }
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
                mPlayOver = false;
                mHasHitNet = false;

                mParentGOH.pPosX = 110.0f;
                mParentGOH.pPosY = -16.0f;

                // -30 -> -90

                Single speed = ((Single)RandomManager.pInstance.RandomPercent() * 2.0f) + 5.0f;

                Vector2 dest = new Vector2((Single)RandomManager.pInstance.RandomPercent() * -60.0f - 30.0f, 16.0f);

                if (!TutorialManager.pInstance.pTutorialCompleted)
                {
                    dest.X = -90;
                }

                mSetServeDestinationMsg.mDestination_Out = dest;
                GameObjectManager.pInstance.BroadcastMessage(mSetServeDestinationMsg, mParentGOH);

                Vector2 vel = MBHEngine.Math.Util.GetArcVelocity(mParentGOH.pPosition, dest, speed, 0.2f);

                mParentGOH.pDirection.mForward = vel;

                mTimeOnGroundToEndPlay.Restart();
                mTimeOnGroundToEndPlay.pIsPaused = true;
            }
            else if (msg is TutorialManager.HighlightBallMessage)
            {
                TutorialManager.HighlightBallMessage temp = (TutorialManager.HighlightBallMessage)msg;

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
