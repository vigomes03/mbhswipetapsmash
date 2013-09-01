using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using MBHEngine.GameObject;
using MBHEngine.IO;
using MBHEngineContentDefs;

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Helper class for managing the playback of music as the user progresses through the 
    /// application.
    /// </summary>
    public class MusicManager
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
            mMainMenuMusic = GameObjectManager.pInstance.pContentManager.Load<Song>("Audio\\Music\\MikeImmonen_Menu");
            mGameplayMusic = GameObjectManager.pInstance.pContentManager.Load<Song>("Audio\\Music\\MikeImmonen_Gameplay");

            // All music in the game repeats.
            MediaPlayer.IsRepeating = true;
        }

        /// <summary>
        /// Call this every frame.
        /// </summary>
        public void Update()
        {
            Boolean musicDisabled = CommandLineManager.pInstance["DisableMusic"] != null;

#if WINDOWS_PHONE && DEBUG
            musicDisabled = true;
#endif
            if (musicDisabled)
            {
                return;
            }

            // If the user is playing their own music, we don't want to override that.
            if (!MediaPlayer.GameHasControl)
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
    }
}
