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
            AnimationClip,
            MaterialProperty,
            SetFxFloat,
            ObjectToggle,
            Scale,
            MaterialSwap,
            ComponentToggle,
            PoiyomiUvTile,
            PoiyomiFlipbookFrame,
            ScssShaderInventory
        }

        public enum MaterialPropertyValueType {
            Float,
            Color,
            Vector,
            TextureScaleOffset
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

            public Renderer materialRenderer;

            public string materialPropertyName;

            public MaterialPropertyValueType materialPropertyValueType = MaterialPropertyValueType.Float;

            public float materialFarValue = 0f;

            public float materialNearValue = 1f;

            public Color materialFarColor = Color.black;

            public Color materialNearColor = Color.white;

            public Vector4 materialFarVector = Vector4.zero;

            public Vector4 materialNearVector = Vector4.one;

            public Vector2 materialFarTextureScale = Vector2.one;

            public Vector2 materialNearTextureScale = Vector2.one;

            public Vector2 materialFarTextureOffset = Vector2.zero;

            public Vector2 materialNearTextureOffset = Vector2.zero;

            public string fxFloatParameterName;

            public float fxFloatFarValue = 0f;

            public float fxFloatNearValue = 1f;

            public GameObject objectToggleTarget;

            public bool objectToggleFarActive = false;

            public bool objectToggleNearActive = true;

            public GameObject scaleTarget;

            public float scaleFarMultiplier = 1f;

            public float scaleNearMultiplier = 1f;

            public Renderer materialSwapRenderer;

            public int materialSwapSlot = 0;

            public Material materialSwapFarMaterial;

            public Material materialSwapNearMaterial;

            public Component componentToggleTarget;

            public bool componentToggleFarEnabled = false;

            public bool componentToggleNearEnabled = true;

            public Renderer poiyomiUvTileRenderer;

            [Range(0, 3)]
            public int poiyomiUvTileRow = 0;

            [Range(0, 3)]
            public int poiyomiUvTileColumn = 0;

            public bool poiyomiUvTileUseDissolve = false;

            public string poiyomiUvTileRenamedMaterial = string.Empty;

            public bool poiyomiUvTileFarActive = false;

            public bool poiyomiUvTileNearActive = true;

            public Renderer poiyomiFlipbookRenderer;

            public int poiyomiFlipbookFarFrame = 0;

            public int poiyomiFlipbookNearFrame = 1;

            public Renderer scssShaderInventoryRenderer;

            public int scssShaderInventorySlot = 1;

            public bool scssShaderInventoryFarActive = false;

            public bool scssShaderInventoryNearActive = true;
        }

        public SocketPreset socketPreset = SocketPreset.Mouth;

        public GameObject targetSocketObject;

        public List<DepthAction> depthActions = new List<DepthAction>();
    }
}
