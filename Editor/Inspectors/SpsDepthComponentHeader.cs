// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MDA.SpsDepth.Editor {
    internal static class SpsDepthComponentHeader {
        private static VisualElement FindEditor(VisualElement element) {
            if (element == null) return null;
            if (element is InspectorElement) return element.parent;
            return FindEditor(element.parent);
        }

        public static VisualElement CreateHeaderOverlay(string title, System.Action repaint) {
            var marker = new VisualElement();
            marker.RegisterCallback<AttachToPanelEvent>(_ => AttachHeaderOverlay(marker, title, repaint));
            return marker;
        }

        private static void AttachHeaderOverlay(VisualElement body, string title, System.Action repaint) {
            var inspectorRoot = FindEditor(body);
            if (inspectorRoot == null) {
                return;
            }

            var headerIndex = inspectorRoot.Children()
                .Select((element, index) => new { element, index })
                .Where(x => x.element.name.EndsWith("Header"))
                .Select(x => x.index)
                .DefaultIfEmpty(-1)
                .First();

            if (headerIndex < 0) {
                return;
            }

            var headerOverlays = RenderHeader(title, repaint);
            for (var index = 0; index < headerOverlays.Length; index++) {
                inspectorRoot.Insert(headerIndex + 1 + index, headerOverlays[index]);
            }

            body.RegisterCallback<DetachFromPanelEvent>(_ => {
                foreach (var headerOverlay in headerOverlays) {
                    headerOverlay.parent?.Remove(headerOverlay);
                }
            });
        }

        private static VisualElement[] RenderHeader(string title, System.Action repaint) {
            var headerArea = new VisualElement {
                style = {
                    height = 20,
                    width = Length.Percent(100),
                    position = Position.Relative,
                },
                pickingMode = PickingMode.Ignore
            };

            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    height = 20,
                    backgroundColor = new StyleColor(EditorGUIUtility.isProSkin
                        ? new Color32(61, 61, 61, 255)
                        : new Color32(194, 194, 194, 255)),
                    marginLeft = 18,
                    marginRight = 60,
                },
                pickingMode = PickingMode.Ignore
            };
            headerArea.Add(row);

            var logoColor = new Color(0.05f, 0.05f, 0.05f);
            var triangleLeft = new VisualElement {
                style = {
                    borderRightColor = new StyleColor(logoColor),
                    borderBottomColor = new StyleColor(logoColor),
                    borderLeftWidth = 5,
                    borderTopWidth = 10,
                    borderRightWidth = 5,
                    borderBottomWidth = 10,
                },
                pickingMode = PickingMode.Ignore
            };
            triangleLeft.style.flexShrink = 0;
            row.Add(triangleLeft);

            var logo = new Label("MDA Fury") {
                style = {
                    color = new StyleColor(new Color(1f, 0.43f, 0.02f)),
                    backgroundColor = new StyleColor(logoColor),
                    paddingLeft = 3,
                    paddingRight = 3,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    flexShrink = 0,
                },
                pickingMode = PickingMode.Ignore
            };
            row.Add(logo);

            var triangleRight = new VisualElement {
                style = {
                    borderLeftColor = new StyleColor(logoColor),
                    borderTopColor = new StyleColor(logoColor),
                    borderLeftWidth = 5,
                    borderTopWidth = 10,
                    borderRightWidth = 5,
                    borderBottomWidth = 10,
                },
                pickingMode = PickingMode.Ignore
            };
            triangleRight.style.flexShrink = 0;
            row.Add(triangleRight);

            var name = new Label(title) {
                style = {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingLeft = 3,
                    flexGrow = 1,
                },
                pickingMode = PickingMode.Ignore
            };
            row.Add(name);

            var visualWrapper = new VisualElement {
                style = {
                    height = 20,
                    marginBottom = -20,
                    top = -21,
                    position = Position.Relative,
                },
                pickingMode = PickingMode.Ignore
            };
            visualWrapper.Add(headerArea);

            var languageControls = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    height = 20,
                    marginBottom = -20,
                    top = -21,
                    position = Position.Relative,
                    width = 150,
                    alignSelf = Align.FlexEnd,
                    marginRight = 62,
                },
                pickingMode = PickingMode.Position
            };

            var languageLabel = new Label(SpsDepthLocalization.T(SpsDepthLocalization.Key.Language)) {
                style = {
                    unityTextAlign = TextAnchor.MiddleRight,
                    marginRight = 4,
                    color = new StyleColor(EditorGUIUtility.isProSkin
                        ? new Color(0.78f, 0.78f, 0.78f)
                        : new Color(0.25f, 0.25f, 0.25f)),
                },
                pickingMode = PickingMode.Ignore
            };
            languageControls.Add(languageLabel);

            var languagePopup = new PopupField<string>(
                SpsDepthLocalization.LanguageModeLabels.ToList(),
                (int)SpsDepthLocalization.Mode
            ) {
                style = {
                    width = 100,
                    height = 18,
                    marginTop = 1,
                    marginRight = 2,
                },
                pickingMode = PickingMode.Position
            };
            languagePopup.RegisterValueChangedCallback(evt => {
                var labels = SpsDepthLocalization.LanguageModeLabels;
                var next = System.Array.IndexOf(labels, evt.newValue);
                if (next < 0) return;

                SpsDepthLocalization.Mode = (SpsDepthLocalization.LanguageMode)next;
                var updatedLabels = SpsDepthLocalization.LanguageModeLabels.ToList();
                languageLabel.text = SpsDepthLocalization.T(SpsDepthLocalization.Key.Language);
                languagePopup.choices = updatedLabels;
                languagePopup.SetValueWithoutNotify(updatedLabels[next]);
                repaint?.Invoke();
            });
            languageControls.Add(languagePopup);

            return new[] { visualWrapper, languageControls };
        }
    }
}
#endif
