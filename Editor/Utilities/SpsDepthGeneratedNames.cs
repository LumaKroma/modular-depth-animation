// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using MDA.SpsDepth;

namespace MDA.SpsDepth.Editor {
    internal static class SpsDepthGeneratedNames {
        public const string GeneratedObjectName = "__MDA_DepthRuntimeFullController";
        public const string GeneratedAssetFolder = "Assets/MDAGenerated";

        // The caller supplies a unique index per build batch; GlobalObjectId may repeat on unsaved clones.
        public static string GetGeneratedObjectName(int batchIndex) {
            return $"{GeneratedObjectName}_{batchIndex}";
        }

        public static string GetDepthParamName(SpsDepthRuntimeFullControllerInstaller installer, int batchIndex = 0) {
            return $"MDA/Depth/{GetStableInstallerId(installer)}_{batchIndex}";
        }

        public static string GetControllerPath(GameObject avatarRoot, SpsDepthRuntimeFullControllerInstaller installer, int batchIndex = 0) {
            var avatarName = avatarRoot != null ? avatarRoot.name : "Avatar";
            return
                $"{GeneratedAssetFolder}/" +
                $"{SanitizeObjectName(avatarName)}_" +
                $"MDA_" +
                $"{SanitizeObjectName(installer.name)}_" +
                $"{SanitizeObjectName(GetStableInstallerId(installer))}_{batchIndex}.controller";
        }

        public static string SanitizeObjectName(string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return "Generated";
            }

            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var chars = value
                .Select(c => invalidChars.Contains(c) ? '_' : c)
                .ToArray();
            return new string(chars);
        }

        private static string GetStableInstallerId(SpsDepthRuntimeFullControllerInstaller installer) {
            var objectId = GlobalObjectId.GetGlobalObjectIdSlow(installer).ToString();
            return StableHash(objectId).ToString("x8");
        }

        private static uint StableHash(string value) {
            unchecked {
                uint hash = 2166136261;
                foreach (var c in value ?? string.Empty) {
                    hash ^= c;
                    hash *= 16777619;
                }
                return hash;
            }
        }
    }
}
#endif
