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
    class PointDisplay : MBHEngine.Behaviour.Behaviour
    {
        public class SetScoreMessage : BehaviourMessage
        {
            public Int32 mScore_In;

            public override void Reset()
            {
                mScore_In = 0;
            }
        }

        private StopWatch mDisplayWatch;

        private List<GameObject> mScoreNums;

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

        public override void Update(GameTime gameTime)
        {
            mParentGOH.pPosY -= 0.1f;

            if (mDisplayWatch.IsExpired())
            {
                GameObjectManager.pInstance.Remove(mParentGOH);
                return;
            }

            UpdateNumberPositions();
        }

        public override void Reset()
        {
            CleanUpScore();
        }

        public override void OnAdd()
        {
            mDisplayWatch = StopWatchManager.pInstance.GetNewStopWatch();

            mDisplayWatch.pLifeTime = 30.0f;
        }

        public override void OnRemove()
        {
            CleanUpScore();

            if (null == mDisplayWatch)
            {
                StopWatchManager.pInstance.RecycleStopWatch(mDisplayWatch);
                mDisplayWatch = null;
            }
        }

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

        private void CleanUpScore()
        {
            for (Int32 i = mScoreNums.Count - 1; i >= 0; i--)
            {
                GameObjectManager.pInstance.Remove(mScoreNums[i]);
            }

            mScoreNums.Clear();
        }

        private void SetScore(Int32 score)
        {
            AddEachDigit(score, 0);

            UpdateNumberPositions();
        }

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
