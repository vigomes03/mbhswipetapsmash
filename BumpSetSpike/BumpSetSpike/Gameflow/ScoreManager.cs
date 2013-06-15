using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MBHEngine.GameObject;
using BumpSetSpike.Behaviour;

namespace BumpSetSpike.Gameflow
{
    public class ScoreManager
    {
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

        private static ScoreManager mInstance;

        private Int32 mTotalScore;

        private PointDisplay.SetScoreMessage mSetScoreMsg;

        private Dictionary<ScoreType, Int32> mScoreMapping;

        public ScoreManager()
        {
            mSetScoreMsg = new PointDisplay.SetScoreMessage();

            mScoreMapping = new Dictionary<ScoreType,int>
            {
                { ScoreType.Spike,      10 },
                { ScoreType.Jump,       5 },
                { ScoreType.Net,        20 },
                { ScoreType.Kabooom,    20 },

                { ScoreType.FingerTips, 25 },
                { ScoreType.HighPoint,  25 },
                { ScoreType.LowPoint,   40 },
                { ScoreType.HangTime,   100 },
                { ScoreType.FadeAway,   50 },
                { ScoreType.Upwards,    75 },
                { ScoreType.Speedy,     75 },
            };

            System.Diagnostics.Debug.Assert(mScoreMapping.Count == (Int32)ScoreType.Count);
        }

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

        private void AddScore(Int32 score, Vector2 positionInWorld)
        {
            GameObject points = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PointDisplay\\PointDisplay");
            points.pPosition = positionInWorld;

            mSetScoreMsg.Reset();
            mSetScoreMsg.mScore_In = score;
            points.OnMessage(mSetScoreMsg);

            // TODO: Bring this back once we have a proper score attack mode.
            //GameObjectManager.pInstance.Add(points);

            mTotalScore = score;
        }

        public void AddScore(ScoreType type, Vector2 positionInWorld)
        {
            AddScore(mScoreMapping[type], positionInWorld);
        }

        public void OnPointEarned()
        {
            LeaderBoardManager.pInstance.pTopScore = mTotalScore;
        }

        public void OnMatchOver()
        {
            mTotalScore = 0;
        }
    }
}
