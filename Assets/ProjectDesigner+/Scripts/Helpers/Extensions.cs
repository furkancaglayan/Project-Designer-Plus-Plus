using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectDesigner.Helpers
{
    public static class Extensions
    {
        /// <summary>
        /// Swaps two elements in a list with the given indices <paramref name="i"/> amd <paramref name="j"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static void Swap<T>(this List<T> list, int i, int j)
        {
            if (i < 0 || j < 0 || i >= list.Count || j >= list.Count)
            {
                throw new IndexOutOfRangeException();
            }

            if (i == j)
            {
                return;
            }

            T t = list[i];
            list[i] = list[j];
            list[j] = t;
        }

        /// <summary>
        /// Moves the element with the given index "up" by one. This means the element at <paramref name="i"/> will be placed in index i - 1.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="i"></param>
        public static void MoveElementAtIndexUp<T>(this List<T> list, int i)
        {
            if (i > 0)
            {
                Swap(list, i, i - 1);
            }
        }

        /// <summary>
        /// Moves the element with the given index "down" by one. This means the element at <paramref name="i"/> will be placed in index i + 1.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="i"></param>
        public static void MoveElementAtIndexDown<T>(this List<T> list, int i)
        {
            if (i < list.Count - 1)
            {
                Swap(list, i, i + 1);
            }
        }

        /// <summary>
        /// Adds a menu option to a generic menu with <paramref name="label"/>. If <paramref name="enabled"/> is false the option will be disabled.
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="label"></param>
        /// <param name="function"></param>
        /// <param name="obj"></param>
        /// <param name="enabled"></param>
        public static void AddMenuActionOnCondition(this GenericMenu menu, string label, GenericMenu.MenuFunction2 function, object obj, bool enabled)
        {
            if (enabled)
            {
                menu.AddItem(new GUIContent(label), false, function, obj);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(label));
            }
        }

        /// <summary>
        /// Adds a menu option to a generic menu with <paramref name="label"/>. If <paramref name="enabled"/> is false the option will be disabled.
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="label"></param>
        /// <param name="function"></param>
        /// <param name="enabled"></param>
        public static void AddMenuActionOnCondition(this GenericMenu menu, string label, GenericMenu.MenuFunction function, bool enabled)
        {
            if (enabled)
            {
                menu.AddItem(new GUIContent(label), false, function);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(label));
            }
        }

        /// <summary>
        /// Returns a random position in the given rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPosition(this Rect rect)
        {
            return new Vector2 (Random.Range(rect.x, rect.xMax), Random.Range(rect.y, rect.yMax));    
        }

        /// <summary>
        /// Returns the top left position of the rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }
        /// <summary>
        /// Returns the bottom left position of the rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector2 BottomLeft(this Rect rect)
        {
            return new Vector2(rect.x, rect.y + rect.height);
        }
        /// <summary>
        /// Returns the bottom right position of the rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector2 BottomRight(this Rect rect)
        {
            return new Vector2(rect.x + rect.width, rect.y + rect.height);
        }
        /// <summary>
        /// Returns the top right position of the rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector2 TopRight(this Rect rect)
        {
            return new Vector2(rect.x + rect.width, rect.y);
        }
        /// <summary>
        /// Scales the <paramref name="rect"/> by <paramref name="scale"/> around <paramref name="pivotPoint"/>.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="scale"></param>
        /// <param name="pivotPoint"></param>
        /// <returns></returns>
        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }

        /// <summary>
        /// Tries to get a basic indication of if a rect is inside of an another one by checking the corners.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool Contains(this Rect rect, Rect other)
        {
            return rect.Contains(other.position) ||
                   rect.Contains(other.BottomRight()) ||
                   rect.Contains(other.BottomLeft()) ||
                   rect.Contains(other.TopRight()) ||
                   rect.Contains(other.center);
        }
    }
}
