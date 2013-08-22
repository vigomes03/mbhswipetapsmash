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
    /// Displays the number of Hits (both current and the record).
    /// </summary>
    class HitCountDisplay : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Increments the current hit count by 1.
        /// </summary>
        public class IncrementHitCountMessage : BehaviourMessage
        {
            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset() { }
        }

        /// <summary>
        /// Resets the hit count to 0.
        /// </summary>
        public class ClearHitCountMessage : BehaviourMessage
        {
            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset() { }
        }

        /// <summary>
        /// Access to the current hit count.
        /// </summary>
        public class GetCurrentHitCountMessage : BehaviourMessage
        {
            /// <summary>
            /// How many points has the player scored this game.
            /// </summary>
            public Int32 mCount_Out;

            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset()
            {
                mCount_Out = 0;
            }
        }

        /// <summary>
        /// A list of Objects, one for each number sprite in the hit count.
        /// </summary>
        private List<GameObject> mHitCounterNums;

        /// <summary>
        /// The number to display.
        /// </summary>
        private Int32 mHitCount;

        /// <summary>
        /// This behaviour is used for both the current hit count and the record, but with slightly
        /// different functionality. This Boolean tells us which logic to use.
        /// </summary>
        private Boolean mDisplayRecord;

        private Int32 mNumCharDisplay;

        private Int32 mMaxScore;

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

            mNumCharDisplay = 3;

            mDisplayRecord = def.mDisplayRecord;
            mHitCounterNums = new List<GameObject>(mNumCharDisplay);

            mHitCount = 0;

            mMaxScore = (Int32)(System.Math.Pow(10.0, (Double)mNumCharDisplay)) - 1;

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
            else if (!mDisplayRecord && msg is GetCurrentHitCountMessage)
            {
                GetCurrentHitCountMessage temp = (GetCurrentHitCountMessage)msg;
                temp.mCount_Out = mHitCount;
            }
            else if (msg is SaveGameManager.ForceUpdateSaveDataMessage)
            {
                LeaderBoardManager.pInstance.pTopHits = mHitCount;
            }
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // The high score constantly checks to see if the current score is the new
            // top score, and updates the display when needed.
            if (mDisplayRecord)
            {
                Int32 curBest = LeaderBoardManager.pInstance.pTopHits;

                // Has the current game beaten the top score?
                if (curBest > mHitCount)
                {
                    SetScore(curBest);
                }
            }
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void OnAdd()
        {
            // For each character in the score, we create an induvidual sprite.
            for (Int32 i = 0; i < mNumCharDisplay; i++)
            {
                GameObject g = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\NumFontUI\\NumFontUI");
                g.pPosition = mParentGOH.pPosition;
                g.pPosX += i * 8.0f;

                GameObjectManager.pInstance.Add(g);

                mHitCounterNums.Add(g);
            }

            // In the case of the Record, we need to set the score now, since it won't be 
            // set on the fly.
            if (mDisplayRecord)
            {
                SetScore(LeaderBoardManager.pInstance.pTopHits);
            }
        }

        /// <summary>
        /// Cleans up all the GameObjects created to display the score.
        /// </summary>
        private void CleanUpScore()
        {
            for (Int32 i = mHitCounterNums.Count - 1; i >= 0; i--)
            {
                GameObjectManager.pInstance.Remove(mHitCounterNums[i]);
            }

            mHitCounterNums.Clear();
        }

        /// <summary>
        /// Updates the score and display.
        /// </summary>
        /// <param name="score"></param>
        private void SetScore(Int32 score)
        {
            // Cap the score based on the number of characters we can display.
            if (score > mMaxScore)
            {
                score = mMaxScore;
            }

            mHitCount = score;

            // Start by setting every character to 0 so that if the score went down we won't
            // be left with numbers on the left side since it will only set numbers that are
            // significant.
            for (Int32 i = 0; i < mHitCounterNums.Count; i++)
            {
                mSetActiveAnimationMsg.mAnimationSetName_In = "0";
                GameObject go = mHitCounterNums[i];
                go.OnMessage(mSetActiveAnimationMsg, mParentGOH);
            }

            AddEachDigit(score, 0);
        }

        /// <summary>
        /// Recursive function for setting up each character of the score.
        /// </summary>
        /// <param name="score"></param>
        /// <param name="count"></param>
        private void AddEachDigit(Int32 score, Int32 count)
        {
            // Jump through each character of the score.
            if(score >= 10)
            {
               AddEachDigit(score / 10, count + 1);
            }

            // What is the digit at this power of 10.
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

            // Grab the corrisponding GameObject for this digit.
            GameObject go = mHitCounterNums[(mHitCounterNums.Count - 1) - count];
            go.OnMessage(mSetActiveAnimationMsg, mParentGOH);
        }
    }
}
