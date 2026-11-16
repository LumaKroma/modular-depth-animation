// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using MDA.SpsDepth;

namespace MDA.SpsDepth.Editor {
    internal static class SpsDepthLocalization {
        public enum LanguageMode {
            Auto,
            English,
            Japanese
        }

        public static class Key {
            public const string ComponentTitle = "component.title";
            public const string ComponentHeaderTitle = "component.headerTitle";
            public const string Language = "language";
            public const string LanguageAuto = "language.auto";
            public const string LanguageEnglish = "language.en";
            public const string LanguageJapanese = "language.ja";
            public const string TargetSocket = "targetSocket";
            public const string TargetSocketHelp = "targetSocket.help";
            public const string AutoDetectType = "autoDetectType";
            public const string AutoDetectSocket = "autoDetectSocket";
            public const string TargetSocketObject = "targetSocketObject";
            public const string DepthActions = "depthActions";
            public const string DepthActionsHelp = "depthActions.help";
            public const string DepthActionsEmpty = "depthActions.empty";
            public const string AddAction = "addAction";
            public const string ClearActions = "clearActions";
            public const string ClearActionsTitle = "clearActions.title";
            public const string ClearActionsMessage = "clearActions.message";
            public const string Ok = "ok";
            public const string Cancel = "cancel";
            public const string Copy = "copy";
            public const string Remove = "remove";
            public const string ActionType = "actionType";
            public const string BlendShape = "blendShape";
            public const string AnimationClip = "animationClip";
            public const string MaterialProperty = "materialProperty";
            public const string SetFxFloat = "setFxFloat";
            public const string ObjectToggle = "objectToggle";
            public const string Scale = "scale";
            public const string MaterialSwap = "materialSwap";
            public const string ComponentToggle = "componentToggle";
            public const string PoiyomiUvTile = "poiyomiUvTile";
            public const string PoiyomiFlipbookFrame = "poiyomiFlipbookFrame";
            public const string ScssShaderInventory = "scssShaderInventory";
            public const string ActivationDistance = "activationDistance";
            public const string ActivationDistanceTooltip = "activationDistance.tooltip";
            public const string Reset = "reset";
            public const string FullyInserted = "fullyInserted";
            public const string Entrance = "entrance";
            public const string FarAway = "farAway";
            public const string BlendShapeRenderer = "blendShapeRenderer";
            public const string BlendShapeName = "blendShapeName";
            public const string FarWeight = "farWeight";
            public const string NearWeight = "nearWeight";
            public const string Clip = "clip";
            public const string ClipHelp = "clip.help";
            public const string MaterialRenderer = "materialRenderer";
            public const string MaterialPropertyName = "materialPropertyName";
            public const string MaterialPropertyValueType = "materialPropertyValueType";
            public const string MaterialPropertyHelp = "materialProperty.help";
            public const string FarColor = "farColor";
            public const string NearColor = "nearColor";
            public const string FarVector = "farVector";
            public const string NearVector = "nearVector";
            public const string FarTextureScale = "farTextureScale";
            public const string NearTextureScale = "nearTextureScale";
            public const string FarTextureOffset = "farTextureOffset";
            public const string NearTextureOffset = "nearTextureOffset";
            public const string FxFloatParameterName = "fxFloatParameterName";
            public const string SetFxFloatHelp = "setFxFloat.help";
            public const string FarValue = "farValue";
            public const string NearValue = "nearValue";
            public const string ObjectToggleTarget = "objectToggleTarget";
            public const string ObjectToggleHelp = "objectToggle.help";
            public const string FarActive = "farActive";
            public const string NearActive = "nearActive";
            public const string ScaleTarget = "scaleTarget";
            public const string ScaleHelp = "scale.help";
            public const string FarScaleMultiplier = "farScaleMultiplier";
            public const string NearScaleMultiplier = "nearScaleMultiplier";
            public const string MaterialSwapRenderer = "materialSwapRenderer";
            public const string MaterialSwapHelp = "materialSwap.help";
            public const string MaterialSwapSlot = "materialSwapSlot";
            public const string FarMaterial = "farMaterial";
            public const string NearMaterial = "nearMaterial";
            public const string ComponentToggleTarget = "componentToggleTarget";
            public const string ComponentToggleHelp = "componentToggle.help";
            public const string FarEnabled = "farEnabled";
            public const string NearEnabled = "nearEnabled";
            public const string PoiyomiRenderer = "poiyomiRenderer";
            public const string PoiyomiUvTileHelp = "poiyomiUvTile.help";
            public const string PoiyomiUvTileRow = "poiyomiUvTileRow";
            public const string PoiyomiUvTileColumn = "poiyomiUvTileColumn";
            public const string PoiyomiUvTileUseDissolve = "poiyomiUvTileUseDissolve";
            public const string PoiyomiUvTileRenamedMaterial = "poiyomiUvTileRenamedMaterial";
            public const string PoiyomiFlipbookFrameHelp = "poiyomiFlipbookFrame.help";
            public const string FarFrame = "farFrame";
            public const string NearFrame = "nearFrame";
            public const string ScssShaderInventoryRenderer = "scssShaderInventoryRenderer";
            public const string ScssShaderInventoryHelp = "scssShaderInventory.help";
            public const string ScssShaderInventorySlot = "scssShaderInventorySlot";
            public const string AutoDetectDialogTitle = "autoDetectDialog.title";
            public const string AutoDetectUndo = "autoDetect.undo";
            public const string ActionBlendShape = "action.blendShape";
            public const string ActionAnimationClip = "action.animationClip";
            public const string ActionMaterialProperty = "action.materialProperty";
            public const string ActionSetFxFloat = "action.setFxFloat";
            public const string ActionObjectToggle = "action.objectToggle";
            public const string ActionScale = "action.scale";
            public const string ActionMaterialSwap = "action.materialSwap";
            public const string ActionComponentToggle = "action.componentToggle";
            public const string ActionPoiyomiUvTile = "action.poiyomiUvTile";
            public const string ActionPoiyomiFlipbookFrame = "action.poiyomiFlipbookFrame";
            public const string ActionScssShaderInventory = "action.scssShaderInventory";
            public const string SocketMouth = "socket.mouth";
            public const string SocketPussy = "socket.pussy";
            public const string SocketAnal = "socket.anal";
            public const string SocketLeftHand = "socket.leftHand";
            public const string SocketRightHand = "socket.rightHand";
            public const string SocketBothHand = "socket.bothHand";
            public const string SocketBoobs = "socket.boobs";
            public const string SocketThigh = "socket.thigh";
            public const string SocketFoot = "socket.foot";
            public const string SocketOther = "socket.other";
            public const string TargetSocketObjectInvalidTitle = "targetSocketObject.invalid.title";
            public const string TargetSocketObjectInvalidMessage = "targetSocketObject.invalid.message";
            public const string TargetSocketObjectMissing = "targetSocketObject.missing";
            public const string ResolveAvatarRootNull = "resolve.avatarRootNull";
            public const string ResolveInstallerNull = "resolve.installerNull";
            public const string ResolveAvatarRootMissing = "resolve.avatarRootMissing";
            public const string ResolveAutoBuildMissing = "resolve.autoBuildMissing";
            public const string ResolveMissingReferenceFallback = "resolve.missingReferenceFallback";
            public const string ResolveAutoInspectDetected = "resolve.autoInspectDetected";
            public const string ResolveAutoInspectMissing = "resolve.autoInspectMissing";
            public const string ResolveExplicitOutsideAvatar = "resolve.explicitOutsideAvatar";
            public const string ResolveExplicitMissingSocket = "resolve.explicitMissingSocket";
            public const string ResolveExplicitUsing = "resolve.explicitUsing";
            public const string ResolveFoundKnownPath = "resolve.foundKnownPath";
            public const string ResolveFoundObjectName = "resolve.foundObjectName";
            public const string ResolveNoMatchingSocket = "resolve.noMatchingSocket";
            public const string ResolveTargetObjectNull = "resolve.targetObjectNull";
            public const string ResolveExpectedComponentType = "resolve.expectedComponentType";
            public const string ResolveVrcfurySocketTypeMissing = "resolve.vrcfurySocketTypeMissing";
            public const string ResolveSpsNdmfUnsupported = "resolve.spsNdmfUnsupported";
            public const string VrcfuryBuildUnavailable = "vrcfury.buildUnavailable";
        }

