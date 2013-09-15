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
    /// Simple class fordisplaying the game over screen.
    /// </summary>
    class GameOver : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private GestureSample mGesture;

        /// <summary>
        /// The sound that plays when you user selects and item on the menu.
        /// </summary>
        private SoundEffect mFxMenuSelect;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private Player.OnGameRestartMessage mGameRestartMsg;
        private Player.GetCurrentStateMessage mGetCurrentStateMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public GameOver(GameObject parentGOH, String fileName)
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

            mGesture = new GestureSample();

            mFxMenuSelect = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\MenuSelect");

            mGameRestartMsg = new Player.OnGameRestartMessage();
            mGetCurrentStateMsg = new Player.GetCurrentStateMessage();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
            {

                mGetCurrentStateMsg.Reset();
                GameObjectManager.pInstance.pPlayer.OnMessage(mGetCurrentStateMsg, mParentGOH);

				// Don't leave game over until the player is on the ground.
                if (mGetCurrentStateMsg.mState_Out == Player.State.Idle)
                {
                    mFxMenuSelect.Play();

                    // Restart the game
                    GameObjectManager.pInstance.BroadcastMessage(mGameRestartMsg, mParentGOH);
                    GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY;
                }

                //TutorialManager.pInstance.StartTutorial();
            }
        }
    }
}
