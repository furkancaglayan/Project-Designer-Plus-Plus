using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Data.Connections
{
    /// <summary>
    /// Base class for representing relation connections such as <see cref="RelationCurvedConnection"/> and <see cref="RelationRegularConnection"/>.
    /// </summary>
    [Serializable]
    public abstract class RelationConnection : ClassConnection
    {
       
    }
}
