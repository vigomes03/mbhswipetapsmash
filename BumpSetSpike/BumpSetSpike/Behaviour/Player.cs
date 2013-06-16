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

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    ///  Simple class which reacts to damage events and as a result flashes the sprite a specified
    ///  colour.
    /// </summary>
    class Player : MBHEngine.Behaviour.Behaviour
    {
        public enum State
        {
            Idle = 0,
            Receiving,
            Jump,
            SpikeAttempt,
            Spike,
            Fall,
        }

        public class OnMatchRestartMessage : BehaviourMessage
        {
            public override void Reset()
            {
            }
        }

        public class OnGameRestartMessage : BehaviourMessage
        {
            public override void Reset()
            {
                
            }
        }

        public class GetCurrentStateMessage : BehaviourMessage
        {
            public State mState_In;

            public override void Reset()
            {
                mState_In = State.Idle;
            }
        }

        private State mCurrentState;

        private StopWatch mStateTimer;

        private List<MBHEngineContentDefs.GameObjectDefinition.Classifications> mBallClassifications;

        private List<GameObject> mCollisionResults;

        Vector2 mTopLeft;
        Vector2 mBottomRight;

        private LineSegment mCollisionWall;
        private LineSegment mMovementLine;

        private Int32 mFramesInAir;

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
            PlayerDefinition def = GameObjectManager.pInstance.pContentManager.Load<PlayerDefinition>(fileName);

            base.LoadContent(fileName);

            //DamageFlashDefinition def = GameObjectManager.pInstance.pContentManager.Load<DamageFlashDefinition>(fileName);

            mCurrentState = State.Receiving;

            mCollisionWall = new LineSegment();
            mMovementLine = new LineSegment();

            mBallClassifications = new List<MBHEngineContentDefs.GameObjectDefinition.Classifications>(1);
            mBallClassifications.Add(MBHEngineContentDefs.GameObjectDefinition.Classifications.SAFE_HOUSE);

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

            if(InputManager.pInstance.CheckGesture(GestureType.Tap, ref gesture) || InputManager.pInstance.CheckAction(InputManager.InputActions.A, true))
            {
                if (mCurrentState == State.Jump && GameflowManager.pInstance.pState == GameflowManager.State.GamePlay)
                {
                    mCurrentState = State.SpikeAttempt;

                    mStateTimer.pLifeTime = 5.0f;
                    mStateTimer.Restart();
                }
                else if (GameflowManager.pInstance.pState == GameflowManager.State.Lose)
                {
                    GameObjectManager.pInstance.BroadcastMessage(mGameRestartMsg, mParentGOH);
                    GameflowManager.pInstance.pState = GameflowManager.State.GamePlay;
                }
            }

            // Start with a fresh gesture.
            gesture = new GestureSample();

            if (InputManager.pInstance.CheckGesture(GestureType.Flick, ref gesture) || InputManager.pInstance.CheckAction(InputManager.InputActions.A, true))
            {
                if (mCurrentState == State.Idle && GameflowManager.pInstance.pState == GameflowManager.State.GamePlay)
                {
#if WINDOWS_PHONE
                    mParentGOH.pDirection.mForward = gesture.Delta * (Single)gameTime.ElapsedGameTime.TotalSeconds * 0.05f;
#else
                    mCollisionResults.Clear();
                    GameObjectManager.pInstance.GetGameObjectsInRange(mParentGOH, ref mCollisionResults, mBallClassifications);

                    List<GameObject> mBalls = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.SAFE_HOUSE);

                    if (mBalls.Count > 0)
                    {
                        mParentGOH.pDirection.mForward = mBalls[0].pCollisionRect.pCenterPoint - mParentGOH.pCollisionRect.pCenterPoint;

                        mParentGOH.pDirection.mForward.Normalize();

                        mParentGOH.pDirection.mForward *= 6.0f;
                    }
#endif
                    mCurrentState = State.Jump;

                    ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Jump, mParentGOH.pPosition);

                    mSetActiveAnimationMsg.mAnimationSetName_In = "JumpUp";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
            }

            if (mCurrentState != State.Spike)
            {
                mParentGOH.pDirection.mForward.Y += 0.2f;
            }

            mParentGOH.pPosition += mParentGOH.pDirection.mForward;

            if (mParentGOH.pPosY > mBottomRight.Y)
            {
                mParentGOH.pPosY = mBottomRight.Y;
                mParentGOH.pDirection.mForward = Vector2.Zero;

                if (mFramesInAir != 0)
                {
                    DebugMessageDisplay.pInstance.AddConstantMessage("Air Time:" + mFramesInAir);
                }

                mFramesInAir = 0;

                if (GameflowManager.pInstance.pState == GameflowManager.State.Lose)
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Sad";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }
                else
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "Idle";
                    mParentGOH.OnMessage(mSetActiveAnimationMsg);
                }

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

            if (mParentGOH.pDirection.mForward.Y < 0.0f)
            {
                //mSetActiveAnimationMsg.mAnimationSetName_In = "JumpUp";
                //mParentGOH.OnMessage(mSetActiveAnimationMsg);
            }
            else if (mParentGOH.pDirection.mForward.Y > 0.0f)
            {
                //mSetActiveAnimationMsg.mAnimationSetName_In = "JumpDown";
                //mParentGOH.OnMessage(mSetActiveAnimationMsg);
            }

            if (mCurrentState == State.SpikeAttempt)
            {
                mSetActiveAnimationMsg.mAnimationSetName_In = "Spike";
                mParentGOH.OnMessage(mSetActiveAnimationMsg);

                mCollisionResults.Clear();
                GameObjectManager.pInstance.GetGameObjectsInRange(mParentGOH, ref mCollisionResults, mBallClassifications);

                if (mCollisionResults.Count > 0)
                {
                    List<GameObject> nets = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.WALL);

                    System.Diagnostics.Debug.Assert(nets.Count == 1);

                    mGetAttachmentPointMsg.Reset();
                    mGetAttachmentPointMsg.mName_In = "SpikePoint";
                    nets[0].OnMessage(mGetAttachmentPointMsg);

                    Vector2 dir = mGetAttachmentPointMsg.mPoisitionInWorld_Out - mCollisionResults[0].pPosition;
                    dir.Normalize();

                    if (Math.Abs(mCollisionResults[0].pDirection.mForward.Y) < 1.0f)
                    {
                        ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.HighPoint, mParentGOH.pPosition);
                    }

                    if (mCollisionResults[0].pCollisionRect.pBottom - mParentGOH.pCollisionRect.pTop < 2.0f)
                    {
                        ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.FingerTips, mParentGOH.pCollisionRect.pCenterPoint);
                    }

                    if (mCollisionResults[0].pPosY >= -15.0f)
                    {
                        ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.LowPoint, mParentGOH.pCollisionRect.pCenterPoint);
                    }

                    if (mFramesInAir >= 30)
                    {
                        ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.HangTime, mParentGOH.pPosition);
                    }

                    if (mParentGOH.pDirection.mForward.X < 0)
                    {
                        ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.FadeAway, mParentGOH.pCollisionRect.pCenterLeft);
                    }

                    if (mCollisionResults[0].pDirection.mForward.Y < -3)
                    {
                        ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Upwards, mParentGOH.pCollisionRect.pBottomRight);
                    }

                    if (mParentGOH.pDirection.mForward.Length() > 7.5f)
                    {
                        ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Speedy, mParentGOH.pCollisionRect.pBottomLeft);
                    }

                    mCollisionResults[0].pDirection.mForward = dir * 10.0f;

                    mCurrentState = State.Spike;

                    mParentGOH.pDirection.mForward = Vector2.Zero;

                    mStateTimer.pLifeTime = 0.0f;
                    mStateTimer.Restart();

                    ScoreManager.pInstance.AddScore(ScoreManager.ScoreType.Spike, mCollisionResults[0].pPosition);
                }
                else if(mStateTimer.IsExpired())
                {
                    mCurrentState = State.Fall;
                }
            }

            if (mCurrentState == State.Spike)
            {
                if (mStateTimer.IsExpired())
                {
                    mCurrentState = State.Fall;

                    mParentGOH.pDirection.mForward.X = -1.0f;
                }
            }

            if (mCurrentState == State.Jump)
            {
                mFramesInAir++;
            }

            if (mCurrentState == State.Receiving)
            {                
                //mCollisionResults.Clear();
                //GameObjectManager.pInstance.GetGameObjectsInRange(mParentGOH, ref mCollisionResults, mBallClassifications);

                List<GameObject> balls = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.SAFE_HOUSE);

                if (balls.Count > 0 && balls[0].pPosY >= -16.0f && balls[0].pPosX <= 0.0f)
                {
                    System.Diagnostics.Debug.Assert(balls.Count == 1);

                    List<GameObject> partners = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.ALLY);
                    
                    if (partners.Count > 0)
                    {
                        System.Diagnostics.Debug.Assert(partners.Count == 1);

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

        public override void PostUpdate(GameTime gameTime)
        {
            List<GameObject> nets = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.WALL);

            System.Diagnostics.Debug.Assert(nets.Count == 1);

            GameObject net = nets[0];

            mMovementLine.pPointA = (mParentGOH.pPrevPos + mParentGOH.pCollisionRoot);
            mMovementLine.pPointB = net.pCollisionRect.pCenterPoint;

            DebugShapeDisplay.pInstance.AddSegment(mMovementLine, Color.Red);

            if (net.pCollisionRect.Intersects(mParentGOH.pCollisionRect))
            {
                if (mParentGOH.pDirection.mForward.X > 0.0f)
                {
                    net.pCollisionRect.GetLeftEdge(ref mCollisionWall);

                    Vector2 intersect = new Vector2();
                    if (mCollisionWall.Intersects(mMovementLine, ref intersect))
                    {
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
            if (msg is OnMatchRestartMessage || msg is OnGameRestartMessage)
            {
                //Single netBuffer = 30.0f;

                //mParentGOH.pPosX = (Single)RandomManager.pInstance.RandomPercent() * (mTopLeft.X + netBuffer) - netBuffer;
            }
            else if (msg is Ball.SetServeDestinationMessage)
            {
                Ball.SetServeDestinationMessage temp = (Ball.SetServeDestinationMessage)msg;

                mParentGOH.pPosX = temp.mDestination_In.X - 4.0f;

                mCurrentState = State.Receiving;
            }
            else if (msg is GetCurrentStateMessage)
            {
                GetCurrentStateMessage temp = (GetCurrentStateMessage)msg;
                temp.mState_In = mCurrentState;
            }
        }
    }
}
