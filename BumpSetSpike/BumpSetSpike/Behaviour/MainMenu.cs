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

namespace BumpSetSpike.Behaviour
{
    class MainMenu : MBHEngine.Behaviour.Behaviour
    {
        private enum State
        {
            OnTitle = 0,
            MoveToCourt,
        }

        private State mCurrentState;

        private StopWatch mWatch;

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

            mWatch.pLifeTime = CameraManager.pInstance.pNumBlendFrames;
            mWatch.pIsPaused = true;

            mGameRestartMsg = new Player.OnGameRestartMessage();
        }

        public override void Update(GameTime gameTime)
        {

            if (GameflowManager.pInstance.pState == GameflowManager.State.MainMenu)
            {
                GestureSample gesture = new GestureSample();

                if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref gesture) || InputManager.pInstance.CheckAction(InputManager.InputActions.A, true))
                {
                    if (mCurrentState == State.OnTitle)
                    {
                        CameraManager.pInstance.pTargetPosition = new Vector2(0, -30.0f);
                        mCurrentState = State.MoveToCourt;

                        mWatch.pIsPaused = false;
                    }
                }

                if (mCurrentState == State.MoveToCourt && mWatch.IsExpired())
                {
                    GameObjectManager.pInstance.BroadcastMessage(mGameRestartMsg, mParentGOH);
                    GameflowManager.pInstance.pState = GameflowManager.State.GamePlay;

                    mWatch.Restart();
                    mWatch.pIsPaused = true;
                }
            }
        }

        /// <summary>
        /// The main interface for communicating between behaviours.  Using polymorphism, we
        /// define a bunch of different messages deriving from BehaviourMessage.  Each behaviour
        /// can then check for particular upcasted messahe types, and either grab some data 
        /// from it (set message) or store some data in it (get message).
        /// </summary>
        /// <param name="msg">The message being communicated to the behaviour.</param>
        public override void OnMessage(ref BehaviourMessage msg)
        {

        }
    }
}
