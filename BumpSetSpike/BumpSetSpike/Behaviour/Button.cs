using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;
using MBHEngine.Input;
using MBHEngine.GameObject;
using Microsoft.Xna.Framework;
using MBHEngine.Render;
using BumpSetSpikeContentDefs;
using MBHEngineContentDefs;
using BumpSetSpike.Gameflow;
using System.Diagnostics;
using MBHEngine.Behaviour;
using Microsoft.Xna.Framework.GamerServices;
using MBHEngine.Trial;
#if __ANDROID__
using Android.Content;
#endif
#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
#endif

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// Simple UI button that can be clicked.
    /// </summary>
    class Button : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// When a button is pressed, it can optionally send out one of these messages, allowing clients
        /// to react to it.
        /// </summary>
        public class OnButtonPressedMessage : BehaviourMessage
        {
            /// <summary>
            /// Did anyone react to and handle the event.
            /// </summary>
            public Boolean mHandled_Out;

            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset()
            {
                mHandled_Out = false;
            }
        }

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private GestureSample mGesture;

        /// <summary>
        /// Hang onto the definition so that we don't need to copy over all the task information.
        /// </summary>
        private BumpSetSpikeContentDefs.ButtonDefinition mDef;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private OnButtonPressedMessage mOnButtonPressedMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public Button(GameObject parentGOH, String fileName)
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

            mDef = GameObjectManager.pInstance.pContentManager.Load<BumpSetSpikeContentDefs.ButtonDefinition>(fileName);

            mGesture = new GestureSample();
            mOnButtonPressedMsg = new OnButtonPressedMessage();

            Reset();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void Reset()
        {
            // Special handling for this type of button.
            if (mDef.mTaskOnRelease.mType == BumpSetSpikeContentDefs.ButtonDefinition.TaskType.OptionToggleTutorial)
            {
                // Turn the button visual on or off depending on the state of the button.
                if (TutorialManager.pInstance.pTutorialCompleted)
                {
                    mParentGOH.pDoRender = false;
                }
                else
                {
                    mParentGOH.pDoRender = true;
                }
            }
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override Boolean HandleUIInput()
        {
            if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
            {
                // The position is in screen space, but our screen is scaled up so we need to convert.
                // Assuming that all buttons will be in UI scale.
                Vector2 scaledPos = mGesture.Position / CameraManager.pInstance.pDefaultZoomScale;

                // Did they tap on this object?
                if (mParentGOH.pCollisionRect.Intersects(scaledPos))
                {
                    BumpSetSpikeContentDefs.ButtonDefinition.Task task = mDef.mTaskOnRelease;

                    // What task should we do now?
                    switch (task.mType)
                    {
                        case BumpSetSpikeContentDefs.ButtonDefinition.TaskType.OpenURL:
                        {
#if WINDOWS_PHONE
                            WebBrowserTask browser = new WebBrowserTask();
                            browser.Uri = new Uri(task.mData, UriKind.Absolute);
                            browser.Show();

                            return true;
#elif __ANDROID__
                            var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(task.mData));
                            Game1.Activity.StartActivity(intent);
                            return true;
#else
                            Process.Start(task.mData);
                            return true;
#endif
                        }

                        case BumpSetSpikeContentDefs.ButtonDefinition.TaskType.PauseGame:
                        {
                            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY_PAUSED;
                            GameObjectManager.pInstance.Add(GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\FSMPauseScreen\\FSMPauseScreen"));
                            return true;
                        }

                        case BumpSetSpikeContentDefs.ButtonDefinition.TaskType.ShowCredits:
                        {
                            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.CREDITS;
                            return true;
                        }

                        case BumpSetSpikeContentDefs.ButtonDefinition.TaskType.LeaveCredits:
                        {
                            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.MAIN_MENU;
                            return true;
                        }

                        case BumpSetSpikeContentDefs.ButtonDefinition.TaskType.OptionToggleTutorial:
                        {
                            // Toggle the state of the tutorial.
                            TutorialManager.pInstance.pTutorialCompleted ^= true;

                            if (TutorialManager.pInstance.pTutorialCompleted)
                            {
                                mParentGOH.pDoRender = false;
                            }
                            else
                            {
                                mParentGOH.pDoRender = true;
                            }

                            return true;
                        }

                        case BumpSetSpikeContentDefs.ButtonDefinition.TaskType.SetGameModeEndurance:
                        {
                            GameModeManager.pInstance.pMode = GameModeManager.GameMode.Endurance;

                            // Let main menu get this input too.
                            return false;
                        }

                        case BumpSetSpikeContentDefs.ButtonDefinition.TaskType.SetGameModeTrickAttack:
                        {
                            GameModeManager.pInstance.pMode = GameModeManager.GameMode.TrickAttack;

                            // Let main menu get this input too.
                            return false;
                        }

                        case BumpSetSpikeContentDefs.ButtonDefinition.TaskType.SendMessage:
                        {
                            mOnButtonPressedMsg.Reset();

                            GameObjectManager.pInstance.BroadcastMessage(mOnButtonPressedMsg, mParentGOH);

                            return mOnButtonPressedMsg.mHandled_Out;
                        }

                        case BumpSetSpikeContentDefs.ButtonDefinition.TaskType.OpenMarketplace:
                        {
#if WINDOWS_PHONE
                            Guide.ShowMarketplace(PlayerIndex.One);
#else
                            TrialModeManager.pInstance.pIsTrialMode = false;
#endif
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="msg"></param>
        public override void OnMessage(ref BehaviourMessage msg)
        {
            // Some things need to reset when the game is restarted.
            if (msg is HitCountDisplay.ResetGameMessage)
            {
                Reset();
            }
        }
    }
}
