using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    public static class BoardLayoutUtility
    {
        private const float AutoLayoutHorizontalGap = 180f;
        private const float AutoLayoutVerticalGap = 56f;
        private const float AutoLayoutComponentGap = 24f;

        private sealed class AutoLayoutComponent
        {
            public int Id;
            public List<BoardNodeModel> Nodes = new List<BoardNodeModel>();
            public HashSet<int> Incoming = new HashSet<int>();
            public HashSet<int> Outgoing = new HashSet<int>();
            public float MinOriginalX;
            public float MinOriginalY;
        }

        private sealed class AutoLayoutEdge
        {
            public string OrderedSourceNodeId;
            public string OrderedTargetNodeId;
        }

        public static Vector2 SnapPosition(Vector2 position)
        {
            float grid = Mathf.Max(1f, ProjectDesignerProductInfo.GridSize);
            return new Vector2(
                Mathf.Round(position.x / grid) * grid,
                Mathf.Round(position.y / grid) * grid);
        }

        public static Rect GetBounds(IEnumerable<BoardNodeModel> nodes)
        {
            List<BoardNodeModel> validNodes = (nodes ?? Enumerable.Empty<BoardNodeModel>())
                .Where(node => node != null)
                .ToList();

            if (validNodes.Count == 0)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            float minX = validNodes.Min(node => node.Position.x);
            float minY = validNodes.Min(node => node.Position.y);
            float maxX = validNodes.Max(node => node.Position.x + node.Size.x);
            float maxY = validNodes.Max(node => node.Position.y + node.Size.y);
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        public static Dictionary<string, Vector2> Arrange(IReadOnlyList<BoardNodeModel> nodes, BoardArrangeMode mode, bool snapToGrid)
        {
            var positions = new Dictionary<string, Vector2>();
            List<BoardNodeModel> validNodes = (nodes ?? new List<BoardNodeModel>())
                .Where(node => node != null)
                .ToList();

            if (validNodes.Count == 0)
            {
                return positions;
            }

            Rect bounds = GetBounds(validNodes);

            switch (mode)
            {
                case BoardArrangeMode.AutoLayoutLeftToRight:
                    ApplyHorizontalDistribution(validNodes, positions);
                    break;

                case BoardArrangeMode.AlignLeft:
                    foreach (BoardNodeModel node in validNodes)
                    {
                        positions[node.Id] = new Vector2(bounds.xMin, node.Position.y);
                    }
                    break;

                case BoardArrangeMode.AlignCenter:
                    float centerX = bounds.center.x;
                    foreach (BoardNodeModel node in validNodes)
                    {
                        positions[node.Id] = new Vector2(centerX - node.Size.x * 0.5f, node.Position.y);
                    }
                    break;

                case BoardArrangeMode.AlignRight:
                    foreach (BoardNodeModel node in validNodes)
                    {
                        positions[node.Id] = new Vector2(bounds.xMax - node.Size.x, node.Position.y);
                    }
                    break;

                case BoardArrangeMode.AlignTop:
                    foreach (BoardNodeModel node in validNodes)
                    {
                        positions[node.Id] = new Vector2(node.Position.x, bounds.yMin);
                    }
                    break;

                case BoardArrangeMode.AlignMiddle:
                    float middleY = bounds.center.y;
                    foreach (BoardNodeModel node in validNodes)
                    {
                        positions[node.Id] = new Vector2(node.Position.x, middleY - node.Size.y * 0.5f);
                    }
                    break;

                case BoardArrangeMode.AlignBottom:
                    foreach (BoardNodeModel node in validNodes)
                    {
                        positions[node.Id] = new Vector2(node.Position.x, bounds.yMax - node.Size.y);
                    }
                    break;

                case BoardArrangeMode.DistributeHorizontal:
                    ApplyHorizontalDistribution(validNodes, positions);
                    break;

                case BoardArrangeMode.DistributeVertical:
                    ApplyVerticalDistribution(validNodes, positions);
                    break;
            }

            if (!snapToGrid)
            {
                return positions;
            }

            List<string> ids = positions.Keys.ToList();
            foreach (string nodeId in ids)
            {
                positions[nodeId] = SnapPosition(positions[nodeId]);
            }

            return positions;
        }

        public static Dictionary<string, Vector2> AutoArrange(BoardDocument document, IReadOnlyList<BoardNodeModel> nodes, bool snapToGrid)
        {
            return AutoArrange((nodes ?? new List<BoardNodeModel>())
                .Where(node => node != null)
                .ToList(), document == null ? Enumerable.Empty<BoardEdgeModel>() : document.Edges, snapToGrid);
        }

        private static Dictionary<string, Vector2> AutoArrange(IReadOnlyList<BoardNodeModel> nodes, IEnumerable<BoardEdgeModel> edges, bool snapToGrid)
        {
            var positions = new Dictionary<string, Vector2>();
            List<BoardNodeModel> validNodes = (nodes ?? new List<BoardNodeModel>())
                .Where(node => node != null)
                .ToList();

            if (validNodes.Count == 0)
            {
                return positions;
            }

            if (validNodes.Count == 1)
            {
                BoardNodeModel onlyNode = validNodes[0];
                positions[onlyNode.Id] = snapToGrid ? SnapPosition(onlyNode.Position) : onlyNode.Position;
                return positions;
            }

            Rect bounds = GetBounds(validNodes);
            Dictionary<string, BoardNodeModel> nodesById = validNodes.ToDictionary(node => node.Id);
            List<AutoLayoutEdge> relevantEdges = (edges ?? Enumerable.Empty<BoardEdgeModel>())
                .Where(edge => edge != null &&
                               nodesById.ContainsKey(edge.SourceNodeId) &&
                               nodesById.ContainsKey(edge.TargetNodeId) &&
                               !string.Equals(edge.SourceNodeId, edge.TargetNodeId))
                .Select(CreateAutoLayoutEdge)
                .Where(edge => edge != null &&
                               !string.IsNullOrEmpty(edge.OrderedSourceNodeId) &&
                               !string.IsNullOrEmpty(edge.OrderedTargetNodeId))
                .ToList();

            if (relevantEdges.Count == 0)
            {
                ApplyHorizontalDistribution(validNodes, positions);
                if (snapToGrid)
                {
                    List<string> ids = positions.Keys.ToList();
                    foreach (string nodeId in ids)
                    {
                        positions[nodeId] = SnapPosition(positions[nodeId]);
                    }
                }

                return positions;
            }

            List<AutoLayoutComponent> components = BuildAutoLayoutComponents(validNodes, relevantEdges);
            List<AutoLayoutComponent> orderedComponents = TopologicallyOrderComponents(components);
            Dictionary<int, int> layerByComponentId = AssignLayers(orderedComponents);
            List<int> orderedLayers = orderedComponents
                .Select(component => layerByComponentId[component.Id])
                .Distinct()
                .OrderBy(layer => layer)
                .ToList();

            var layerXPositions = new Dictionary<int, float>();
            float cursorX = bounds.xMin;
            float maxLayerWidth = 0f;
            for (int index = 0; index < orderedLayers.Count; index++)
            {
                int layer = orderedLayers[index];
                if (index > 0)
                {
                    cursorX += maxLayerWidth + AutoLayoutHorizontalGap;
                }

                layerXPositions[layer] = cursorX;
                maxLayerWidth = orderedComponents
                    .Where(component => layerByComponentId[component.Id] == layer)
                    .SelectMany(component => component.Nodes)
                    .DefaultIfEmpty(null)
                    .Max(node => node == null ? 0f : node.Size.x);
            }

            foreach (int layer in orderedLayers)
            {
                List<AutoLayoutComponent> componentsInLayer = orderedComponents
                    .Where(component => layerByComponentId[component.Id] == layer)
                    .OrderBy(component => component.MinOriginalY)
                    .ThenBy(component => component.MinOriginalX)
                    .ToList();

                List<BoardNodeModel> orderedNodes = new List<BoardNodeModel>();
                for (int index = 0; index < componentsInLayer.Count; index++)
                {
                    AutoLayoutComponent component = componentsInLayer[index];
                    List<BoardNodeModel> componentNodes = component.Nodes
                        .OrderBy(node => GetAutoLayoutSortKey(node))
                        .ThenBy(node => node.Position.y)
                        .ThenBy(node => node.Position.x)
                        .ToList();
                    orderedNodes.AddRange(componentNodes);
                    if (index < componentsInLayer.Count - 1)
                    {
                        orderedNodes.Add(null);
                    }
                }

                float totalHeight = 0f;
                for (int index = 0; index < orderedNodes.Count; index++)
                {
                    BoardNodeModel node = orderedNodes[index];
                    if (node == null)
                    {
                        totalHeight += AutoLayoutComponentGap;
                        continue;
                    }

                    totalHeight += node.Size.y;
                    if (index < orderedNodes.Count - 1 && orderedNodes[index + 1] != null)
                    {
                        totalHeight += AutoLayoutVerticalGap;
                    }
                }

                float cursorY = bounds.center.y - totalHeight * 0.5f;
                float x = layerXPositions[layer];
                foreach (BoardNodeModel node in orderedNodes)
                {
                    if (node == null)
                    {
                        cursorY += AutoLayoutComponentGap;
                        continue;
                    }

                    Vector2 position = new Vector2(x, cursorY);
                    positions[node.Id] = snapToGrid ? SnapPosition(position) : position;
                    cursorY += node.Size.y + AutoLayoutVerticalGap;
                }
            }

            foreach (BoardNodeModel node in validNodes)
            {
                if (!positions.ContainsKey(node.Id))
                {
                    positions[node.Id] = snapToGrid ? SnapPosition(node.Position) : node.Position;
                }
            }

            return positions;
        }

        private static void ApplyHorizontalDistribution(IReadOnlyList<BoardNodeModel> nodes, IDictionary<string, Vector2> positions)
        {
            if (nodes.Count < 3)
            {
                foreach (BoardNodeModel node in nodes)
                {
                    positions[node.Id] = node.Position;
                }
                return;
            }

            List<BoardNodeModel> orderedNodes = nodes.OrderBy(node => node.Position.x).ToList();
            float left = orderedNodes.First().Position.x;
            float right = orderedNodes.Last().Position.x + orderedNodes.Last().Size.x;
            float totalWidth = orderedNodes.Sum(node => node.Size.x);
            float gap = (right - left - totalWidth) / (orderedNodes.Count - 1);
            float cursor = left;

            foreach (BoardNodeModel node in orderedNodes)
            {
                positions[node.Id] = new Vector2(cursor, node.Position.y);
                cursor += node.Size.x + gap;
            }
        }

        private static void ApplyVerticalDistribution(IReadOnlyList<BoardNodeModel> nodes, IDictionary<string, Vector2> positions)
        {
            if (nodes.Count < 3)
            {
                foreach (BoardNodeModel node in nodes)
                {
                    positions[node.Id] = node.Position;
                }
                return;
            }

            List<BoardNodeModel> orderedNodes = nodes.OrderBy(node => node.Position.y).ToList();
            float top = orderedNodes.First().Position.y;
            float bottom = orderedNodes.Last().Position.y + orderedNodes.Last().Size.y;
            float totalHeight = orderedNodes.Sum(node => node.Size.y);
            float gap = (bottom - top - totalHeight) / (orderedNodes.Count - 1);
            float cursor = top;

            foreach (BoardNodeModel node in orderedNodes)
            {
                positions[node.Id] = new Vector2(node.Position.x, cursor);
                cursor += node.Size.y + gap;
            }
        }

        private static List<AutoLayoutComponent> BuildAutoLayoutComponents(IReadOnlyList<BoardNodeModel> nodes, IReadOnlyList<AutoLayoutEdge> edges)
        {
            Dictionary<string, BoardNodeModel> nodesById = nodes.ToDictionary(node => node.Id);
            Dictionary<string, List<string>> adjacency = nodes.ToDictionary(node => node.Id, _ => new List<string>());
            foreach (AutoLayoutEdge edge in edges)
            {
                adjacency[edge.OrderedSourceNodeId].Add(edge.OrderedTargetNodeId);
            }

            int index = 0;
            var indexById = new Dictionary<string, int>();
            var lowLinkById = new Dictionary<string, int>();
            var stack = new Stack<string>();
            var onStack = new HashSet<string>();
            var componentIdByNodeId = new Dictionary<string, int>();
            var orderedComponents = new List<List<string>>();

            foreach (BoardNodeModel node in nodes)
            {
                if (!indexById.ContainsKey(node.Id))
                {
                    StrongConnect(
                        node.Id,
                        adjacency,
                        ref index,
                        indexById,
                        lowLinkById,
                        stack,
                        onStack,
                        componentIdByNodeId,
                        orderedComponents);
                }
            }

            List<AutoLayoutComponent> components = orderedComponents
                .Select((nodeIds, componentIndex) =>
                {
                    List<BoardNodeModel> componentNodes = nodeIds
                        .Select(nodeId => nodesById[nodeId])
                        .Where(node => node != null)
                        .ToList();
                    return new AutoLayoutComponent
                    {
                        Id = componentIndex,
                        Nodes = componentNodes,
                        MinOriginalX = componentNodes.Min(node => node.Position.x),
                        MinOriginalY = componentNodes.Min(node => node.Position.y)
                    };
                })
                .ToList();

            foreach (AutoLayoutEdge edge in edges)
            {
                int sourceComponentId = componentIdByNodeId[edge.OrderedSourceNodeId];
                int targetComponentId = componentIdByNodeId[edge.OrderedTargetNodeId];
                if (sourceComponentId == targetComponentId)
                {
                    continue;
                }

                components[sourceComponentId].Outgoing.Add(targetComponentId);
                components[targetComponentId].Incoming.Add(sourceComponentId);
            }

            return components;
        }

        private static AutoLayoutEdge CreateAutoLayoutEdge(BoardEdgeModel edge)
        {
            if (edge == null)
            {
                return null;
            }

            if (string.Equals(edge.TypeId, BoardEdgeTypeIds.Dependency))
            {
                return new AutoLayoutEdge
                {
                    OrderedSourceNodeId = edge.TargetNodeId,
                    OrderedTargetNodeId = edge.SourceNodeId
                };
            }

            return new AutoLayoutEdge
            {
                OrderedSourceNodeId = edge.SourceNodeId,
                OrderedTargetNodeId = edge.TargetNodeId
            };
        }

        private static void StrongConnect(
            string nodeId,
            IReadOnlyDictionary<string, List<string>> adjacency,
            ref int index,
            IDictionary<string, int> indexById,
            IDictionary<string, int> lowLinkById,
            Stack<string> stack,
            ISet<string> onStack,
            IDictionary<string, int> componentIdByNodeId,
            IList<List<string>> orderedComponents)
        {
            indexById[nodeId] = index;
            lowLinkById[nodeId] = index;
            index++;
            stack.Push(nodeId);
            onStack.Add(nodeId);

            List<string> outgoing = adjacency.ContainsKey(nodeId) ? adjacency[nodeId] : new List<string>();
            foreach (string nextNodeId in outgoing)
            {
                if (!indexById.ContainsKey(nextNodeId))
                {
                    StrongConnect(nextNodeId, adjacency, ref index, indexById, lowLinkById, stack, onStack, componentIdByNodeId, orderedComponents);
                    lowLinkById[nodeId] = Mathf.Min(lowLinkById[nodeId], lowLinkById[nextNodeId]);
                }
                else if (onStack.Contains(nextNodeId))
                {
                    lowLinkById[nodeId] = Mathf.Min(lowLinkById[nodeId], indexById[nextNodeId]);
                }
            }

            if (lowLinkById[nodeId] != indexById[nodeId])
            {
                return;
            }

            int componentId = orderedComponents.Count;
            var componentNodeIds = new List<string>();
            string poppedNodeId;
            do
            {
                poppedNodeId = stack.Pop();
                onStack.Remove(poppedNodeId);
                componentIdByNodeId[poppedNodeId] = componentId;
                componentNodeIds.Add(poppedNodeId);
            }
            while (!string.Equals(poppedNodeId, nodeId));

            orderedComponents.Add(componentNodeIds);
        }

        private static List<AutoLayoutComponent> TopologicallyOrderComponents(IReadOnlyList<AutoLayoutComponent> components)
        {
            var componentsById = components.ToDictionary(component => component.Id);
            var remainingIncoming = components.ToDictionary(component => component.Id, component => component.Incoming.Count);
            var ready = components
                .Where(component => component.Incoming.Count == 0)
                .OrderBy(component => component.MinOriginalX)
                .ThenBy(component => component.MinOriginalY)
                .ToList();

            var ordered = new List<AutoLayoutComponent>();
            while (ready.Count > 0)
            {
                AutoLayoutComponent component = ready[0];
                ready.RemoveAt(0);
                ordered.Add(component);

                foreach (int targetComponentId in component.Outgoing)
                {
                    remainingIncoming[targetComponentId]--;
                    if (remainingIncoming[targetComponentId] == 0)
                    {
                        ready.Add(componentsById[targetComponentId]);
                        ready = ready
                            .OrderBy(item => item.MinOriginalX)
                            .ThenBy(item => item.MinOriginalY)
                            .ToList();
                    }
                }
            }

            if (ordered.Count == components.Count)
            {
                return ordered;
            }

            foreach (AutoLayoutComponent component in components
                .Where(component => ordered.All(existing => existing.Id != component.Id))
                .OrderBy(component => component.MinOriginalX)
                .ThenBy(component => component.MinOriginalY))
            {
                ordered.Add(component);
            }

            return ordered;
        }

        private static Dictionary<int, int> AssignLayers(IReadOnlyList<AutoLayoutComponent> orderedComponents)
        {
            var layerByComponentId = new Dictionary<int, int>();
            foreach (AutoLayoutComponent component in orderedComponents)
            {
                int layer = 0;
                foreach (int incomingComponentId in component.Incoming)
                {
                    if (layerByComponentId.TryGetValue(incomingComponentId, out int incomingLayer))
                    {
                        layer = Mathf.Max(layer, incomingLayer + 1);
                    }
                }

                layerByComponentId[component.Id] = layer;
            }

            return layerByComponentId;
        }

        private static int GetAutoLayoutSortKey(BoardNodeModel node)
        {
            if (node == null)
            {
                return int.MaxValue;
            }

            int categoryPriority;
            switch (node.Category)
            {
                case BoardNodeCategories.Planning:
                    categoryPriority = 0;
                    break;
                case BoardNodeCategories.Reference:
                    categoryPriority = 1;
                    break;
                case BoardNodeCategories.TechnicalDesign:
                    categoryPriority = 2;
                    break;
                default:
                    categoryPriority = 3;
                    break;
            }

            return (node.IsPinned ? -10 : 0) + categoryPriority;
        }
    }
}
