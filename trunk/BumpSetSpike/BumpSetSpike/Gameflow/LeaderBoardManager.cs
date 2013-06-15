using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BumpSetSpike.Gameflow
{
    public class LeaderBoardManager
    {
        public static LeaderBoardManager mInstance;

        public struct Records
        {
            public Int32 mHits;
            public Int32 mScore;
        }

        private Records mRecords;

        public void Initialize()
        {
            mRecords = new Records();
        }

        public Records GetRecords()
        {
            return mRecords;
        }

        public void SetRecords(Records newRecords)
        {
            mRecords = newRecords;
        }

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

        public Int32 pTopScore
        {
            get
            {
                return mRecords.mScore;
            }
            set
            {
                if (value > mRecords.mScore)
                {
                    mRecords.mScore = value;
                }
            }
        }

        public Int32 pTopHits
        {
            get
            {
                return mRecords.mHits;
            }
            set
            {
                if (value > mRecords.mHits)
                {
                    mRecords.mHits = value;
                }
            }
        }
    }
}
