using ProjectDesigner.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Data.Connections
{
    /// <summary>
    /// Base class for representing class node connections such as <see cref="AssociationConnection"/>, <see cref="InheritanceConnection"/> and <see cref="DependencyConnection"/>.
    /// </summary>
    [Serializable]
    public abstract class ClassConnection : ConnectionBase
    {

    }
}
