using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngineContentDefs;
using Microsoft.Xna.Framework.Content;

namespace BumpSetSpikeContentDefs
{
    public class ButtonDefinition : BehaviourDefinition
    {
        /// <summary>
        /// The different types of tasks that can be performed as a result of the button
        /// changing states.
        /// </summary>
        public enum TaskType
        {
            OpenURL,
            PauseGame,
            ResumeGame,
            ShowCredits,
            OptionToggleTutorial,
        }

        /// <summary>
        /// Defines a single task that will be executed when the button enters a state.
        /// </summary>
        public class Task
        {
            /// <summary>
            /// The type of task to perform.
            /// </summary>
            public TaskType mType;

            /// <summary>
            /// Data associated with this task. Changes depending on the TaskType.
            /// </summary>
            [ContentSerializer(Optional = true)]
            public String mData;
        }

        /// <summary>
        /// Task to be performed when the button is pressed and then released.
        /// </summary>
        public Task mTaskOnRelease;
    }
}
