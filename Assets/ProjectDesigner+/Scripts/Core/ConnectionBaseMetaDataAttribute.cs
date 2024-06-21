using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Class used to add metadata to <see cref="ConnectionBase"/>s. Only to be used in classes inheriting from ConnectionBase.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConnectionBaseMetaDataAttribute : Attribute
    {
        /// <summary>
        /// Display name used in context menus when creating a connection.
        /// </summary>
        public string DisplayName { get; private set; }

        public ConnectionBaseMetaDataAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
