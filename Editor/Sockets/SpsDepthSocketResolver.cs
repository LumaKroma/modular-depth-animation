// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MDA.SpsDepth;

namespace MDA.SpsDepth.Editor {
    internal static class SpsDepthSocketResolver {
        internal enum ResolveStatus {
            Found,
            FoundWithWarning,
            Skip,
            Error
        }

        private sealed class SocketPresetDefinition {
            public readonly SpsDepthRuntimeFullControllerInstaller.SocketPreset Preset;
            public readonly string[] Aliases;

            public SocketPresetDefinition(
                SpsDepthRuntimeFullControllerInstaller.SocketPreset preset,
                params string[] aliases
            ) {
                Preset = preset;
                Aliases = aliases;
            }
        }

        private static readonly SocketPresetDefinition[] PresetDefinitions = {
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Mouth, "Mouth", "Blow"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Pussy, "Pussy", "Vagina", "Front"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Anal, "Anal", "Anus", "Butt", "Ass", "Back"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.LeftHand, "Left Hand", "LeftHand", "L Hand", "LHand"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.RightHand, "Right Hand", "RightHand", "R Hand", "RHand"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.BothHand, "Both Hand", "BothHand", "Both Hands", "BothHands", "Hands", "Hand"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Boobs, "Boobs", "Boob", "Breast", "Breasts", "Chest", "Nipple", "Tit"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Thigh, "Thigh", "Thighs", "Leg", "Legs"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Foot, "Foot", "Feet", "Left Foot", "LeftFoot", "Right Foot", "RightFoot", "L Foot", "LFoot", "R Foot", "RFoot"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Other, "Other", "Custom", "Socket")
        };

        public static ResolveStatus ResolveForBuild(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller installer,
            out GameObject socketObject,
            out string message
        ) {
            socketObject = null;
            message = null;

            if (avatarRoot == null) {
                message = SpsDepthLocalization.T(SpsDepthLocalization.Key.ResolveAvatarRootNull);
                return ResolveStatus.Error;
            }

            var hasMissingTargetSocketReference = HasMissingTargetSocketReference(installer);
            if (!hasMissingTargetSocketReference && installer.targetSocketObject != null) {
                return ResolveExplicitSocket(avatarRoot, installer.targetSocketObject, out socketObject, out message);
            }

            if (TryFindSocket(avatarRoot, installer.socketPreset, out socketObject, out message)) {
                if (hasMissingTargetSocketReference) {
                    message = SpsDepthLocalization.Format(
                        SpsDepthLocalization.Key.ResolveMissingReferenceFallback,
                        message
                    );
                    return ResolveStatus.FoundWithWarning;
                }

                return ResolveStatus.Found;
            }

            message = SpsDepthLocalization.Format(
                SpsDepthLocalization.Key.ResolveAutoBuildMissing,
                GetDisplayName(installer.socketPreset),
                message
            );
            if (hasMissingTargetSocketReference) {
                message = SpsDepthLocalization.Format(
                    SpsDepthLocalization.Key.ResolveMissingReferenceFallback,
                    message
                );
            }
            return ResolveStatus.Skip;
        }

        public static bool TryAutoDetectForInspector(
            SpsDepthRuntimeFullControllerInstaller installer,
            out GameObject socketObject,
            out string message
        ) {
            socketObject = null;
            message = null;

            if (installer == null) {
                message = SpsDepthLocalization.T(SpsDepthLocalization.Key.ResolveInstallerNull);
                return false;
            }

            var avatarRoot = FindAvatarRoot(installer.gameObject);
            if (avatarRoot == null) {
                message = SpsDepthLocalization.T(SpsDepthLocalization.Key.ResolveAvatarRootMissing);
                return false;
            }

            if (TryFindSocket(avatarRoot, installer.socketPreset, out socketObject, out message)) {
                message = SpsDepthLocalization.Format(
                    SpsDepthLocalization.Key.ResolveAutoInspectDetected,
                    GetDisplayName(installer.socketPreset),
                    GetPath(socketObject.transform)
                );
                return true;
            }

            message = SpsDepthLocalization.Format(
                SpsDepthLocalization.Key.ResolveAutoInspectMissing,
                GetDisplayName(installer.socketPreset),
                GetPath(avatarRoot.transform),
                message
            );
            return false;
        }

