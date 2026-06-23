// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using MDA.SpsDepth;

namespace MDA.SpsDepth.Editor {
    internal static class SpsDepthAnimatorControllerFactory {
        public static bool TryCreate(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller installer,
            string injectedDepthParam,
            out SpsDepthAnimatorControllerBuildResult result,
            out string error
        ) {
            result = null;
            error = null;

            if (!ValidateInputs(avatarRoot, installer, injectedDepthParam, out var actions, out error)) {
                return false;
            }

            EnsureGeneratedAssetFolder();

            var controllerPath = SpsDepthGeneratedNames.GetControllerPath(avatarRoot, installer);
            AssetDatabase.DeleteAsset(controllerPath);

            var animatorController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            if (animatorController == null) {
                error = $"Failed to create AnimatorController asset: {controllerPath}";
                return false;
            }

            animatorController.AddParameter(injectedDepthParam, AnimatorControllerParameterType.Float);

            var bindingPaths = new List<string>();
            var globalParameters = new List<string>();
            var context = new SpsDepthActionBuildContext {
                AvatarRoot = avatarRoot,
                Installer = installer,
                InjectedDepthParam = injectedDepthParam,
                AnimatorController = animatorController,
                BindingPaths = bindingPaths,
                GlobalParameters = globalParameters
            };

            for (var i = 0; i < actions.Length; i++) {
                SpsDepthActionLayerBuilder.CreateActionLayer(context, actions[i], i);
            }

            EnsureAllLayersActive(animatorController);

            EditorUtility.SetDirty(animatorController);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(controllerPath);

            result = new SpsDepthAnimatorControllerBuildResult {
                Controller = animatorController,
                AssetPath = controllerPath,
                BindingPath = string.Join(", ", bindingPaths.Distinct()),
                GlobalParameters = globalParameters.Distinct().ToArray(),
                Summary = DescribeAnimatorController(animatorController)
            };
            return true;
        }

        private static bool ValidateInputs(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller installer,
            string injectedDepthParam,
            out SpsDepthRuntimeFullControllerInstaller.DepthAction[] actions,
            out string error
        ) {
            actions = null;
            error = null;

            if (avatarRoot == null) {
                error = "Avatar root is null.";
                return false;
            }

            if (installer == null) {
                error = "Installer is null.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(injectedDepthParam)) {
                error = "Injected depth parameter is empty.";
                return false;
            }

            actions = (installer.depthActions ?? new List<SpsDepthRuntimeFullControllerInstaller.DepthAction>())
                .Where(a => a != null)
                .ToArray();
            if (actions.Length == 0) {
                error = "At least one Depth Action is required.";
                return false;
            }

            foreach (var action in actions) {
                if (!SpsDepthActionLayerBuilder.ValidateAction(avatarRoot, action, out error)) {
                    return false;
                }
            }

            if (actions.Any(a => Mathf.Approximately(a.activationDistance.x, a.activationDistance.y))) {
                error = "Activation Distance min and max must be different.";
                return false;
            }

            return true;
        }

        private static void EnsureAllLayersActive(AnimatorController animatorController) {
            if (animatorController == null) return;

            var layers = animatorController.layers;
            for (var i = 0; i < layers.Length; i++) {
                layers[i].defaultWeight = 1f;
            }
            animatorController.layers = layers;
        }

        private static string DescribeAnimatorController(AnimatorController animatorController) {
            if (animatorController == null) {
                return "<null>";
            }

            var layerDetails = animatorController.layers
                .Select(l => $"{l.name}(weight={l.defaultWeight:0.###}, states={(l.stateMachine != null ? l.stateMachine.states.Length : 0)})");
            var paramDetails = animatorController.parameters
                .Select(p => $"{p.name}:{p.type}");
            return
                $"layers=[{string.Join(", ", layerDetails)}], " +
                $"parameters=[{string.Join(", ", paramDetails)}]";
        }

        private static void EnsureGeneratedAssetFolder() {
            if (!AssetDatabase.IsValidFolder(SpsDepthGeneratedNames.GeneratedAssetFolder)) {
                AssetDatabase.CreateFolder("Assets", "MDAGenerated");
            }
        }
    }
}
#endif
