using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MBHEngine.GameObject;
using BumpSetSpike.Behaviour;

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Helper singleton for awarding points based on skill moves.
    /// </summary>
    public class ScoreManager
    {
        /// <summary>
        /// The different types of moves.
        /// </summary>
        public enum ScoreType
        {
            // Basics
            Spike = 0,
            Jump,
            Net,
            Kabooom,

            // Advanced
            FingerTips,     // Hit the ball at the top of the collision box.
            HighPoint,      // Hit the ball near it's peak.
            LowPoint,       // Hit the ball near it's lowest point.
            HangTime,       // Hit the ball after being in the air for a while.
            FadeAway,       // Hit the ball while moving backwards.
            Upwards,        // Hit the ball while it is still moving upwards.
            Speedy,         // Hit the ball while moving very fast.

            Count,
        }

        /// <summary>
        /// Singleton.
        /// </summary>
        private static ScoreManager mInstance;

        /// <summary>
        /// The current total score.
        /// </summary>
        private Int32 mTotalScore;

        /// <summary>
        /// Maps a type of move to a point value.
        /// </summary>
        private Dictionary<Int32, Int32> mScoreMapping;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private PointDisplay.SetScoreMessage mSetScoreMsg;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScoreManager()
        {
            mSetScoreMsg = new PointDisplay.SetScoreMessage();

            mScoreMapping = new Dictionary<Int32, Int32>
            {
                { (Int32)ScoreType.Spike,      10 },
                { (Int32)ScoreType.Jump,       5 },
                { (Int32)ScoreType.Net,        20 },
                { (Int32)ScoreType.Kabooom,    20 },

                { (Int32)ScoreType.FingerTips, 25 },
                { (Int32)ScoreType.HighPoint,  25 },
                { (Int32)ScoreType.LowPoint,   40 },
                { (Int32)ScoreType.HangTime,   100 },
                { (Int32)ScoreType.FadeAway,   50 },
                { (Int32)ScoreType.Upwards,    75 },
                { (Int32)ScoreType.Speedy,     75 },
            };

            System.Diagnostics.Debug.Assert(mScoreMapping.Count == (Int32)ScoreType.Count);
        }

        /// <summary>
        /// Call this before using the class for the first time.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Access to the singleton.
        /// </summary>
        public static ScoreManager pInstance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ScoreManager();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// When points are earned, they should be awarded through this function.
        /// </summary>
        /// <param name="score">How manay points are being awarded.</param>
        /// <param name="positionInWorld">Where the points should appear in world space.</param>
        private void AddScore(Int32 score, Vector2 positionInWorld)
        {
            if (GameModeManager.pInstance.pMode != GameModeManager.GameMode.TrickAttack)
            {
                return;
            }

            GameObject points = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PointDisplay\\PointDisplay");
            points.pPosition = positionInWorld;

            mSetScoreMsg.Reset();
            mSetScoreMsg.mScore_In = score;
            points.OnMessage(mSetScoreMsg);
             
            GameObjectManager.pInstance.Add(points);

            mTotalScore += score;

            HitCountDisplay.SetScoreMessage setHitCountMsg = new HitCountDisplay.SetScoreMessage();
            setHitCountMsg.mCount_In = mTotalScore;

            GameObjectManager.pInstance.BroadcastMessage(setHitCountMsg);
        }

        /// <summary>
        /// When points are earned, they should be awarded through this function.
        /// </summary>
        /// <param name="type">The type of move performed which should be awarded points for.</param>
        /// <param name="positionInWorld">Where the points should appear in world space.</param>
        public void AddScore(ScoreType type, Vector2 positionInWorld)
        {
            AddScore(mScoreMapping[(Int32)type], positionInWorld);
        }

        /// <summary>
        /// Call this when the match is reset so that the total score can be reset too.
        /// </summary>
        public void OnMatchOver()
        {
            mTotalScore = 0;
        }
    }
}