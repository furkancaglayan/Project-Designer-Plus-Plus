using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Editor
{
    public class FeedbackEditorWindow : UnityEditor.EditorWindow
    {
        private enum FeedbackType
        {
            Improvement,
            Bug,
            Crash,
            RequestFeature,
            Other
        }

        private const float Width = 250;
        private const float Height = 400;
        private const float Padding = 15;
        private const float LabelWidth = 45;
        private const int SubjectCharacterMinLimit = 8;
        private const int SubjectCharacterMaxLimit = 40;
        private const int MessageCharacterMinLimit = 60;
        private const int MessageCharacterMaxLimit = 1000;
        private const int NameCharacterMinLimit = 10;
        private const int NameCharacterMaxLimit = 60;

        private float TextFieldWidth => Width - 2 * Padding - LabelWidth - 5;

        [SerializeField]
        private FeedbackType _feedbackType;
        [SerializeField]
        private bool _specifySubject;
        [SerializeField]
        private string _subject;
        [SerializeField]
        private string _message;
        [SerializeField]
        private string _name;
        private string _error;

        [MenuItem("Tools/Project Designer/Send Feedback", false, priority = 111)]
        public static void CreateWindow()
        {
            FeedbackEditorWindow window = CreateWindow<FeedbackEditorWindow>();
            window.maxSize = new Vector2(Width, Height);
            window.minSize = window.maxSize;
            window.titleContent = new GUIContent("Send Feedback");
            window.Show();
        }

        private void OnGUI()
        {
            CustomGUILayout.BeginArea(new Rect(Padding, Padding, Width - 2 * Padding, Height - 2 * Padding));
            CustomGUILayout.BeginVertical(w: Width - 2 * Padding);

            CustomGUILayout.BeginHorizontal();
            CustomGUILayout.Label("Name: ", EditorStyles.miniLabel, w: LabelWidth);
            _name = CustomGUILayout.TextField(string.Empty, string.Empty, _name, w: TextFieldWidth, characterLimit: NameCharacterMaxLimit);
            CustomGUILayout.EndHorizontal();

            CustomGUILayout.BeginHorizontal();
            CustomGUILayout.Label("Subject: ", EditorStyles.miniLabel, w: LabelWidth);
            if (_specifySubject)
            {
                _subject = CustomGUILayout.TextField(string.Empty, string.Empty, _subject, w: TextFieldWidth, characterLimit: SubjectCharacterMaxLimit);
            }
            else
            {
                _feedbackType = CustomGUILayout.EnumPopup(_feedbackType, string.Empty);
            }
            CustomGUILayout.EndHorizontal();

            CustomGUILayout.BeginHorizontal();
            CustomGUILayout.Label("Custom Subject: ", EditorStyles.miniLabel, w: LabelWidth * 2);
            _specifySubject = CustomGUILayout.Toggle(_specifySubject);
            CustomGUILayout.EndHorizontal();

            CustomGUILayout.Label("Message: ", EditorStyles.miniLabel);
            GUIStyle textFieldStyle = new GUIStyle(EditorStyles.textField);
            textFieldStyle.wordWrap = true;
            textFieldStyle.stretchHeight = true;
            textFieldStyle.stretchWidth = true;
            _message = CustomGUILayout.TextArea(_message, string.Empty, textFieldStyle, characterLimit: MessageCharacterMaxLimit);

            if (!string.IsNullOrEmpty(_error))
            {
                CustomGUILayout.HelpBox(_error, MessageType.Error);
            }

            if (CustomGUILayout.Button("Send Feedback"))
            {
                SendFeedback();
            }

            CustomGUILayout.EndVertical();
            CustomGUILayout.EndArea();
        }

        private void SendFeedback()
        {
            _error = string.Empty;
            if (CheckForErrors(out _error))
            {
                return;
            }

            string subject = _specifySubject ? _subject : GetSubject(_feedbackType);
            string mailToLink = $"mailto:{ProjectDesigner.Core.ProjectDesigner.FeedbackMail}?subject={subject}&body=Dear Project Designer Team,%0D%0A%0D%0A{_message}%0D%0A%0D%0A{_name}";
            Application.OpenURL(mailToLink);
        }

        private bool CheckForErrors(out string error)
        {
            error = string.Empty;
            if (string.IsNullOrEmpty(_name) || string.IsNullOrEmpty(_message))
            {
                error = "Name or message is empty";
                return true;
            }

            if (_name.Length < NameCharacterMinLimit)
            {
                error = "Name is too short";
                return true;
            }

            if (_message.Length < MessageCharacterMinLimit)
            {
                error = "Message is too short";
                return true;
            }

            if (_specifySubject && (string.IsNullOrEmpty(_subject) || _subject.Length < SubjectCharacterMinLimit))
            {
                error = "Subject is missing or too short";
                return true;
            }

            return false;
        }

        private string GetSubject(FeedbackType feedbackType)
        {
            switch (feedbackType)
            {
                case FeedbackType.RequestFeature:
                    return "Request Feature";
                default:
                    return feedbackType.ToString();
            }
        }
    }
}
