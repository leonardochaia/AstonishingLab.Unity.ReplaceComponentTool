using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AstonishingLab.Core.Editor;

namespace AstonishingLab.ReplaceComponentTool.Editor
{
    public class ReplaceComponentToolEditorWindow : TypeSelectionEditorWindow
    {
        private static Type componentTypeToReplace;
        private static bool hasAnyPrefabSelected;
        private static bool addAtSameOrder = false;

        [MenuItem("CONTEXT/Component/Replace with sub-type")]
        private static void MoveToTop(MenuCommand command)
        {
            var componentType = command.context.GetType();

            var subTypes = AstonishingEditorUtils.GetValidSubClassesOf(componentType);

            if (subTypes.Any())
            {
                var windowTitle = "Replace " + componentType.Name;
                var window = GetWindow<ReplaceComponentToolEditorWindow>(true, windowTitle, true);
                componentTypeToReplace = componentType;

                hasAnyPrefabSelected = Selection.gameObjects.Any(go => PrefabUtility.GetPrefabParent(go) != null);

                window.types = subTypes;
                window.ShowPopup();
            }
            else
            {
                EditorUtility.DisplayDialog("No sub-types found", "Could not find public sub-types for "
                    + componentType.Name + " in current AppDomain Assemblies.", "Ok");
            }
        }

        public ReplaceComponentToolEditorWindow()
        {
            onTypeSelected += OnTypeSelected;
        }

        protected override void OnGUI()
        {
            if (componentTypeToReplace != null)
            {
                var typeName = componentTypeToReplace.Name;
                GUILayout.Label("Replace " + typeName + " with sub-type", new GUIStyle
                {
                    margin = new RectOffset(2, 0, 0, 8)
                });

                if (hasAnyPrefabSelected)
                {
                    addAtSameOrder = GUILayout.Toggle(addAtSameOrder, "Keep order (looses prefabs connection)");
                }
            }

            base.OnGUI();
        }

        protected virtual void OnTypeSelected(Type selectedSubType)
        {
            // Register game object to support undoing.
            Undo.RecordObjects(Selection.gameObjects, "Replace " + componentTypeToReplace.Name + " with" + selectedSubType.Name);

            foreach (var gameObject in Selection.gameObjects)
            {
                var componentToReplace = gameObject.GetComponent(componentTypeToReplace);

                Component[] allComponents = null;
                int originalIndex = 0;
                if (addAtSameOrder)
                {
                    allComponents = gameObject.GetComponents<Component>();
                    originalIndex = Array.IndexOf(allComponents, componentToReplace);
                }

                // Add the new component.
                var newComponent = Undo.AddComponent(gameObject, selectedSubType);
                CopyComponents(componentToReplace, newComponent);

                // Remove the original component.
                Undo.DestroyObjectImmediate(componentToReplace);
                componentToReplace = null;

                // Move the new component to match the previous order.
                if (addAtSameOrder)
                {
                    var newIndex = allComponents.Length - 1;
                    for (int i = 0; i < newIndex - originalIndex; i++)
                    {
                        UnityEditorInternal.ComponentUtility.MoveComponentUp(newComponent);
                    }
                }

                EditorUtility.SetDirty(gameObject);
            }
        }

        /// <summary>
        /// Copy/paste all properties from original to the clone using Unity's Serialization.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="clone"></param>
        protected virtual void CopyComponents(Component original, Component clone)
        {
            var newSerializedObject = new SerializedObject(clone);
            var originalSerializedObject = new SerializedObject(original);
            SerializedProperty prop = originalSerializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    // Ignore unity's Script property
                    if (prop.name == "m_Script")
                    {
                        // If we don't do this the new component's serialized data
                        // will be pointing to the original script.
                        // TODO: Find a better way of doing this without hard-codding the variable's name.
                        continue;
                    }

                    newSerializedObject.CopyFromSerializedProperty(prop);
                }
                while (prop.NextVisible(false));
            }

            // Persist changes on the new component.
            newSerializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}