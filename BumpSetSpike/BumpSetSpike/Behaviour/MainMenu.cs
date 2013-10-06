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
using MBHEngine.Input;
using MBHEngineContentDefs;
using Microsoft.Xna.Framework.Audio;

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// Simple class for waiting for the user to tap the screen to start the game.
    /// </summary>
    class MainMenu : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Manages the state of the object.
        /// </summary>
        private enum State
        {
            OnTitle = 0,
            ModeSelect,
            MoveToCourt,
        }

        /// <summary>
        /// The current state of the object.
        /// </summary>
        private State mCurrentState;

        /// <summary>
        /// We don't have a great way of knowing when the camera has reached its desination, so
        /// instead we just start a timer that takes the same amount of time and wait for it to
        /// expire.
        /// </summary>
        private StopWatch mWatch;

        /// <summary>
        /// Used for grabbing the current gesture info.
        /// </summary>
        private GestureSample mGesture;

        /// <summary>
        /// The sound that plays when you user selects and item on the menu.
        /// </summary>
        private SoundEffect mFxMenuSelect;

        /// <summary>
        /// Displays the "Tap to Start" text.
        /// </summary>
        private GameObject mTapStart;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private Player.OnGameRestartMessage mGameRestartMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public MainMenu(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// Call this to initialize a Behaviour with data supplied in a file.
        /// </summary>
        /// <param name="fileName">The file to load from.</param>
        public override void LoadContent(String fileName)
        {
            base.LoadContent(fileName);

            mCurrentState = State.OnTitle;

            mWatch = StopWatchManager.pInstance.GetNewStopWatch();

            // Make the timer last the same amount of time it will take the camera to reach
            // its destination.
            mWatch.pLifeTime = CameraManager.pInstance.pNumBlendFrames;
            mWatch.pIsPaused = true;

            mGesture = new GestureSample();

            mFxMenuSelect = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\MenuSelect");

            Single x = ((GameObjectManager.pInstance.pGraphicsDevice.Viewport.Width * 0.5f) / CameraManager.pInstance.pZoomScale);
            Single y = ((GameObjectManager.pInstance.pGraphicsDevice.Viewport.Height * 0.5f) / CameraManager.pInstance.pZoomScale);

            mTapStart = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\TapStart\\TapStart");
            mTapStart.pPosX = x;
            mTapStart.pPosY = y + 12;
            GameObjectManager.pInstance.Add(mTapStart);

            mGameRestartMsg = new Player.OnGameRestartMessage();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override Boolean HandleUIInput()
        {
            // If we are in the main menu, start looking for button presses.
            // TODO: Move this to update passes.
            if (GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.MAIN_MENU ||
                GameObjectManager.pInstance.pCurUpdatePass == BehaviourDefinition.Passes.MAIN_MENU_MODE_SELECT)
            {
                if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture) || 
                    InputManager.pInstance.CheckAction(InputManager.InputActions.START, true))
                {
                    if (mCurrentState == State.ModeSelect)
                    {
                        // Move down to the gameplay camera position.
                        CameraManager.pInstance.pTargetPosition = new Vector2(0, -30.0f);
                        mCurrentState = State.MoveToCourt;

                        mFxMenuSelect.Play();

                        GameObjectManager.pInstance.Remove(mTapStart);

                        mWatch.pIsPaused = false;

                        return true;
                    }
                    else if (mCurrentState == State.OnTitle)
                    {
                        GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.MAIN_MENU_MODE_SELECT;
                        mCurrentState = State.ModeSelect;
                    }
                }

                // Once the timer expires the camera should be in place and the game can start.
                if (mCurrentState == State.MoveToCourt && mWatch.IsExpired())
                {
                    GameObjectManager.pInstance.BroadcastMessage(mGameRestartMsg, mParentGOH);
                    GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY;
                    //GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.TUTORIAL;

                    GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\ScoreLabel\\ScoreLabel"));
                    GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\HitCountDisplay\\HitCountDisplay"));

                    TutorialManager.pInstance.StartTutorial();

                    mWatch.Restart();
                    mWatch.pIsPaused = true;
                }
            }

            return false;
        }
    }
}
