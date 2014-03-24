using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using MBHEngine.GameObject;
using MBHEngine.IO;
using MBHEngineContentDefs;
#if __ANDROID__
using Android.Content;
#endif // __ANDROID__

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Helper class for managing the playback of music as the user progresses through the 
    /// application.
    /// </summary>
    public class MusicManager : GameObjectManager.IUpdatePassChangeReciever
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static MusicManager mInstance;

        /// <summary>
        /// Music played during gameplay.
        /// </summary>
        private Song mGameplayMusic;

        /// <summary>
        /// Music played during the main menu.
        /// </summary>
        private Song mMainMenuMusic;

        /// <summary>
        /// Cache the command line arg so we don't have to do string compare over and over.
        /// </summary>
        private Boolean mDebugMusicDisabled;

        /// <summary>
        /// Allows us to manually disable music. Useful for platform specfic functionality.
        /// </summary>
        private Boolean mManualMusicDisabled;

        /// <summary>
        /// Access to the singleton.
        /// </summary>
        public static MusicManager pInstance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new MusicManager();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// Call this before using the manager.
        /// </summary>
        public void Initialize()
        {
#if MONOGL && WINDOWS
            //mMainMenuMusic = GameObjectManager.pInstance.pContentManager.Load<Song>("Audio\\Music\\MikeImmonen_Menu_Wave");
            //mGameplayMusic = GameObjectManager.pInstance.pContentManager.Load<Song>("Audio\\Music\\MikeImmonen_Gameplay_Wave");
#else
            mMainMenuMusic = GameObjectManager.pInstance.pContentManager.Load<Song>("Audio\\Music\\MikeImmonen_Menu");
            mGameplayMusic = GameObjectManager.pInstance.pContentManager.Load<Song>("Audio\\Music\\MikeImmonen_Gameplay");
#endif

            // All music in the game repeats.
            MediaPlayer.IsRepeating = true;

            mManualMusicDisabled = false;

#if (WINDOWS_PHONE && DEBUG) || (MONOGL && WINDOWS) || (__ANDROID__ && DEBUG)
            mDebugMusicDisabled = true; // We can't pass command line args to WP.
#else
            mDebugMusicDisabled = CommandLineManager.pInstance["DisableMusic"] != null;
#endif

            GameObjectManager.pInstance.RegisterUpdatePassChangeReceiver(this);

            ChangeMusic();
        }

        /// <summary>
        /// Implementation of Interface.
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="oldState"></param>
        public override void OnStateChange(BehaviourDefinition.Passes newState, BehaviourDefinition.Passes oldState)
        {
            // The state has changed so we may need to change the music. We are so careful about this, because
            // checking the state of the MediaPlayer seems to be very expensive.
            ChangeMusic();
        }

        /// <summary>
        /// Call this when you want to evaluate the current state of the game and potentially change
        /// the music based on the current state.
        /// </summary>
        public void ChangeMusic()
        {
            bool AudioManagerDisabledAudio = false;

            if (mDebugMusicDisabled || !MediaPlayer.GameHasControl || mManualMusicDisabled)
            {
                return;
            }

            if (mMainMenuMusic == null || mGameplayMusic == null)
            {
                return;
            }

            // Based on the current game state, we play different music.
            switch (GameObjectManager.pInstance.pCurUpdatePass)
            {
                case MBHEngineContentDefs.BehaviourDefinition.Passes.MAIN_MENU:
                {
                    if (Microsoft.Xna.Framework.Media.MediaPlayer.Queue == null ||
                        Microsoft.Xna.Framework.Media.MediaPlayer.Queue.ActiveSong == null ||
                        Microsoft.Xna.Framework.Media.MediaPlayer.Queue.ActiveSong.Name != mMainMenuMusic.Name)
                    {
                        MediaPlayer.Play(mMainMenuMusic);
                    }

                    const Single vol = 1.0f;
                    if (MediaPlayer.Volume != vol)
                    {
                        MediaPlayer.Volume = vol;
                    }

                    break;
                }
                case MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY:
                {
                    if (Microsoft.Xna.Framework.Media.MediaPlayer.Queue == null ||
                        Microsoft.Xna.Framework.Media.MediaPlayer.Queue.ActiveSong == null ||
                        Microsoft.Xna.Framework.Media.MediaPlayer.Queue.ActiveSong.Name != mGameplayMusic.Name)
                    {
                        MediaPlayer.Play(mGameplayMusic);
                    }

                    const Single vol = 1.0f;
                    if (MediaPlayer.Volume != vol)
                    {
                        MediaPlayer.Volume = vol;
                    }

                    break;
                }
                case MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY_PAUSED:
                {
                    if (Microsoft.Xna.Framework.Media.MediaPlayer.Queue == null ||
                        Microsoft.Xna.Framework.Media.MediaPlayer.Queue.ActiveSong == null ||
                        Microsoft.Xna.Framework.Media.MediaPlayer.Queue.ActiveSong.Name != mGameplayMusic.Name)
                    {
                        MediaPlayer.Play(mGameplayMusic);
                    }

                    const Single vol = 0.2f;
                    if (MediaPlayer.Volume != vol)
                    {
                        MediaPlayer.Volume = vol;
                    }

                    break;
                }
            }
        }

        public Boolean pManualMusicDisabled
        {
            get
            {
                return mManualMusicDisabled;
            }
            set
            {
                mManualMusicDisabled = value;
            }
        }
    }
}
