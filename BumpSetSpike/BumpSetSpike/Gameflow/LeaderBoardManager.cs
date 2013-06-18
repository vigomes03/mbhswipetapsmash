using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Singleton for tracking highscores in a global way.
    /// </summary>
    public class LeaderBoardManager
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static LeaderBoardManager mInstance;

        /// <summary>
        /// Stores all the highscores for easy save/load.
        /// </summary>
        public struct Records
        {
            public Int32 mHits;
            public Int32 mScore;
        }

        /// <summary>
        /// The current top scores.
        /// </summary>
        private Records mRecords;

        /// <summary>
        /// Call this before using the singleton.
        /// </summary>
        public void Initialize()
        {
            mRecords = new Records();
        }

        /// <summary>
        /// Access to the current records.
        /// </summary>
        /// <returns></returns>
        public Records GetRecords()
        {
            return mRecords;
        }

        /// <summary>
        /// Updates the current records.
        /// </summary>
        /// <param name="newRecords">The updated records.</param>
        public void SetRecords(Records newRecords)
        {
            mRecords = newRecords;
        }

        /// <summary>
        /// Access to the singleton.
        /// </summary>
        public static LeaderBoardManager pInstance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new LeaderBoardManager();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// The current top score.
        /// </summary>
        public Int32 pTopScore
        {
            get
            {
                return mRecords.mScore;
            }
            set
            {
                // Allow this property to be spammed, and only the best will be used.
                if (value > mRecords.mScore)
                {
                    mRecords.mScore = value;
                }
            }
        }

        /// <summary>
        /// The current highest number of hits.
        /// </summary>
        public Int32 pTopHits
        {
            get
            {
                return mRecords.mHits;
            }
            set
            {
                // Allow this property to be spammed, and only the best will be used.
                if (value > mRecords.mHits)
                {
                    mRecords.mHits = value;
                }
            }
        }
    }
}
