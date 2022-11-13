using strange.extensions.context.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class ResouceUpdateContextView : ContextView
    {
        private void Awake()
        {
            context = new ResouceUpdateContext(this);
        }
    }
}