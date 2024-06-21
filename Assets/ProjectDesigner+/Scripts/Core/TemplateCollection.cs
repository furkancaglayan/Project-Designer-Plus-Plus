using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using static ProjectDesigner.Core.DefaultContextHandlerAttribute;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A class used for gathering all reflection-referenced attributes, classes and methods.
    /// </summary>
    [InitializeOnLoad]
    public static class TemplateCollection
    {
        /// <summary>
        /// <see cref="NodeBaseMetaDataAttribute"/> wrapper struct.
        /// </summary>
        public readonly struct NodeTypeData
        {
            /// <summary>
            /// Type of the NodeBase.
            /// </summary>
            public readonly Type Type;
            /// <summary>
            /// Display name of the node type.
            /// </summary>
            public readonly string DisplayName;
            /// <summary>
            /// Max count per project designer window.
            /// </summary>
            public readonly int MaxCountPerProject;
            /// <summary>
            /// Should add on window creation?
            /// </summary>
            public readonly bool AddOnCreation;

            public NodeTypeData(Type type, string displayName, int maxCountPerProject = 1, bool addOnCreation = false)
            {
                Type = type;
                DisplayName = displayName;
                MaxCountPerProject = maxCountPerProject;
                AddOnCreation = addOnCreation;
            }
        }

        /// <summary>
        /// <see cref="ConnectionBaseMetaDataAttribute"/> wrapper struct.
        /// </summary>
        public readonly struct ConnectionTypeData
        {
            /// <summary>
            /// Type of the ConnectionBase.
            /// </summary>
            public readonly Type Type;
            /// <summary>
            /// Display name of the connection type.
            /// </summary>
            public readonly string DisplayName;

            public ConnectionTypeData(Type type, string displayName)
            {
                Type = type;
                DisplayName = displayName;
            }
        }

        public delegate NodeBase CreateNodeFromAssetDelegate(Object obj);

        private static List<NodeTypeData> _drawableNodeTypes = new List<NodeTypeData>();
        private static List<ConnectionTypeData> _drawableConnectionTypes = new List<ConnectionTypeData>();
        private static Dictionary<Type, (NodeBaseAssetMapAttribute, MethodInfo)> _autoCreationFunctions = new Dictionary<Type, (NodeBaseAssetMapAttribute, MethodInfo)>();
        private static List<ContextMenuHandlerDelegate> _defaultContextHandlers = new List<ContextMenuHandlerDelegate>();

        static TemplateCollection()
        {
            Collect();
        }

        private static void Collect()
        {
            _drawableNodeTypes.Clear();
            _drawableConnectionTypes.Clear();
            _autoCreationFunctions.Clear();
            _defaultContextHandlers.Clear();

            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in allAssemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (!type.IsAbstract && type.IsSubclassOf(typeof(NodeBase)))
                    {
                        AddDrawableNodeType(type);
                    }
                    else if (!type.IsAbstract && type.IsSubclassOf(typeof(ConnectionBase)))
                    {
                        AddConnectionBaseType(type);
                    }

                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Default);
                    foreach (var method in methods)
                    {
                        NodeBaseAssetMapAttribute nodeBaseMetaData = method.GetCustomAttribute<NodeBaseAssetMapAttribute>();
                        if (!type.IsAbstract && nodeBaseMetaData != null && nodeBaseMetaData.Type != null)
                        {
                            AddObjectCreationFunction(method, nodeBaseMetaData);
                        }
                        else if (method.IsStatic)
                        {
                            ParameterInfo[] parameters = method.GetParameters();
                            DefaultContextHandlerAttribute contextHandler = method.GetCustomAttribute<DefaultContextHandlerAttribute>();
                            bool parameterCheck = parameters.Length == 3 && parameters[0].ParameterType == typeof(IEditorContext) && parameters[1].ParameterType == typeof(Vector2) && parameters[2].ParameterType == typeof(GenericMenu);
                            Debug.Assert(contextHandler == null || parameterCheck);
                            if (contextHandler != null && parameterCheck)
                            {
                                _defaultContextHandlers.Add(CreateDelegate<ContextMenuHandlerDelegate>(method));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="CreateNodeFromAssetDelegate"/> node creation function for given type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static bool GetNodeCreateDelegate(Type type, out CreateNodeFromAssetDelegate func)
        {
            func = null;
            if (_autoCreationFunctions.TryGetValue(type, out (NodeBaseAssetMapAttribute, MethodInfo) attributeAndMethod))
            {
                func = (CreateNodeFromAssetDelegate)attributeAndMethod.Item2.CreateDelegate(typeof(CreateNodeFromAssetDelegate));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all collected <see cref="NodeBase"/> types.
        /// </summary>
        /// <returns></returns>
        public static List<NodeTypeData> GetDrawableNodeTypes()
        {
            return _drawableNodeTypes;
        }

        /// <summary>
        /// Gets all collected <see cref="ConnectionBase"/> types.
        /// </summary>
        /// <returns></returns>
        public static List<ConnectionTypeData> GetDrawableConnectionTypes()
        {
            return _drawableConnectionTypes;
        }
        
        /// <summary>
        /// Gets all collected ContextMenuHandler functions.
        /// </summary>
        /// <returns></returns>
        public static List<ContextMenuHandlerDelegate> GetDefaultContextHandlers()
        {
            return _defaultContextHandlers;
        }

        private static void AddObjectCreationFunction(MethodInfo method, NodeBaseAssetMapAttribute attribute)
        {
            if (method.ReturnType != typeof(NodeBase))
            {
                Debug.Assert(false, "Method return Type should be NodeBase");
                return;
            }

            if (method.GetParameters().Length != 1 || method.GetParameters()[0].ParameterType != typeof(UnityEngine.Object))
            {
                Debug.Assert(false, "Method parameter type should be UnityObject");
                return;
            }

            if (_autoCreationFunctions.TryGetValue(attribute.Type, out (NodeBaseAssetMapAttribute, MethodInfo) oldAttributeAndMethod))
            {
                if (attribute.Priority > oldAttributeAndMethod.Item1.Priority)
                {
                    _autoCreationFunctions[attribute.Type] = (attribute, method);
                }
            }
            else
            {
                _autoCreationFunctions[attribute.Type] = (attribute, method);
            }
        }

        private static void AddDrawableNodeType(Type type)
        {
            NodeBaseMetaDataAttribute metaData = type.GetCustomAttribute<NodeBaseMetaDataAttribute>();
            Debug.Assert(type.IsSubclassOf(typeof(NodeBase)));
            NodeTypeData data;
            if (metaData != null)
            {
                data = new NodeTypeData(type, metaData.DisplayName, metaData.MaxCount, metaData.AddOnWindowCreation);
            }
            else
            {
                data = new NodeTypeData(type, type.Name);
            }

            _drawableNodeTypes.Add(data);
        }

        private static void AddConnectionBaseType(Type type)
        {
            ConnectionBaseMetaDataAttribute metaData = type.GetCustomAttribute<ConnectionBaseMetaDataAttribute>();
            Debug.Assert(type.IsSubclassOf(typeof(ConnectionBase)));

            ConnectionTypeData data;
            if (metaData != null)
            {
                data = new ConnectionTypeData(type, metaData.DisplayName);
            }
            else
            {
                data = new ConnectionTypeData(type, type.Name);
            }

            _drawableConnectionTypes.Add(data);
        }

        private static T CreateDelegate<T>(MethodInfo methodInfo) where T : Delegate
        {
            try
            {
                return (T)Delegate.CreateDelegate(typeof(T), methodInfo);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
