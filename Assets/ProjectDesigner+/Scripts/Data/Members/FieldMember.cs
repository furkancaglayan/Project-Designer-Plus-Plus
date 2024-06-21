using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Data.Members
{
    /// <summary>
    /// A <see cref="FieldMember"/> holds multiple fields that represent actual variables of a class.
    /// </summary>
    [Serializable]
    public class FieldMember : MemberBase
    {
        [Serializable]
        public class Field
        {
            [SerializeField]
            public string Name;
            [SerializeField]
            public string Type;
            [SerializeField]
            public AccessModifiers AccessModifier = AccessModifiers.Public;

            public Field(string name = "foo", string type = "int", AccessModifiers accessModifier = AccessModifiers.Public)
            {
                AccessModifier = accessModifier;
                Name = name;
                Type = type;
            }
        }

        private const int LabelWidth = 40;
        private const int DropDownButtonWidth = 65;
        private const int ButtonWidth = 25;

        [SerializeField]
        private List<Field> _fields = new List<Field>();

        public FieldMember() : base(5)
        {

        }

        public FieldMember(FieldMember fieldMember) : this()
        {
            _fields = new List<Field>(fieldMember._fields);
        }

        /// <summary>
        /// Adds a new Field to the member.
        /// </summary>
        /// <param name="field"></param>
        public void AddField(Field field)
        {
            Debug.Assert(field != null && !_fields.Contains(field));
            _fields.Add(field);
        }

        public override void OnAdded(NodeBase nodeBase)
        {
        }

        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            List<Field> temp = new List<Field>(_fields);

            for (int i = 0; i < _fields.Count; i++)
            {
                Field field = _fields[i];

                CustomGUILayout.BeginHorizontal();
                if (parent.IsExpanded)
                {
                    if (CustomGUILayout.Button(field.AccessModifier.ToString(), w: DropDownButtonWidth))
                    {
                        context.ShowDropdownOutsideZoomArea(() => ChooseAccessModifier(field));
                    }

                    CustomGUILayout.Label("Field:", LeftAlignedLabelStyle, w: LabelWidth);
                    field.Name = CustomGUILayout.TextField(field.Name, $"{Id}_0_{i}");
                    CustomGUILayout.Label("Type:", LeftAlignedLabelStyle, w: LabelWidth);
                    field.Type = CustomGUILayout.TextField(field.Type, $"{Id}_1_{i}");

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

                    if (i == _fields.Count - 1)
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
                    CustomGUILayout.Label($"{GetAccessModifierCharacter(field.AccessModifier)} {field.Name}: {field.Type}", LabelStyle);
                }
                CustomGUILayout.EndHorizontal();
            }

            if (parent.IsExpanded)
            {
                if (CustomGUILayout.Button("New Field", EditorStyles.toolbarButton))
                {
                    temp.Add(new Field());
                }
            }
            else if (_fields.Count == 0)
            {
                CustomGUILayout.Label("Expand to add Fields", LabelStyle);
            }

            _fields = temp;
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

        private GenericMenu ChooseAccessModifier(Field field)
        {
            field.AccessModifier = CustomGUILayout.EnumPopup(field.AccessModifier);
            GenericMenu menu = new GenericMenu();
            foreach (var modifier in Enum.GetValues(typeof(AccessModifiers)))
            {
                GUIContent content = new GUIContent(modifier.ToString());
                menu.AddItem(content, false, () => { field.AccessModifier = (AccessModifiers)modifier; });
            }
            return menu;
        }

        public override MemberBase Copy(NodeBase parent)
        {
            return new FieldMember(this);
        }
    }
}
