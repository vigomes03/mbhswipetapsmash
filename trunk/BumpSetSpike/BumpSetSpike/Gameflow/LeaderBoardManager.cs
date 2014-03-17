using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if __ANDROID__
using BumpSetSpike_Android;
#endif // __ANDROID__

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
        /// Sets the top score for the currently active game mode. The caller doesn't need to know what mode
        /// they are actually in.
        /// Will only set the score if it is greater than the current high score.
        /// </summary>
        /// <param name="score">The new high score.</param>
        public void SetCurrentModeTopScore(Int32 score)
        {
            if (GameModeManager.pInstance.pMode == GameModeManager.GameMode.Endurance)
            {
                pTopHits = score;
            }
            else if (GameModeManager.pInstance.pMode == GameModeManager.GameMode.TrickAttack)
            {
                pTopScore = score;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Unknown game mode.");
            }
        }

        /// <summary>
        /// Retrieves the current highscore for the current game mode. The called need not know which
        /// game mode is currently active.
        /// </summary>
        /// <returns></returns>
        public Int32 GetCurrentModeTopScore()
        {
            if (GameModeManager.pInstance.pMode == GameModeManager.GameMode.Endurance)
            {
                return pTopHits;
            }
            else if (GameModeManager.pInstance.pMode == GameModeManager.GameMode.TrickAttack)
            {
                return pTopScore;
            }
            else if (GameModeManager.pInstance.pMode == GameModeManager.GameMode.None)
            {
                return 0;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Unknown game mode.");

                return -1;
            }
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
                if (value >= 500)
                {
                    AchievementManager.pInstance.UnlockAchievement(AchievementManager.Achievements.Fortune500);
                }

                // Allow this property to be spammed, and only the best will be used.
                if (value > mRecords.mScore)
                {
                    mRecords.mScore = value;
                    SaveGameManager.pInstance.WriteSaveGameXML();

#if __ANDROID__
                    BumpSetSpike_Android.Activity1 activity = (Game1.Activity as BumpSetSpike_Android.Activity1);
                    if(activity.pGooglePlayClient.IsConnected)
                    {
                        activity.pGooglePlayClient.SubmitScoreImmediate(activity, activity.Resources.GetString(Resource.String.leaderboard_trick_attack), mRecords.mScore);
                    }
#endif // __ANDROID__
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
                if (value >= 7)
                {
                    AchievementManager.pInstance.UnlockAchievement(AchievementManager.Achievements.Lucky_7);
                }

                // Allow this property to be spammed, and only the best will be used.
                if (value > mRecords.mHits)
                {
                    mRecords.mHits = value;
                    SaveGameManager.pInstance.WriteSaveGameXML();
#if __ANDROID__
                    BumpSetSpike_Android.Activity1 activity = (Game1.Activity as BumpSetSpike_Android.Activity1);
                    if(activity.pGooglePlayClient.IsConnected)
                    {
                        activity.pGooglePlayClient.SubmitScoreImmediate(activity, activity.Resources.GetString(Resource.String.leaderboard_endurnace), mRecords.mHits);
                    }
#endif // __ANDROID__
                }
            }
        }
    }
}
