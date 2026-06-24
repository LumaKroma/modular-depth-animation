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
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialProperty:
                    return ValidateMaterialPropertyAction(avatarRoot, action, out error);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.SetFxFloat:
                    return ValidateSetFxFloatAction(action, out error);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.ObjectToggle:
                    return ValidateObjectToggleAction(avatarRoot, action, out error);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.Scale:
                    return ValidateScaleAction(avatarRoot, action, out error);
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
            var actionName = SanitizeAnimatorName(GetActionName(action, actionIndex));
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
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialProperty:
                    CreateMaterialPropertyAction(context, action, actionName, blendTree);
                    break;
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.SetFxFloat:
                    CreateSetFxFloatAction(context, action, actionName, blendTree);
                    break;
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.ObjectToggle:
                    CreateObjectToggleAction(context, action, actionName, blendTree);
                    break;
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.Scale:
                    CreateScaleAction(context, action, actionName, blendTree);
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

            if (
                action.actionType == SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialProperty &&
                !string.IsNullOrWhiteSpace(action.materialPropertyName)
            ) {
                return $"Material Property {actionIndex + 1} - {action.materialPropertyName}";
            }

            if (
                action.actionType == SpsDepthRuntimeFullControllerInstaller.DepthActionType.SetFxFloat &&
                !string.IsNullOrWhiteSpace(action.fxFloatParameterName)
            ) {
                return $"Set FX Float {actionIndex + 1} - {action.fxFloatParameterName}";
            }

            if (
                action.actionType == SpsDepthRuntimeFullControllerInstaller.DepthActionType.ObjectToggle &&
                action.objectToggleTarget != null
            ) {
                return $"Object Toggle {actionIndex + 1} - {action.objectToggleTarget.name}";
            }

            if (
                action.actionType == SpsDepthRuntimeFullControllerInstaller.DepthActionType.Scale &&
                action.scaleTarget != null
            ) {
                return $"Scale {actionIndex + 1} - {action.scaleTarget.name}";
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

        private static bool ValidateMaterialPropertyAction(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            out string error
        ) {
            error = null;

            var renderer = action.materialRenderer;
            if (renderer == null) {
                error = "Material Renderer is not assigned.";
                return false;
            }

            if (!IsSameOrChildOf(renderer.transform, avatarRoot.transform)) {
                error =
                    "Material Renderer must be the avatar root or one of its children. " +
                    $"Assigned object: {GetPath(renderer.transform)}";
                return false;
            }

            if (string.IsNullOrWhiteSpace(action.materialPropertyName)) {
                error = "Material Property Name is empty.";
                return false;
            }

            var materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0 || materials.All(m => m == null)) {
                error = $"Material Renderer has no shared materials: {GetPath(renderer.transform)}";
                return false;
            }

            if (!materials.Where(m => m != null).Any(m => IsFloatMaterialProperty(m, action.materialPropertyName))) {
                error =
                    $"Float or Range Material Property was not found on renderer materials: \"{action.materialPropertyName}\". " +
                    $"Renderer: {GetPath(renderer.transform)}";
                return false;
            }

            return true;
        }

        private static bool IsFloatMaterialProperty(Material material, string propertyName) {
            if (material == null || material.shader == null || string.IsNullOrWhiteSpace(propertyName)) {
                return false;
            }

            var shader = material.shader;
            for (var i = 0; i < ShaderUtil.GetPropertyCount(shader); i++) {
                if (ShaderUtil.GetPropertyName(shader, i) != propertyName) {
                    continue;
                }

                var propertyType = ShaderUtil.GetPropertyType(shader, i);
                return propertyType == ShaderUtil.ShaderPropertyType.Float ||
                    propertyType == ShaderUtil.ShaderPropertyType.Range;
            }

            return false;
        }

        private static bool ValidateSetFxFloatAction(
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            out string error
        ) {
            error = null;

            if (string.IsNullOrWhiteSpace(action.fxFloatParameterName)) {
                error = "FX Float Parameter is empty.";
                return false;
            }

            return true;
        }

        private static bool ValidateObjectToggleAction(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            out string error
        ) {
            error = null;

            var target = action.objectToggleTarget;
            if (target == null) {
                error = "Object Toggle Target is not assigned.";
                return false;
            }

            if (!IsSameOrChildOf(target.transform, avatarRoot.transform)) {
                error =
                    "Object Toggle Target must be the avatar root or one of its children. " +
                    $"Assigned object: {GetPath(target.transform)}";
                return false;
            }

            return true;
        }

        private static bool ValidateScaleAction(
            GameObject avatarRoot,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            out string error
        ) {
            error = null;

            var target = action.scaleTarget;
            if (target == null) {
                error = "Scale Target is not assigned.";
                return false;
            }

            if (!IsSameOrChildOf(target.transform, avatarRoot.transform)) {
                error =
                    "Scale Target must be the avatar root or one of its children. " +
                    $"Assigned object: {GetPath(target.transform)}";
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

        private static void CreateMaterialPropertyAction(
            SpsDepthActionBuildContext context,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            string actionName,
            BlendTree blendTree
        ) {
            var renderer = action.materialRenderer;
            var rendererPath = AnimationUtility.CalculateTransformPath(renderer.transform, context.AvatarRoot.transform);
            context.BindingPaths.Add(rendererPath);

            var nearClip = CreateFloatCurveClip(
                $"SPS Depth {actionName} Near",
                rendererPath,
                typeof(Renderer),
                $"material.{action.materialPropertyName}",
                action.materialNearValue
            );
            var farClip = CreateFloatCurveClip(
                $"SPS Depth {actionName} Far",
                rendererPath,
                typeof(Renderer),
                $"material.{action.materialPropertyName}",
                action.materialFarValue
            );
            AssetDatabase.AddObjectToAsset(nearClip, context.AnimatorController);
            AssetDatabase.AddObjectToAsset(farClip, context.AnimatorController);
            AddDepthChildren(blendTree, action, nearClip, farClip);
        }

        private static void CreateSetFxFloatAction(
            SpsDepthActionBuildContext context,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            string actionName,
            BlendTree blendTree
        ) {
            EnsureFloatParameter(context.AnimatorController, action.fxFloatParameterName);
            context.GlobalParameters.Add(action.fxFloatParameterName);
            context.BindingPaths.Add($"<FX parameter:{action.fxFloatParameterName}>");

            var nearClip = CreateFloatCurveClip(
                $"SPS Depth {actionName} Near",
                string.Empty,
                typeof(Animator),
                action.fxFloatParameterName,
                action.fxFloatNearValue
            );
            var farClip = CreateFloatCurveClip(
                $"SPS Depth {actionName} Far",
                string.Empty,
                typeof(Animator),
                action.fxFloatParameterName,
                action.fxFloatFarValue
            );
            AssetDatabase.AddObjectToAsset(nearClip, context.AnimatorController);
            AssetDatabase.AddObjectToAsset(farClip, context.AnimatorController);
            AddDepthChildren(blendTree, action, nearClip, farClip);
        }

        private static void CreateObjectToggleAction(
            SpsDepthActionBuildContext context,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            string actionName,
            BlendTree blendTree
        ) {
            var target = action.objectToggleTarget;
            var targetPath = AnimationUtility.CalculateTransformPath(target.transform, context.AvatarRoot.transform);
            context.BindingPaths.Add(string.IsNullOrEmpty(targetPath) ? "<avatar root>" : targetPath);

            var nearClip = CreateFloatCurveClip(
                $"SPS Depth {actionName} Near",
                targetPath,
                typeof(GameObject),
                "m_IsActive",
                action.objectToggleNearActive ? 1f : 0f
            );
            var farClip = CreateFloatCurveClip(
                $"SPS Depth {actionName} Far",
                targetPath,
                typeof(GameObject),
                "m_IsActive",
                action.objectToggleFarActive ? 1f : 0f
            );
            AssetDatabase.AddObjectToAsset(nearClip, context.AnimatorController);
            AssetDatabase.AddObjectToAsset(farClip, context.AnimatorController);
            AddDepthChildren(blendTree, action, nearClip, farClip);
        }

        private static void CreateScaleAction(
            SpsDepthActionBuildContext context,
            SpsDepthRuntimeFullControllerInstaller.DepthAction action,
            string actionName,
            BlendTree blendTree
        ) {
            var target = action.scaleTarget;
            var targetPath = AnimationUtility.CalculateTransformPath(target.transform, context.AvatarRoot.transform);
            context.BindingPaths.Add(string.IsNullOrEmpty(targetPath) ? "<avatar root>" : targetPath);

            var localScale = target.transform.localScale;
            var nearClip = CreateScaleClip(
                $"SPS Depth {actionName} Near",
                targetPath,
                localScale * action.scaleNearMultiplier
            );
            var farClip = CreateScaleClip(
                $"SPS Depth {actionName} Far",
                targetPath,
                localScale * action.scaleFarMultiplier
            );
            AssetDatabase.AddObjectToAsset(nearClip, context.AnimatorController);
            AssetDatabase.AddObjectToAsset(farClip, context.AnimatorController);
            AddDepthChildren(blendTree, action, nearClip, farClip);
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

            layers[layerIndex].name = SanitizeAnimatorName(layerName);
            animatorController.layers = layers;
        }

        private static string SanitizeAnimatorName(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return "SPS Depth Action";
            }

            return name.Replace(".", "_");
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

        private static AnimationClip CreateFloatCurveClip(
            string clipName,
            string path,
            Type type,
            string propertyName,
            float value
        ) {
            var clip = new AnimationClip {
                name = clipName
            };
            var binding = EditorCurveBinding.FloatCurve(path, type, propertyName);
            AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, 1f / 60f, value));
            return clip;
        }

        private static AnimationClip CreateScaleClip(string clipName, string path, Vector3 scale) {
            var clip = new AnimationClip {
                name = clipName
            };
            SetConstantFloatCurve(clip, path, typeof(Transform), "m_LocalScale.x", scale.x);
            SetConstantFloatCurve(clip, path, typeof(Transform), "m_LocalScale.y", scale.y);
            SetConstantFloatCurve(clip, path, typeof(Transform), "m_LocalScale.z", scale.z);
            return clip;
        }

        private static void SetConstantFloatCurve(
            AnimationClip clip,
            string path,
            Type type,
            string propertyName,
            float value
        ) {
            var binding = EditorCurveBinding.FloatCurve(path, type, propertyName);
            AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, 1f / 60f, value));
        }

        private static void EnsureFloatParameter(AnimatorController animatorController, string parameterName) {
            if (
                animatorController == null ||
                string.IsNullOrWhiteSpace(parameterName) ||
                animatorController.parameters.Any(p => p.name == parameterName)
            ) {
                return;
            }

            animatorController.AddParameter(parameterName, AnimatorControllerParameterType.Float);
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
