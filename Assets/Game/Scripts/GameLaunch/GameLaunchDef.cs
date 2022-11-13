using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class GameContextKeys
    {
        public const string UICanvasRoot = "UICanvasRoot";
        public const string HotUpdateRoot = "HotUpdateRoot";
    }

    public class GameLaunchDef
    {

    }

    public class GameLaunchEvent
    {
        public const string RunHotUpdate = "RunHotUpdate";
    }
}