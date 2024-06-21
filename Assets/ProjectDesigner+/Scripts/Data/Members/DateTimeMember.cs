using UnityEngine;
using UnityEditor;
using System;
using ProjectDesigner.Core;
using System.Globalization;
using ProjectDesigner.Helpers;

namespace ProjectDesigner.Data.Members
{
    /// <summary>
    /// A <see cref="DateTimeMember"/> is used for setting due dates on nodes.
    /// </summary>
    [Serializable]
    public class DateTimeMember : MemberBase
    {
        private const int DayLabelWidth = 20;
        private const int MonthLabelWidth = 60;
        private const int YearLabelWidth = 40;

        private string[] Months => new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"}; 
        [SerializeField]
        private int _day;
        [SerializeField]
        private int _monthIndex;
        [SerializeField]
        private int _year;
        [SerializeField]
        private string _name;

        public DateTimeMember(string name) : base(3)
        {
            _name = name;
            DateTime target = DateTime.Now.AddDays(7);
            _day = target.Day;
            _monthIndex = target.Month - 1;
            _year = target.Year;
        }

        public DateTimeMember(DateTimeMember other) : this(other._name)
        {
            _day = other._day;
            _monthIndex = other._monthIndex;
            _year = other._year;
        }

        public override void OnAdded(NodeBase nodeBase)
        {

        }

        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            Vector2 s = Event.current.mousePosition;
            if (parent.IsExpanded)
            {
                CustomGUILayout.Label(_name, LabelStyle);
                CustomGUILayout.BeginHorizontal(EditorStyles.toolbar);
                CustomGUILayout.Label(_day.ToString(), w: DayLabelWidth);
                if (CustomGUILayout.DropdownButton(new GUIContent("Day"), EditorStyles.toolbarDropDown))
                {
                    context.ShowDropdownOutsideZoomArea(GetDaysGenericMenu);
                    //ShowGenericMenuDays(context ,parent, s);
                }
                CustomGUILayout.FlexibleSpace();
                CustomGUILayout.Label(Months[_monthIndex], w: MonthLabelWidth);
                if (CustomGUILayout.DropdownButton(new GUIContent("Month"), EditorStyles.toolbarDropDown))
                {
                    context.ShowDropdownOutsideZoomArea(GetMonthsGenericMenu);
                }
                CustomGUILayout.FlexibleSpace();
                CustomGUILayout.Label(_year.ToString(), w: YearLabelWidth);
                if (CustomGUILayout.DropdownButton(new GUIContent("Year"), EditorStyles.toolbarDropDown))
                {
                    context.ShowDropdownOutsideZoomArea(GetYearsGenericMenu);
                }
                CustomGUILayout.EndHorizontal();
                CustomGUILayout.Label(GetDateDisplay(), LabelStyle);
            }
            else
            {
                CustomGUILayout.BeginHorizontal(EditorStyles.toolbar);
                Texture2D icon = GetDueTimeIcon();
                if (icon != null)
                {
                    CustomGUILayout.Image(icon, LabelStyle, w: EditorGUIUtility.singleLineHeight, h: EditorGUIUtility.singleLineHeight);
                }
                CustomGUILayout.Label(_name, LabelStyle, h: EditorGUIUtility.singleLineHeight);
                CustomGUILayout.Label(GetDateDisplay(), LabelStyle, h: EditorGUIUtility.singleLineHeight);
                CustomGUILayout.EndHorizontal();
            }
        }

        public bool IsPastDueTime()
        {
            return GetDueTime() < DateTime.Now;
        }    

        /// <summary>
        /// Returns the due time.
        /// </summary>
        /// <returns></returns>
        public DateTime GetDueTime()
        {
            return new DateTime(_year, _monthIndex + 1, _day);
        }

        private GenericMenu GetMonthsGenericMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            int i = 0;
            foreach (var month in Months)
            {
                int index = i;
                genericMenu.AddItem(new GUIContent(month), false, () =>
                {
                    _monthIndex = index;
                });
                i++;
            }
            return genericMenu;
        }

        private GenericMenu GetDaysGenericMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            for (int i = 0; i < DateTime.DaysInMonth(_year, _monthIndex + 1); i++)
            {
                int day = i + 1;
                genericMenu.AddItem(new GUIContent($"{day}"), false, () =>
                {
                    _day = day;
                });
            }

            return genericMenu;
        }

        private GenericMenu GetYearsGenericMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            for (int i = DateTime.Now.Year; i < DateTime.Now.Year + 10; i++)
            {
                int year = i;
                genericMenu.AddItem(new GUIContent($"{year}"), false, () =>
                {
                    _year = year;
                });
            }

            return genericMenu;
        }

        public string GetDateDisplay()
        {
            return $"{Months[_monthIndex]} {_day}, {_year}";
        }

        public override MemberBase Copy(NodeBase parent)
        {
            return new DateTimeMember(this);
        }

        /// <summary>
        /// Returns an image representing the due time of the member.
        /// </summary>
        /// <returns></returns>
        public Texture2D GetDueTimeIcon()
        {
            if (IsPastDueTime())
            {
                return GUIStyleCollection.GetTexture("redlight");
            }
            else
            {
                return GUIStyleCollection.GetTexture("greenlight");
            }
        }
    }
}