        public static string GetPath(Transform t) {
            if (t == null) return "<null>";

            var path = t.name;
            var current = t.parent;

            while (current != null) {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        public static bool IsValidSocketObject(GameObject targetObject, out string error) {
            return HasVrcfurySocketComponent(targetObject, out error);
        }

        public static bool HasMissingTargetSocketReference(SpsDepthRuntimeFullControllerInstaller installer) {
            if (installer == null) {
                return false;
            }

            var serializedObject = new SerializedObject(installer);
            var property = serializedObject.FindProperty(nameof(SpsDepthRuntimeFullControllerInstaller.targetSocketObject));
            return property != null &&
                property.objectReferenceValue == null &&
                property.objectReferenceInstanceIDValue != 0;
        }

        private static ResolveStatus ResolveExplicitSocket(
            GameObject avatarRoot,
            GameObject targetSocketObject,
            out GameObject socketObject,
            out string message
        ) {
            socketObject = null;
            message = null;

            if (!IsSameOrChildOf(targetSocketObject.transform, avatarRoot.transform)) {
                message = SpsDepthLocalization.Format(
                    SpsDepthLocalization.Key.ResolveExplicitOutsideAvatar,
                    GetPath(targetSocketObject.transform)
                );
                return ResolveStatus.Error;
            }

            if (!HasVrcfurySocketComponent(targetSocketObject, out var socketError)) {
                message = SpsDepthLocalization.Format(
                    SpsDepthLocalization.Key.ResolveExplicitMissingSocket,
                    socketError,
                    GetPath(targetSocketObject.transform)
                );
                return ResolveStatus.Error;
            }

            socketObject = targetSocketObject;
            message = SpsDepthLocalization.Format(
                SpsDepthLocalization.Key.ResolveExplicitUsing,
                GetPath(targetSocketObject.transform)
            );
            return ResolveStatus.Found;
        }

        private static bool TryFindSocket(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller.SocketPreset preset,
            out GameObject socketObject,
            out string message
        ) {
            socketObject = null;
            message = null;

            if (!TryGetVrcfurySocketType(out var socketType, out var socketTypeError)) {
                message = socketTypeError;
                return false;
            }

            var definition = GetDefinition(preset);
            socketObject = FindByKnownPath(avatarRoot.transform, socketType, definition);
            if (socketObject != null) {
                message = SpsDepthLocalization.Format(
                    SpsDepthLocalization.Key.ResolveFoundKnownPath,
                    GetPath(socketObject.transform)
                );
                return true;
            }

            socketObject = FindByObjectName(avatarRoot, socketType, definition);
            if (socketObject != null) {
                message = SpsDepthLocalization.Format(
                    SpsDepthLocalization.Key.ResolveFoundObjectName,
                    GetPath(socketObject.transform)
                );
                return true;
            }

            message = SpsDepthLocalization.Format(
                SpsDepthLocalization.Key.ResolveNoMatchingSocket,
                GetDisplayName(preset)
            );
            return false;
        }

        private static GameObject FindByKnownPath(
            Transform avatarRoot,
            Type socketType,
            SocketPresetDefinition definition
        ) {
            foreach (var path in GetKnownRelativePaths(definition)) {
                var transform = avatarRoot.Find(path);
                if (transform != null && transform.gameObject.GetComponent(socketType) != null) {
                    return transform.gameObject;
                }
            }

            return null;
        }

        private static GameObject FindByObjectName(
            GameObject avatarRoot,
            Type socketType,
            SocketPresetDefinition definition
        ) {
            var sockets = avatarRoot
                .GetComponentsInChildren<Component>(true)
                .Where(c => c != null && socketType.IsInstanceOfType(c))
                .Select(c => c.gameObject);

            foreach (var socket in sockets) {
                if (definition.Aliases.Any(alias => ContainsIgnoreCase(socket.name, alias))) {
                    return socket;
                }
            }

            return null;
        }

        private static IEnumerable<string> GetKnownRelativePaths(SocketPresetDefinition definition) {
            var targetRoots = new[] {
                "Penetration Contact System/Target Objects",
                "PCS/Target Objects",
                "SPS/Target Objects"
            };

            foreach (var alias in definition.Aliases) {
                foreach (var targetRoot in targetRoots) {
                    yield return $"{targetRoot}/<PCS Target> {alias}/{alias} Socket";
                    yield return $"{targetRoot}/<PCS Target> {alias}/{alias}";
                    yield return $"{targetRoot}/<PCS Target> {alias}";
                    yield return $"{targetRoot}/{alias}/{alias} Socket";
                    yield return $"{targetRoot}/{alias}/{alias}";
                    yield return $"{targetRoot}/{alias} Socket";
                }

                yield return $"{alias}/{alias} Socket";
                yield return $"{alias}/{alias}";
                yield return $"{alias} Socket";
                yield return alias;
            }
        }

        private static bool HasVrcfurySocketComponent(GameObject targetObject, out string error) {
            error = null;

            if (targetObject == null) {
                error = SpsDepthLocalization.T(SpsDepthLocalization.Key.ResolveTargetObjectNull);
                return false;
            }

            if (!TryGetVrcfurySocketType(out var socketType, out error)) {
                return false;
            }

            if (targetObject.GetComponent(socketType) == null) {
                error = SpsDepthLocalization.Format(
                    SpsDepthLocalization.Key.ResolveExpectedComponentType,
                    socketType.FullName
                );
                return false;
            }

            return true;
        }

        private static bool TryGetVrcfurySocketType(out Type socketType, out string error) {
            socketType = FindType("VF.Component.VRCFuryHapticSocket");
            error = null;

            if (socketType == null) {
                error = SpsDepthLocalization.T(SpsDepthLocalization.Key.ResolveVrcfurySocketTypeMissing);
                return false;
            }

            return true;
        }

        private static Type FindType(string fullName) {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => {
                    try {
                        return a.GetType(fullName, false);
                    } catch {
                        return null;
                    }
                })
                .FirstOrDefault(t => t != null);
        }

        private static GameObject FindAvatarRoot(GameObject source) {
            if (source == null) {
                return null;
            }

            var avatarDescriptorType = FindType("VRC.SDK3.Avatars.Components.VRCAvatarDescriptor");
            if (avatarDescriptorType != null) {
                var current = source.transform;
                while (current != null) {
                    if (current.gameObject.GetComponent(avatarDescriptorType) != null) {
                        return current.gameObject;
                    }

                    current = current.parent;
                }
            }

            var root = source.transform;
            while (root.parent != null) {
                root = root.parent;
            }

            return root.gameObject;
        }

        private static SocketPresetDefinition GetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset preset) {
            return PresetDefinitions.FirstOrDefault(d => d.Preset == preset) ?? PresetDefinitions[0];
        }

        private static string GetDisplayName(SpsDepthRuntimeFullControllerInstaller.SocketPreset preset) {
            return SpsDepthLocalization.SocketPresetLabel(preset);
        }

        private static bool IsSameOrChildOf(Transform candidate, Transform root) {
            if (candidate == null || root == null) {
                return false;
            }

            var current = candidate;
            while (current != null) {
                if (current == root) {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static bool ContainsIgnoreCase(string value, string fragment) {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(fragment)) {
                return false;
            }

            return value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
#endif
