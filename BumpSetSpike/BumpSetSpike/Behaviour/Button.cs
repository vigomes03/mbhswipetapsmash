using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;
using MBHEngine.Input;
using MBHEngine.GameObject;
using Microsoft.Xna.Framework;
#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
#endif
using MBHEngine.Render;
using BumpSetSpikeContentDefs;
using MBHEngineContentDefs;
using BumpSetSpike.Gameflow;
using System.Diagnostics;

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// Simple UI button that can be clicked.
    /// </summary>
    class Button : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private GestureSample mGesture;

        /// <summary>
        /// Hang onto the definition so that we don't need to copy over all the task information.
        /// </summary>
        private ButtonDefinition mDef;

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

            mDef = GameObjectManager.pInstance.pContentManager.Load<ButtonDefinition>(fileName);

            mGesture = new GestureSample();

            // Special handling for this type of button.
            if (mDef.mTaskOnRelease.mType == ButtonDefinition.TaskType.OptionToggleTutorial)
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
                Vector2 scaledPos = mGesture.Position / CameraManager.pInstance.pZoomScale;

                // Did they tap on this object?
                if (mParentGOH.pCollisionRect.Intersects(scaledPos))
                {
                    ButtonDefinition.Task task = mDef.mTaskOnRelease;

                    // What task should we do now?
                    switch (task.mType)
                    {
                        case ButtonDefinition.TaskType.OpenURL:
                        {
#if WINDOWS_PHONE
                            WebBrowserTask browser = new WebBrowserTask();
                            browser.Uri = new Uri(task.mData, UriKind.Absolute);
                            browser.Show();

                            return true;
#else
                            Process.Start(task.mData);
                            return true;
#endif
                        }

                        case ButtonDefinition.TaskType.PauseGame:
                        {
                            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY_PAUSED;
                            return true;
                        }

                        case ButtonDefinition.TaskType.ResumeGame:
                        {
                            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY;
                            return true;
                        }

                        case ButtonDefinition.TaskType.ShowCredits:
                        {
                            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.CREDITS;
                            return true;
                        }

                        case ButtonDefinition.TaskType.LeaveCredits:
                        {
                            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.MAIN_MENU;
                            return true;
                        }

                        case ButtonDefinition.TaskType.OptionToggleTutorial:
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
                    }
                }
            }

            return false;
        }
    }
}
