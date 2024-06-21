using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Data.Members
{
    /// <summary>
    /// A <see cref="MethodMember"/> holds multiple methods that represent actual functions of a class.
    /// </summary>
    [Serializable]
    public class MethodMember : MemberBase
    {
        [Serializable]
        public class Method
        {
            [SerializeField]
            public string Name;
            [SerializeField]
            public AccessModifiers AccessModifier = AccessModifiers.Public;

            public Method(string name = "foo", AccessModifiers accessModifier = AccessModifiers.Public)
            {
                AccessModifier = accessModifier;
                Name = name;
            }
        }

        private const int LabelWidth = 70;
        private const int ButtonWidth = 25;

        [SerializeField]
        private List<Method> _methods = new List<Method>();
        public MethodMember() : base()
        {

        }

        public MethodMember(MethodMember other) : this()
        {
            _methods = new List<Method>(other._methods);
        }

        /// <summary>
        /// Adds a new method to the member.
        /// </summary>
        /// <param name="method"></param>
        public void AddMethod(Method method)
        {
            Debug.Assert(method != null && !_methods.Contains(method));
            _methods.Add(method);
        }

        public override void OnAdded(NodeBase nodeBase)
        {
        }

        public override void Draw(IEditorContext context,NodeBase parent, float width)
        {
            List<Method> temp = new List<Method>(_methods);
            for (int i = 0; i < _methods.Count; i++)
            {
                Method method = _methods[i];

                CustomGUILayout.BeginHorizontal();
                if (parent.IsExpanded)
                {
                    if (CustomGUILayout.Button(method.AccessModifier.ToString()))
                    {
                        context.ShowDropdownOutsideZoomArea(() => ChooseAccessModifier(method));
                    }
                    CustomGUILayout.Label("Method: ", LeftAlignedLabelStyle, w: LabelWidth);
                    method.Name = CustomGUILayout.TextField(method.Name, $"{Id}_{i}");

                    if (CustomGUILayout.Button("X", EditorStyles.miniButton, w: ButtonWidth))
                    {
                        temp.RemoveAt(i);
                    }

                    if (i == 0)
                    {
                        GUI.enabled = false;
                    }

                    if (CustomGUILayout.Button("↑", EditorStyles.miniButton, w: ButtonWidth))
                    {
                        temp.MoveElementAtIndexUp(i);
                    }
                    GUI.enabled = true;

                    if (i == _methods.Count - 1)
                    {
                        GUI.enabled = false;
                    }

                    if (CustomGUILayout.Button("↓", EditorStyles.miniButton, w: ButtonWidth))
                    {
                        temp.MoveElementAtIndexDown(i);
                    }
                    GUI.enabled = true;
                }
                else
                {
                    CustomGUILayout.Label($"{GetAccessModifierCharacter(method.AccessModifier)} method: {method.Name}", LabelStyle);
                }
                CustomGUILayout.EndHorizontal();
            }

            if (parent.IsExpanded)
            {
                if (CustomGUILayout.Button("New Method", EditorStyles.toolbarButton))
                {
                    temp.Add(new Method());
                }
            }
            else if (_methods.Count == 0)
            {
                CustomGUILayout.Label("Expand to add Methods", LabelStyle);
            }

            _methods = temp;
        }

        public override MemberBase Copy(NodeBase parent)
        {
            return new MethodMember(this);
        }

        private static char GetAccessModifierCharacter(AccessModifiers accessModifiers)
        {
            switch (accessModifiers)
            {
                case AccessModifiers.Public:
                    return '+';
                case AccessModifiers.Private:
                    return '-';
                case AccessModifiers.Protected:
                    return '#';
                case AccessModifiers.Internal:
                    return '~';
                default: return '+';
            }
        }


        private GenericMenu ChooseAccessModifier(Method method)
        {
            method.AccessModifier = CustomGUILayout.EnumPopup(method.AccessModifier);
            GenericMenu menu = new GenericMenu();
            foreach (var modifier in Enum.GetValues(typeof(AccessModifiers)))
            {
                GUIContent content = new GUIContent(modifier.ToString());
                menu.AddItem(content, false, () => { method.AccessModifier = (AccessModifiers)modifier; });
            }

            return menu;
        }

    }
}
