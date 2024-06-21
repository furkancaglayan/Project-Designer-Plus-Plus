using ProjectDesigner.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Editor
{
    public static class DefaultContextMenu
    {
        [DefaultContextHandler]
        static void CreateNewNode(IEditorContext context, Vector2 position, GenericMenu menu)
        {
            foreach (var nodeType in TemplateCollection.GetDrawableNodeTypes())
            {
                string name = nodeType.DisplayName;
                bool canAdd = nodeType.MaxCountPerProject > context.GetDrawables<NodeBase>(x => x.GetType().IsAssignableFrom(nodeType.Type)).Count;
                if (canAdd)
                {
                    menu.AddItem(new GUIContent("New Node/" + name), false, context.ProcessAction, GetNewCreateNode(context, position, nodeType.Type));
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("New Node/" + name));
                }
            }
        }

        [DefaultContextHandler]
        static void ShowHiddenDrawables(IEditorContext context, Vector2 position, GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Show hidden nodes"), false, context.ProcessAction, GetShowNodes(context.GetDrawables<NodeBase>()));
        }

        static CreateNodeAction GetNewCreateNode(IEditorContext context, Vector2 position, Type type)
        {
            return new CreateNodeAction(new NodeCreationContext(context.GetGUIPosition(position), type));
        }

        static ShowNodesAction GetShowNodes(List<NodeBase> nodes)
        {
            return new ShowNodesAction(nodes);
        }
    }
}
