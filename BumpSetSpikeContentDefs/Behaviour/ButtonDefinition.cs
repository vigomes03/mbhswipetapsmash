using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngineContentDefs;

namespace BumpSetSpikeContentDefs
{
    public class ButtonDefinition : BehaviourDefinition
    {
        public enum TaskType
        {
            OpenURL,
        }

        public class Task
        {
            public TaskType mType;

            public String mData;
        }

        public Task mTaskOnRelease;
    }
}
