using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Acts as a debugger to keep track of logs in the editor.
    /// </summary>
    [Serializable]
    public class EditorLogHistory 
    {
        [Serializable]
        public struct Log
        {
            [field: SerializeField]
            public string Text {  get; private set; }
            [field: SerializeField]
            public string Description { get; private set; }
            [field: SerializeField]
            public LogType Type {  get; private set; }
            [field: SerializeField]
            public bool IsContextAction { get; private set; }

            public Log(string text, string description, LogType type, bool isContextAction)
            {
                Text = text;
                Description = description;
                Type = type;
                IsContextAction = isContextAction;
            }


            public MessageType GetMessageType()
            {
                switch (this.Type)
                {
                    case LogType.Log:
                        return MessageType.Info;
                    case LogType.Error:
                    case LogType.Exception:
                    case LogType.Assert:
                        return MessageType.Error;
                    case LogType.Warning:
                        return MessageType.Warning;
                    default:
                        return MessageType.Info;
                }
            }
        }

        private const int MaxHistoryLength = 200;

        [SerializeField]
        private List<Log> _logs = new List<Log>();

        public ReadOnlyCollection<Log> Logs = null;

        public EditorLogHistory()
        {
            Logs = new ReadOnlyCollection<Log>(_logs);
        }

        public void TextLog(string text, string description = null, bool isContextAction = false)
        {
            //Debug.Log(text);
            LogInternal(text, LogType.Log, description, isContextAction);
        }

        public void TextWarning(string text, string description = null, bool isContextAction = false)
        {
            Debug.LogWarning(text);
            LogInternal(text, LogType.Warning, description, isContextAction);
        }

        public void TextError(string text, string description = null, bool isContextAction = false)
        {
            Debug.LogError(text);
            LogInternal(text, LogType.Error, description, isContextAction);
        }

        private void LogInternal(string text, LogType type, string description, bool isContextAction)
        {
            Log log = new Log(text, description, type, isContextAction);
            _logs.Add(log);
            TrimHistory();
        }

        private void TrimHistory()
        {
            while (_logs.Count > MaxHistoryLength)
            {
                _logs.RemoveAt(0);
            }
        }

        internal void Clear()
        {
            _logs.Clear();
        }
    }
}
