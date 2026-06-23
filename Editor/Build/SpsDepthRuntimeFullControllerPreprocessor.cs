// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using MDA.SpsDepth;

namespace MDA.SpsDepth.Editor {
    public sealed class SpsDepthRuntimeFullControllerPreprocessor : IVRCSDKPreprocessAvatarCallback {
        // Run immediately before VRCFury's own preprocessor.
        public int callbackOrder => -10001;

        public bool OnPreprocessAvatar(GameObject avatarGameObject) {
            var installers = avatarGameObject
                .GetComponentsInChildren<SpsDepthRuntimeFullControllerInstaller>(true)
                .Where(i => i != null && i.enabled)
                .ToArray();

            foreach (var installer in installers) {
                if (!ProcessInstaller(avatarGameObject, installer)) {
                    return false;
                }
            }

            return true;
        }

        private static bool ProcessInstaller(GameObject avatarRoot, SpsDepthRuntimeFullControllerInstaller installer) {
            var injectedDepthParam = SpsDepthGeneratedNames.GetDepthParamName(installer);

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
                out var controllerError
            )) {
                Debug.LogError(
                    $"{LogPrefix(installer)} Failed to generate depth AnimatorController.\n{controllerError}",
                    installer
                );
                return false;
            }

            DeleteExistingGeneratedObject(socketObject.transform);

            var generated = new GameObject(SpsDepthGeneratedNames.GeneratedObjectName);
            generated.transform.SetParent(socketObject.transform, false);
            generated.transform.localPosition = Vector3.zero;
            generated.transform.localRotation = Quaternion.identity;
            generated.transform.localScale = Vector3.one;
            generated.hideFlags = HideFlags.None;

            try {
                if (!SpsDepthVrcfuryFullControllerFactory.TryCreate(
                    generated,
                    avatarRoot,
                    controllerResult.Controller,
                    injectedDepthParam,
                    out var createError
                )) {
                    Debug.LogError(
                        $"{LogPrefix(installer)} Failed to create VRCFury Full Controller.\n{createError}",
                        installer
                    );
                    UnityEngine.Object.DestroyImmediate(generated);
                    return false;
                }

                EditorUtility.SetDirty(generated);
                EditorUtility.SetDirty(socketObject);

                Debug.Log(
                    $"{LogPrefix(installer)} Generated VRCFury Full Controller under socket: {SpsDepthSocketResolver.GetPath(socketObject.transform)}\n" +
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
                if (child != null && child.name == SpsDepthGeneratedNames.GeneratedObjectName) {
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
