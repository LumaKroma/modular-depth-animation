// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private const string SocketTypeName = "VF.Component.VRCFuryHapticSocket";

        private static readonly SocketPresetDefinition[] PresetDefinitions = {
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Mouth, "Mouth", "Blow", "Blowjob", "Oral", "Head"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Pussy, "Pussy", "Vagina", "Vag", "Front", "Hips Front", "HipsFront", "HipsFrontSocket", "Cooch", "Coochy"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Anal, "Anal", "Anus", "Butt", "Ass", "Back", "Hips Back", "HipsBack", "HipsBackSocket", "Assjob"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.LeftHand, "Left Hand", "LeftHand", "L Hand", "LHand", "Handjob Left", "Left Handjob", "Hand Left", "HandLeft"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.RightHand, "Right Hand", "RightHand", "R Hand", "RHand", "Handjob Right", "Right Handjob", "Hand Right", "HandRight"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.BothHand, "Double Handjob", "DoubleHandjob", "Both Hand", "BothHand", "Both Hands", "BothHands", "Hands", "Handjob", "Hand"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Boobs, "Boobs", "Boob", "Breast", "Breasts", "Chest", "Nipple", "Tit", "Titjob"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Thigh, "Thigh", "Thighs", "Leg", "Legs", "Thighjob"),
            new SocketPresetDefinition(SpsDepthRuntimeFullControllerInstaller.SocketPreset.Foot, "Foot", "Feet", "Left Foot", "LeftFoot", "Right Foot", "RightFoot", "L Foot", "LFoot", "R Foot", "RFoot", "Footjob", "Steppies", "Sole", "Soles", "Foot Left", "FootLeft", "Foot Right", "FootRight"),
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

            if (!SpsDepthVrcfuryFullControllerFactory.IsAvailable(out message)) {
                return ResolveStatus.Error;
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

            if (!TryGetSocketType(out var socketType, out var socketTypeError)) {
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
            var bestScore = 0;
            GameObject bestSocket = null;
            var sockets = avatarRoot
                .GetComponentsInChildren<Component>(true)
                .Where(c => c != null && socketType.IsInstanceOfType(c))
                .ToArray();

            foreach (var socketComponent in sockets) {
                var score = GetSocketMatchScore(socketComponent, definition);
                if (score > bestScore) {
                    bestScore = score;
                    bestSocket = socketComponent.gameObject;
                }
            }

            return bestSocket;
        }

        private static IEnumerable<string> GetKnownRelativePaths(SocketPresetDefinition definition) {
            var targetRoots = new[] {
                "Penetration Contact System/Target Objects",
                "PCS/Target Objects",
                "SPS/Target Objects"
            };
            var spsRoots = new[] {
                "SPS",
                "SPS/Handjob",
                "SPS/Special",
                "SPS/Feet",
                "Handjob",
                "Special",
                "Feet"
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

                foreach (var spsRoot in spsRoots) {
                    yield return $"{spsRoot}/{alias}/{alias} Socket";
                    yield return $"{spsRoot}/{alias}/{alias}";
                    yield return $"{spsRoot}/{alias} Socket";
                    yield return $"{spsRoot}/{alias}";
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

            if (targetObject.GetComponents<Component>().Any(c => c != null &&
                c.GetType().FullName == "SpsNdmf.Component.VRCFuryHapticSocket")) {
                error = SpsDepthLocalization.T(SpsDepthLocalization.Key.ResolveSpsNdmfUnsupported);
                return false;
            }

            if (!TryGetSocketType(out var socketType, out error)) {
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

        private static bool TryGetSocketType(out Type socketType, out string error) {
            socketType = null;
            if (!SpsDepthVrcfuryFullControllerFactory.IsAvailable(out error)) {
                return false;
            }

            socketType = FindType(SocketTypeName);
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

        private static int GetSocketMatchScore(Component socketComponent, SocketPresetDefinition definition) {
            if (socketComponent == null || definition == null) {
                return 0;
            }

            var aliases = definition.Aliases ?? Array.Empty<string>();
            var score = 0;
            score = Math.Max(score, GetTextMatchScore(socketComponent.gameObject.name, aliases, 100));
            score = Math.Max(score, GetTextMatchScore(GetStringField(socketComponent, "name"), aliases, 120));
            score = Math.Max(score, GetTextMatchScore(GetStringField(socketComponent, "oscId"), aliases, 80));

            foreach (var tag in GetSocketChannels(socketComponent)) {
                score = Math.Max(score, GetTextMatchScore(tag, aliases, 160));
            }

            return score;
        }

        private static IEnumerable<string> GetSocketChannels(Component socketComponent) {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var type = socketComponent.GetType();
            var settings = type.GetField("channelSettings", flags)?.GetValue(socketComponent);
            var channels = settings?.GetType().GetField("channels", flags)?.GetValue(settings)
                as System.Collections.IEnumerable;
            if (channels != null) {
                foreach (var channel in channels) {
                    var id = GetStringField(channel, "id");
                    if (!string.IsNullOrWhiteSpace(id)) yield return id;
                }
            }

            // VRCFury retains obsolete tags after migration. Do not let a stale
            // tag override a channel the user subsequently renamed or removed.
            var migrated = type.GetField("tagsUpgradedToChannels", flags)?.GetValue(socketComponent);
            if (migrated is bool upgraded && upgraded) yield break;
            foreach (var tag in GetStringListField(socketComponent, "tags")) yield return tag;
        }

        private static int GetTextMatchScore(string value, IEnumerable<string> aliases, int baseScore) {
            if (string.IsNullOrWhiteSpace(value) || aliases == null) {
                return 0;
            }

            var normalizedValue = NormalizeSearchText(value);
            if (string.IsNullOrEmpty(normalizedValue)) {
                return 0;
            }

            var best = 0;
            foreach (var alias in aliases) {
                var normalizedAlias = NormalizeSearchText(alias);
                if (string.IsNullOrEmpty(normalizedAlias)) {
                    continue;
                }

                if (normalizedValue == normalizedAlias) {
                    best = Math.Max(best, baseScore + 1000 + normalizedAlias.Length);
                } else if (normalizedValue.Contains(normalizedAlias)) {
                    best = Math.Max(best, baseScore + normalizedAlias.Length);
                }
            }

            return best;
        }

        private static string GetStringField(object target, string fieldName) {
            if (target == null || string.IsNullOrWhiteSpace(fieldName)) {
                return null;
            }

            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            return field?.GetValue(target) as string;
        }

        private static IEnumerable<string> GetStringListField(object target, string fieldName) {
            if (target == null || string.IsNullOrWhiteSpace(fieldName)) {
                yield break;
            }

            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            var values = field?.GetValue(target) as IEnumerable<string>;
            if (values == null) {
                yield break;
            }

            foreach (var value in values) {
                if (!string.IsNullOrWhiteSpace(value)) {
                    yield return value;
                }
            }
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

        private static string NormalizeSearchText(string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return string.Empty;
            }

            return new string(value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
        }
    }
}
#endif
