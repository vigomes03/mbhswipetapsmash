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
    class HitCountDisplay : MBHEngine.Behaviour.Behaviour
    {
        public class IncrementHitCountMessage : BehaviourMessage
        {
            public override void Reset()
            {
                
            }
        }

        public class ClearHitCountMessage : BehaviourMessage
        {
            public override void Reset()
            {
                
            }
        }

        private List<GameObject> mHitCounterNums;

        private Int32 mHitCount;

        private Boolean mDisplayRecord;

        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public HitCountDisplay(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// Call this to initialize a Behaviour with data supplied in a file.
        /// </summary>
        /// <param name="fileName">The file to load from.</param>
        public override void LoadContent(String fileName)
        {
            HitCountDisplayDefinition def = GameObjectManager.pInstance.pContentManager.Load<HitCountDisplayDefinition>(fileName);

            base.LoadContent(fileName);

            mDisplayRecord = def.mDisplayRecord;

            mHitCounterNums = new List<GameObject>(3);

            mHitCount = 0;

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
            if (!mDisplayRecord && msg is IncrementHitCountMessage)
            {
                SetScore(mHitCount + 1);
            }
            else if (!mDisplayRecord && (msg is ClearHitCountMessage || msg is Player.OnGameRestartMessage))
            {
                LeaderBoardManager.pInstance.pTopHits = mHitCount;
                ScoreManager.pInstance.OnMatchOver();

                SetScore(0);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (mDisplayRecord)
            {
                Int32 curBest = LeaderBoardManager.pInstance.pTopHits;

                if (curBest > mHitCount)
                {
                    SetScore(curBest);
                }
            }
        }

        public override void OnAdd()
        {
            for (Int32 i = 0; i < 3; i++)
            {
                GameObject g = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\NumFontUI\\NumFontUI");
                g.pPosition = mParentGOH.pPosition;
                g.pPosX += i * 8.0f;

                GameObjectManager.pInstance.Add(g);

                mHitCounterNums.Add(g);
            }

            if (mDisplayRecord)
            {
                SetScore(LeaderBoardManager.pInstance.pTopHits);
            }
        }

        private void CleanUpScore()
        {
            for (Int32 i = mHitCounterNums.Count - 1; i >= 0; i--)
            {
                GameObjectManager.pInstance.Remove(mHitCounterNums[i]);
            }

            mHitCounterNums.Clear();
        }

        private void SetScore(Int32 score)
        {
            if (score > 999)
            {
                score = 999;
            }

            mHitCount = score;

            for (Int32 i = 0; i < 3; i++)
            {
                mSetActiveAnimationMsg.mAnimationSetName_In = "0";
                GameObject go = mHitCounterNums[i];
                go.OnMessage(mSetActiveAnimationMsg, mParentGOH);
            }

            AddEachDigit(score, 0);
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

            GameObject go = mHitCounterNums[2 - count];
            go.OnMessage(mSetActiveAnimationMsg, mParentGOH);
        }
    }
}
