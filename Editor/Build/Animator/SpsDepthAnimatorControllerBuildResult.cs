// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using UnityEngine;

namespace MDA.SpsDepth.Editor {
    internal sealed class SpsDepthAnimatorControllerBuildResult {
        public RuntimeAnimatorController Controller { get; set; }
        public string AssetPath { get; set; }
        public string BindingPath { get; set; }
        public string Summary { get; set; }
    }
}
#endif
