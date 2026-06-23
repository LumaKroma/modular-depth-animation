// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using MDA.SpsDepth;

namespace MDA.SpsDepth.Editor {
    internal static class SpsDepthActionLayerBuilder {
        private const int AnimationClipSampleCount = 17;

        public static bool ValidateAction(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            out string error
        ) {
            error = null;

            if (action == null) {
                error = "Depth Action is null.";
                return false;
            }

            switch (action.actionType) {
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape:
                    return ValidateBlendShapeAction(avatarRoot, action, out error);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.AnimationClip:
                    return ValidateAnimationClipAction(action, out error);
                default:
                    error = $"Unsupported Depth Action type: {action.actionType}";
                    return false;
            }
        }

        public static void CreateActionLayer(
            SpsDepthActionBuildContext context,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            int actionIndex
        ) {
            if (actionIndex > 0) {
                context.AnimatorController.AddLayer($"SPS Depth Action {actionIndex + 1}");
            }

            var layerIndex = actionIndex;
            var actionName = GetActionName(action, actionIndex);
            var state = context.AnimatorController.CreateBlendTreeInController(
                actionName,
                out var blendTree,
                layerIndex
            );
            state.writeDefaultValues = false;
            RenameLayer(
                context.AnimatorController,
                layerIndex,
                $"SPS Depth Runtime - {context.Installer.name} - {actionName}"
            );

            blendTree.name = $"SPS Depth {actionName}";
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = context.InjectedDepthParam;
            blendTree.useAutomaticThresholds = false;

            switch (action.actionType) {
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape:
                    CreateBlendShapeAction(context, action, actionName, blendTree);
                    break;
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.AnimationClip:
                    CreateAnimationClipAction(context, action, actionName, blendTree);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported Depth Action type: {action.actionType}");
            }
        }

        private static string GetActionName(SpsDepthRuntimeFullControllerInstaller.DepthAction action, int actionIndex) {
            if (
                action.actionType == SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape &&
                !string.IsNullOrWhiteSpace(action.blendShapeName)
            ) {
                return $"BlendShape {actionIndex + 1} - {action.blendShapeName}";
            }

            if (
                action.actionType == SpsDepthRuntimeFullControllerInstaller.DepthActionType.AnimationClip &&
                action.animationClip != null
            ) {
                return $"Animation Clip {actionIndex + 1} - {action.animationClip.name}";
            }

            return $"{action.actionType} {actionIndex + 1}";
        }

        private static bool ValidateBlendShapeAction(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            out string error
        ) {
            error = null;

            var renderer = action.blendShapeRenderer;
            if (renderer == null) {
                error = "BlendShape Renderer is not assigned.";
                return false;
            }

            if (!IsSameOrChildOf(renderer.transform, avatarRoot.transform)) {
                error =
                    "BlendShape Renderer must be the avatar root or one of its children. " +
                    $"Assigned object: {GetPath(renderer.transform)}";
                return false;
            }

            var mesh = renderer.sharedMesh;
            if (mesh == null) {
                error = $"BlendShape Renderer has no shared mesh: {GetPath(renderer.transform)}";
                return false;
            }

            if (mesh.blendShapeCount <= 0) {
                error = $"BlendShape Renderer mesh has no BlendShapes: {mesh.name}";
                return false;
            }

            var blendShapeName = action.blendShapeName;
            if (string.IsNullOrWhiteSpace(blendShapeName) && mesh.blendShapeCount == 1) {
                blendShapeName = mesh.GetBlendShapeName(0);
            }

            if (FindBlendShapeIndex(mesh, blendShapeName) < 0) {
                error =
                    $"BlendShape was not found on mesh {mesh.name}: \"{blendShapeName}\". " +
                    $"Available BlendShapes: {DescribeBlendShapes(mesh)}";
                return false;
            }

            return true;
        }

        private static bool ValidateAnimationClipAction(
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            out string error
        ) {
            error = null;

            if (action.animationClip == null) {
                error = "Animation Clip is not assigned.";
                return false;
            }

            if (action.animationClip.legacy) {
                error = $"Animation Clip is legacy and cannot be used in an AnimatorController: {action.animationClip.name}";
                return false;
            }

            return true;
        }

        private static void CreateBlendShapeAction(
            SpsDepthActionBuildContext context,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            string actionName,
            BlendTree blendTree
        ) {
            var renderer = action.blendShapeRenderer;
            var mesh = renderer.sharedMesh;
            var blendShapeName = action.blendShapeName;
            if (string.IsNullOrWhiteSpace(blendShapeName) && mesh.blendShapeCount == 1) {
                blendShapeName = mesh.GetBlendShapeName(0);
            }

            var rendererPath = AnimationUtility.CalculateTransformPath(renderer.transform, context.AvatarRoot.transform);
            context.BindingPaths.Add(rendererPath);

            var nearBlendShapeClip = CreateBlendShapeClip(
                $"SPS Depth {actionName} Near",
                rendererPath,
                blendShapeName,
                action.nearWeight
            );
            var farBlendShapeClip = CreateBlendShapeClip(
                $"SPS Depth {actionName} Far",
                rendererPath,
                blendShapeName,
                action.farWeight
            );
            AssetDatabase.AddObjectToAsset(nearBlendShapeClip, context.AnimatorController);
            AssetDatabase.AddObjectToAsset(farBlendShapeClip, context.AnimatorController);
            AddDepthChildren(blendTree, action, nearBlendShapeClip, farBlendShapeClip);
        }