        [Serializable]
        private sealed class LocalizationFile {
            public LocalizationEntry[] entries;
        }

        [Serializable]
        private sealed class LocalizationEntry {
            public string key;
            public string value;
        }

        private const string LanguageModePrefKey = "MDA.SpsDepth.LanguageMode";
        private const string LocalizationAssetPrefix = "SpsDepthLocalization.";
        private static Dictionary<string, string> english;
        private static Dictionary<string, string> japanese;

        public static LanguageMode Mode {
            get {
                var value = EditorPrefs.GetInt(LanguageModePrefKey, (int)LanguageMode.Auto);
                return Enum.IsDefined(typeof(LanguageMode), value) ? (LanguageMode)value : LanguageMode.Auto;
            }
            set {
                EditorPrefs.SetInt(LanguageModePrefKey, (int)value);
            }
        }

        public static LanguageMode ActiveLanguage {
            get {
                if (Mode != LanguageMode.Auto) {
                    return Mode;
                }

                return Application.systemLanguage == SystemLanguage.Japanese ? LanguageMode.Japanese : LanguageMode.English;
            }
        }

        public static string T(string key) {
            var dictionary = ActiveLanguage == LanguageMode.Japanese ? Japanese : English;
            if (dictionary.TryGetValue(key, out var value)) {
                return value;
            }

            return English.TryGetValue(key, out value) ? value : key;
        }

