using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.Debug;

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Platform agnostic wrapper for achievement functionality.
    /// </summary>
    public class AchievementManager
    {
        public enum Achievements
        {
            TheUbi,             // Make it passed the Main Menu. Not for the faint of heart!
            Participation,      // Complete the Tutorial (Endurance Mode).
            DoubleTrouble,      // Knock down 2 opponents with 1 hit. "KAPOOOW!!"
            Fortune500,         // Get 500+ points in Trick Attack mode.
            Lucky_7,            // Get 7+ points in Endurance Attack mode.
            BendTheRules,       // Complete a play after the ball hits the net.

            Count,
        }

        /// <summary>
        /// Wraps up all the data about a single Achievement
        /// </summary>
        public struct AchievementData
        {
            /// <summary>
            /// The id used for looking up this Achievement.
            /// </summary>
            public Int32 mResourceId;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="resourceId">Resource identifier.</param>
            public AchievementData(Int32 resourceId)
            {
                mResourceId = resourceId;
            }
        }

        /// <summary>
        /// Single static instance of this class.
        /// </summary>
        private static AchievementManager mInstance;

        /// <summary>
        /// Maps an Achiements enum to actual achievement data.
        /// </summary>
        private Dictionary<Int32, AchievementData> mAchievementMapping;

        /// <summary>
        /// Call this before using the singleton.
        /// </summary>
        public void Initialize()
        {
            mAchievementMapping = new Dictionary<Int32, AchievementData>
            {
#if __ANDROID__
                { (Int32)Achievements.TheUbi,           new AchievementData(Resource.String.achievement_the_ubi) }, 
                { (Int32)Achievements.Participation,    new AchievementData(Resource.String.achievement_participation) }, 
                { (Int32)Achievements.DoubleTrouble,    new AchievementData(Resource.String.achievement_double_trouble) }, 
                { (Int32)Achievements.Fortune500,       new AchievementData(Resource.String.achievement_fortune_500) }, 
                { (Int32)Achievements.Lucky_7,          new AchievementData(Resource.String.achievement_lucky_7) }, 
                { (Int32)Achievements.BendTheRules,     new AchievementData(Resource.String.achievement_bend_the_rules) }, 
#endif // __ANDROID__
            };
            
#if __ANDROID__
            System.Diagnostics.Debug.Assert(mAchievementMapping.Count == (Int32)Achievements.Count);
#endif // __ANDROID__
        }

        /// <summary>
        /// Platform agnostic way for unlocking achievements. Can be called repeatedly.
        /// </summary>
        /// <param name="ach">The achievement to unlock.</param>
        public void UnlockAchievement(Achievements ach)
        {
#if __ANDROID__
            BumpSetSpike_Android.Activity1 activity = Game1.Activity as BumpSetSpike_Android.Activity1;
            activity.pGooglePlayClient.UnlockAchievement(activity.Resources.GetString(mAchievementMapping[(Int32)ach].mResourceId));
#endif //__ANDROID__
            DebugMessageDisplay.pInstance.AddConstantMessage("Unlocked: " + ach.ToString());
        }

        /// <summary>
        /// Access to the singleton.
        /// </summary>
        public static AchievementManager pInstance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new AchievementManager();
                }

                return mInstance;
            }
        }
    }
}
