using System;

namespace AutomizeMyDroughts.Common
{
    public static class AutomizationEvent
    {
        [Serializable]
        [Flags]
        public enum Type
        {
            None      = 0,
            Drought   = 1 << 0,
            Temperate = 1 << 1,
            Day       = 1 << 2,
            Night     = 1 << 3,
            Wind      = 1 << 4,
        }

    }
}