        public static string Format(string key, params object[] args) {
            return string.Format(T(key), args);
        }

        public static GUIContent Content(string key, string tooltipKey = null) {
            return new GUIContent(T(key), string.IsNullOrEmpty(tooltipKey) ? string.Empty : T(tooltipKey));
        }

        public static string[] LanguageModeLabels {
            get {
                return new[] {
                    T(Key.LanguageAuto),
                    T(Key.LanguageEnglish),
                    T(Key.LanguageJapanese)
                };
            }
        }

        public static string[] SocketPresetLabels {
            get {
                return new[] {
                    T(Key.SocketMouth),
                    T(Key.SocketPussy),
                    T(Key.SocketAnal),
                    T(Key.SocketLeftHand),
                    T(Key.SocketRightHand),
                    T(Key.SocketBothHand),
                    T(Key.SocketBoobs),
                    T(Key.SocketThigh),
                    T(Key.SocketFoot),
                    T(Key.SocketOther)
                };
            }
        }

        public static string ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType actionType) {
            switch (actionType) {
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape:
                    return T(Key.ActionBlendShape);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.AnimationClip:
                    return T(Key.ActionAnimationClip);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialProperty:
                    return T(Key.ActionMaterialProperty);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.SetFxFloat:
                    return T(Key.ActionSetFxFloat);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.ObjectToggle:
                    return T(Key.ActionObjectToggle);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.Scale:
                    return T(Key.ActionScale);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialSwap:
                    return T(Key.ActionMaterialSwap);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.ComponentToggle:
                    return T(Key.ActionComponentToggle);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.PoiyomiUvTile:
                    return T(Key.ActionPoiyomiUvTile);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.PoiyomiFlipbookFrame:
                    return T(Key.ActionPoiyomiFlipbookFrame);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.ScssShaderInventory:
                    return T(Key.ActionScssShaderInventory);
                default:
                    return actionType.ToString();
            }
        }

        public static string SocketPresetLabel(SpsDepthRuntimeFullControllerInstaller.SocketPreset preset) {
            var labels = SocketPresetLabels;
            var index = (int)preset;
            return index >= 0 && index < labels.Length ? labels[index] : preset.ToString();
        }

        private static Dictionary<string, string> English {
            get {
                return english ?? (english = LoadDictionary("en", null));
            }
        }

        private static Dictionary<string, string> Japanese {
            get {
                return japanese ?? (japanese = LoadDictionary("ja", English));
            }
        }

        private static Dictionary<string, string> LoadDictionary(string languageCode, Dictionary<string, string> fallback) {
            var dictionary = fallback != null
                ? new Dictionary<string, string>(fallback)
                : new Dictionary<string, string>();
            var asset = LoadLocalizationAsset(languageCode);
            if (asset == null) {
                return dictionary;
            }

            var file = JsonUtility.FromJson<LocalizationFile>(asset.text);
            if (file?.entries == null) {
                return dictionary;
            }

            foreach (var entry in file.entries) {
                if (string.IsNullOrWhiteSpace(entry?.key)) {
                    continue;
                }

                dictionary[entry.key] = entry.value ?? string.Empty;
            }

            return dictionary;
        }

        private static TextAsset LoadLocalizationAsset(string languageCode) {
            var fileName = LocalizationAssetPrefix + languageCode;
            foreach (var guid in AssetDatabase.FindAssets("SpsDepthLocalization t:TextAsset")) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != fileName) {
                    continue;
                }

                return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            }

            return null;
        }
    }
}
#endif
