// Copyright (c) 2026 LumaKroma
// SPDX-License-Identifier: MIT

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MDA.SpsDepth.Editor {
    internal static class SpsDepthVrcfuryFullControllerFactory {
        public static bool TryCreate(
            GameObject targetObject,
            GameObject avatarRoot,
            RuntimeAnimatorController controller,
            string injectSpsDepthParam,
            IEnumerable<string> globalParameters,
            out string error
        ) {
            error = null;

            if (targetObject == null) {
                error = "Target object is null.";
                return false;
            }

            if (controller == null) {
                error = "RuntimeAnimatorController is null.";
                return false;
            }

            var vrcfuryType = FindType("VF.Model.VRCFury");
            if (vrcfuryType == null) {
                error = "Could not find VRCFury component type.";
                return false;
            }

            var fullControllerType = FindType("VF.Model.Feature.FullController");
            if (fullControllerType == null) {
                error = "Could not find VRCFury FullController feature type.";
                return false;
            }

            var vrcfuryComponent = targetObject.GetComponent(vrcfuryType) ?? targetObject.AddComponent(vrcfuryType);
            if (vrcfuryComponent == null) {
                error = $"Failed to add VRCFury component of type {vrcfuryType.FullName}.";
                return false;
            }

            object featureInstance;
            try {
                featureInstance = Activator.CreateInstance(fullControllerType);
            } catch (Exception e) {
                error = $"Failed to instantiate {fullControllerType.FullName}.\n{e}";
                return false;
            }

            if (!SetControllerList(featureInstance, controller, out error)) {
                return false;
            }

            SetField(featureInstance, "rootObjOverride", avatarRoot);
            SetField(featureInstance, "rootBindingsApplyToAvatar", true);
            SetGlobalParameters(featureInstance, globalParameters);

            if (!SetField(featureInstance, "injectSpsDepthParam", injectSpsDepthParam)) {
                error = $"Could not assign injectSpsDepthParam on {fullControllerType.FullName}.";
                return false;
            }

            if (!SetField(vrcfuryComponent, "content", featureInstance)) {
                error = $"Could not assign {fullControllerType.FullName} content on {vrcfuryType.FullName}.";
                return false;
            }

            EditorUtility.SetDirty(vrcfuryComponent);
            TryMarkVrcfuryDirty(vrcfuryComponent);
            return true;
        }

        private static void SetGlobalParameters(object fullController, IEnumerable<string> globalParameters) {
            if (fullController == null || globalParameters == null) {
                return;
            }

            var listField = fullController.GetType().GetField(
                "globalParams",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            var list = listField?.GetValue(fullController) as IList;
            if (list == null) {
                return;
            }

            foreach (var parameter in globalParameters.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct()) {
                if (!list.Contains(parameter)) {
                    list.Add(parameter);
                }
            }
        }

        private static bool SetControllerList(
            object fullController,
            RuntimeAnimatorController controller,
            out string error
        ) {
            error = null;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var listField = fullController.GetType().GetField("controllers", flags);
            if (listField == null) {
                error = "Could not find FullController.controllers.";
                return false;
            }

            var elementType = GetListElementType(listField.FieldType);
            if (elementType == null) {
                error = $"FullController.controllers is not a supported list type: {listField.FieldType.FullName}";
                return false;
            }

            var controllerField = elementType.GetField("controller", flags);
            if (controllerField == null) {
                error = $"Could not find {elementType.FullName}.controller.";
                return false;
            }

            object entry;
            object wrapper;
            try {
                entry = Activator.CreateInstance(elementType);
                wrapper = Activator.CreateInstance(controllerField.FieldType);
            } catch (Exception e) {
                error = $"Failed to create VRCFury controller entry.\n{e}";
                return false;
            }

            if (!SetField(wrapper, "objRef", controller)) {
                error = $"Could not assign objRef on {controllerField.FieldType.FullName}.";
                return false;
            }
            SetField(wrapper, "id", string.Empty);
            controllerField.SetValue(entry, wrapper);

            var list = listField.GetValue(fullController) as IList;
            if (list == null) {
                try {
                    list = (IList)Activator.CreateInstance(listField.FieldType);
                    listField.SetValue(fullController, list);
                } catch (Exception e) {
                    error = $"Failed to create FullController.controllers list.\n{e}";
                    return false;
                }
            }

            list.Clear();
            list.Add(entry);
            return true;
        }

        private static bool SetField(object target, string fieldName, object value) {
            if (target == null || string.IsNullOrWhiteSpace(fieldName)) {
                return false;
            }

            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            if (field == null) {
                return false;
            }

            if (value != null && !field.FieldType.IsInstanceOfType(value)) {
                return false;
            }

            field.SetValue(target, value);
            return true;
        }

        private static Type GetListElementType(Type listType) {
            if (listType == null) {
                return null;
            }

            if (listType.IsGenericType && listType.GetGenericArguments().Length == 1) {
                return listType.GetGenericArguments()[0];
            }

            return listType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IList<>))
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();
        }

        private static void TryMarkVrcfuryDirty(Component vrcfuryComponent) {
            if (vrcfuryComponent == null) return;

            var markDirty = vrcfuryComponent
                .GetType()
                .GetMethod(
                    "MarkDirty",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(Component) },
                    null
                );
            if (markDirty == null) return;

            try {
                markDirty.Invoke(null, new object[] { vrcfuryComponent });
            } catch {
                // EditorUtility.SetDirty above is enough for the build path; VRCFury's marker is best-effort.
            }
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
    }
}
#endif
