#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SirenixEditorGUI.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
    using System.Globalization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Collection of various editor GUI functions.
    /// </summary>
    [InitializeOnLoad]
    public static class SirenixEditorGUI
    {
        private static readonly Dictionary<Type, Delegate> DynamicFieldDrawers = new Dictionary<Type, Delegate>()
        {
            { typeof(char),     (Func<GUIContent, char, GUILayoutOption[], char>)       ((label, value, options) => {
                string str = value.ToString();
                str = SirenixEditorFields.TextField(label, str, options);
                if (str.Length == 0) return default(char);
                else return str[0];
            }) },
            { typeof(string),   (Func<GUIContent, string, GUILayoutOption[], string>)   ((label, value, options) => SirenixEditorFields.TextField(label, value, options)) },
            { typeof(sbyte),    (Func<GUIContent, sbyte, GUILayoutOption[], sbyte>)     ((label, value, options) => (sbyte)Mathf.Clamp(SirenixEditorFields.IntField(label, value, options), sbyte.MinValue, sbyte.MaxValue)) },
            { typeof(byte),     (Func<GUIContent, byte, GUILayoutOption[], byte>)       ((label, value, options) => (byte)Mathf.Clamp(SirenixEditorFields.IntField(label, value, options), byte.MinValue, byte.MaxValue)) },
            { typeof(short),    (Func<GUIContent, short, GUILayoutOption[], short>)     ((label, value, options) => (short)Mathf.Clamp(SirenixEditorFields.IntField(label, value, options), short.MinValue, short.MaxValue)) },
            { typeof(ushort),   (Func<GUIContent, ushort, GUILayoutOption[], ushort>)   ((label, value, options) => (ushort)Mathf.Clamp(SirenixEditorFields.IntField(label, value, options), ushort.MinValue, ushort.MaxValue)) },
            { typeof(int),      (Func<GUIContent, int, GUILayoutOption[], int>)         ((label, value, options) => SirenixEditorFields.IntField(label, value, options)) },
            { typeof(uint),     (Func<GUIContent, uint, GUILayoutOption[], uint>)       ((label, value, options) => (uint)Mathf.Clamp(EditorGUILayout.LongField(label, value, options), uint.MinValue, uint.MaxValue)) },
            { typeof(long),     (Func<GUIContent, long, GUILayoutOption[], long>)       ((label, value, options) => EditorGUILayout.LongField(label, value, options)) },
            { typeof(ulong),    (Func<GUIContent, ulong, GUILayoutOption[], ulong>)     ((label, value, options) => {
                string str = value.ToString(CultureInfo.InvariantCulture);
                str = EditorGUILayout.DelayedTextField(label, str, EditorStyles.textField);

                ulong newValue;

                if (GUI.changed && ulong.TryParse(str, NumberStyles.Any, null, out newValue))
                {
                    value = newValue;
                }

                return value;
            }) },
            { typeof(float),    (Func<GUIContent, float, GUILayoutOption[], float>)     ((label, value, options) => SirenixEditorFields.FloatField(label, value, options)) },
            { typeof(double),   (Func<GUIContent, double, GUILayoutOption[], double>)   ((label, value, options) => EditorGUILayout.DoubleField(label, value, options)) },
            { typeof(decimal),  (Func<GUIContent, decimal, GUILayoutOption[], decimal>) ((label, value, options) => {
                string str = value.ToString(CultureInfo.InvariantCulture);
                str = EditorGUILayout.DelayedTextField(label, str, EditorStyles.textField);
                decimal newValue;

                if (GUI.changed && decimal.TryParse(str, NumberStyles.Any, null, out newValue))
                {
                    value = newValue;
                }

                return value;
            }) },
            { typeof(Guid),   (Func<GUIContent, Guid, GUILayoutOption[], Guid>)   ((label, value, options) => SirenixEditorFields.GuidField(label, value, options)) },
        };
        private static readonly object fadeGroupKey = new object();
        private static readonly object shakeableGroupKey = new object();
        private static readonly object menuListKey = new object();
        private static readonly object animatedTabGroupKey = new object();
        private static readonly GUIScopeStack<Rect> verticalListBorderRects = new GUIScopeStack<Rect>();
        private static readonly List<int> currentListItemIndecies = new List<int>();
        private static int animatingFadeGroupIndex = -1;
        private static int currentFadeGroupIndex = 0;
        private static int currentScope = 0;
        private static float currentDrawingToolbarHeight;
        private static float slideRectSensitivity = 0f;
        private static float defaultFadeGroupDuration = 0.13f;
        private static float tabPageSlideAnimationDuration = 0.13f;
        private static float shakingAnimationDuration = 0.5f;
        private static bool expandFoldoutByDefault = true;
        private static object drawColorPaletteColorPickerKey = new object();
        private static BeginAutoScrollBoxInfo currentBeginAutoScrollBoxInfo;
        //private static int currentDrawingToolbarPaddingTop;
        private static float slideDeltaBuffer;
        private static VerticalMenuListInfo currentVerticalMenuListInfo;
        private static bool popMenuItemLabelColor = false;

        /// <summary>
        /// Default fade group animation duration.
        /// </summary>
        public static float DefaultFadeGroupDuration
        {
            get
            {
                return defaultFadeGroupDuration;
            }
            set
            {
                defaultFadeGroupDuration = value;
                EditorPrefs.SetFloat("SirenixEditorGUI.DefaultFadeGroupDuration", value);
            }
        }

        /// <summary>
        /// Tab page slide animation duration.
        /// </summary>
        public static float TabPageSlideAnimationDuration
        {
            get
            {
                return tabPageSlideAnimationDuration;
            }
            set
            {
                tabPageSlideAnimationDuration = value;
                EditorPrefs.SetFloat("SirenixEditorGUI.TabPageSlideAnimationDuration", value);
            }
        }

        /// <summary>
        /// Shaking animation duration.
        /// </summary>
        public static float ShakingAnimationDuration
        {
            get
            {
                return shakingAnimationDuration;
            }
            set
            {
                shakingAnimationDuration = value;
                EditorPrefs.SetFloat("SirenixEditorGUI.ShakingAnimationDuration", value);
            }
        }

        /// <summary>
        /// Expand foldouts by default.
        /// </summary>
        public static bool ExpandFoldoutByDefault
        {
            get
            {
                return expandFoldoutByDefault;
            }
            set
            {
                expandFoldoutByDefault = value;
                EditorPrefs.SetBool("SirenixEditorGUI.ExpandFoldoutByDefault", value);
            }
        }

        /// <summary>
        /// Draws a GUI field for objects.
        /// </summary>
        /// <param name="rect">The rect to draw the field in.</param>
        /// <param name="label">The label of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="objectType">The object type for the field.</param>
        /// <param name="allowSceneObjects">If set to <c>true</c> then allow scene objects to be assigned to the field.</param>
        /// <param name="isReadOnly">If set to <c>true</c> the field is readonly.</param>
        /// <returns>The object assigned to the field.</returns>
        [Obsolete("Use SirenixEditorFields.UnityObjectField instead")]
        public static UnityEngine.Object ObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, bool isReadOnly = false)
        {
            bool removeFocus = false;
            if (isReadOnly)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    var objectFieldRect = new Rect(rect.x + 20, rect.y, rect.width - 20, 16);

                    if (new Rect(rect.xMax - 20, rect.y, 20, rect.height).Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                    }
                    else if (objectFieldRect.Contains(Event.current.mousePosition))
                    {
                        removeFocus = true;
                    }
                }
            }

            // Sometimes a Unity value is *so null* that Unity can't even handle it
            // In these cases, we just pass in a straight null instead, to make Unity
            // feel better about itself.
            bool isNull = value == null;// || value.SafeIsUnityNull();

            var penRect = rect;
            penRect.x += penRect.width - 38;
            penRect.width = 20;
            
            BeginDrawOpenInspector(penRect, value, IndentLabelRect(rect, label != null));

            //rect.xMin += 4;
            value = label == null ?
                EditorGUI.ObjectField(rect, isNull ? null : value, objectType, allowSceneObjects) :
                EditorGUI.ObjectField(rect, label, isNull ? null : value, objectType, allowSceneObjects);

            EndDrawOpenInspector(penRect, value);

            if (removeFocus)
            {
                GUIHelper.RemoveFocusControl();
            }

            return value;
        }

        internal static Rect IndentLabelRect(Rect totalPosition, bool hasLabel)
        {
            if (!hasLabel)
            {
                return EditorGUI.IndentedRect(totalPosition);
            }
            else
            {
                return new Rect(totalPosition.x + EditorGUIUtility.labelWidth, totalPosition.y, totalPosition.width - EditorGUIUtility.labelWidth, totalPosition.height);
            }
        }

        /// <summary>
        /// Draws an GUI field for objects.
        /// </summary>
        /// <param name="label">The label for the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="objectType">The object type for the field.</param>
        /// <param name="allowSceneObjects">If set to <c>true</c> then allow scene objects to be assigned to the field.</param>
        /// <param name="isReadOnly">If set to <c>true</c> the field is readonly.</param>
        /// <returns>The object assigned to the field.</returns>
        [Obsolete("Use SirenixEditorFields.UnityObjectField instead")]
        public static UnityEngine.Object ObjectField(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, bool isReadOnly = false)
        {
            var rect = EditorGUILayout.GetControlRect();
            return ObjectField(rect, label, value, objectType, allowSceneObjects, isReadOnly);
        }

        /// <summary>
        /// Draws a GUI field for objects.
        /// </summary>
        /// <param name="key">The key for the field.</param>
        /// <param name="type">The type.</param>
        /// <param name="label">The label for the field.</param>
        /// <param name="value">The current value for the field.</param>
        /// <param name="allowSceneObjects">If set to <c>true</c> then allow scene objects to be assigned to the field.</param>
        /// <returns>
        /// The object assigned to the field.
        /// </returns>
        [Obsolete("Use SirenixEditorFields.PolymorphicObjectField instead")]
        public static object ObjectField(object key, Type type, GUIContent label, object value, bool allowSceneObjects = true)
        {
            var dropZone = DragAndDropManager.BeginDropZone(key, type, false);

            var rect = EditorGUILayout.GetControlRect();

            if (label != null && label.text != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }
            else
            {
                rect = EditorGUI.IndentedRect(rect);
            }

            GUIContent title;

            if (EditorGUI.showMixedValue)
            {
                title = new GUIContent("   " + "— Conflict (" + type.GetNiceName() + ")");
            }
            else if (value == null)
            {
                title = new GUIContent("   " + "Null (" + type.GetNiceName() + ")");
            }
            else
            {
                string baseType = value.GetType() == type ? "" : " : " + type.GetNiceName();
                title = new GUIContent("   " + value.GetType().GetNiceName() + baseType);
            }

            GUI.Label(rect, title, EditorStyles.objectField);
            EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height));

            var eventType = Event.current.type;

            if (dropZone.IsReadyToClaim)
            {
                object droppedObject = dropZone.ClaimObject();
                GUI.changed = true;
                return droppedObject;
            }

            // Handle Unity dragging manually for now
            else if ((eventType == EventType.DragUpdated || eventType == EventType.DragPerform) && dropZone.Rect.Contains(Event.current.mousePosition) && DragAndDrop.objectReferences.Length == 1)
            {
                UnityEngine.Object obj = DragAndDrop.objectReferences[0];

                bool accept = false;

                if (type.IsAssignableFrom(obj.GetType()))
                {
                    accept = true;
                }
                else if (obj is GameObject && (type.InheritsFrom(typeof(Component)) || type.IsInterface))
                {
                    obj = (obj as GameObject).GetComponent(type);

                    if (obj != null)
                    {
                        accept = true;
                    }
                }

                if (accept)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();

                    if (eventType == EventType.dragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        GUI.changed = true;
                        return obj;
                    }
                }
            }

            DragAndDropManager.EndDropZone();

            var objectPicker = ObjectPicker.GetObjectPicker(key, type);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                objectPicker.ShowObjectPicker(allowSceneObjects, rect);
            }

            if (objectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
            {
                GUI.changed = true;
                return objectPicker.ClaimObject();
            }

            return value;
        }

        /// <summary>
        /// Draws a nicely formatted title with an optinal sub-title and horizontal ruler.
        /// </summary>
        public static void Title(string title, string subtitle, TextAlignment textAlignment, bool horizontalLine, bool boldLabel = true)
        {
            GUIStyle titleStyle = null;
            GUIStyle subtitleStyle = null;

            switch (textAlignment)
            {
                case TextAlignment.Left:
                    titleStyle = boldLabel ? SirenixGUIStyles.BoldTitle : SirenixGUIStyles.Title;
                    subtitleStyle = SirenixGUIStyles.Subtitle;
                    break;

                case TextAlignment.Center:
                    titleStyle = boldLabel ? SirenixGUIStyles.BoldTitleCentered : SirenixGUIStyles.TitleCentered;
                    subtitleStyle = SirenixGUIStyles.SubtitleCentered;
                    break;

                case TextAlignment.Right:
                    titleStyle = boldLabel ? SirenixGUIStyles.BoldTitleRight : SirenixGUIStyles.TitleRight;
                    subtitleStyle = SirenixGUIStyles.SubtitleRight;
                    break;

                default:
                    // Hidden feature by calling: Title("title", "subTitle", (TextAlignment)3, true, true);
                    // This hidden feature is added because the TitleAlignment enum located in the Sirenix.OdinInspector.Attribute assembly have an extra split option.
                    // But we don't have access to the assembly from here.
                    titleStyle = boldLabel ? SirenixGUIStyles.BoldTitle : SirenixGUIStyles.Title;
                    subtitleStyle = SirenixGUIStyles.SubtitleRight;
                    break;
            }

            if ((int)textAlignment > (int)TextAlignment.Right)
            {
                var rect = GUILayoutUtility.GetRect(0, 18, titleStyle, GUILayoutOptions.ExpandWidth());

                GUI.Label(rect, title, titleStyle);
                rect.y += 3;
                GUI.Label(rect, subtitle, subtitleStyle);

                if (horizontalLine)
                {
                    SirenixEditorGUI.HorizontalLineSeparator(SirenixGUIStyles.LightBorderColor, 1);
                    GUILayout.Space(3f);
                }
            }
            else
            {
                Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
                GUI.Label(rect, title, titleStyle);

                if (subtitle != null && !subtitle.IsNullOrWhitespace())
                {
                    rect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(GUIHelper.TempContent(subtitle), subtitleStyle));
                    GUI.Label(rect, subtitle, subtitleStyle);
                }

                if (horizontalLine)
                {
                    SirenixEditorGUI.DrawSolidRect(rect.AlignBottom(1), SirenixGUIStyles.LightBorderColor);
                    GUILayout.Space(3f);
                }
            }
        }

        /// <summary>
        /// Draws a GUI color field.
        /// </summary>
        /// <param name="rect">The rect to draw the field in.</param>
        /// <param name="color">The color of the field.</param>
        /// <param name="useAlphaInPreview">If set to <c>true</c> then use alpha in the preview.</param>
        /// <param name="showAlphaBar">If set to <c>true</c> then show alpha bar in the preview.</param>
        /// <returns>The color assigned to the field.</returns>
        public static Color DrawColorField(Rect rect, Color color, bool useAlphaInPreview = true, bool showAlphaBar = false)
        {
            const int margin = 3;

            EditorGUI.LabelField(rect, GUIContent.none, SirenixGUIStyles.ColorFieldBackground);

            rect.x += margin;
            rect.y += margin;
            rect.width -= margin * 2;
            rect.height -= margin * 2;

            if (Event.current.type != EventType.Repaint)
            {
#pragma warning disable 0618 // Type or member is obsolete
                color = EditorGUI.ColorField(rect, GUIContent.none, color, false, true, false, null);
#pragma warning restore 0618 // Type or member is obsolete
            }

            DrawSolidRect(rect, useAlphaInPreview ? color : new Color(color.r, color.g, color.b, 1f));

            if (showAlphaBar)
            {
                rect.y += rect.height - 7;
                rect.height = 7;
                DrawSolidRect(rect, Color.black);
                rect.width *= color.a;
                DrawSolidRect(rect, Color.white);
            }

            return color;
        }

        /// <summary>
        ///	Draws a warning message box.
        /// </summary>
        /// <remarks>
        /// Also triggers a warning during validation checks done by <see cref="OdinInspectorValidationChecker"/>
        /// </remarks>
        /// <param name="message">The message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void WarningMessageBox(string message, bool wide = true)
        {
            MessageBox(message, MessageType.Warning, wide);

            if (OdinInspectorValidationChecker.IsRunningValidationCheck)
            {
                OdinInspectorValidationChecker.LogWarning(message);
            }
            else
            {
                if (Event.current.button == 1 && Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Copy warning message"), false, () => { Clipboard.Copy(message); });
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
        }

        /// <summary>
        ///	Draws an error message box.
        /// </summary>
        /// <remarks>
        /// Also triggers an error during validation checks done by <see cref="OdinInspectorValidationChecker"/>
        /// </remarks>
        /// <param name="message">The message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void ErrorMessageBox(string message, bool wide = true)
        {
            MessageBox(message, MessageType.Error, wide);
        }

        /// <summary>
        /// Draws a info message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void InfoMessageBox(string message, bool wide = true)
        {
            MessageBox(message, MessageType.Info, wide);
        }

        /// <summary>
        /// Draws a message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void MessageBox(string message, bool wide = true)
        {
            MessageBox(message, MessageType.None, wide);
        }

        /// <summary>
        /// Draws a message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void MessageBox(string message, MessageType messageType, bool wide = true)
        {
            PrivateMessageBox(message, messageType, SirenixGUIStyles.MessageBox, wide);
        }

        /// <summary>
        /// Draws a message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="messageBoxStyle">The style of the message box.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        private static Rect PrivateMessageBox(string message, MessageType messageType, GUIStyle messageBoxStyle, bool wide = true)
        {
            // TODO: Implement wide properly or make it obsolete as we don't really use it anyway.
            Texture icon = null;
            switch (messageType)
            {
                case MessageType.Info:
                    icon = EditorIcons.UnityInfoIcon;
                    break;

                case MessageType.Warning:
                    if (OdinInspectorValidationChecker.IsRunningValidationCheck)
                    {
                        OdinInspectorValidationChecker.LogWarning(message);
                    }

                    icon = EditorIcons.UnityWarningIcon;
                    break;

                case MessageType.Error:
                    if (OdinInspectorValidationChecker.IsRunningValidationCheck)
                    {
                        OdinInspectorValidationChecker.LogError(message);
                    }

                    icon = EditorIcons.UnityErrorIcon;
                    break;

                default:
                    icon = null;
                    break;
            }

            var rect = GUILayoutUtility.GetRect(GUIHelper.TempContent(message, icon), messageBoxStyle);

            // // TODO: Mimic GetControlRect instead of this.
            // var propertWidth = EditorGUILayout.GetControlRect(false, 1);
            // rect.xMin = propertWidth.xMin;
            // rect.xMax = propertWidth.xMax;
            // GUILayout.Space(-3);
            // // TODO: Mimic GetControlRect instead of this.

            rect = EditorGUI.IndentedRect(rect);

            GUI.Label(rect, GUIHelper.TempContent(message, icon), messageBoxStyle);

            if (Event.current.button == 1 && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy message"), false, () => { Clipboard.Copy(message); });
                menu.ShowAsContext();
                Event.current.Use();
            }

            return rect;
        }

        /// <summary>
        /// Draws a message box that can be expanded to show more details.
        /// </summary>
        /// <param name="message">The message of the message box.</param>
        /// <param name="detailedMessage">The detailed message of the message box.</param>
        /// <param name="messageType">Type of the message box.</param>
        /// <param name="isFolded">If set to <c>true</c> the detailed message is hidden.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        /// <returns>State of isFolded.</returns>
        public static bool DetailedMessageBox(string message, string detailedMessage, MessageType messageType, bool isFolded, bool wide = true)
        {
            if (OdinInspectorValidationChecker.IsRunningValidationCheck)
            {
                message = message + "\n\n" + detailedMessage;
                detailedMessage = message;
            }

            var rect = PrivateMessageBox(isFolded ? message : detailedMessage, messageType, SirenixGUIStyles.DetailedMessageBox, wide);

            var e = Event.current.rawType;

            if (e == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                isFolded = !isFolded;
                // We can only eat events while GUI.enabled is true.
                GUIHelper.PushGUIEnabled(true);
                Event.current.Use();
                GUIHelper.PopGUIEnabled();
            }

            if (e == EventType.Repaint)
            {
                rect.y += rect.height * 0.5f - 9;
                rect.x += rect.width - 9 - 10;
                rect.height = rect.width = 18;

                var arrow = isFolded ? EditorIcons.TriangleDown : EditorIcons.TriangleUp;
                GUI.Label(rect, GUIHelper.TempContent(arrow.Active, isFolded ? "Show details" : "Hide details"));
            }

            return isFolded;
        }

        /// <summary>
        /// Draws a horizontal line separator.
        /// </summary>
        /// <param name="lineWidth">Width of the line.</param>
        public static void HorizontalLineSeparator(int lineWidth = 1)
        {
            HorizontalLineSeparator(SirenixGUIStyles.BorderColor, lineWidth);
        }

        /// <summary>
        /// Draws a horizontal line separator.
        /// </summary>
        /// <param name="color">The color of the line.</param>
        /// <param name="lineWidth">The size of the line.</param>
        public static void HorizontalLineSeparator(Color color, int lineWidth = 1)
        {
            Rect rect = GUILayoutUtility.GetRect(lineWidth, lineWidth, GUILayoutOptions.ExpandWidth(true));
            DrawSolidRect(rect, color, true);
        }

        /// <summary>
        /// Draws a vertical line separator.
        /// </summary>
        /// <param name="lineWidth">Width of the line.</param>
        public static void VerticalLineSeparator(int lineWidth = 1)
        {
            VerticalLineSeparator(SirenixGUIStyles.BorderColor, lineWidth);
        }

        /// <summary>
        /// Draws a vertical line separator.
        /// </summary>
        /// <param name="color">The color of the line.</param>
        /// <param name="lineWidth">Width of the line.</param>
        public static void VerticalLineSeparator(Color color, int lineWidth = 1)
        {
            Rect rect = GUILayoutUtility.GetRect(lineWidth, lineWidth, GUILayoutOptions.ExpandHeight(true).Width(lineWidth));
            DrawSolidRect(rect, color, true);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="icon">The editor icon for the button.</param>
        /// <param name="width">The width of the button.</param>
        /// <param name="height">The height of the button.</param>
        /// <param name="tooltip">The tooltip of the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(EditorIcon icon, int width = 18, int height = 18, string tooltip = "")
        {
            return IconButton(icon, null, width, height, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="icon">The editor icon for the button.</param>
        /// <param name="style">The GUI style for the button.</param>
        /// <param name="width">The width of the button.</param>
        /// <param name="height">The height of the button.</param>
        /// <param name="tooltip">The tooltip of the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(EditorIcon icon, GUIStyle style, int width = 18, int height = 18, string tooltip = "")
        {
            style = style ?? SirenixGUIStyles.IconButton;
            Rect rect = GUILayoutUtility.GetRect(icon.HighlightedGUIContent, style, GUILayoutOptions.ExpandWidth(false).Width(width).Height(height));
            return IconButton(rect, icon, style, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The editor icon for the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(Rect rect, EditorIcon icon)
        {
            return IconButton(rect, icon, null, "");
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The editor icon for the button.</param>
        /// <param name="tooltip">The tooltip of the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(Rect rect, EditorIcon icon, string tooltip)
        {
            return IconButton(rect, icon, null, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The editor icon for the button.</param>
        /// <param name="style">The GUI style for the button.</param>
        /// <param name="tooltip">The tooltip of the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(Rect rect, EditorIcon icon, GUIStyle style, string tooltip)
        {
            style = style ?? SirenixGUIStyles.IconButton;
            if (GUI.Button(rect, GUIContent.none, style))
            {
                GUIHelper.RemoveFocusControl();
                return true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                float size = Mathf.Min(rect.height, rect.width);
                icon.Draw(rect.AlignCenter(size, size));
            }

            return false;
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The icon texture.</param>
        /// <param name="tooltip">The tooltip for the button.</param>
        /// <returns><c>true</c> when the button is pressed.</returns>
        public static bool IconButton(Rect rect, Texture icon, string tooltip)
        {
            return IconButton(rect, icon, null, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The icon texture.</param>
        /// <param name="style">Style for the button.</param>
        /// <param name="tooltip">The tooltip for the button.</param>
        /// <returns><c>true</c> when the button is pressed.</returns>
        public static bool IconButton(Rect rect, Texture icon, GUIStyle style, string tooltip)
        {
            var content = new GUIContent(icon, tooltip);
            style = style ?? SirenixGUIStyles.IconButton;

            if (GUI.Button(rect, content, style))
            {
                GUIHelper.RemoveFocusControl();
                return true;
            }

            if (Event.current.isMouse)
            {
                GUIHelper.RequestRepaint();
            }

            return false;
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="icon">The icon texture.</param>
        /// <param name="width">Width of the button in pixels.</param>
        /// <param name="height">Height of the button in pixels.</param>
        /// <param name="tooltip">The tooltip for the button.</param>
        /// <returns><c>true</c> when the button is pressed.</returns>
        public static bool IconButton(Texture icon, int width = 18, int height = 18, string tooltip = "")
        {
            return IconButton(icon, SirenixGUIStyles.IconButton, width, height, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="icon">The icon texture.</param>
        /// <param name="style">Style for the button.</param>
        /// <param name="width">Width of the button in pixels.</param>
        /// <param name="height">Height of the button in pixels.</param>
        /// <param name="tooltip">The tooltip for the button.</param>
        /// <returns><c>true</c> when the button is pressed.</returns>
        public static bool IconButton(Texture icon, GUIStyle style, int width = 18, int height = 18, string tooltip = "")
        {
            style = style ?? SirenixGUIStyles.IconButton;
            Rect rect = GUILayoutUtility.GetRect(GUIHelper.TempContent(icon), style, GUILayoutOptions.ExpandWidth(false).Width(width).Height(height));
            return IconButton(rect, icon, style, tooltip);
        }

        /// <summary>
        /// Draws a repeating icon button.
        /// </summary>
        /// <param name="icon">The icon for the button.</param>
        /// <returns><c>true</c> while the button is active. Otherwise <c>false</c>.</returns>
        public static bool IconRepeatButton(EditorIcon icon)
        {
            if (IconRepeatButton(icon, 18))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a repeating icon button.
        /// </summary>
        /// <param name="icon">The icon for the button.</param>
        /// <param name="size">The size.</param>
        /// <returns><c>true</c> while the button is active. Otherwise <c>false</c>.</returns>
        public static bool IconRepeatButton(EditorIcon icon, int size)
        {
            if (IconRepeatButton(icon, size, size))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a repeating icon button.
        /// </summary>
        /// <param name="icon">The icon for the button.</param>
        /// <param name="width">The width of the button.</param>
        /// <param name="height">The height of the button.</param>
        /// <returns><c>true</c> while the button is active. Otherwise <c>false</c>.</returns>
        public static bool IconRepeatButton(EditorIcon icon, int width, int height)
        {
            var rect = GUILayoutUtility.GetRect(width, height);

            if (GUI.RepeatButton(rect, icon.ActiveGUIContent, SirenixGUIStyles.None))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a toolbar icon button.
        /// </summary>
        /// <param name="icon">The icon for the button.</param>
        /// <param name="ignoreGUIEnabled">If true, the button clickable while GUI.enabled == false.</param>
        /// <returns>
        ///   <c>true</c> if the button was pressed. Otherwise <c>false</c>.
        /// </returns>
        public static bool ToolbarButton(EditorIcon icon, bool ignoreGUIEnabled = false)
        {
            var rect = GUILayoutUtility.GetRect(currentDrawingToolbarHeight, currentDrawingToolbarHeight);
            if (GUI.Button(rect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                rect.y -= 1;
                //rect.y -= 1;
                //rect.x -= 1;
                //rect = rect.AlignCenter(16, 16);
                icon.Draw(rect, 16);
            }

            if (ignoreGUIEnabled)
            {
                if (Event.current.button == 0 && Event.current.rawType == EventType.MouseDown)
                {
                    if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        GUIHelper.RemoveFocusControl();
                        GUIHelper.RequestRepaint();
                        GUIHelper.PushGUIEnabled(true);
                        Event.current.Use();
                        GUIHelper.PopGUIEnabled();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Draws a toolbar icon button.
        /// </summary>
        /// <param name="content">The GUI content for the button.</param>
        /// <param name="selected">Whether the button state is selected or not</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool ToolbarButton(GUIContent content, bool selected = false)
        {
            if (GUILayout.Button(content, selected ? SirenixGUIStyles.ToolbarButtonSelected : SirenixGUIStyles.ToolbarButton, GUILayoutOptions.Height(currentDrawingToolbarHeight)))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a toolbar toggle.
        /// </summary>
        /// <param name="isActive">Current state of the toggle.</param>
        /// <param name="icon">The icon for the toggle.</param>
        /// <returns>The state of the toggle.</returns>
        public static bool ToolbarToggle(bool isActive, EditorIcon icon)
        {
            if (GUILayout.Toggle(isActive, icon.Highlighted, SirenixGUIStyles.ToolbarButton, GUILayoutOptions.Width(currentDrawingToolbarHeight).Height(currentDrawingToolbarHeight)))
            {
                if (isActive == false)
                {
                    GUIHelper.RemoveFocusControl();
                    GUIHelper.RequestRepaint();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws a toolbar toggle.
        /// </summary>
        /// <param name="isActive">Current state of the toggle.</param>
        /// <param name="content">The GUI content for the button.</param>
        /// <returns>The state of the toggle.</returns>
        public static bool ToolbarToggle(bool isActive, GUIContent content)
        {
            if (GUILayout.Toggle(isActive, content, SirenixGUIStyles.ToolbarButton, GUILayoutOptions.Height(currentDrawingToolbarHeight)))
            {
                if (isActive == false)
                {
                    GUIHelper.RemoveFocusControl();
                    GUIHelper.RequestRepaint();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws a toolbar toggle.
        /// </summary>
        /// <param name="isActive">Current state of the toggle.</param>
        /// <param name="text">The text for the toggle.</param>
        /// <returns>The state of the toggle.</returns>
        public static bool ToolbarToggle(bool isActive, string text)
        {
            return ToolbarToggle(isActive, GUIHelper.TempContent(text));

            //EditorStyles.toolbarButton.fixedHeight = currentDrawingToolbarHeight;
            //if (GUILayout.Toggle(isActive, text, EditorStyles.toolbarButton, GUILayoutOptions.Height(currentDrawingToolbarHeight)))
            //{
            //    if (isActive == false)
            //    {
            //        GUIHelper.FocusControl();
            //        GUIHelper.RequestRepaint();
            //    }
            //    return true;
            //}
            //return false;
        }

        /// <summary>
        /// Draws a toolbar tab.
        /// </summary>
        /// <param name="isActive">If <c>true</c> the tab will be the active tab.</param>
        /// <param name="label">Name for the tab.</param>
        /// <returns>State of isActive.</returns>
        public static bool ToolbarTab(bool isActive, string label)
        {
            return ToolbarTab(isActive, GUIHelper.TempContent(label));
        }

        /// <summary>
        /// Draws a toolbar tab.
        /// </summary>
        /// <param name="isActive">If <c>true</c> the tab will be the active tab.</param>
        /// <param name="label">Label for the tab.</param>
        /// <returns>State of isActive.</returns>
        public static bool ToolbarTab(bool isActive, GUIContent label)
        {
            if (GUILayout.Toggle(isActive, label, SirenixGUIStyles.ToolbarTab, GUILayoutOptions.Height(currentDrawingToolbarHeight)))
            {
                if (isActive == false)
                {
                    GUIHelper.RemoveFocusControl();
                    GUIHelper.RequestRepaint();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws a solid color rectangle.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="color">The color.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawSolidRect(Rect rect, Color color, bool usePlaymodeTint = true)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (usePlaymodeTint)
                {
                    EditorGUI.DrawRect(rect, color);
                }
                else
                {
                    GUIHelper.PushColor(color);
                    GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
                    GUIHelper.PopColor();
                }
            }
        }

        /// <summary>
        /// Draws a solid color rectangle.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="color">The color.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        /// <returns>The rect created.</returns>
        public static Rect DrawSolidRect(float width, float height, Color color, bool usePlaymodeTint = true)
        {
            var rect = GUILayoutUtility.GetRect(width, height, GUILayoutOptions.ExpandWidth(false));
            DrawSolidRect(rect, color, usePlaymodeTint);
            return rect;
        }

        /// <summary>
        /// Draws borders around a rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="borderWidth">The width of the border on all sides.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int borderWidth, bool usePlaymodeTint = true)
        {
            DrawBorders(rect, borderWidth, borderWidth, borderWidth, borderWidth, SirenixGUIStyles.BorderColor, usePlaymodeTint);
        }

        /// <summary>
        /// Draws borders around a rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="borderWidth">The width of the border on all sides.</param>
        /// <param name="color">The color of the border.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int borderWidth, Color color, bool usePlaymodeTint = true)
        {
            DrawBorders(rect, borderWidth, borderWidth, borderWidth, borderWidth, color, usePlaymodeTint);
        }

        /// <summary>
        /// Draws borders around a rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="left">The left size.</param>
        /// <param name="right">The right size.</param>
        /// <param name="top">The top size.</param>
        /// <param name="bottom">The bottom size.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int left, int right, int top, int bottom, bool usePlaymodeTint = true)
        {
            DrawBorders(rect, left, right, top, bottom, SirenixGUIStyles.BorderColor, usePlaymodeTint);
        }

        /// <summary>
        /// Draws borders around a rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="left">The left size.</param>
        /// <param name="right">The right size.</param>
        /// <param name="top">The top size.</param>
        /// <param name="bottom">The bottom size.</param>
        /// <param name="color">The color of the borders.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int left, int right, int top, int bottom, Color color, bool usePlaymodeTint = true)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (left > 0)
                {
                    var borderRect = rect;
                    borderRect.width = left;
                    DrawSolidRect(borderRect, color, usePlaymodeTint);
                }

                if (top > 0)
                {
                    var borderRect = rect;
                    borderRect.height = top;
                    DrawSolidRect(borderRect, color, usePlaymodeTint);
                }

                if (right > 0)
                {
                    var borderRect = rect;
                    borderRect.x += rect.width - right;
                    borderRect.width = right;
                    DrawSolidRect(borderRect, color, usePlaymodeTint);
                }

                if (bottom > 0)
                {
                    var borderRect = rect;
                    borderRect.y += rect.height - bottom;
                    borderRect.height = bottom;
                    DrawSolidRect(borderRect, color, usePlaymodeTint);
                }
            }
        }

        /// <summary>
        /// Draws a toolbar search field.
        /// </summary>
        /// <param name="searchText">The current search text.</param>
        /// <param name="forceFocus">If set to <c>true</c> the force focus on the field.</param>
        /// <param name="marginLeftRight">The left and right margin.</param>
        /// <returns>The current search text.</returns>
        public static string ToolbarSearchField(string searchText, bool forceFocus = false, float marginLeftRight = 5)
        {
            searchText = searchText ?? "";
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, SirenixGUIStyles.ToolbarSeachTextField, GUILayoutOptions.ExpandWidth(true));
            rect.y += (currentDrawingToolbarHeight - 19) * 0.5f;
            rect.x += marginLeftRight;
            rect.width -= marginLeftRight * 2;
            rect.width -= 16;

            if (forceFocus)
            {
                GUI.SetNextControlName("ToolbarSearchField");
            }

            bool ignore = Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.UpArrow;

            if (ignore)
            {
                GUIHelper.PushEventType(EventType.Ignore);
            }

            searchText = EditorGUI.TextField(rect, searchText, SirenixGUIStyles.ToolbarSeachTextField);

            if (ignore)
            {
                GUIHelper.PopEventType();
            }

            if (forceFocus)
            {
                GUI.FocusControl("ToolbarSearchField");
            }

            rect.x += rect.width;
            rect.width = 16;

            if (GUI.Button(rect, GUIContent.none, SirenixGUIStyles.ToolbarSeachCancelButton))
            {
                searchText = "";
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
            }

            return searchText;
        }

        /// <summary>
        /// Begins a horizontal toolbar. Remember to end with <see cref="EndHorizontalToolbar"/>.
        /// </summary>
        /// <param name="height">The height of the toolbar.</param>
        /// <param name="paddingTop">Padding for the top of the toolbar.</param>
        /// <returns>The rect of the horizontal toolbar.</returns>
        public static Rect BeginHorizontalToolbar(float height = 22, int paddingTop = 4)
        {
            var rect = BeginHorizontalToolbar(SirenixGUIStyles.ToolbarBackground, height, paddingTop);
            return rect;
        }

        /// <summary>
        /// Begins a horizontal toolbar. Remember to end with <see cref="EndHorizontalToolbar" />.
        /// </summary>
        /// <param name="style">The style for the toolbar.</param>
        /// <param name="height">The height of the toolbar.</param>
        /// <param name="topPadding">The top padding.</param>
        /// <returns>
        /// The rect of the horizontal toolbar.
        /// </returns>
        public static Rect BeginHorizontalToolbar(GUIStyle style, float height = 22, int topPadding = 4)
        {
            currentDrawingToolbarHeight = height;
            var rect = EditorGUILayout.BeginHorizontal(style, GUILayoutOptions.Height(height).ExpandWidth(false));
            //style.fixedHeight = prev;
            GUIHelper.PushHierarchyMode(false);
            GUIHelper.PushIndentLevel(0);
            //style.padding.top = prevPadding;
            return rect;
        }

        /// <summary>
        /// Ends a horizontal toolbar started by <see cref="BeginHorizontalToolbar(int, int)"/>.
        /// </summary>
        public static void EndHorizontalToolbar()
        {
            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUIHelper.GetCurrentLayoutRect();
                rect.yMin -= 1;
                DrawBorders(rect, 1);
            }

            GUIHelper.PopIndentLevel();
            GUIHelper.PopHierarchyMode();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Begins a horizontal indentation. Remember to end with <see cref="EndIndentedHorizontal"/>.
        /// </summary>
        /// <param name="options">The GUI layout options.</param>
        public static void BeginIndentedHorizontal(params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(GUIStyle.none, options);
            if (EditorGUI.indentLevel != 0)
            {
                IndentSpace();
            }
            GUIHelper.PushIndentLevel(0);
        }

        /// <summary>
        /// Begins a horizontal indentation. Remember to end with <see cref="EndIndentedHorizontal"/>.
        /// </summary>
        /// <param name="style">The style of the indentation.</param>
        /// <param name="options">The GUI layout options.</param>
        public static void BeginIndentedHorizontal(GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(style, options);
            if (EditorGUI.indentLevel != 0)
            {
                IndentSpace();
            }
            GUIHelper.PushIndentLevel(0);
        }

        /// <summary>
        /// Ends a identation horizontal layout group started by <see cref="BeginIndentedHorizontal(GUILayoutOption[])"/>.
        /// </summary>
        public static void EndIndentedHorizontal()
        {
            GUIHelper.PopIndentLevel();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Begins a vertical indentation. Remember to end with <see cref="EndIndentedVertical"/>.
        /// </summary>
        /// <param name="options">The GUI layout options.</param>
        public static void BeginIndentedVertical(params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            if (EditorGUI.indentLevel != 0)
            {
                GUILayout.Space(GUIHelper.CurrentIndentAmount);
            }
            GUIHelper.PushIndentLevel(0);
            GUILayout.BeginVertical(options);
            //GUILayout.Space(0); // Unity bug fixing.
        }

        /// <summary>
        /// Begins a vertical indentation. Remember to end with <see cref="EndIndentedVertical"/>.
        /// </summary>
        /// <param name="style">The style of the indentation.</param>
        /// <param name="options">The GUI layout options.</param>
        public static void BeginIndentedVertical(GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(GUIStyle.none);
            if (EditorGUI.indentLevel != 0)
            {
                GUILayout.Space(GUIHelper.CurrentIndentAmount);
            }
            GUIHelper.PushIndentLevel(0);
            GUILayout.BeginVertical(style ?? GUIStyle.none, options);
            //GUILayout.Space(0); // Unity bug fixing.
        }

        /// <summary>
        /// Ends a identation vertical layout group started by <see cref="BeginIndentedVertical(GUILayoutOption[])"/>.
        /// </summary>
        public static void EndIndentedVertical()
        {
            //GUILayout.Space(-2); // Unity bug fixing.
            GUILayout.EndVertical();
            GUIHelper.PopIndentLevel();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Indents by the current indent value, <see cref="GUIHelper.CurrentIndentAmount"/>.
        /// </summary>
        public static void IndentSpace()
        {
            GUILayout.Space(GUIHelper.CurrentIndentAmount);
        }

        /// <summary>
        /// Draws a menu button.
        /// </summary>
        /// <param name="indent">The indent of the button.</param>
        /// <param name="text">The text of the button.</param>
        /// <param name="isActive">The current state of the button.</param>
        /// <param name="icon">The texture icon for the button.</param>
        /// <returns>The current state of the button.</returns>
        public static bool MenuButton(int indent, string text, bool isActive, Texture icon)
        {
            bool isDown = false;

            var rect = EditorGUILayout.BeginHorizontal(SirenixGUIStyles.MenuButtonBackground);
            bool isMouseOver = rect.Contains(Event.current.mousePosition);
            if (isActive)
            {
                DrawSolidRect(rect, isMouseOver ? SirenixGUIStyles.MenuButtonActiveBgColor : SirenixGUIStyles.MenuButtonActiveBgColor);
            }
            else
            {
                DrawSolidRect(rect, isMouseOver ? SirenixGUIStyles.MenuButtonHoverColor : SirenixGUIStyles.MenuButtonColor);
            }

            DrawBorders(rect, 0, 0, 0, 1, SirenixGUIStyles.MenuButtonBorderColor);

            if (Event.current.type == EventType.MouseDown)
            {
                if (isMouseOver)
                {
                    Event.current.Use();
                    isDown = true;
                }
                GUIHelper.RequestRepaint();
            }

            var style = new GUIStyle(EditorStyles.label);
            style.fixedHeight = 20;
            if (isActive)
            {
                style.normal.textColor = Color.white;
            }

            GUILayout.Space(indent * 10);

            //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
            GUILayout.Label(new GUIContent(text, icon), style);
            //GL.sRGBWrite = false;
            EditorGUILayout.EndHorizontal();
            return isDown;
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        public static bool BeginFadeGroup(object key, bool isVisible)
        {
            return BeginFadeGroup(key, isVisible, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup" />.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="t">A value between 0 and 1 indicating how expanded the fade group is.</param>
        public static bool BeginFadeGroup(object key, bool isVisible, out float t)
        {
            return BeginFadeGroup(key, isVisible, out t, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="primaryKey">The primary key for the fade group.</param>
        /// <param name="secondaryKey">The secondly key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        public static bool BeginFadeGroup(object primaryKey, object secondaryKey, bool isVisible)
        {
            return BeginFadeGroup(primaryKey, secondaryKey, isVisible, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="name">The name of the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        public static bool BeginFadeGroup(object key, string name, bool isVisible)
        {
            return BeginFadeGroup(key, name, isVisible, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="duration">The duration of fade in and out.</param>
        public static bool BeginFadeGroup(object key, bool isVisible, float duration)
        {
            EditorTimeHelper.Time.Update();
            var t = GUIHelper.GetTemporaryContext(fadeGroupKey, key, isVisible ? 1f : 0f);
            if (Event.current.type == EventType.Layout)
            {
                t.Value = Mathf.MoveTowards(t.Value, isVisible ? 1 : 0, EditorTimeHelper.Time.DeltaTime * (1f / duration));
            }
            return BeginFadeGroup(t.Value);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup" />.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="t">A value between 0 and 1 indicating how expanded the fade group is.</param>
        /// <param name="duration">The duration of fade in and out.</param>
        public static bool BeginFadeGroup(object key, bool isVisible, out float t, float duration)
        {
            EditorTimeHelper.Time.Update();
            var tt = GUIHelper.GetTemporaryContext(fadeGroupKey, key, isVisible ? 1f : 0f);
            if (Event.current.type == EventType.Layout)
            {
                tt.Value = Mathf.MoveTowards(tt.Value, isVisible ? 1 : 0, EditorTimeHelper.Time.DeltaTime * (1f / duration));
            }
            t = tt.Value;
            return BeginFadeGroup(tt.Value);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="primaryKey">The primary key for the fade group.</param>
        /// <param name="secondaryKey">The secondly key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="duration">The duration of fade in and out.</param>
        public static bool BeginFadeGroup(object primaryKey, object secondaryKey, bool isVisible, float duration)
        {
            EditorTimeHelper.Time.Update();
            var t = GUIHelper.GetTemporaryContext(primaryKey, secondaryKey, isVisible ? 1f : 0f);
            if (Event.current.type == EventType.Layout)
            {
                t.Value = Mathf.MoveTowards(t.Value, isVisible ? 1 : 0, EditorTimeHelper.Time.DeltaTime * (1f / duration));
            }
            return BeginFadeGroup(t.Value);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="name">The name of the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="duration">The duration of fade in and out.</param>
        public static bool BeginFadeGroup(object key, string name, bool isVisible, float duration)
        {
            EditorTimeHelper.Time.Update();
            var t = GUIHelper.GetTemporaryContext(key, name, isVisible ? 1f : 0f);
            if (Event.current.type == EventType.Layout)
            {
                t.Value = Mathf.MoveTowards(t.Value, isVisible ? 1 : 0, EditorTimeHelper.Time.DeltaTime * (1f / duration));
            }
            return BeginFadeGroup(t.Value);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="t">The current fading value between 0 and 1.</param>
        public static bool BeginFadeGroup(float t)
        {
            if (t > 0 && t < 1 && animatingFadeGroupIndex == -1)
            {
                t = Mathf.Clamp01(t * t * (3f - 2f * t));
                EditorGUILayout.BeginFadeGroup(t);
                GUIHelper.PushColor(GUI.color * new Color(1, 1, 1, t));
                animatingFadeGroupIndex = currentFadeGroupIndex;
                GUIHelper.RequestRepaint();
            }
            currentFadeGroupIndex++;
            return t > 0;
        }

        /// <summary>
        /// Ends a fade group started by any BeginFadeGroup.
        /// </summary>
        public static void EndFadeGroup()
        {
            currentFadeGroupIndex--;
            if (animatingFadeGroupIndex == currentFadeGroupIndex)
            {
                EditorGUILayout.EndFadeGroup();
                GUIHelper.PopColor();
                animatingFadeGroupIndex = -1;
            }
        }

        /// <summary>
        /// Draws a foldout field where clicking on the label toggles to the foldout too.
        /// </summary>
        /// <param name="isVisible">The current state of the foldout.</param>
        /// <param name="label">The label of the foldout.</param>
        /// <param name="style">The GUI style.</param>
        /// <returns>
        /// The current state of the foldout.
        /// </returns>
        public static bool Foldout(bool isVisible, string label, GUIStyle style = null)
        {
            return Foldout(isVisible, new GUIContent(label), style);
        }

        /// <summary>
        /// Draws a foldout field where clicking on the label toggles to the foldout too.
        /// </summary>
        /// <param name="isVisible">The current state of the foldout.</param>
        /// <param name="label">The label of the foldout.</param>
        /// <param name="style">The GUI style.</param>
        /// <returns>
        /// The current state of the foldout.
        /// </returns>
        public static bool Foldout(bool isVisible, GUIContent label, GUIStyle style = null)
        {
            Rect rect = GUILayoutUtility.GetRect(label, SirenixGUIStyles.Foldout);
            rect.height = EditorGUIUtility.singleLineHeight;

            return Foldout(rect, isVisible, label, style);
        }

        /// <summary>
        /// Draws a foldout field where clicking on the label toggles to the foldout too.
        /// </summary>
        /// <param name="rect">The rect to draw the foldout field in.</param>
        /// <param name="isVisible">The current state of the foldout.</param>
        /// <param name="label">The label of the foldout.</param>
        /// <param name="style">The style.</param>
        /// <returns>
        /// The current state of the foldout.
        /// </returns>
        public static bool Foldout(Rect rect, bool isVisible, GUIContent label, GUIStyle style = null)
        {
            style = style ?? SirenixGUIStyles.Foldout;

            var e = Event.current.rawType;
            bool isHovering = false;
            if (e != EventType.Layout)
            {
                // Swallow foldout icon as well
                rect.x -= 9;
                rect.width += 9;
                isHovering = rect.Contains(Event.current.mousePosition);
                rect.width -= 9;
                rect.x += 9;
            }

            if (isHovering)
            {
                GUIHelper.PushLabelColor(SirenixGUIStyles.HighlightedTextColor);
            }

            if (isHovering && e == EventType.MouseMove)
            {
                GUIHelper.RequestRepaint();
            }

            if (e == EventType.MouseDown && isHovering && Event.current.button == 0)
            {
                // Foldout works when GUI.enabled = false
                // Enable GUI, en order to Use() the the event properly.
                isVisible = !isVisible;
                GUIHelper.RequestRepaint();
                GUIHelper.PushGUIEnabled(true);
                Event.current.Use();
                GUIHelper.PopGUIEnabled();
                GUIHelper.RemoveFocusControl();
            }

            isVisible = EditorGUI.Foldout(rect, isVisible, label, style);

            if (isHovering)
            {
                GUIHelper.PopLabelColor();
            }

            return isVisible;
        }

        /// <summary>
        /// Begins drawing a box. Remember to end with <see cref="EndBox"/>.
        /// </summary>
        /// <param name="label">The label of the box.</param>
        /// <param name="centerLabel">If set to <c>true</c> then center label.</param>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginBox(string label, bool centerLabel = false, params GUILayoutOption[] options)
        {
            if (string.IsNullOrEmpty(label))
            {
                return BeginBox(options);
            }
            else
            {
                return BeginBox(GUIHelper.TempContent(label), centerLabel, options);
            }
        }

        /// <summary>
        /// Begins drawing a box. Remember to end with <see cref="EndBox"/>.
        /// </summary>
        /// <param name="label">The label of the box.</param>
        /// <param name="centerLabel">If set to <c>true</c> then center label.</param>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginBox(GUIContent label, bool centerLabel = false, params GUILayoutOption[] options)
        {
            var rect = BeginBox(options);

            if (label != null)
            {
                BeginBoxHeader();
                GUILayout.Label(label, centerLabel ? SirenixGUIStyles.LabelCentered : SirenixGUIStyles.Label);
                EndBoxHeader();
            }

            return rect;
        }

        /// <summary>
        /// Begins drawing a box. Remember to end with <see cref="EndBox"/>.
        /// </summary>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginBox(params GUILayoutOption[] options)
        {
            BeginIndentedVertical(SirenixGUIStyles.BoxContainer, options);
            var labelWidth = EditorGUIUtility.labelWidth - 4;
            GUIHelper.PushHierarchyMode(false);
            GUIHelper.PushLabelWidth(labelWidth);
            return GUIHelper.GetCurrentLayoutRect();
        }

        /// <summary>
        /// Ends drawing a box started by any BeginBox.
        /// </summary>
        public static void EndBox()
        {
            GUIHelper.PopLabelWidth();
            GUIHelper.PopHierarchyMode();
            EndIndentedVertical();
        }

        /// <summary>
        /// Begins drawing an inline box. Remember to end with <see cref="EndInlineBox"/>.
        /// </summary>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginInlineBox(params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(SirenixGUIStyles.BoxContainer, options);
            GUIHelper.PushHierarchyMode(false);
            return GUIHelper.GetCurrentLayoutRect();
        }

        /// <summary>
        /// Ends drawing an inline box started by any BeginInlineBox.
        /// </summary>
        public static void EndInlineBox()
        {
            GUIHelper.PopHierarchyMode();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Begins drawing a box header. Remember to end with <seealso cref="EndBoxHeader"/>.
        /// </summary>
        public static void BeginBoxHeader()
        {
            var headerBgRect = EditorGUILayout.BeginHorizontal(SirenixGUIStyles.BoxHeaderStyle, GUILayoutOptions.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                headerBgRect.x -= 3;
                headerBgRect.width += 6;
                headerBgRect.y -= 2;
                headerBgRect.height += 4;
                GUIHelper.PushColor(SirenixGUIStyles.HeaderBoxBackgroundColor);
                GUI.DrawTexture(headerBgRect, Texture2D.whiteTexture);
                GUIHelper.PopColor();
                DrawBorders(headerBgRect, 0, 0, 0, 1, new Color(0, 0, 0, 0.4f));
            }
            GUIHelper.PushLabelWidth(EditorGUIUtility.labelWidth - 4);
        }

        /// <summary>
        /// Ends drawing a box header started by <see cref="BeginBoxHeader"/>,
        /// </summary>
        public static void EndBoxHeader()
        {
            GUIHelper.PopLabelWidth();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Starts the shaking animation of a shaking group.
        /// </summary>
        public static void StartShakingGroup(object key)
        {
            StartShakingGroup(key, ShakingAnimationDuration);
        }

        /// <summary>
        /// Starts the shaking animation of a shaking group.
        /// </summary>
        public static void StartShakingGroup(object key, float duration)
        {
            var c = GUIHelper.GetTemporaryContext<ShakeGroup>(shakeableGroupKey, key, ShakeGroup.Default);
            c.Value.StartTime = (float)EditorApplication.timeSinceStartup;
            c.Value.Duration = Mathf.Max(0.1f, 1 / duration);
            c.Value.IsShaking = true;
        }

        /// <summary>
        /// Begins a shakeable group.
        /// </summary>
        public static void BeginShakeableGroup(object key)
        {
            if (Event.current.OnRepaint())
            {
                const float speed = 50;
                const float dist = 5;
                float time = (float)EditorApplication.timeSinceStartup;
                var c = GUIHelper.GetTemporaryContext<ShakeGroup>(shakeableGroupKey, key, ShakeGroup.Default);
                var t = 1 - (time - c.Value.StartTime) * c.Value.Duration;
                c.Value.IsShaking = t > 0;
                if (c.Value.IsShaking)
                {
                    GUIHelper.PushMatrix(
                        GUI.matrix *
                        Matrix4x4.TRS(                                                           // ease in fast!
                            new Vector3(Mathf.Cos(time * speed) * dist * MathUtilities.EaseInOut(t * t * t), 0f),
                            Quaternion.identity,
                            Vector3.one));
                    GUIHelper.RequestRepaint();
                }
            }
        }

        /// <summary>
        /// Ends the shakeable group.
        /// </summary>
        public static void EndShakeableGroup(object key)
        {
            if (Event.current.OnRepaint())
            {
                var c = GUIHelper.GetTemporaryContext<ShakeGroup>(shakeableGroupKey, key, ShakeGroup.Default);
                if (c.Value.IsShaking)
                {
                    GUIHelper.PopMatrix();
                }
            }
        }

        /// <summary>
        /// Begins drawing a vertical menu list.
        /// </summary>
        /// <param name="key">The key for the menu list.</param>
        /// <returns>The rect created.</returns>
        public static Rect BeginVerticalMenuList(object key)
        {
            var context = GUIHelper.GetTemporaryContext<VerticalMenuListInfo>(menuListKey, key).Value;
            var rect = GUIHelper.BeginLayoutMeasuring();

            if (Event.current.type == EventType.Repaint)
            {
                context.Height = rect.height;
            }

            currentVerticalMenuListInfo = context;
            currentVerticalMenuListInfo.CurrentIndex = 0;
            context.ScrollPositiion = EditorGUILayout.BeginScrollView(context.ScrollPositiion, false, false);

            if (Event.current.type == EventType.KeyDown && context.MenuItemCount > 0)
            {
                if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    context.NextSelectedIndex = context.SelectedItemIndex + 1;
                    context.ScrollToCurrent = true;
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    context.NextSelectedIndex = context.SelectedItemIndex - 1;
                    context.ScrollToCurrent = true;
                    Event.current.Use();
                }
            }

            if (context.NextSelectedIndex.HasValue && Event.current.type == EventType.Layout)
            {
                context.SelectedItemIndex = Mathf.Clamp(context.NextSelectedIndex.Value, 0, context.MenuItemCount - 1);
                context.NextSelectedIndex = null;
            }

            return BeginVerticalList(false);
        }

        /// <summary>
        /// Begins drawing a menu list item. Remember to end with <see cref="EndMenuListItem"/>
        /// </summary>
        /// <param name="isSelected">Value indicating whether the item is selected.</param>
        /// <param name="isMouseDown">Value indicating if the mouse is pressed on the item.</param>
        /// <param name="setAsSelected">If set to <c>true</c> the item is set as selected..</param>
        /// <returns>The rect used for the item.</returns>
        public static Rect BeginMenuListItem(out bool isSelected, out bool isMouseDown, bool setAsSelected = false)
        {
            isMouseDown = false;

            if (setAsSelected)
            {
                currentVerticalMenuListInfo.NextSelectedIndex = currentVerticalMenuListInfo.CurrentIndex;
            }

            isSelected = currentVerticalMenuListInfo.SelectedItemIndex == currentVerticalMenuListInfo.CurrentIndex;

            var rect = EditorGUILayout.BeginVertical(SirenixGUIStyles.ListItem);

            if (isSelected)
            {
                popMenuItemLabelColor = true;
                GUIHelper.PushLabelColor(Color.white);
                isSelected = true;

                if (currentVerticalMenuListInfo.ScrollToCurrent && Event.current.type == EventType.Repaint)
                {
                    float from = currentVerticalMenuListInfo.ScrollPositiion.y;
                    float to = from + currentVerticalMenuListInfo.Height;

                    if (rect.y < from)
                    {
                        currentVerticalMenuListInfo.ScrollPositiion.y = rect.y;
                    }
                    else if (rect.yMax > to)
                    {
                        currentVerticalMenuListInfo.ScrollPositiion.y = rect.yMax - currentVerticalMenuListInfo.Height;
                    }

                    currentVerticalMenuListInfo.ScrollToCurrent = false;
                }
            }

            if (Event.current.type == EventType.MouseMove && rect.Contains(Event.current.mousePosition))
            {
                currentVerticalMenuListInfo.NextSelectedIndex = currentVerticalMenuListInfo.CurrentIndex;
            }
            else if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                isMouseDown = true;
                Event.current.Use();
            }

            currentVerticalMenuListInfo.CurrentIndex++;

            DrawSolidRect(rect, isSelected ? SirenixGUIStyles.MenuButtonActiveBgColor : (currentVerticalMenuListInfo.CurrentIndex % 2 == 0 ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd));

            return rect;
        }

        /// <summary>
        /// Ends drawing a menu list item started by <see cref="BeginMenuListItem(out bool, out bool, bool)"/>
        /// </summary>
        public static void EndMenuListItem()
        {
            if (popMenuItemLabelColor)
            {
                GUIHelper.PopLabelColor();
                popMenuItemLabelColor = false;
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Ends drawing a vertical menu list started by <see cref="BeginVerticalMenuList(object)"/>
        /// </summary>
        public static void EndVerticalMenuList()
        {
            EndVerticalList();
            EditorGUILayout.EndScrollView();

            currentVerticalMenuListInfo.MenuItemCount = currentVerticalMenuListInfo.CurrentIndex;
            currentVerticalMenuListInfo = null;
            GUIHelper.EndLayoutMeasuring();
        }

        /// <summary>
        /// Begins drawing a vertical list.
        /// </summary>
        /// <param name="drawBorder">If set to <c>true</c> borders will be drawn around the vertical list.</param>
        /// <param name="drawDarkBg">If set to <c>true</c> a dark background will be drawn.</param>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect used for the list.</returns>
        public static Rect BeginVerticalList(bool drawBorder = true, bool drawDarkBg = true, params GUILayoutOption[] options)
        {
            currentScope++;
            currentListItemIndecies.SetLength(Mathf.Max(currentListItemIndecies.Count, currentScope + 1));
            currentListItemIndecies[currentScope] = 0;

            if (Event.current.type == EventType.MouseMove)
            {
                GUIHelper.RequestRepaint();
            }

            var rect = EditorGUILayout.BeginVertical(options);

            if (drawDarkBg)
            {
                DrawSolidRect(rect, SirenixGUIStyles.ListItemDragBgColor);
            }

            if (drawBorder)
            {
                verticalListBorderRects.Push(rect);
            }
            else
            {
                verticalListBorderRects.Push(new Rect(-1, rect.y, rect.width, rect.height));
            }

            return rect;
        }

        /// <summary>
        /// Ends drawing a vertical list started by <see cref="BeginVerticalList(bool, bool, GUILayoutOption[])"/>.
        /// </summary>
        public static void EndVerticalList()
        {
            currentScope--;
            var rect = verticalListBorderRects.Pop();
            if (rect.x > 0)
            {
                rect.y -= 1;
                rect.height += 1;
                DrawBorders(rect, 1, 1, 1, 1);
            }
            rect = GUIHelper.GetCurrentLayoutRect();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Begins drawing a list item.
        /// </summary>
        /// <param name="allowHover">If set to <c>true</c> the item can be hovered with the mouse.</param>
        /// <param name="style">The style for the vertical list item.</param>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect used for the item.</returns>
        public static Rect BeginListItem(bool allowHover = true, GUIStyle style = null, params GUILayoutOption[] options)
        {
            bool isMouseOver;
            return BeginListItem(allowHover, style, out isMouseOver, options);
        }

        /// <summary>
        /// Begins drawing a list item.
        /// </summary>
        /// <param name="allowHover">If set to <c>true</c> the item can be hovered with the mouse.</param>
        /// <param name="style">The style for the vertical list item.</param>
        /// <param name="isMouseOver">Value indicating if the mouse is hovering in the item.</param>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect used for the item.</returns>
        private static Rect BeginListItem(bool allowHover, GUIStyle style, out bool isMouseOver, params GUILayoutOption[] options)
        {
            currentListItemIndecies.SetLength(Mathf.Max(currentListItemIndecies.Count, currentScope));
            int i = currentListItemIndecies[currentScope];
            currentListItemIndecies[currentScope] = i + 1;

            GUILayout.BeginVertical(style ?? SirenixGUIStyles.ListItem, options);
            var rect = GUIHelper.GetCurrentLayoutRect();
            isMouseOver = rect.Contains(Event.current.mousePosition);

            if (Event.current.type == EventType.Repaint)
            {
                Color color = i % 2 == 0 ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd;
                Color hover = color;
                if (DragAndDropManager.IsDragInProgress == false && allowHover)
                {
                    hover = i % 2 == 0 ? SirenixGUIStyles.ListItemColorHoverEven : SirenixGUIStyles.ListItemColorHoverOdd;
                }
                DrawSolidRect(rect, isMouseOver ? hover : color);
            }

            return rect;
        }

        /// <summary>
        /// Ends drawing a list item started by <see cref="BeginListItem(bool, GUIStyle, GUILayoutOption[])"/>.
        /// </summary>
        public static void EndListItem()
        {
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Creates a animated tab group.
        /// </summary>
        /// <param name="key">The key for the tab group..</param>
        /// <returns>An animated tab group.</returns>
        public static GUITabGroup CreateAnimatedTabGroup(object key)
        {
            var t = GUIHelper.GetTemporaryContext<GUITabGroup>(animatedTabGroupKey, key).Value;
            t.AnimationSpeed = 1f / SirenixEditorGUI.TabPageSlideAnimationDuration;
            return t;
        }

        /// <summary>
        /// Begins drawing a toggle group. Remember to end with <see cref="EndToggleGroup"/>.
        /// </summary>
        /// <param name="key">The key of the group.</param>
        /// <param name="enabled">Value indicating if the group is enabled.</param>
        /// <param name="visible">Value indicating if the group is visible.</param>
        /// <param name="title">The title of the group.</param>
        /// <returns>Value indicating if the group is toggled.</returns>
        public static bool BeginToggleGroup(object key, ref bool enabled, ref bool visible, string title)
        {
            return BeginToggleGroup(key, ref enabled, ref visible, title, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins drawing a toggle group. Remember to end with <see cref="EndToggleGroup"/>.
        /// </summary>
        /// <param name="key">The key of the group.</param>
        /// <param name="enabled">Value indicating if the group is enabled.</param>
        /// <param name="visible">Value indicating if the group is visible.</param>
        /// <param name="title">The title of the group.</param>
        /// <param name="animationDuration">Duration of the animation.</param>
        /// <returns>Value indicating if the group is toggled.</returns>
        public static bool BeginToggleGroup(object key, ref bool enabled, ref bool visible, string title, float animationDuration)
        {
            var rect = GUILayoutUtility.GetRect(16, SirenixGUIStyles.ToggleGroupTitleBg.fixedHeight, SirenixGUIStyles.ToggleGroupTitleBg);
            rect = EditorGUI.IndentedRect(rect);
            GUIHelper.IndentRect(ref rect);
            rect.xMin += 3;
            rect.xMax -= 3;
            GUI.Box(rect, title, SirenixGUIStyles.ToggleGroupTitleBg);
            if (Event.current.type == EventType.Repaint)
            {
                var toggleIconRect = rect;
                toggleIconRect.xMin = toggleIconRect.xMax - 20;
                toggleIconRect.size *= 0.8f;
                //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                GUI.DrawTexture(toggleIconRect, visible ? EditorIcons.TriangleDown.Active : EditorIcons.TriangleLeft.Active);
                //GL.sRGBWrite = false;
            }

            var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            var e = Event.current;

            if (Event.current.type == EventType.Repaint)
            {
                SirenixGUIStyles.ToggleGroupCheckbox.Draw(toggleRect, false, false, enabled, false);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                toggleRect.x -= 2;
                toggleRect.y -= 2;
                toggleRect.width += 4;
                toggleRect.height += 4;

                if (toggleRect.Contains(e.mousePosition))
                {
                    GUIHelper.RequestRepaint();
                    enabled = !enabled;
                    e.Use();
                }
                else if (rect.Contains(e.mousePosition))
                {
                    GUIHelper.RequestRepaint();
                    visible = !visible;
                }
            }

            var result = BeginFadeGroup(key, visible, animationDuration);
            GUILayout.BeginVertical(SirenixGUIStyles.None);
            GUIHelper.PushGUIEnabled(enabled);
            GUIHelper.PushHierarchyMode(false);
            EditorGUI.indentLevel++;
            return result;
        }

        /// <summary>
        /// Ends drawing a toggle group started by <see cref="BeginToggleGroup(object, ref bool, ref bool, string, float)"/>.
        /// </summary>
        public static void EndToggleGroup()
        {
            EditorGUI.indentLevel--;
            GUIHelper.PopHierarchyMode();
            GUIHelper.PopGUIEnabled();
            GUILayout.EndVertical();
            EndFadeGroup();
        }

        /// <summary>
        /// Begins drawing a horizontal auto scroll box. Remember to end with <see cref="EndHorizontalAutoScrollBox"/>.
        /// </summary>
        /// <param name="key">The for the field.</param>
        /// <param name="options">The GUILayout options.</param>
        /// <returns>The rect used for the field.</returns>
        public static Rect BeginHorizontalAutoScrollBox(object key, params GUILayoutOption[] options)
        {
            if (currentBeginAutoScrollBoxInfo != null && currentBeginAutoScrollBoxInfo.IsActive == true)
            {
                Debug.LogError("EndAutoScrollBox must be called before beginning another.");
            }

            var config = GUIHelper.GetTemporaryNullableContext<BeginAutoScrollBoxInfo>(drawColorPaletteColorPickerKey, key);
            config.Value = currentBeginAutoScrollBoxInfo = config.Value ?? new BeginAutoScrollBoxInfo();
            config.Value.IsActive = true;
            config.Value.TmpOuterRect = EditorGUILayout.BeginVertical(options);

            if (Event.current.type == EventType.Repaint)
            {
                currentBeginAutoScrollBoxInfo.OuterRect = currentBeginAutoScrollBoxInfo.TmpOuterRect;
            }

            bool wasScrollWheel = Event.current.type == EventType.ScrollWheel;
            if (wasScrollWheel) Event.current.type = EventType.Ignore;
            GUILayout.BeginScrollView(config.Value.ScrollPosition, GUIStyle.none, GUIStyle.none, options);
            if (wasScrollWheel) Event.current.type = EventType.ScrollWheel;

            currentBeginAutoScrollBoxInfo.InnerRect = EditorGUILayout.BeginHorizontal(options);
            return currentBeginAutoScrollBoxInfo.OuterRect;
        }

        /// <summary>
        /// Ends drawing a horizontal auto scroll box started by <see cref="BeginHorizontalAutoScrollBox(object, GUILayoutOption[])"/>.
        /// </summary>
        public static void EndHorizontalAutoScrollBox()
        {
            if (currentBeginAutoScrollBoxInfo == null || currentBeginAutoScrollBoxInfo.IsActive == false)
            {
                Debug.LogError("EndAutoScrollBox was called before BeginAutoScrollBox.");
            }
            EditorGUILayout.EndHorizontal();

            bool wasScrollWheel = Event.current.type == EventType.ScrollWheel;
            if (wasScrollWheel) Event.current.type = EventType.Ignore;
            GUILayout.EndScrollView();
            if (wasScrollWheel) Event.current.type = EventType.ScrollWheel;

            EditorGUILayout.EndVertical();

            currentBeginAutoScrollBoxInfo.TmpOuterRect.x += 10;
            currentBeginAutoScrollBoxInfo.TmpOuterRect.width -= 20;
            currentBeginAutoScrollBoxInfo.TmpOuterRect.y -= 10;
            currentBeginAutoScrollBoxInfo.TmpOuterRect.height += 20;
            if (currentBeginAutoScrollBoxInfo.TmpOuterRect.Contains(Event.current.mousePosition))
            {
                float overflow = Mathf.Max(0, currentBeginAutoScrollBoxInfo.InnerRect.width - currentBeginAutoScrollBoxInfo.TmpOuterRect.width);
                float percentage = (Event.current.mousePosition.x - currentBeginAutoScrollBoxInfo.TmpOuterRect.x) / currentBeginAutoScrollBoxInfo.TmpOuterRect.width;
                currentBeginAutoScrollBoxInfo.ScrollPosition.x = percentage * overflow;
                GUIHelper.RequestRepaint();
            }
            currentBeginAutoScrollBoxInfo.IsActive = false;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled to change a value up or down.
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="id">The control ID for the sliding.</param>
        /// <param name="t">The current value.</param>
        /// <returns>
        /// The current value.
        /// </returns>
        public static int SlideRectInt(Rect rect, int id, int t)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);

            if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                // Grab
                GUIUtility.hotControl = id;
                EditorGUIUtility.SetWantsMouseJumping(1);
                Event.current.Use();

                slideRectSensitivity = Mathf.Max(0.5f, Mathf.Pow(Mathf.Abs(t), 0.5f) * 0.03f);
                slideDeltaBuffer = 0f;
            }
            else if (GUIUtility.hotControl == id)
            {
                // Update
                if (Event.current.rawType == EventType.MouseDrag)
                {
                    float delta = (HandleUtility.niceMouseDelta + slideDeltaBuffer) * slideRectSensitivity;
                    t += (int)delta;
                    slideDeltaBuffer = delta - (int)delta; // Store remainder in buffer, for use in next event.

                    GUI.changed = true;
                    Event.current.Use();
                }

                // Release
                else if (Event.current.rawType == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Event.current.Use();
                }
            }

            return t;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled to change a value up or down.
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="id">The control ID for the sliding.</param>
        /// <param name="t">The current value.</param>
        /// <returns>
        /// The current value.
        /// </returns>
        public static long SlideRectLong(Rect rect, int id, long t)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);

            if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                EditorGUIUtility.SetWantsMouseJumping(1);
                Event.current.Use();

                slideRectSensitivity = Mathf.Max(0.5f, Mathf.Pow(Mathf.Abs(t), 0.5f) * 0.03f);
                slideDeltaBuffer = 0f;
            }
            else if (GUIUtility.hotControl == id)
            {
                // Update T
                if (Event.current.rawType == EventType.MouseDrag)
                {
                    float delta = (HandleUtility.niceMouseDelta + slideDeltaBuffer) * slideRectSensitivity;
                    t += (long)delta;
                    slideDeltaBuffer = delta - (int)delta; // Store remainder in buffer, for use in next event.

                    GUI.changed = true;
                    Event.current.Use();
                }

                // Release
                else if (Event.current.rawType == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Event.current.Use();
                }
            }

            return t;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled to change a value up or down.
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="id">The control ID for the sliding.</param>
        /// <param name="t">The current value.</param>
        /// <returns>
        /// The current value.
        /// </returns>
        public static float SlideRect(Rect rect, int id, float t)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);

            if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                GUIUtility.keyboardControl = id;
                EditorGUIUtility.SetWantsMouseJumping(1);
                Event.current.Use();

                if (float.IsInfinity(t) || float.IsNaN(t))
                {
                    slideRectSensitivity = 0f;
                }
                else
                {
                    slideRectSensitivity = Mathf.Max(0.5f, Mathf.Pow(Mathf.Abs(t), 0.5f) * 0.03f);
                }
            }
            else if (GUIUtility.hotControl == id)
            {
                // Update T
                if (Event.current.type == EventType.MouseDrag)
                {
                    //t += Mathf.Sign(Event.current.delta.x) * slideRectSensitivity;
                    t += HandleUtility.niceMouseDelta * slideRectSensitivity;
                    t = (float)MathUtilities.RoundBasedOnMinimumDifference(t, slideRectSensitivity);

                    GUI.changed = true;
                    Event.current.Use();
                }

                // Release
                else if (Event.current.rawType == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Event.current.Use();
                }
            }

            return t;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled to change a value up or down.
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="id">The control ID for the sliding.</param>
        /// <param name="t">The current value.</param>
        /// <returns>
        /// The current value.
        /// </returns>
        public static double SlideRectDouble(Rect rect, int id, double t)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);

            if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                EditorGUIUtility.SetWantsMouseJumping(1);
                Event.current.Use();

                if (double.IsInfinity(t) || double.IsNaN(t))
                {
                    slideRectSensitivity = 0f;
                }
                else
                {
                    slideRectSensitivity = Mathf.Max(0.5f, Mathf.Pow(Mathf.Abs((float)t), 0.5f) * 0.03f);
                }
            }
            else if (GUIUtility.hotControl == id)
            {
                // Update T
                if (Event.current.type == EventType.MouseDrag)
                {
                    //t += Mathf.Sign(Event.current.delta.x) * slideRectSensitivity;
                    t += HandleUtility.niceMouseDelta * slideRectSensitivity;
                    t = (float)MathUtilities.RoundBasedOnMinimumDifference(t, slideRectSensitivity);

                    GUI.changed = true;
                    Event.current.Use();
                }

                // Release
                else if (Event.current.rawType == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Event.current.Use();
                }
            }

            return t;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <returns>
        /// The the mouse delta position.
        /// </returns>
        public static Vector2 SlideRect(Rect rect, MouseCursor cursor = MouseCursor.SlideArrow)
        {
            EditorGUIUtility.AddCursorRect(rect, cursor);

            var id = GUIUtility.GetControlID(FocusType.Passive);

            if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                EditorGUIUtility.SetWantsMouseJumping(1);
                Event.current.Use();
            }
            else if (GUIUtility.hotControl == id)
            {
                // Update T
                if (Event.current.type == EventType.MouseDrag)
                {
                    GUI.changed = true;
                    Event.current.Use();
                    return Event.current.delta;
                }

                // Release
                else if (Event.current.type == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Event.current.Use();
                }
            }
            return Vector2.zero;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="rect">The grabbable rect.</param>
        /// <returns>
        /// The the mouse delta position.
        /// </returns>
        public static Vector2 SlideRect(Vector2 position, Rect rect)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);

            var id = GUIUtility.GetControlID(FocusType.Passive);

            if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                Event.current.Use();
                var p = Event.current.mousePosition - rect.position;
                return p;
            }
            else if (GUIUtility.hotControl == id)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    GUI.changed = true;
                    Event.current.Use();
                    var p = Event.current.mousePosition - rect.position;
                    return p;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
            }
            return position;
        }

        /// <summary>
        /// Draws a field for a value of type T - dynamically choosing an appropriate drawer for the type.
        /// Currently supported are: char, string, sbyte, byte, short, ushort, int, uint, long, ulong, float, double, decimal, Guid and all enums.
        /// </summary>
        /// <typeparam name="T">The type of the value to draw.</typeparam>
        /// <param name="label">The label of the fields.</param>
        /// <param name="value">The value to draw.</param>
        /// <param name="options">The layout options.</param>
        /// <returns>The possibly changed value.</returns>
        public static T DynamicPrimitiveField<T>(GUIContent label, T value, params GUILayoutOption[] options)
        {
            options = options ?? GUILayoutOptions.EmptyGUIOptions;

            Delegate del;

            if (typeof(T).IsEnum)
            {
                if (typeof(T).GetAttribute<FlagsAttribute>() != null)
                {
                    return (T)(object)SirenixEditorFields.EnumDropdown(label, (Enum)(object)value, options);
                }
                else
                {
                    return (T)(object)SirenixEditorFields.EnumDropdown(label, (Enum)(object)value, options);
                }
            }

            if (!DynamicFieldDrawers.TryGetValue(typeof(T), out del))
            {
                EditorGUILayout.LabelField(label, new GUIContent("DynamicPrimitiveField does not support drawing the type '" + typeof(T).GetNiceName() + "'."));
                return value;
            }
            else
            {
                var drawerFunc = (Func<GUIContent, T, GUILayoutOption[], T>)del;
                return drawerFunc(label, value, options);
            }
        }

        /// <summary>
        /// Checks whether a given type can be drawn as a dynamic field by <see cref="DynamicPrimitiveField{T}(GUIContent, T, GUILayoutOption[])"/>
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns>True if the type can be drawn, otherwise false.</returns>
        public static bool DynamicPrimitiveFieldCanDraw<T>()
        {
            return typeof(T).IsEnum || DynamicFieldDrawers.ContainsKey(typeof(T));
        }

        internal static void BeginDrawOpenInspector(Rect rect, UnityEngine.Object obj, Rect btnRect)
        {
            var prevEnabled = GUI.enabled;
            GUI.enabled = obj != null;
            if (GUI.enabled && Event.current.isMouse && rect.Contains(Event.current.mousePosition))
            {
                GUIHelper.RequestRepaint();
            }
            if (SirenixEditorGUI.IconButton(rect, EditorIcons.Transparent, "Inspect object"))
            {
                if (obj is Sprite && AssetDatabase.Contains(obj))
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    obj = AssetDatabase.LoadMainAssetAtPath(path) ?? obj;
                }

                if (Event.current.button == 0 || obj.GetType() == typeof(GameObject))
                {
                    GUIHelper.OpenInspectorWindow(obj);
                }
                else if (Event.current.button == 1)
                {
                    GUIHelper.OpenEditorInOdinDropDown(obj, btnRect);
                }
            }
            GUI.enabled = prevEnabled;
        }

        internal static void EndDrawOpenInspector(Rect rect, UnityEngine.Object obj)
        {
            var prevEnabled = GUI.enabled;
            GUI.enabled = obj != null;
            rect.x -= 2;
            rect = rect.AlignRight(rect.height);
            EditorIcons.Pen.Draw(rect);
            GUI.enabled = prevEnabled;
        }

        [InitializeOnLoadMethod]
        private static void LoadEditorPrefs()
        {
            defaultFadeGroupDuration = EditorPrefs.GetFloat("SirenixEditorGUI.DefaultFadeGroupDuration", defaultFadeGroupDuration);
            tabPageSlideAnimationDuration = EditorPrefs.GetFloat("SirenixEditorGUI.TabPageSlideAnimationDuration", tabPageSlideAnimationDuration);
            shakingAnimationDuration = EditorPrefs.GetFloat("SirenixEditorGUI.ShakingAnimationDuration", shakingAnimationDuration);
            expandFoldoutByDefault = EditorPrefs.GetBool("SirenixEditorGUI.ExpandFoldoutByDefault", expandFoldoutByDefault);
        }

        private class VerticalMenuListInfo
        {
            public int SelectedItemIndex;
            public int MenuItemCount;
            public int CurrentIndex;
            public int? NextSelectedIndex = null;
            public float Height;
            public Vector2 ScrollPositiion;
            public bool ScrollToCurrent;
        }

        private struct ShakeGroup
        {
            public bool IsShaking;
            public float StartTime;
            public float Duration;

            public static ShakeGroup Default = new ShakeGroup() { StartTime = -100, IsShaking = false, Duration = 0.5f };
        }

        private class BeginAutoScrollBoxInfo
        {
            public Rect TmpOuterRect;
            public Rect InnerRect;
            public bool IsActive;
            public Rect OuterRect;
            public Vector2 ScrollPosition;
        }
    }
}
#endif