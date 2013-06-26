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
using MBHEngine.Input;
using MBHEngineContentDefs;

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    ///  Simple class which reacts to damage events and as a result flashes the sprite a specified
    ///  colour.
    /// </summary>
    class Player : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// The current state of this character.
        /// </summary>
        public enum State
        {
            Idle = 0,       // Standing around.
            Receiving,      // Waiting for the initial serve.
            Jump,           // Jump initiated.
            SpikeAttempt,   // Spike button pressed.
            Fall,           // Spike attempt failed.
        }

        /// <summary>
        /// Sent when a match is restarted (eg. a successful point).
        /// </summary>
        public class OnMatchRestartMessage : BehaviourMessage
        {
            public override void Reset()
            {
            }
        }

        /// <summary>
        /// Sent when the game is restarted (eg. after the match is lost).
        /// </summary>
        public class OnGameRestartMessage : BehaviourMessage
        {
            public override void Reset()
            {
                
            }
        }

        /// <summary>
        /// Allows other characters to check the state of the Player.
        /// </summary>
        public class GetCurrentStateMessage : BehaviourMessage
        {
            /// <summary>
            /// The state that the object is currently in.
            /// </summary>
            public State mState_Out;

            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset()
            {
                mState_Out = State.Idle;
            }
        }

        /// <summary>
        /// The current state of the player.
        /// </summary>
        private State mCurrentState;

        /// <summary>
        /// How long has the player been in this state.
        /// </summary>
        private StopWatch mStateTimer;

        /// <summary>
        /// When finding the ball, we need to pass in a preallocated list of classifications.
        /// </summary>
        private List<MBHEngineContentDefs.GameObjectDefinition.Classifications> mBallClassifications;

        /// <summary>
        /// When searching for objects we need to pass in a ref to a list.
        /// </summary>
        private List<GameObject> mCollisionResults;

        /// <summary>
        /// Defines the dimension of the court as the player is allowed to move around.
        /// </summary>
        private Vector2 mTopLeft;
        private Vector2 mBottomRight;

        /// <summary>
        /// Used for collision corrections. Preallocated to avoid GC.
        /// </summary>
        private LineSegment mCollisionWall;
        private LineSegment mMovementLine;

        /// <summary>
        /// Tracks how long the player has been in the air for some of the skills.
        /// </summary>
        private Int32 mFramesInAir;

        /// <summary>
        /// Preallocated messages.
        /// </summary>
        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;
        private SpriteRender.GetAttachmentPointMessage mGetAttachmentPointMsg;
        private OnMatchRestartMessage mMatchRestartMsg;
        private OnGameRestartMessage mGameRestartMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public Player(GameObject parentGOH, String fileName)
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

            PlayerDefinition def = GameObjectManager.pInstance.pContentManager.Load<PlayerDefinition>(fileName);

            mCurrentState = State.Receiving;

            mCollisionWall = new LineSegment();
            mMovementLine = new LineSegment();

            mBallClassifications = new List<MBHEngineContentDefs.GameObjectDefinition.Classifications>(1);
            mBallClassifications.Add(MBHEngineContentDefs.GameObjectDefinition.Classifications.VOLLEY_BALL);

            mCollisionResults = new List<GameObject>(16);

            mStateTimer = StopWatchManager.pInstance.GetNewStopWatch();

            mTopLeft = new Vector2(-90.0f, -80.0f);
            mBottomRight = new Vector2(90.0f, 0.0f);

            mFramesInAir = 0;

            mSetActiveAnimationMsg = new SpriteRender.SetActiveAnimationMessage();
            mGetAttachmentPointMsg = new SpriteRender.GetAttachmentPointMessage();
            mMatchRestartMsg = new OnMatchRestartMessage();
            mGameRestartMsg = new OnGameRestartMessage();
        }

        /// <summary>
        /// Called once per frame by the game object.
        /// </summary>
        /// <param name="gameTime">The amount of time that has passed this frame.</param>
        public override void Update(GameTime gameTime)
        {
            GestureSample gesture = new GestureSample();

            // Is the player tapping the screen?
            if(InputManager.pInstance.CheckGesture(GestureType.Tap, ref gesture) || InputManager.pInstance.CheckAction(InputManager.InputActions.A, true))
            {
                // If we are jumping and the player taps the screen, we spike the ball.
                if (mCurrentState == State.Jump && GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_PLAY)
                {
                    mCurrentState = State.SpikeAttempt;

                    // The player has some number of frames to move into hit range before a spike attempt
                    // is failed. This is to make the game a little more forgiving.
                    mStateTimer.pLifeTime = 5.0f;
                    mStateTimer.Restart();
                }
                else if (GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_OVER)
                {
                    // If the game is over, tapping the screen restarts the match.
                    GameObjectManager.pInstance.BroadcastMessage(mGameRestartMsg, mParentGOH);
                    GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY;
                }
            }

            // Start with a fresh gesture.
            gesture = new GestureSample();

            // Is the player flicking the screen, trying to throw the player into the air?
            if (InputManager.pInstance.CheckGesture(GestureType.Flick, ref gesture) || InputManager.pInstance.CheckAction(InputManager.InputActions.A, true))
            {
                // Only allow jumping if you are currently in the Idle state.
                if (mCurrentState == State.Idle && GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_PLAY)
                {
#if WINDOWS_PHONE
                    mParentGOH.pDirection.mForward = gesture.Delta * (Single)gameTime.ElapsedGameTime.TotalSeconds * 0.05f;
#else
                    // Find the ball so that we can force the player to jump towards it.
                    List<GameObject> mBalls = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.VOLLEY_BALL);

                    if (mBalls.Count > 0)
                    {
                        System.Diagnostics.Debug.Assert(mBalls.Count == 1);

                        // Automatically throw the player at the ball. Doesn't work great, but good enough
                        // for testing right now on PC.
                        mParentGOH.pDirection.mForward = mBalls[0].pCollisionRect.pCenterPoint - mParentGOH.pCollisionRect.pCenterPoint;
                        mParentGOH.pDirection.mForward.Normalize();
                        mParentGOH.pDirection.mForward *= 6.0f;
                    }
#endif
                    // We are now jumping.
                    mCurrentState = State.Jump;

                    ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Jump, mParentGOH.pPosition);

                    mSetActiveAnimationMsg.mAnimationSetName_In = "JumpUp";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
            }

            // Apply gravity.
            mParentGOH.pDirection.mForward.Y += 0.2f;

            // Always move in our forward direction.
            mParentGOH.pPosition += mParentGOH.pDirection.mForward;

            // Have we hit the floor?
            if (mParentGOH.pPosY > mBottomRight.Y)
            {
                // Reset the position and stop moving.
                mParentGOH.pPosY = mBottomRight.Y;
                mParentGOH.pDirection.mForward = Vector2.Zero;

                if (mFramesInAir != 0)
                {
                    DebugMessageDisplay.pInstance.AddConstantMessage("Air Time:" + mFramesInAir);
                }

                // Reset the counter since we are now on the ground.
                mFramesInAir = 0;

                // We may have lost the game so look sad.
                if (GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.GAME_OVER)
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Sad";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
                else
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Idle";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }

                // Receiving overrides Idle.
                if (mCurrentState != State.Receiving)
                {
                    mCurrentState = State.Idle;
                }
            }
            else if (mParentGOH.pPosY < mTopLeft.Y)
            {
                mParentGOH.pPosY = mTopLeft.Y;
                mParentGOH.pDirection.mForward.Y = 0.0f;
            }

            if (mParentGOH.pPosX < mTopLeft.X)
            {
                mParentGOH.pPosX = mTopLeft.X;
                mParentGOH.pDirection.mForward.X = 0.0f;
            }
            else if (mParentGOH.pPosX > mBottomRight.X)
            {
                mParentGOH.pPosX = mBottomRight.X;
                mParentGOH.pDirection.mForward.X = 0.0f;
            }

            // Has a spike been started?
            if (mCurrentState == State.SpikeAttempt)
            {
                mSetActiveAnimationMsg.mAnimationSetName_In = "Spike";
                mParentGOH.OnMessage(mSetActiveAnimationMsg);

                // Find if we are hitting any balls.
                mCollisionResults.Clear();
                GameObjectManager.pInstance.GetGameObjectsInRange(mParentGOH, ref mCollisionResults, mBallClassifications);

                if (mCollisionResults.Count > 0)
                {
                    System.Diagnostics.Debug.Assert(mCollisionResults.Count == 1);

                    // Now find any nets. We need the net to figure out where to hit the ball.
                    List<GameObject> nets = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.WALL);

                    System.Diagnostics.Debug.Assert(nets.Count == 1);

                    if (nets.Count > 0)
                    {
                        // There is a spot on the net, where we always try to spike into, so that we get various
                        // angles (rather than spiking to a position on the ground).
                        mGetAttachmentPointMsg.Reset();
                        mGetAttachmentPointMsg.mName_In = "SpikePoint";
                        nets[0].OnMessage(mGetAttachmentPointMsg);

                        Vector2 dir = mGetAttachmentPointMsg.mPoisitionInWorld_Out - mCollisionResults[0].pPosition;
                        dir.Normalize();

                        // Have we (almost) stopped moving, meaning we are near the peak of the jump?
                        if (Math.Abs(mCollisionResults[0].pDirection.mForward.Y) < 1.0f)
                        {
                            ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.HighPoint, mParentGOH.pPosition);
                        }

                        // Did the ball hit near the tips of our fingers?
                        if (mCollisionResults[0].pCollisionRect.pBottom - mParentGOH.pCollisionRect.pTop < 2.0f)
                        {
                            ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.FingerTips, mParentGOH.pCollisionRect.pCenterPoint);
                        }

                        // Was the ball close to the ground.
                        if (mCollisionResults[0].pPosY >= -15.0f)
                        {
                            ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.LowPoint, mParentGOH.pCollisionRect.pCenterPoint);
                        }

                        // Were we in the air long enough for it to be considered "Hang Time"?
                        if (mFramesInAir >= 30)
                        {
                            ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.HangTime, mParentGOH.pPosition);
                        }

                        // Were we moving backwards at the point of contact?
                        if (mParentGOH.pDirection.mForward.X < 0)
                        {
                            ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.FadeAway, mParentGOH.pCollisionRect.pCenterLeft);
                        }

                        // Did we hit the ball as it was moving upwards?
                        if (mCollisionResults[0].pDirection.mForward.Y < -3)
                        {
                            ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Upwards, mParentGOH.pCollisionRect.pBottomRight);
                        }

                        // Did we throw ourselves really fast into the ball?
                        if (mParentGOH.pDirection.mForward.Length() > 7.5f)
                        {
                            ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Speedy, mParentGOH.pCollisionRect.pBottomLeft);
                        }

                        // Launch the ball at the SpikePoint.
                        mCollisionResults[0].pDirection.mForward = dir * 10.0f;

                        mCurrentState = State.Fall;

                        // Stop the player's movement in Y but knock him back a little in X.
                        mParentGOH.pDirection.mForward.X = -1.0f;
                        mParentGOH.pDirection.mForward.Y = 0.0f;

                        mStateTimer.pLifeTime = 0.0f;
                        mStateTimer.Restart();

                        ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Spike, mCollisionResults[0].pPosition);
                    }
                }
                else if(mStateTimer.IsExpired())
                {
                    // The player has taken too long to hit the after starting a spike attempt.
                    mCurrentState = State.Fall;
                }
            }

            // Increment our time in the air.
            if (mCurrentState == State.Jump)
            {
                mFramesInAir++;
            }

            // Are we waiting for the serve to reach us.
            if (mCurrentState == State.Receiving)
            {                
                List<GameObject> balls = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.VOLLEY_BALL);

                // Basically we assume the ball was thrown at us, so all we care about is whether or not
                // the ball is over the net and below a certain point.
                if (balls.Count > 0 && balls[0].pPosY >= -16.0f && balls[0].pPosX <= 0.0f)
                {
                    System.Diagnostics.Debug.Assert(balls.Count == 1);

                    List<GameObject> partners = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.ALLY);
                    
                    if (partners.Count > 0)
                    {
                        System.Diagnostics.Debug.Assert(partners.Count == 1);

                        // Hit the ball to our partner.
                        Single speed = ((Single)RandomManager.pInstance.RandomPercent() * 3.0f) + 2.0f;
                        Vector2 dest = partners[0].pPosition;
                        dest.X -= 4.0f;
                        dest.Y = balls[0].pPosY;
                        Vector2 source = balls[0].pPosition;
                        Vector2 vel = MBHEngine.Math.Util.GetArcVelocity(source, dest, speed, 0.2f);

                        balls[0].pDirection.mForward = vel;

                        mCurrentState = State.Idle;
                    }
                }

                // When the ball is in a certain range, start playing the bump animation.
                if (Vector2.Distance(balls[0].pPosition, mParentGOH.pPosition) <= 45.0f)
                {
                    mSetActiveAnimationMsg.Reset();
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Bump";
                    mSetActiveAnimationMsg.mDoNotRestartIfCompleted_In = true;
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
            }

            DebugMessageDisplay.pInstance.AddDynamicMessage("Player: " + mParentGOH.pPosition);
        }

        /// <summary>
        /// See Parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void PostUpdate(GameTime gameTime)
        {
            // After all positions have been update, we want to correct the position
            // of the player to make sure he doesn't jump through the net.
            //

            // Find teh net.
            List<GameObject> nets = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.WALL);

            System.Diagnostics.Debug.Assert(nets.Count == 1);

            GameObject net = nets[0];

            // Create a line defining our movement.
            mMovementLine.pPointA = (mParentGOH.pPrevPos + mParentGOH.pCollisionRoot);
            mMovementLine.pPointB = net.pCollisionRect.pCenterPoint;

            //DebugShapeDisplay.pInstance.AddSegment(mMovementLine, Color.Red);

            // Are we colliding with the net?
            if (net.pCollisionRect.Intersects(mParentGOH.pCollisionRect))
            {
                // Are we moving forward?
                if (mParentGOH.pDirection.mForward.X > 0.0f)
                {
                    // Find the left edge of the net so we can push off of it.
                    net.pCollisionRect.GetLeftEdge(ref mCollisionWall);

                    // Find out where and if our movement line crosses the left wall of the 
                    // net. If these intersect it means we are running into this wall.
                    Vector2 intersect = new Vector2();
                    if (mCollisionWall.Intersects(mMovementLine, ref intersect))
                    {
                        // Stop moving forward, and move back to the edge.
                        mParentGOH.pDirection.mForward.X = 0.0f;
                        mParentGOH.pPosX = intersect.X - mParentGOH.pCollisionRect.pDimensionsHalved.X - mParentGOH.pCollisionRoot.X;
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
            if (msg is Ball.GetServeDestinationMessage)
            {
                Ball.GetServeDestinationMessage temp = (Ball.GetServeDestinationMessage)msg;

                mParentGOH.pPosX = temp.mDestination_Out.X - 4.0f;

                mCurrentState = State.Receiving;
            }
            else if (msg is GetCurrentStateMessage)
            {
                GetCurrentStateMessage temp = (GetCurrentStateMessage)msg;
                temp.mState_Out = mCurrentState;
            }
        }
    }
}
