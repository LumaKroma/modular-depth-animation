// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using MDA.SpsDepth;

namespace MDA.SpsDepth.Editor {
    [CustomEditor(typeof(SpsDepthRuntimeFullControllerInstaller))]
    public sealed class SpsDepthRuntimeFullControllerInstallerEditor : UnityEditor.Editor {
        private const float SliderMin = -1f;
        private const float SliderMax = 3f;
        private const float EntranceSnapDistance = 0.03f;

        private static class PropertyNames {
            public const string ActionType = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.actionType);
            public const string ActivationDistance = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.activationDistance);
            public const string BlendShapeRenderer = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.blendShapeRenderer);
            public const string BlendShapeName = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.blendShapeName);
            public const string FarWeight = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.farWeight);
            public const string NearWeight = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.nearWeight);
            public const string AnimationClip = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.animationClip);
            public const string MaterialRenderer = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.materialRenderer);
            public const string MaterialPropertyName = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.materialPropertyName);
            public const string MaterialFarValue = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.materialFarValue);
            public const string MaterialNearValue = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.materialNearValue);
            public const string FxFloatParameterName = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.fxFloatParameterName);
            public const string FxFloatFarValue = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.fxFloatFarValue);
            public const string FxFloatNearValue = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.fxFloatNearValue);
            public const string ObjectToggleTarget = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.objectToggleTarget);
            public const string ObjectToggleFarActive = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.objectToggleFarActive);
            public const string ObjectToggleNearActive = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.objectToggleNearActive);
            public const string ScaleTarget = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.scaleTarget);
            public const string ScaleFarMultiplier = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.scaleFarMultiplier);
            public const string ScaleNearMultiplier = nameof(SpsDepthRuntimeFullControllerInstaller.DepthAction.scaleNearMultiplier);
        }

        private int activeSliderControl;
        private int activeSliderHandle;
        private Rect lastActivationSliderRect;

        private SerializedProperty socketPreset;
        private SerializedProperty targetSocketObject;
        private SerializedProperty depthActions;

        private void OnEnable() {
            socketPreset = serializedObject.FindProperty(nameof(SpsDepthRuntimeFullControllerInstaller.socketPreset));
            targetSocketObject = serializedObject.FindProperty(nameof(SpsDepthRuntimeFullControllerInstaller.targetSocketObject));
            depthActions = serializedObject.FindProperty(nameof(SpsDepthRuntimeFullControllerInstaller.depthActions));
        }

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();
            root.Add(SpsDepthComponentHeader.CreateHeaderOverlay(SpsDepthLocalization.T(SpsDepthLocalization.Key.ComponentHeaderTitle), Repaint));
            root.Add(new IMGUIContainer(DrawInspectorBody));
            return root;
        }

        public override void OnInspectorGUI() {
            DrawInspectorBody();
        }

        private void DrawInspectorBody() {
            if (!EnsureSerializedProperties()) {
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();

            DrawTargetSocket();
            DrawDepthActions();

            serializedObject.ApplyModifiedProperties();
        }

        private bool EnsureSerializedProperties() {
            socketPreset = socketPreset ?? serializedObject.FindProperty(nameof(SpsDepthRuntimeFullControllerInstaller.socketPreset));
            targetSocketObject = targetSocketObject ?? serializedObject.FindProperty(nameof(SpsDepthRuntimeFullControllerInstaller.targetSocketObject));
            depthActions = depthActions ?? serializedObject.FindProperty(nameof(SpsDepthRuntimeFullControllerInstaller.depthActions));

            return socketPreset != null && targetSocketObject != null && depthActions != null;
        }

        private void DrawTargetSocket() {
            EditorGUILayout.LabelField(SpsDepthLocalization.T(SpsDepthLocalization.Key.TargetSocket), EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                SpsDepthLocalization.T(SpsDepthLocalization.Key.TargetSocketHelp),
                MessageType.Info
            );

            DrawSocketPresetPopup();
            if (GUILayout.Button(SpsDepthLocalization.T(SpsDepthLocalization.Key.AutoDetectSocket))) {
                AutoDetectSocket();
            }

            DrawTargetSocketObjectField();
        }

        private void DrawSocketPresetPopup() {
            var labels = SpsDepthLocalization.SocketPresetLabels;
            socketPreset.enumValueIndex = EditorGUILayout.Popup(
                SpsDepthLocalization.Content(SpsDepthLocalization.Key.AutoDetectType),
                Mathf.Clamp(socketPreset.enumValueIndex, 0, labels.Length - 1),
                labels
            );
        }

        private void DrawTargetSocketObjectField() {
            var current = targetSocketObject.objectReferenceValue as GameObject;
            var hasMissingReference = targetSocketObject.objectReferenceValue == null &&
                targetSocketObject.objectReferenceInstanceIDValue != 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var next = EditorGUILayout.ObjectField(
                SpsDepthLocalization.Content(SpsDepthLocalization.Key.TargetSocketObject),
                current,
                typeof(GameObject),
                true
            ) as GameObject;
            var changed = EditorGUI.EndChangeCheck();
            var reset = GUILayout.Button(SpsDepthLocalization.T(SpsDepthLocalization.Key.Reset), GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            if (reset) {
                targetSocketObject.objectReferenceValue = null;
                return;
            }

            if (hasMissingReference) {
                EditorGUILayout.HelpBox(
                    SpsDepthLocalization.T(SpsDepthLocalization.Key.TargetSocketObjectMissing),
                    MessageType.Warning
                );
            }

            if (!changed) {
                return;
            }

            if (next == null) {
                targetSocketObject.objectReferenceValue = null;
                return;
            }

            if (SpsDepthSocketResolver.IsValidSocketObject(next, out var error)) {
                targetSocketObject.objectReferenceValue = next;
                return;
            }

            EditorUtility.DisplayDialog(
                SpsDepthLocalization.T(SpsDepthLocalization.Key.TargetSocketObjectInvalidTitle),
                SpsDepthLocalization.Format(SpsDepthLocalization.Key.TargetSocketObjectInvalidMessage, error),
                SpsDepthLocalization.T(SpsDepthLocalization.Key.Ok)
            );
            targetSocketObject.objectReferenceValue = current;
        }

        private void AutoDetectSocket() {
            serializedObject.ApplyModifiedProperties();

            var installer = target as SpsDepthRuntimeFullControllerInstaller;
            if (SpsDepthSocketResolver.TryAutoDetectForInspector(installer, out var socketObject, out var message)) {
                Undo.RecordObject(installer, SpsDepthLocalization.T(SpsDepthLocalization.Key.AutoDetectUndo));
                installer.targetSocketObject = socketObject;
                EditorUtility.SetDirty(installer);
                serializedObject.Update();
                Debug.Log($"[MDA Installer: {installer.name}] {message}", installer);
                return;
            }

            EditorUtility.DisplayDialog(
                SpsDepthLocalization.T(SpsDepthLocalization.Key.AutoDetectDialogTitle),
                message,
                SpsDepthLocalization.T(SpsDepthLocalization.Key.Ok)
            );
        }

        private void DrawDepthActions() {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(SpsDepthLocalization.T(SpsDepthLocalization.Key.DepthActions), EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                SpsDepthLocalization.T(SpsDepthLocalization.Key.DepthActionsHelp),
                MessageType.Info
            );

            if (depthActions.arraySize == 0) {
                EditorGUILayout.HelpBox(
                    SpsDepthLocalization.T(SpsDepthLocalization.Key.DepthActionsEmpty),
                    MessageType.Warning
                );
            }

            for (var i = 0; i < depthActions.arraySize; i++) {
                DrawDepthAction(depthActions.GetArrayElementAtIndex(i), i);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(SpsDepthLocalization.T(SpsDepthLocalization.Key.AddAction))) {
                AddAction();
            }
            using (new EditorGUI.DisabledScope(depthActions.arraySize == 0)) {
                if (GUILayout.Button(SpsDepthLocalization.T(SpsDepthLocalization.Key.ClearActions))) {
                    if (EditorUtility.DisplayDialog(
                        SpsDepthLocalization.T(SpsDepthLocalization.Key.ClearActionsTitle),
                        SpsDepthLocalization.T(SpsDepthLocalization.Key.ClearActionsMessage),
                        SpsDepthLocalization.T(SpsDepthLocalization.Key.ClearActions),
                        SpsDepthLocalization.T(SpsDepthLocalization.Key.Cancel)
                    )) {
                        depthActions.ClearArray();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDepthAction(SerializedProperty action, int index) {
            var actionType = action.FindPropertyRelative(PropertyNames.ActionType);
            var activationDistance = action.FindPropertyRelative(PropertyNames.ActivationDistance);
            var actionRenderer = action.FindPropertyRelative(PropertyNames.BlendShapeRenderer);
            var actionBlendShapeName = action.FindPropertyRelative(PropertyNames.BlendShapeName);
            var farWeight = action.FindPropertyRelative(PropertyNames.FarWeight);
            var nearWeight = action.FindPropertyRelative(PropertyNames.NearWeight);
            var animationClip = action.FindPropertyRelative(PropertyNames.AnimationClip);
            var materialRenderer = action.FindPropertyRelative(PropertyNames.MaterialRenderer);
            var materialPropertyName = action.FindPropertyRelative(PropertyNames.MaterialPropertyName);
            var materialFarValue = action.FindPropertyRelative(PropertyNames.MaterialFarValue);
            var materialNearValue = action.FindPropertyRelative(PropertyNames.MaterialNearValue);
            var fxFloatParameterName = action.FindPropertyRelative(PropertyNames.FxFloatParameterName);
            var fxFloatFarValue = action.FindPropertyRelative(PropertyNames.FxFloatFarValue);
            var fxFloatNearValue = action.FindPropertyRelative(PropertyNames.FxFloatNearValue);
            var objectToggleTarget = action.FindPropertyRelative(PropertyNames.ObjectToggleTarget);
            var objectToggleFarActive = action.FindPropertyRelative(PropertyNames.ObjectToggleFarActive);
            var objectToggleNearActive = action.FindPropertyRelative(PropertyNames.ObjectToggleNearActive);
            var scaleTarget = action.FindPropertyRelative(PropertyNames.ScaleTarget);
            var scaleFarMultiplier = action.FindPropertyRelative(PropertyNames.ScaleFarMultiplier);
            var scaleNearMultiplier = action.FindPropertyRelative(PropertyNames.ScaleNearMultiplier);

            var title = $"{index + 1}. {GetActionTitle(action)}";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            action.isExpanded = EditorGUILayout.Foldout(action.isExpanded, title, true);
            if (GUILayout.Button(SpsDepthLocalization.T(SpsDepthLocalization.Key.Remove), GUILayout.Width(70))) {
                depthActions.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (action.isExpanded) {
                DrawActionTypePopup(actionType);
                DrawActivationDistance(activationDistance);

                switch ((SpsDepthRuntimeFullControllerInstaller.DepthActionType)actionType.enumValueIndex) {
                    case SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape:
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(SpsDepthLocalization.T(SpsDepthLocalization.Key.BlendShape), EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(actionRenderer, SpsDepthLocalization.Content(SpsDepthLocalization.Key.BlendShapeRenderer));
                        DrawBlendShapeName(actionRenderer, actionBlendShapeName);
                        EditorGUILayout.PropertyField(farWeight, SpsDepthLocalization.Content(SpsDepthLocalization.Key.FarWeight));
                        EditorGUILayout.PropertyField(nearWeight, SpsDepthLocalization.Content(SpsDepthLocalization.Key.NearWeight));
                        break;
                    case SpsDepthRuntimeFullControllerInstaller.DepthActionType.AnimationClip:
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(SpsDepthLocalization.T(SpsDepthLocalization.Key.AnimationClip), EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox(
                            SpsDepthLocalization.T(SpsDepthLocalization.Key.ClipHelp),
                            MessageType.Info
                        );
                        EditorGUILayout.PropertyField(animationClip, SpsDepthLocalization.Content(SpsDepthLocalization.Key.Clip));
                        break;
                    case SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialProperty:
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(SpsDepthLocalization.T(SpsDepthLocalization.Key.MaterialProperty), EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox(
                            SpsDepthLocalization.T(SpsDepthLocalization.Key.MaterialPropertyHelp),
                            MessageType.Info
                        );
                        EditorGUILayout.PropertyField(materialRenderer, SpsDepthLocalization.Content(SpsDepthLocalization.Key.MaterialRenderer));
                        EditorGUILayout.PropertyField(materialPropertyName, SpsDepthLocalization.Content(SpsDepthLocalization.Key.MaterialPropertyName));
                        EditorGUILayout.PropertyField(materialFarValue, SpsDepthLocalization.Content(SpsDepthLocalization.Key.FarValue));
                        EditorGUILayout.PropertyField(materialNearValue, SpsDepthLocalization.Content(SpsDepthLocalization.Key.NearValue));
                        break;
                    case SpsDepthRuntimeFullControllerInstaller.DepthActionType.SetFxFloat:
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(SpsDepthLocalization.T(SpsDepthLocalization.Key.SetFxFloat), EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox(
                            SpsDepthLocalization.T(SpsDepthLocalization.Key.SetFxFloatHelp),
                            MessageType.Info
                        );
                        EditorGUILayout.PropertyField(fxFloatParameterName, SpsDepthLocalization.Content(SpsDepthLocalization.Key.FxFloatParameterName));
                        EditorGUILayout.PropertyField(fxFloatFarValue, SpsDepthLocalization.Content(SpsDepthLocalization.Key.FarValue));
                        EditorGUILayout.PropertyField(fxFloatNearValue, SpsDepthLocalization.Content(SpsDepthLocalization.Key.NearValue));
                        break;
                    case SpsDepthRuntimeFullControllerInstaller.DepthActionType.ObjectToggle:
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(SpsDepthLocalization.T(SpsDepthLocalization.Key.ObjectToggle), EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox(
                            SpsDepthLocalization.T(SpsDepthLocalization.Key.ObjectToggleHelp),
                            MessageType.Info
                        );
                        EditorGUILayout.PropertyField(objectToggleTarget, SpsDepthLocalization.Content(SpsDepthLocalization.Key.ObjectToggleTarget));
                        EditorGUILayout.PropertyField(objectToggleFarActive, SpsDepthLocalization.Content(SpsDepthLocalization.Key.FarActive));
                        EditorGUILayout.PropertyField(objectToggleNearActive, SpsDepthLocalization.Content(SpsDepthLocalization.Key.NearActive));
                        break;
                    case SpsDepthRuntimeFullControllerInstaller.DepthActionType.Scale:
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(SpsDepthLocalization.T(SpsDepthLocalization.Key.Scale), EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox(
                            SpsDepthLocalization.T(SpsDepthLocalization.Key.ScaleHelp),
                            MessageType.Info
                        );
                        EditorGUILayout.PropertyField(scaleTarget, SpsDepthLocalization.Content(SpsDepthLocalization.Key.ScaleTarget));
                        EditorGUILayout.PropertyField(scaleFarMultiplier, SpsDepthLocalization.Content(SpsDepthLocalization.Key.FarScaleMultiplier));
                        EditorGUILayout.PropertyField(scaleNearMultiplier, SpsDepthLocalization.Content(SpsDepthLocalization.Key.NearScaleMultiplier));
                        break;
                }
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawActionTypePopup(SerializedProperty actionType) {
            var labels = new[] {
                SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape),
                SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.AnimationClip),
                SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialProperty),
                SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.SetFxFloat),
                SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.ObjectToggle),
                SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.Scale)
            };
            actionType.enumValueIndex = EditorGUILayout.Popup(
                SpsDepthLocalization.Content(SpsDepthLocalization.Key.ActionType),
                Mathf.Clamp(actionType.enumValueIndex, 0, labels.Length - 1),
                labels
            );
        }

        private void DrawActivationDistance(SerializedProperty activationDistance) {
            var range = activationDistance.vector2Value;
            range.x = Mathf.Min(range.x, range.y);
            range.y = Mathf.Max(range.x, range.y);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                SpsDepthLocalization.Content(SpsDepthLocalization.Key.ActivationDistance, SpsDepthLocalization.Key.ActivationDistanceTooltip)
            );
            if (GUILayout.Button(SpsDepthLocalization.T(SpsDepthLocalization.Key.Reset), GUILayout.Width(70))) {
                range = new Vector2(-0.25f, 0f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            range.x = EditorGUILayout.DelayedFloatField(range.x, GUILayout.Width(55));
            DrawHandleSlider(ref range.x, ref range.y);
            range.y = EditorGUILayout.DelayedFloatField(range.y, GUILayout.Width(55));
            EditorGUILayout.EndHorizontal();

            DrawActivationDistanceLabels();

            var min = Mathf.Min(range.x, range.y);
            var max = Mathf.Max(range.x, range.y);
            range.x = SnapActivationDistance(Mathf.Clamp(min, SliderMin, SliderMax));
            range.y = SnapActivationDistance(Mathf.Clamp(max, SliderMin, SliderMax));
            activationDistance.vector2Value = range;
        }

        private void DrawHandleSlider(ref float nearDistance, ref float farDistance) {
            var sliderRect = GUILayoutUtility.GetRect(80f, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
            lastActivationSliderRect = sliderRect;
            var controlId = GUIUtility.GetControlID(FocusType.Passive, sliderRect);
            var evt = Event.current;

            EditorGUI.DrawRect(
                new Rect(sliderRect.x, sliderRect.center.y - 1f, sliderRect.width, 2f),
                new Color(0.35f, 0.35f, 0.35f)
            );

            var nearX = ValueToSliderX(nearDistance, sliderRect);
            var farX = ValueToSliderX(farDistance, sliderRect);
            var selectionRect = Rect.MinMaxRect(
                Mathf.Min(nearX, farX),
                sliderRect.center.y - 3f,
                Mathf.Max(nearX, farX),
                sliderRect.center.y + 3f
            );
            EditorGUI.DrawRect(selectionRect, new Color(0.65f, 0.65f, 0.65f));
            DrawEntranceTick(sliderRect);
            DrawSliderHandle(nearX, sliderRect);
            DrawSliderHandle(farX, sliderRect);

            switch (evt.GetTypeForControl(controlId)) {
                case EventType.MouseDown:
                    if (evt.button == 0 && sliderRect.Contains(evt.mousePosition)) {
                        GUIUtility.hotControl = controlId;
                        activeSliderControl = controlId;
                        activeSliderHandle = Mathf.Abs(evt.mousePosition.x - nearX) <= Mathf.Abs(evt.mousePosition.x - farX) ? 0 : 1;
                        UpdateSliderValue(evt.mousePosition.x, sliderRect, ref nearDistance, ref farDistance);
                        evt.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId && activeSliderControl == controlId) {
                        UpdateSliderValue(evt.mousePosition.x, sliderRect, ref nearDistance, ref farDistance);
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId) {
                        GUIUtility.hotControl = 0;
                        activeSliderControl = 0;
                        activeSliderHandle = 0;
                        evt.Use();
                    }
                    break;
                case EventType.Repaint:
                    EditorGUIUtility.AddCursorRect(sliderRect, MouseCursor.SlideArrow);
                    break;
            }
        }

        private void DrawActivationDistanceLabels() {
            var rowRect = GUILayoutUtility.GetRect(0f, EditorGUIUtility.singleLineHeight);
            if (lastActivationSliderRect.width <= 0) {
                return;
            }

            var entranceX = ValueToSliderX(0f, lastActivationSliderRect);
            GUI.Label(
                new Rect(lastActivationSliderRect.xMin - 4f, rowRect.y, 100f, rowRect.height),
                SpsDepthLocalization.T(SpsDepthLocalization.Key.FullyInserted),
                EditorStyles.miniLabel
            );
            GUI.Label(
                new Rect(entranceX - 28f, rowRect.y, 70f, rowRect.height),
                SpsDepthLocalization.T(SpsDepthLocalization.Key.Entrance),
                EditorStyles.miniLabel
            );
            GUI.Label(
                new Rect(lastActivationSliderRect.xMax - 48f, rowRect.y, 60f, rowRect.height),
                SpsDepthLocalization.T(SpsDepthLocalization.Key.FarAway),
                EditorStyles.miniLabel
            );
        }

        private void UpdateSliderValue(float mouseX, Rect sliderRect, ref float nearDistance, ref float farDistance) {
            var value = SnapActivationDistance(SliderXToValue(mouseX, sliderRect));
            if (activeSliderHandle == 0) {
                nearDistance = Mathf.Min(value, farDistance);
            } else {
                farDistance = Mathf.Max(value, nearDistance);
            }
            Repaint();
        }

        private static void DrawEntranceTick(Rect sliderRect) {
            var x = ValueToSliderX(0f, sliderRect);
            EditorGUI.DrawRect(
                new Rect(x - 1f, sliderRect.center.y - 8f, 2f, 16f),
                new Color(0.95f, 0.95f, 0.95f)
            );
        }

        private static void DrawSliderHandle(float x, Rect sliderRect) {
            var handleRect = new Rect(x - 4f, sliderRect.center.y - 6f, 8f, 12f);
            EditorGUI.DrawRect(handleRect, new Color(0.78f, 0.78f, 0.78f));
        }

        private static float ValueToSliderX(float value, Rect sliderRect) {
            var t = Mathf.InverseLerp(SliderMin, SliderMax, value);
            return Mathf.Lerp(sliderRect.xMin, sliderRect.xMax, t);
        }

        private static float SliderXToValue(float x, Rect sliderRect) {
            var t = Mathf.InverseLerp(sliderRect.xMin, sliderRect.xMax, x);
            return Mathf.Lerp(SliderMin, SliderMax, Mathf.Clamp01(t));
        }

        private static float SnapActivationDistance(float value) {
            if (Mathf.Abs(value) <= EntranceSnapDistance) {
                return 0f;
            }

            return (float)System.Math.Round(value, 4);
        }

        private void AddAction() {
            var index = depthActions.arraySize;
            depthActions.InsertArrayElementAtIndex(index);

            var action = depthActions.GetArrayElementAtIndex(index);
            action.isExpanded = true;
            InitializeAction(action, SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape);
        }

        private static void InitializeAction(
            SerializedProperty action,
            SpsDepthRuntimeFullControllerInstaller.DepthActionType actionType
        ) {
            action.FindPropertyRelative(PropertyNames.ActionType).enumValueIndex = (int)actionType;
            action.FindPropertyRelative(PropertyNames.ActivationDistance).vector2Value = new Vector2(-0.25f, 0f);

            action.FindPropertyRelative(PropertyNames.BlendShapeRenderer).objectReferenceValue = null;
            action.FindPropertyRelative(PropertyNames.BlendShapeName).stringValue = string.Empty;
            action.FindPropertyRelative(PropertyNames.FarWeight).floatValue = 0f;
            action.FindPropertyRelative(PropertyNames.NearWeight).floatValue = 100f;

            action.FindPropertyRelative(PropertyNames.AnimationClip).objectReferenceValue = null;

            action.FindPropertyRelative(PropertyNames.MaterialRenderer).objectReferenceValue = null;
            action.FindPropertyRelative(PropertyNames.MaterialPropertyName).stringValue = string.Empty;
            action.FindPropertyRelative(PropertyNames.MaterialFarValue).floatValue = 0f;
            action.FindPropertyRelative(PropertyNames.MaterialNearValue).floatValue = 1f;

            action.FindPropertyRelative(PropertyNames.FxFloatParameterName).stringValue = string.Empty;
            action.FindPropertyRelative(PropertyNames.FxFloatFarValue).floatValue = 0f;
            action.FindPropertyRelative(PropertyNames.FxFloatNearValue).floatValue = 1f;

            action.FindPropertyRelative(PropertyNames.ObjectToggleTarget).objectReferenceValue = null;
            action.FindPropertyRelative(PropertyNames.ObjectToggleFarActive).boolValue = false;
            action.FindPropertyRelative(PropertyNames.ObjectToggleNearActive).boolValue = true;

            action.FindPropertyRelative(PropertyNames.ScaleTarget).objectReferenceValue = null;
            action.FindPropertyRelative(PropertyNames.ScaleFarMultiplier).floatValue = 1f;
            action.FindPropertyRelative(PropertyNames.ScaleNearMultiplier).floatValue = 1f;
        }

        private static string GetActionTitle(SerializedProperty action) {
            var actionType = action.FindPropertyRelative(PropertyNames.ActionType);
            var blendShapeName = action.FindPropertyRelative(PropertyNames.BlendShapeName);
            var animationClip = action.FindPropertyRelative(PropertyNames.AnimationClip);
            var materialPropertyName = action.FindPropertyRelative(PropertyNames.MaterialPropertyName);
            var fxFloatParameterName = action.FindPropertyRelative(PropertyNames.FxFloatParameterName);
            var objectToggleTarget = action.FindPropertyRelative(PropertyNames.ObjectToggleTarget);
            var scaleTarget = action.FindPropertyRelative(PropertyNames.ScaleTarget);
            var title = GetActionTypeLabel(actionType.enumValueIndex);

            if (
                actionType.enumValueIndex == (int)SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape &&
                !string.IsNullOrWhiteSpace(blendShapeName.stringValue)
            ) {
                return $"{title}: {blendShapeName.stringValue}";
            }

            if (
                actionType.enumValueIndex == (int)SpsDepthRuntimeFullControllerInstaller.DepthActionType.AnimationClip &&
                animationClip.objectReferenceValue != null
            ) {
                return $"{title}: {animationClip.objectReferenceValue.name}";
            }

            if (
                actionType.enumValueIndex == (int)SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialProperty &&
                !string.IsNullOrWhiteSpace(materialPropertyName.stringValue)
            ) {
                return $"{title}: {materialPropertyName.stringValue}";
            }

            if (
                actionType.enumValueIndex == (int)SpsDepthRuntimeFullControllerInstaller.DepthActionType.SetFxFloat &&
                !string.IsNullOrWhiteSpace(fxFloatParameterName.stringValue)
            ) {
                return $"{title}: {fxFloatParameterName.stringValue}";
            }

            if (
                actionType.enumValueIndex == (int)SpsDepthRuntimeFullControllerInstaller.DepthActionType.ObjectToggle &&
                objectToggleTarget.objectReferenceValue != null
            ) {
                return $"{title}: {objectToggleTarget.objectReferenceValue.name}";
            }

            if (
                actionType.enumValueIndex == (int)SpsDepthRuntimeFullControllerInstaller.DepthActionType.Scale &&
                scaleTarget.objectReferenceValue != null
            ) {
                return $"{title}: {scaleTarget.objectReferenceValue.name}";
            }

            return title;
        }

        private static string GetActionTypeLabel(int enumValueIndex) {
            switch ((SpsDepthRuntimeFullControllerInstaller.DepthActionType)enumValueIndex) {
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape:
                    return SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.BlendShape);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.AnimationClip:
                    return SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.AnimationClip);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialProperty:
                    return SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.MaterialProperty);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.SetFxFloat:
                    return SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.SetFxFloat);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.ObjectToggle:
                    return SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.ObjectToggle);
                case SpsDepthRuntimeFullControllerInstaller.DepthActionType.Scale:
                    return SpsDepthLocalization.ActionTypeLabel(SpsDepthRuntimeFullControllerInstaller.DepthActionType.Scale);
                default:
                    return enumValueIndex.ToString();
            }
        }

        private void DrawBlendShapeName(SerializedProperty rendererProperty, SerializedProperty blendShapeNameProperty) {
            var renderer = rendererProperty.objectReferenceValue as SkinnedMeshRenderer;
            var mesh = renderer != null ? renderer.sharedMesh : null;

            if (mesh == null || mesh.blendShapeCount == 0) {
                EditorGUILayout.PropertyField(blendShapeNameProperty, SpsDepthLocalization.Content(SpsDepthLocalization.Key.BlendShapeName));
                return;
            }

            var names = Enumerable.Range(0, mesh.blendShapeCount)
                .Select(mesh.GetBlendShapeName)
                .ToArray();
            var currentIndex = Mathf.Max(0, System.Array.IndexOf(names, blendShapeNameProperty.stringValue));
            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, SpsDepthLocalization.Content(SpsDepthLocalization.Key.BlendShapeName));
            var nextIndex = EditorGUI.Popup(rect, currentIndex, names);

            blendShapeNameProperty.stringValue = names[nextIndex];
        }
    }
}
#endif
