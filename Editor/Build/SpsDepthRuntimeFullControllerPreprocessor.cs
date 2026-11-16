// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using MDA.SpsDepth;

namespace MDA.SpsDepth.Editor {
    public sealed class SpsDepthRuntimeFullControllerPreprocessor : IVRCSDKPreprocessAvatarCallback {
        // Run immediately before the VRCFury preprocessor.
        public int callbackOrder => -10001;

        public bool OnPreprocessAvatar(GameObject avatarGameObject) {
            var installers = avatarGameObject
                .GetComponentsInChildren<SpsDepthRuntimeFullControllerInstaller>(true)
                .Where(i => i != null && i.enabled)
                .ToArray();

            if (installers.Length > 0 && !SpsDepthVrcfuryFullControllerFactory.IsAvailable(out var buildError)) {
                Debug.LogError($"[MDA] {buildError}", avatarGameObject);
                return false;
            }

            var cleanedSockets = new HashSet<GameObject>();
            for (var i = 0; i < installers.Length; i++) {
                if (!ProcessInstaller(avatarGameObject, installers[i], i, cleanedSockets)) {
                    return false;
                }
            }

            return true;
        }

        private static bool ProcessInstaller(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller installer,
            int batchIndex,
            HashSet<GameObject> cleanedSockets
        ) {
            var injectedDepthParam = SpsDepthGeneratedNames.GetDepthParamName(installer, batchIndex);

            var socketStatus = SpsDepthSocketResolver.ResolveForBuild(
                avatarRoot,
                installer,
                out var socketObject,
                out var socketMessage
            );
            switch (socketStatus) {
                case SpsDepthSocketResolver.ResolveStatus.Found:
                    break;
                case SpsDepthSocketResolver.ResolveStatus.FoundWithWarning:
                    Debug.LogWarning(
                        $"{LogPrefix(installer)} {socketMessage}",
                        installer
                    );
                    break;
                case SpsDepthSocketResolver.ResolveStatus.Skip:
                    Debug.LogWarning(
                        $"{LogPrefix(installer)} {socketMessage}",
                        installer
                    );
                    return true;
                default:
                    Debug.LogError(
                        $"{LogPrefix(installer)} {socketMessage}",
                        installer
                    );
                    return false;
            }

            if (!SpsDepthAnimatorControllerFactory.TryCreate(
                avatarRoot,
                installer,
                injectedDepthParam,
                out var controllerResult,
                out var controllerError,
                batchIndex
            )) {
                Debug.LogError(
                    $"{LogPrefix(installer)} Failed to generate depth AnimatorController.\n{controllerError}",
                    installer
                );
                return false;
            }

            if (cleanedSockets.Add(socketObject)) {
                DeleteExistingGeneratedObject(socketObject.transform);
            }

            var generated = new GameObject(SpsDepthGeneratedNames.GetGeneratedObjectName(batchIndex));
            generated.transform.SetParent(socketObject.transform, false);
            generated.transform.localPosition = Vector3.zero;
            generated.transform.localRotation = Quaternion.identity;
            generated.transform.localScale = Vector3.one;
            generated.hideFlags = HideFlags.None;

            try {
                if (!SpsDepthVrcfuryFullControllerFactory.TryCreate(
                    generated,
                    avatarRoot,
                    socketObject,
                    controllerResult.Controller,
                    injectedDepthParam,
                    controllerResult.GlobalParameters,
                    out var createError
                )) {
                    Debug.LogError(
                        $"{LogPrefix(installer)} Failed to create SPS depth Full Controller.\n{createError}",
                        installer
                    );
                    UnityEngine.Object.DestroyImmediate(generated);
                    return false;
                }

                EditorUtility.SetDirty(generated);
                EditorUtility.SetDirty(socketObject);

                Debug.Log(
                    $"{LogPrefix(installer)} Generated SPS depth Full Controller under socket: {SpsDepthSocketResolver.GetPath(socketObject.transform)}\n" +
                    $"Socket resolve: {socketMessage}\n" +
                    $"Injected depth param: {injectedDepthParam}\n" +
                    $"Generated controller: {controllerResult.AssetPath}\n" +
                    $"Action binding paths: {controllerResult.BindingPath}\n" +
                    $"Generated controller details: {controllerResult.Summary}",
                    installer
                );

                return true;
            } catch (Exception e) {
                Debug.LogError(
                    $"{LogPrefix(installer)} Failed to generate runtime Full Controller.\n{e}",
                    installer
                );

                if (generated != null) {
                    UnityEngine.Object.DestroyImmediate(generated);
                }

                return false;
            }
        }

        private static void DeleteExistingGeneratedObject(Transform socketTransform) {
            if (socketTransform == null) return;

            for (var i = socketTransform.childCount - 1; i >= 0; i--) {
                var child = socketTransform.GetChild(i);
                if (child != null && (child.name == SpsDepthGeneratedNames.GeneratedObjectName ||
                    child.name.StartsWith(SpsDepthGeneratedNames.GeneratedObjectName + "_", StringComparison.Ordinal))) {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static string LogPrefix(SpsDepthRuntimeFullControllerInstaller installer) {
            return $"[MDA Installer: {installer.name}]";
        }
    }
}
#endif