        private static void CreateAnimationClipAction(
            SpsDepthActionBuildContext context,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            string actionName,
            BlendTree blendTree
        ) {
            var sourceClip = action.animationClip;
            AddAnimationClipBindingPaths(sourceClip, context.BindingPaths);

            var nearDistance = Mathf.Min(action.activationDistance.x, action.activationDistance.y);
            var farDistance = Mathf.Max(action.activationDistance.x, action.activationDistance.y);

            for (var i = 0; i < AnimationClipSampleCount; i++) {
                var depthT = AnimationClipSampleCount <= 1 ? 0f : (float)i / (AnimationClipSampleCount - 1);
                var sourceT = 1f - depthT;
                var threshold = Mathf.Lerp(nearDistance, farDistance, depthT);
                var sampleClip = CreateSampledAnimationClip(
                    $"SPS Depth {actionName} Sample {i + 1:00}",
                    sourceClip,
                    sourceT
                );
                AssetDatabase.AddObjectToAsset(sampleClip, context.AnimatorController);
                blendTree.AddChild(sampleClip, threshold);
            }
        }

        private static void AddDepthChildren(
            BlendTree blendTree,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            Motion nearMotion,
            Motion farMotion
        ) {
            var nearDistance = Mathf.Min(action.activationDistance.x, action.activationDistance.y);
            var farDistance = Mathf.Max(action.activationDistance.x, action.activationDistance.y);
            blendTree.AddChild(nearMotion, nearDistance);
            blendTree.AddChild(farMotion, farDistance);
        }

        private static void RenameLayer(AnimatorController animatorController, int layerIndex, string layerName) {
            if (animatorController == null || string.IsNullOrWhiteSpace(layerName)) return;

            var layers = animatorController.layers;
            if (layerIndex < 0 || layerIndex >= layers.Length) return;

            layers[layerIndex].name = layerName.Replace(".", "");
            animatorController.layers = layers;
        }

        private static AnimationClip CreateBlendShapeClip(
            string clipName,
            string rendererPath,
            string blendShapeName,
            float weight
        ) {
            var clip = new AnimationClip {
                name = clipName
            };
            var binding = EditorCurveBinding.FloatCurve(
                rendererPath,
                typeof(SkinnedMeshRenderer),
                $"blendShape.{blendShapeName}"
            );
            AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, 1f / 60f, weight));
            return clip;
        }

        private static void AddAnimationClipBindingPaths(AnimationClip clip, ICollection<string> bindingPaths) {
            if (clip == null || bindingPaths == null) {
                return;
            }

            foreach (var binding in AnimationUtility.GetCurveBindings(clip)) {
                bindingPaths.Add(string.IsNullOrEmpty(binding.path) ? "<avatar root>" : binding.path);
            }

            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip)) {
                bindingPaths.Add(string.IsNullOrEmpty(binding.path) ? "<avatar root>" : binding.path);
            }
        }

        private static AnimationClip CreateSampledAnimationClip(
            string clipName,
            AnimationClip sourceClip,
            float normalizedTime
        ) {
            var sampleClip = new AnimationClip {
                name = clipName
            };
            if (sourceClip == null) {
                return sampleClip;
            }

            var sourceTime = Mathf.Clamp01(normalizedTime) * Mathf.Max(0f, sourceClip.length);
            foreach (var binding in AnimationUtility.GetCurveBindings(sourceClip)) {
                var curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                if (curve == null) {
                    continue;
                }

                AnimationUtility.SetEditorCurve(
                    sampleClip,
                    binding,
                    AnimationCurve.Constant(0f, 1f / 60f, curve.Evaluate(sourceTime))
                );
            }

            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(sourceClip)) {
                var value = SampleObjectReferenceValue(sourceClip, binding, sourceTime);
                if (value == null) {
                    continue;
                }

                AnimationUtility.SetObjectReferenceCurve(
                    sampleClip,
                    binding,
                    new[] {
                        new ObjectReferenceKeyframe {
                            time = 0f,
                            value = value
                        }
                    }
                );
            }

            return sampleClip;
        }

        private static UnityEngine.Object SampleObjectReferenceValue(
            AnimationClip sourceClip,
            EditorCurveBinding binding,
            float sourceTime
        ) {
            var keyframes = AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);
            if (keyframes == null || keyframes.Length == 0) {
                return null;
            }

            var value = keyframes[0].value;
            foreach (var keyframe in keyframes) {
                if (keyframe.time > sourceTime) {
                    break;
                }

                value = keyframe.value;
            }

            return value;
        }

        private static int FindBlendShapeIndex(Mesh mesh, string blendShapeName) {
            if (mesh == null || string.IsNullOrWhiteSpace(blendShapeName)) {
                return -1;
            }

            for (var i = 0; i < mesh.blendShapeCount; i++) {
                if (mesh.GetBlendShapeName(i) == blendShapeName) {
                    return i;
                }
            }

            return -1;
        }

        private static string DescribeBlendShapes(Mesh mesh) {
            if (mesh == null || mesh.blendShapeCount <= 0) {
                return "<none>";
            }

            return string.Join(
                ", ",
                Enumerable.Range(0, mesh.blendShapeCount)
                    .Select(i => mesh.GetBlendShapeName(i))
            );
        }

        private static string GetPath(Transform t) {
            if (t == null) return "<null>";

            var path = t.name;
            var current = t.parent;

            while (current != null) {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
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
    }
}
#endif
