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
    class Ball : MBHEngine.Behaviour.Behaviour
    {
        public class OnPlayOverMessage : BehaviourMessage
        {
            public override void Reset()
            {
                
            }
        }

        public class SetServeDestinationMessage : BehaviourMessage
        {
            public Vector2 mDestination_In;

            public override void Reset()
            {
                mDestination_In = Vector2.Zero;
            }
        }

        private LineSegment mCollisionWall;
        private LineSegment mBallMovementLine;

        private StopWatch mTimeOnGroundToEndPlay;

        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;
        private SpriteRender.GetAttachmentPointMessage mGetAttachmentPointMsg;
        private OnPlayOverMessage mOnPlayOverMsg;
        private Player.OnMatchRestartMessage mOnMatchRestartMsg;
        private HitCountDisplay.IncrementHitCountMessage mIncrementHitCountMsg;
        private SetServeDestinationMessage mSetServeDestinationMsg;

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
            //PlayerDefinition def = GameObjectManager.pInstance.pContentManager.Load<PlayerDefinition>(fileName);

            base.LoadContent(fileName);

            //DamageFlashDefinition def = GameObjectManager.pInstance.pContentManager.Load<DamageFlashDefinition>(fileName);

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
            mSetServeDestinationMsg = new SetServeDestinationMessage();
        }

        /// <summary>
        /// Called once per frame by the game object.
        /// </summary>
        /// <param name="gameTime">The amount of time that has passed this frame.</param>
        public override void Update(GameTime gameTime)
        {
            if (GameflowManager.pInstance.pState == GameflowManager.State.MainMenu)
            {
                mParentGOH.pDoRender = false;

                return;
            }

            DebugShapeDisplay.pInstance.AddTransform(new Vector2(-60.0f, -16.0f));

            mParentGOH.pDoRender = true;

            mParentGOH.pDirection.mForward.Y += 0.2f;
            mParentGOH.pPosition += mParentGOH.pDirection.mForward;

            Vector2 topLeft = new Vector2(-108.0f, -80.0f);
            Vector2 bottomRight = new Vector2(108.0f, 0.0f);

            if (mParentGOH.pPosY > bottomRight.Y)
            {
                mParentGOH.pPosY = bottomRight.Y;
                mParentGOH.pDirection.mForward.Y *= -0.6f;
                mParentGOH.pDirection.mForward.X *= 0.9f;

                mGetAttachmentPointMsg.mName_In = "Dust";
                mParentGOH.OnMessage(mGetAttachmentPointMsg);

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
            else if (mParentGOH.pPosY < topLeft.Y)
            {
                //mParentGOH.pPosY = topLeft.Y;
                //mParentGOH.pDirection.mForward.Y = 0.0f;
            }

            /*
            if (mParentGOH.pPosX < topLeft.X || mParentGOH.pPosX > bottomRight.X)
            {
                if (mParentGOH.pPosX < bottomRight.X)
                {
                    GameflowManager.pInstance.pState = GameflowManager.State.Lose;
                }
                else
                {
                    GameObjectManager.pInstance.BroadcastMessage(mOnMatchRestartMsg, mParentGOH);
                }
            }
            */

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

            if (mTimeOnGroundToEndPlay.IsExpired())
            {
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

            //mParentGOH.pScaleY = 1.0f + Math.Abs(mParentGOH.pDirection.mForward.Y) * 0.1f;
            //mParentGOH.pScaleX = 1.0f + Math.Abs(mParentGOH.pDirection.mForward.X) * 0.1f;

            DebugMessageDisplay.pInstance.AddDynamicMessage("Ball: " + mParentGOH.pPosition);
        }

        public override void PostUpdate(GameTime gameTime)
        {
            List<GameObject> nets = GameObjectManager.pInstance.GetGameObjectsOfClassification(MBHEngineContentDefs.GameObjectDefinition.Classifications.WALL);

            System.Diagnostics.Debug.Assert(nets.Count == 1);

            GameObject net = nets[0];

            if(net.pCollisionRect.Intersects(mParentGOH.pCollisionRect))
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
                /*
                mParentGOH.pPosX = -20.0f; // bottomRight.X;
                mParentGOH.pPosY = -80.0f;
                mParentGOH.pDirection.mForward.X = 0.0f;
                mParentGOH.pDirection.mForward.Y = 10.0f;
                 * */

                mParentGOH.pPosX = 110.0f; // bottomRight.X;
                mParentGOH.pPosY = -16.0f;

                // -30 -> -90

                Single speed = ((Single)RandomManager.pInstance.RandomPercent() * 2.0f) + 5.0f;

                Vector2 dest = new Vector2((Single)RandomManager.pInstance.RandomPercent() * -60.0f - 30.0f, 16.0f);

                mSetServeDestinationMsg.mDestination_In = dest;
                GameObjectManager.pInstance.BroadcastMessage(mSetServeDestinationMsg, mParentGOH);

                Vector2 vel = MBHEngine.Math.Util.GetArcVelocity(mParentGOH.pPosition, dest, speed, 0.2f);
                /*
                Vector2 vel = dest - mParentGOH.pPosition;

                Single xDist = vel.X;

                vel.Normalize();

                Single speed = ((Single)RandomManager.pInstance.RandomPercent() * 3.0f) + 2.0f;

                Single time = Math.Abs(xDist / speed);

                Single timeHalf = time * 0.5f;

                Single yVel = timeHalf * 0.2f;

                vel.X = vel.X * speed;
                vel.Y = -yVel;
                */
                mParentGOH.pDirection.mForward = vel;

                mTimeOnGroundToEndPlay.Restart();
                mTimeOnGroundToEndPlay.pIsPaused = true;
            }
        }
    }
}
