// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MDA.SpsDepth {
    [DisallowMultipleComponent]
    [AddComponentMenu("Modular Depth Animation/MDA Fury SPS Depth")]
    public sealed class SpsDepthRuntimeFullControllerInstaller : MonoBehaviour {
        public enum DepthActionType {
            BlendShape,
            AnimationClip
        }

        public enum SocketPreset {
            Mouth,
            Pussy,
            Anal,
            LeftHand,
            RightHand,
            BothHand,
            Boobs,
            Thigh,
            Foot,
            Other
        }

        [Serializable]
        public sealed class DepthAction {
            public DepthActionType actionType = DepthActionType.BlendShape;

            public Vector2 activationDistance = new Vector2(-0.25f, 0f);

            public SkinnedMeshRenderer blendShapeRenderer;

            public string blendShapeName;

            [Range(0f, 100f)]
            public float farWeight = 0f;

            [Range(0f, 100f)]
            public float nearWeight = 100f;

            public AnimationClip animationClip;
        }

        public SocketPreset socketPreset = SocketPreset.Mouth;

        public GameObject targetSocketObject;

        public List<DepthAction> depthActions = new List<DepthAction>();
    }
}
