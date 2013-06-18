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
        /// Preallocated messages to avoid GC.
        /// </summary>
        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;
        private SpriteRender.GetAttachmentPointMessage mGetAttachmentPointMsg;
        private OnPlayOverMessage mOnPlayOverMsg;
        private Player.OnMatchRestartMessage mOnMatchRestartMsg;
        private HitCountDisplay.IncrementHitCountMessage mIncrementHitCountMsg;
        private GetServeDestinationMessage mSetServeDestinationMsg;

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

            mSetActiveAnimationMsg = new SpriteRender.SetActiveAnimationMessage();
            mGetAttachmentPointMsg = new SpriteRender.GetAttachmentPointMessage();
            mOnPlayOverMsg = new OnPlayOverMessage();
            mOnMatchRestartMsg = new Player.OnMatchRestartMessage();
            mIncrementHitCountMsg = new HitCountDisplay.IncrementHitCountMessage();
            mSetServeDestinationMsg = new GetServeDestinationMessage();
        }

        /// <summary>
        /// Called once per frame by the game object.
        /// </summary>
        /// <param name="gameTime">The amount of time that has passed this frame.</param>
        public override void Update(GameTime gameTime)
        {
            // While in the main menu, the ball should not be rendered.
            // TODO: Move to render passes.
            if (GameflowManager.pInstance.pState == GameflowManager.State.MainMenu)
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
                // Correct the position and dampen the movement so that it slows down a little
                // each time it hits the ground.
                mParentGOH.pPosY = bottomRight.Y;
                mParentGOH.pDirection.mForward.Y *= -0.6f;
                mParentGOH.pDirection.mForward.X *= 0.9f;

                mGetAttachmentPointMsg.mName_In = "Dust";
                mParentGOH.OnMessage(mGetAttachmentPointMsg);

                // Create a dust effect at the point of contact with the ground.
                GameObject dust = GameObjectFactory.pInstance.GetTemplate("GameObjects\\Items\\Dust\\Dust");
                dust.pPosition = mGetAttachmentPointMsg.mPoisitionInWorld_Out;
                GameObjectManager.pInstance.Add(dust);

                if (mTimeOnGroundToEndPlay.pIsPaused)
                {
                    mTimeOnGroundToEndPlay.Restart();
                    mTimeOnGroundToEndPlay.pIsPaused = false;

                    GameObjectManager.pInstance.BroadcastMessage(mOnPlayOverMsg, mParentGOH);
                }
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

            mParentGOH.OnMessage(mSetActiveAnimationMsg);

            // Has enough time passed since the play ended?
            if (mTimeOnGroundToEndPlay.IsExpired())
            {
                // Left of the net is a loss. Right of the net is win and requires the next play start.
                if (mParentGOH.pPosX < 0.0f)
                {
                    GameflowManager.pInstance.pState = GameflowManager.State.Lose;
                }
                else
                {
                    GameObjectManager.pInstance.BroadcastMessage(mIncrementHitCountMsg, mParentGOH);
                    GameObjectManager.pInstance.BroadcastMessage(mOnMatchRestartMsg, mParentGOH);
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

                            ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Net, mParentGOH.pPosition);
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
                mParentGOH.pPosX = 110.0f;
                mParentGOH.pPosY = -16.0f;

                // -30 -> -90

                Single speed = ((Single)RandomManager.pInstance.RandomPercent() * 2.0f) + 5.0f;

                Vector2 dest = new Vector2((Single)RandomManager.pInstance.RandomPercent() * -60.0f - 30.0f, 16.0f);

                mSetServeDestinationMsg.mDestination_Out = dest;
                GameObjectManager.pInstance.BroadcastMessage(mSetServeDestinationMsg, mParentGOH);

                Vector2 vel = MBHEngine.Math.Util.GetArcVelocity(mParentGOH.pPosition, dest, speed, 0.2f);

                mParentGOH.pDirection.mForward = vel;

                mTimeOnGroundToEndPlay.Restart();
                mTimeOnGroundToEndPlay.pIsPaused = true;
            }
        }
    }
}
