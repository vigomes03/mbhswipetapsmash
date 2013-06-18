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

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// Used for showing the points the player earns, in the world and at the position they were earned.
    /// For example, a spike point might be shown at the point of impact with the hand and ball.
    /// </summary>
    class PointDisplay : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Sets which point value to display.
        /// </summary>
        public class SetScoreMessage : BehaviourMessage
        {
            /// <summary>
            /// Point value to display.
            /// </summary>
            public Int32 mScore_In;

            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset()
            {
                mScore_In = 0;
            }
        }

        /// <summary>
        /// How long to show the points for before vanishing.
        /// </summary>
        private StopWatch mDisplayWatch;

        /// <summary>
        /// For each digit displayed, we have a different number sprite.
        /// </summary>
        private List<GameObject> mScoreNums;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public PointDisplay(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// Call this to initialize a Behaviour with data supplied in a file.
        /// </summary>
        /// <param name="fileName">The file to load from.</param>
        public override void LoadContent(String fileName)
        {
            PointDisplayDefinition def = GameObjectManager.pInstance.pContentManager.Load<PointDisplayDefinition>(fileName);

            base.LoadContent(fileName);

            mScoreNums = new List<GameObject>(16);

            mSetActiveAnimationMsg = new SpriteRender.SetActiveAnimationMessage();
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
            if (msg is SetScoreMessage)
            {
                SetScoreMessage temp = (SetScoreMessage)msg;

                SetScore(temp.mScore_In);
            }
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Float up a little bit every frame.
            mParentGOH.pPosY -= 0.1f;

            // Once enough time has passed, this guy can vanish.
            if (mDisplayWatch.IsExpired())
            {
                GameObjectManager.pInstance.Remove(mParentGOH);
                return;
            }

            // If we are still alive, update all the sprites positions based on our
            // position. Remeber that this object is just a container. 
            UpdateNumberPositions();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void Reset()
        {
            CleanUpScore();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void OnAdd()
        {
            mDisplayWatch = StopWatchManager.pInstance.GetNewStopWatch();

            // These numbers vanish in 1 second.
            mDisplayWatch.pLifeTime = 30.0f;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void OnRemove()
        {
            CleanUpScore();

            if (null == mDisplayWatch)
            {
                StopWatchManager.pInstance.RecycleStopWatch(mDisplayWatch);
                mDisplayWatch = null;
            }
        }

        /// <summary>
        /// The numbers are like children of this object, so this function needs to
        /// update all the positions to move with us.
        /// </summary>
        private void UpdateNumberPositions()
        {
            Int32 count = mScoreNums.Count;
            Single offset = 8.0f;

            Single startX = mParentGOH.pPosX - (count * 0.5f * offset);

            for (Int32 i = mScoreNums.Count - 1; i >= 0; i--)
            {
                mScoreNums[i].pPosition = mParentGOH.pPosition;
                mScoreNums[i].pPosX = startX + (i * offset);
            }
        }

        /// <summary>
        /// Free up all the objects we added to the GameObjectManager.
        /// </summary>
        private void CleanUpScore()
        {
            for (Int32 i = mScoreNums.Count - 1; i >= 0; i--)
            {
                GameObjectManager.pInstance.Remove(mScoreNums[i]);
            }

            mScoreNums.Clear();
        }

        /// <summary>
        /// Update the current score.
        /// </summary>
        /// <param name="score"></param>
        private void SetScore(Int32 score)
        {
            AddEachDigit(score, 0);

            UpdateNumberPositions();
        }

        /// <summary>
        /// Recursive function to set all the digits in the score.
        /// </summary>
        /// <param name="score"></param>
        /// <param name="count"></param>
        private void AddEachDigit(Int32 score, Int32 count)
        {
            if(score >= 10)
            {
               AddEachDigit(score / 10, count + 1);
            }

            Int32 digit = score % 10;

            switch (digit)
            {
                case 0:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "0";
                    break;
                }
                case 1:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "1";
                    break;
                }
                case 2:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "2";
                    break;
                }
                case 3:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "3";
                    break;
                }
                case 4:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "4";
                    break;
                }
                case 5:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "5";
                    break;
                }
                case 6:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "6";
                    break;
                }
                case 7:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "7";
                    break;
                }
                case 8:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "8";
                    break;
                }
                case 9:
                {
                    mSetActiveAnimationMsg.mAnimationSetName_In = "9";
                    break;
                }
            }

            // TODO: Bring back
            //GameObject go = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\NumFont\\NumFont");
            //go.OnMessage(mSetActiveAnimationMsg, mParentGOH);
            //mScoreNums.Add(go);
            //GameObjectManager.pInstance.Add(go);
        }
    }
}
