// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace MDA.SpsDepth.Editor {
    internal sealed class SpsDepthAnimatorControllerBuildResult {
        public RuntimeAnimatorController Controller { get; set; }
        public string AssetPath { get; set; }
        public string BindingPath { get; set; }
        public IReadOnlyList<string> GlobalParameters { get; set; }
        public string Summary { get; set; }
    }
}
#endif
