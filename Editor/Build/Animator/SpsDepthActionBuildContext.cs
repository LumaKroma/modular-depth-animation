// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using MDA.SpsDepth;

namespace MDA.SpsDepth.Editor {
    internal sealed class SpsDepthActionBuildContext {
        public GameObject AvatarRoot { get; set; }
        public SpsDepthRuntimeFullControllerInstaller Installer { get; set; }
        public string InjectedDepthParam { get; set; }
        public AnimatorController AnimatorController { get; set; }
        public ICollection<string> BindingPaths { get; set; }
        public ICollection<string> GlobalParameters { get; set; }
    }
}
#endif
