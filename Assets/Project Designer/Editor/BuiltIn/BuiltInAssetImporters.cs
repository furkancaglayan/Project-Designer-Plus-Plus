using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.V2.BuiltIn
{
    internal sealed class TextAssetReferenceImporter : IProjectDesignerAssetImporter
    {
        public int Priority { get { return 50; } }

        public bool CanImport(Object asset)
        {
            return asset is TextAsset;
        }

        public IEnumerable<BoardNodeModel> Import(Object asset, Vector2 position)
        {
            TextAsset textAsset = asset as TextAsset;
            if (textAsset == null)
            {
                yield break;
            }

            var node = new ReferenceNodeModel
            {
                Title = textAsset.name,
                Summary = "Text reference",
                TextReference = textAsset.text,
                AssetPath = AssetDatabase.GetAssetPath(textAsset),
                Position = position
            };

            yield return node;
        }
    }

    internal sealed class TextureReferenceImporter : IProjectDesignerAssetImporter
    {
        public int Priority { get { return 60; } }

        public bool CanImport(Object asset)
        {
            return asset is Texture2D || asset is Sprite;
        }

        public IEnumerable<BoardNodeModel> Import(Object asset, Vector2 position)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            var node = new ReferenceNodeModel
            {
                Title = asset.name,
                Summary = "Visual reference",
                AssetPath = path,
                ImageAssetPath = path,
                Position = position
            };

            yield return node;
        }
    }

    internal sealed class MonoScriptClassImporter : IProjectDesignerAssetImporter
    {
        public int Priority { get { return 90; } }

        public bool CanImport(Object asset)
        {
            return asset is MonoScript;
        }

        public IEnumerable<BoardNodeModel> Import(Object asset, Vector2 position)
        {
            MonoScript monoScript = asset as MonoScript;
            if (monoScript == null)
            {
                yield break;
            }

            System.Type type = monoScript.GetClass();
            if (type == null)
            {
                yield break;
            }

            var node = new ClassNodeModel
            {
                Title = type.Name,
                NamespaceName = type.Namespace ?? "Game",
                Summary = "Imported from " + monoScript.name,
                Position = position
            };

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                node.Fields.Add(new BoardClassMemberData(field.Name + " : " + field.FieldType.Name, ResolveVisibility(field.IsPublic, field.IsPrivate, field.IsFamily)));
            }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                string parameters = string.Join(", ", method.GetParameters().Select(parameter => parameter.ParameterType.Name));
                node.Methods.Add(new BoardClassMemberData(method.Name + "(" + parameters + ")", ResolveVisibility(method.IsPublic, method.IsPrivate, method.IsFamily)));
            }

            yield return node;
        }

        private static string ResolveVisibility(bool isPublic, bool isPrivate, bool isProtected)
        {
            if (isPublic)
            {
                return "public";
            }

            if (isProtected)
            {
                return "protected";
            }

            if (isPrivate)
            {
                return "private";
            }

            return "internal";
        }
    }

    internal sealed class GenericObjectReferenceImporter : IProjectDesignerAssetImporter
    {
        public int Priority { get { return 1; } }

        public bool CanImport(Object asset)
        {
            return asset != null;
        }

        public IEnumerable<BoardNodeModel> Import(Object asset, Vector2 position)
        {
            var node = new ReferenceNodeModel
            {
                Title = asset.name,
                Summary = "Linked Unity asset",
                AssetPath = AssetDatabase.GetAssetPath(asset),
                Position = position
            };

            yield return node;
        }
    }
}
