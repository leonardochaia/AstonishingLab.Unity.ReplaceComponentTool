using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AstonishingLab.Core.Editor
{
    public abstract class TypeSelectionEditorWindow : EditorWindow
    {
        public IEnumerable<Type> types;

        public Action<Type> onTypeSelected;

        public bool closeOnSelection = true;

        private IEnumerable<IGrouping<string, Type>> typesByNamespace;

        private Vector2 scrollPosition = Vector2.zero;

        public TypeSelectionEditorWindow()
        {
            minSize = new Vector2(300, 250);
        }

        protected virtual void OnGUI()
        {
            if (types != null && typesByNamespace == null)
            {
                typesByNamespace = types.GroupBy(t => t.Namespace);
            }

            if (typesByNamespace != null)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(minSize.x), GUILayout.Height(minSize.y));

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    margin = new RectOffset(24, 24, 4, 4)
                };

                foreach (var group in typesByNamespace)
                {
                    GUILayout.Label(group.Key ?? "Global Namespace");
                    foreach (var type in group)
                    {
                        if (GUILayout.Button(type.Name, buttonStyle))
                        {
                            if (onTypeSelected != null)
                            {
                                onTypeSelected.Invoke(type);
                            }

                            if (closeOnSelection)
                            {
                                Close();
                            }
                        }
                    }
                }

                GUILayout.EndScrollView();
            }
        }
    }
}