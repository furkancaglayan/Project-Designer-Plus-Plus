using ProjectDesigner.Core;
using ProjectDesigner.Data.Connections;
using ProjectDesigner.Data.Members;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Data.Nodes
{
    /// <summary>
    /// <see cref="Class"/> represents a UML class that visualizes methods and variables of a c# class.
    /// </summary>
    [Serializable, NodeBaseMetaData("C# Class Node", false)]
    public class Class : NodeBase
    {
        public override Vector2 MinSize => new Vector2(400, 400);
        public override Vector2 MaxSize => new Vector2(480, 480);
        public override string IconKey => "script";
        public override string TextureKeyOff => "node2";
        public override string TextureKeyOn => "node2 on";
        public override bool CanBeCopied => true;
        protected override int HeaderHeight => 70;
        protected override int FooterHeight => 10;
        public override float IconSize => 48;

        private CommentMember _summary => GetFirstMember<CommentMember>();
        public Class(string header) : base()
        {
            HeaderText = header;
        }

        public Class() : this("Class")
        {

        }

        public Class(Class other) : base(other)
        {

        }
        public override NodeBase Copy()
        {
            return new Class(this);
        }

        protected override void OnContextClick(IEditorContext context, Vector2 position, GenericMenu menu)
        {
            menu.AddMenuActionOnCondition("Add Field Member", context.ProcessAction, new AddMemberAction(this, new FieldMember()), !HasMember<FieldMember>());
            menu.AddMenuActionOnCondition("Add Method Member", context.ProcessAction, new AddMemberAction(this, new MethodMember()), !HasMember<MethodMember>());

            if (_summary == null)
            {
                menu.AddItem(new GUIContent("Add Class Summary"), false, context.ProcessAction, new AddMemberAction(this, new CommentMember("Summary", priority: 25)));
            }
            else
            {
                menu.AddItem(new GUIContent("Remove Class Summary"), false, context.ProcessAction, new RemoveMemberAction(this, _summary));
            }
        }

        protected override void OnAddedInternal(IEditorContext context, DrawableCreationType drawableCreationType)
        {
            if (drawableCreationType == DrawableCreationType.Default)
            {
                AddMember(new FieldMember());
                AddMember(new MethodMember());
            }
        }

        protected override bool CanHaveConnections()
        {
            return true;
        }

        protected override bool CanHaveConnectionOfType(Type connectionType)
        {
            return connectionType.IsSubclassOf(typeof(ClassConnection));
        }

        [NodeBaseAssetMap(typeof(MonoScript))]
        private static NodeBase CreateClassNodeFromMonoScript(UnityEngine.Object obj)
        {
            MonoScript monoScript = obj as MonoScript;
            Type type = monoScript.GetClass();
            Class node = null;

            if (type != null)
            {
                node = new Class(type.Name);

                FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                FieldMember fieldMember = node.AddMember<FieldMember>();
                MethodMember methodMember = node.AddMember<MethodMember>();

                foreach (var field in fields)
                {
                    if (!field.IsDefined(typeof(CompilerGeneratedAttribute)))
                    {
                        fieldMember.AddField(new FieldMember.Field(field.Name, field.FieldType.Name, GetAccessModifiers(field)));
                    }
                }

                PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty);
                foreach (var property in properties)
                {
                    MethodInfo get = property.GetMethod;
                    MethodInfo set = property.SetMethod;
                    if (get != null)
                    {
                        methodMember.AddMethod(new MethodMember.Method(get.Name, GetAccessModifiers(get)));
                    }

                    if (set != null)
                    {
                        methodMember.AddMethod(new MethodMember.Method(set.Name, GetAccessModifiers(set)));
                    }
                }

                MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (var method in methods)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    string args = string.Empty;
                    string name = method.Name;
                    if (parameters != null)
                    {
                        args = string.Join(", ", parameters.Select(x => x.ParameterType.Name));
                        name = $"{name}({args})";
                    }

                    methodMember.AddMethod(new MethodMember.Method(name, GetAccessModifiers(method)));
                }

            }
           
            return node;
        }

        private static AccessModifiers GetAccessModifiers(FieldInfo field)
        {
            if (field.IsPublic)
            {
                return AccessModifiers.Public;
            }
            else if (field.IsPrivate)
            {
                return AccessModifiers.Private;
            }
            else if (field.IsFamily)
            {
                return AccessModifiers.Protected;
            }
            else if (field.IsAssembly)
            {
                return AccessModifiers.Internal;
            }

            return AccessModifiers.Internal;
        }

        private static AccessModifiers GetAccessModifiers(MethodInfo methodInfo)
        {
            if (methodInfo.IsPublic)
            {
                return AccessModifiers.Public;
            }
            else if (methodInfo.IsPrivate)
            {
                return AccessModifiers.Private;
            }
            else if (methodInfo.IsFamily)
            {
                return AccessModifiers.Protected;
            }
            else if (methodInfo.IsAssembly)
            {
                return AccessModifiers.Internal;
            }

            return AccessModifiers.Internal;
        }
    }
}
