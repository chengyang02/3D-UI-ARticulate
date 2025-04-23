using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class EditorUtilities
{
    #region BAR

    private static void FoldBarManager(SerializedObject serializedObject, SerializedProperty serializedProperty, string property, string label = "", EditorLayout settings = null)
    {
        settings ??= new EditorLayout();
        string displayLabel = string.IsNullOrEmpty(label) ? property : label;

        DrawBackground(settings.Height, settings.Color, settings.info);
        DrawFoldout(serializedObject, serializedProperty, property, displayLabel, settings.Height);
    }
    private static void FoldBarGroupManager(SerializedObject serializedObject, SerializedProperty serializedProperty, string unfold, string enable, string label, EditorLayout settings = null)
    {
        settings ??= new EditorLayout();

        DrawBackground(settings.Height, settings.Color, settings.info);
        DrawFoldoutGroup(serializedObject, serializedProperty, unfold, enable, label, settings.Height);
    }


    private static void DrawBackground(float backgroundHeight, Color backgroundColor, string tooltip)
    {
        Color Line = new Color32(0, 0, 0, 50);  // Línea predeterminada

        //-------------------- BACKGROUND ---------------------//

        EditorGUILayout.BeginVertical();

        // Top Line
        Rect LineRectTop = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(new Rect(0, LineRectTop.y + 2, EditorGUIUtility.currentViewWidth, 0.5f), Line);

        // Background
        Rect BackgroundRect = EditorGUILayout.GetControlRect(false, backgroundHeight);
        EditorGUI.DrawRect(new Rect(0, BackgroundRect.y, EditorGUIUtility.currentViewWidth, backgroundHeight), backgroundColor);

        // Bottom Line
        Rect LineRectBottom = EditorGUILayout.GetControlRect(false, 1);
        //EditorGUI.DrawRect(new Rect(0, LineRectBottom.y - 2, EditorGUIUtility.currentViewWidth, 0.5f), Line);

        if (BackgroundRect.Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.AddCursorRect(BackgroundRect, MouseCursor.Arrow);
            GUI.Label(BackgroundRect, new GUIContent("", tooltip));
        }

        EditorGUILayout.EndVertical();
    }
    private static void DrawFoldout(SerializedObject serializedObject, SerializedProperty serializedProperty, string unfold, string label, float backgroundHeight)
    {
        float contentHeight = 18;
        Rect lastRect = GUILayoutUtility.GetLastRect();
        float centerY = lastRect.y + (backgroundHeight - contentHeight) / 2;

        Rect foldRect = new Rect(15, centerY + 2, 20, contentHeight);
        Rect backgroundRect = new Rect(0, lastRect.y, EditorGUIUtility.currentViewWidth, backgroundHeight);

        GUILayout.Space(-backgroundHeight);

        // Manejo de clics
        if (Event.current.type == EventType.MouseDown && backgroundRect.Contains(Event.current.mousePosition))
        {
            SerializedProperty property = serializedObject?.FindProperty(unfold) ?? serializedProperty?.FindPropertyRelative(unfold);
            if (property != null)
            {
                property.boolValue = !property.boolValue;
                Event.current.Use();
            }
        }

        // Dibujar Foldout y Label
        EditorGUILayout.BeginHorizontal();
        SerializedProperty foldProperty = serializedObject?.FindProperty(unfold) ?? serializedProperty?.FindPropertyRelative(unfold);

        if (foldProperty != null)
        {
            foldProperty.boolValue = EditorGUI.Foldout(foldRect, foldProperty.boolValue, GUIContent.none, true);
        }

        EditorGUI.LabelField(new Rect(20, centerY + 2, EditorGUIUtility.currentViewWidth - 20, contentHeight), label);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(backgroundHeight - 10);
    }
    private static void DrawFoldoutGroup(SerializedObject serializedObject, SerializedProperty serializedProperty, string unfold, string enable, string label, float backgroundHeight)
    {
        float contentHeight = 18;
        Rect lastRect = GUILayoutUtility.GetLastRect();
        float centerY = lastRect.y + (backgroundHeight - contentHeight) / 2;

        Rect foldRect = new Rect(15, centerY + 2, 10, contentHeight);
        Rect toggleRect = new Rect(20, centerY + 2, 15, contentHeight);
        Rect toggleLabelRect = new Rect(50, centerY + 2, EditorGUIUtility.currentViewWidth - 50, contentHeight);
        Rect backgroundRect = new Rect(0, lastRect.y, EditorGUIUtility.currentViewWidth, backgroundHeight);

        GUILayout.Space(-backgroundHeight);

        // Manejo de clics
        if (Event.current.type == EventType.MouseDown && backgroundRect.Contains(Event.current.mousePosition) && !toggleRect.Contains(Event.current.mousePosition))
        {
            SerializedProperty property = serializedObject?.FindProperty(unfold) ?? serializedProperty?.FindPropertyRelative(unfold);
            if (property != null)
            {
                property.boolValue = !property.boolValue;
                Event.current.Use();
            }
        }

        EditorGUILayout.BeginHorizontal();

        // Dibujar Foldout
        SerializedProperty foldProperty = serializedObject?.FindProperty(unfold) ?? serializedProperty?.FindPropertyRelative(unfold);
        if (foldProperty != null)
        {
            foldProperty.boolValue = EditorGUI.Foldout(foldRect, foldProperty.boolValue, GUIContent.none, true);
        }

        // Dibujar Toggle
        SerializedProperty toggleProperty = serializedObject?.FindProperty(enable) ?? serializedProperty?.FindPropertyRelative(enable);

        bool toggleValue;
        bool newToggleValue;

        if (toggleProperty != null)
        {
            toggleValue = toggleProperty.boolValue;
            newToggleValue = EditorGUI.Toggle(toggleRect, toggleValue);
        }
        else
        {
            toggleValue = true;
            EditorGUI.BeginDisabledGroup(toggleValue == true);
            newToggleValue = EditorGUI.Toggle(toggleRect, toggleValue);
            EditorGUI.EndDisabledGroup();
        }

        if (toggleProperty != null && newToggleValue != toggleValue)
        {
            toggleProperty.boolValue = newToggleValue;
        }

        EditorGUI.LabelField(toggleLabelRect, label);

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(backgroundHeight - 10);
    }
    public static bool FoldBase(SerializedObject serializedObject, SerializedProperty serializedProperty, string unfoldProperty, string enableProperty, string label, EditorLayout settings = null)
    {
        var property = serializedObject?.FindProperty(unfoldProperty) ?? serializedProperty?.FindPropertyRelative(unfoldProperty);
        if (property == null) return false;

        bool isUnfold = property.boolValue;
        settings ??= new EditorLayout();

        if (settings.enabled)
        {
            if (!string.IsNullOrEmpty(enableProperty))
                FoldBarGroupManager(serializedObject, serializedProperty, unfoldProperty, enableProperty, label, settings);
            else
                FoldBarManager(serializedObject, serializedProperty, unfoldProperty, label, settings);
        }

        if (isUnfold) GUILayout.Space(5);
        return isUnfold;
    }



    //Main Voids
    public static void FoldGroup(SerializedObject obj, string unfoldProperty, string label, System.Action content, EditorLayout settings = null)
    {
        settings ??= new EditorLayout();

        if (settings.enabled && FoldBase(obj, null, unfoldProperty, "null", label, settings))
        {
            GUILayout.Space(settings.topOffset);

            content?.Invoke();

            GUILayout.Space(settings.bottomOffset);
        }

    }
    public static void FoldGroup(SerializedObject obj, string unfoldProperty, string enable_property, string label, System.Action content, EditorLayout settings = null)
    {
        settings ??= new EditorLayout();

        if (settings.enabled && FoldBase(obj, null, unfoldProperty, enable_property, label, settings))
        {
            GUILayout.Space(settings.topOffset);

            content?.Invoke();

            GUILayout.Space(settings.bottomOffset);
        }

    }
    public static void FoldGroup(SerializedProperty serializedProperty, string unfoldProperty, string enable_property, string label, System.Action content, EditorLayout settings = null)
    {
        settings ??= new EditorLayout();

        if (settings.enabled && FoldBase(null, serializedProperty, unfoldProperty, enable_property, label, settings))
        {
            GUILayout.Space(settings.topOffset);

            content?.Invoke();

            GUILayout.Space(settings.bottomOffset);
        }
    }

    public static void Fold(SerializedObject serializedObject, string unfold, string label, System.Action content, EditorLayout settings = null)
    {
        settings ??= new EditorLayout();

        if (settings.enabled && FoldBase(serializedObject, null, unfold, "", label, settings))
        {
            GUILayout.Space(settings.topOffset);

            content?.Invoke();

            GUILayout.Space(settings.bottomOffset);
        }
    }
    public static void Fold(SerializedProperty serializedProperty, string unfold, string label, System.Action content, EditorLayout settings = null)
    {
        settings ??= new EditorLayout();

        if (settings.enabled && FoldBase(null, serializedProperty, unfold, "", label, settings))
        {
            GUILayout.Space(settings.topOffset);

            content?.Invoke();

            GUILayout.Space(settings.bottomOffset);
        }
    }


    #endregion

    #region GETPROPERTY
    public static SerializedProperty GetPropertyManager(SerializedObject serializedObject, SerializedProperty serializedProperty, string property, string label, EditorLayout editorLayout)
    {
        SerializedProperty prop = null;

        if (serializedObject != null)
        {
            prop = serializedObject.FindProperty(property);
        }

        if (serializedProperty != null)
        {
            prop = serializedProperty.FindPropertyRelative(property);
        }


        if (prop != null)
        {
            if (label != "")
            {
                if (editorLayout != null)
                {
                    EditorGUILayout.Space(editorLayout.space);
                }

                EditorGUILayout.PropertyField(prop, new GUIContent(label));

                if (editorLayout != null)
                {
                    EditorGUILayout.Space(editorLayout.space);
                }
            }
        }
        else
        {
            Debug.LogError("Couldnt find the property : " + property);
        }

        return prop;
    }

    public static SerializedProperty GetProperty(SerializedObject serializedObject, string property)
    {
        return GetPropertyManager(serializedObject, null, property, "", null);
    }
    public static SerializedProperty GetProperty(SerializedObject serializedObject, string property, string label = "")
    {
        return GetPropertyManager(serializedObject, null, property, label, null);
    }
    public static SerializedProperty GetProperty(SerializedObject serializedObject, string property, string label = "", EditorLayout editorLayout = null)
    {
        return GetPropertyManager(serializedObject, null, property, label, editorLayout);
    }

    public static SerializedProperty GetRelative(SerializedProperty serializedProperty, string property)
    {
        return GetPropertyManager(null, serializedProperty, property, "", null);
    }
    public static SerializedProperty GetRelative(SerializedProperty serializedProperty, string property, string label = "")
    {
        return GetPropertyManager(null, serializedProperty, property, label, null);
    }
    public static SerializedProperty GetRelative(SerializedProperty serializedProperty, string property, string label = "", EditorLayout editorLayout = null)
    {
        return GetPropertyManager(null, serializedProperty, property, label, editorLayout);
    }
    #endregion


    public class EditorLayout
    {
        public Color Color = new Color32(0, 0, 0, 50);
        public string info;

        public float Height = 30f;
        public int topOffset = 10;
        public int bottomOffset = 10;
        public int space = 10;

        public bool useLabel = true;
        public bool enabled = true;

    }

    public static void Section(string Name, System.Action content, EditorLayout sectionStyle = null)
    {
        sectionStyle ??= new EditorLayout();

        if (sectionStyle.enabled)
        {
            GUILayout.Space(sectionStyle.topOffset);

            if (sectionStyle.useLabel) EditorGUILayout.LabelField(Name, EditorStyles.boldLabel); GUILayout.Space(10);

            content?.Invoke();

            GUILayout.Space(sectionStyle.bottomOffset);
        }

    }
    public static void Section(System.Action content, EditorLayout sectionStyle = null)
    {
        sectionStyle ??= new EditorLayout();

        if (sectionStyle.enabled)
        {
            GUILayout.Space(sectionStyle.topOffset);

            content?.Invoke();

            GUILayout.Space(sectionStyle.bottomOffset);
        }
    }
    public static void SliderToggle(SerializedObject serializedObject, string Label, string enableProperty, string floatProperty, float min, float max)
    {
        EditorGUILayout.BeginHorizontal();

        SerializedProperty value = serializedObject.FindProperty(enableProperty);

        if (value != null)
        {
            value.boolValue = EditorGUILayout.Toggle(value.boolValue, GUILayout.Width(20));
        }
        else
        {
            EditorGUILayout.Toggle(true, GUILayout.Width(20));
        }

        float labelWidth = Mathf.Clamp(Label.Length * 7f, 100f, 300f); // Ajusta el factor 7f si necesitas más o menos espacio

        if (value != null) { EditorGUI.BeginDisabledGroup(value.boolValue == false); }
        EditorGUILayout.LabelField(Label, GUILayout.Width(labelWidth));
        serializedObject.FindProperty(floatProperty).floatValue = EditorGUILayout.Slider("", serializedObject.FindProperty(floatProperty).floatValue, min, max);
        if (value != null) { EditorGUI.EndDisabledGroup(); }

        EditorGUILayout.EndHorizontal();
    }
    public static bool GetBool(SerializedObject objectproperty, string property, string label = "")
    {
        SerializedProperty targetBool = GetProperty(objectproperty, property);

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        if (label != string.Empty) { EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth)); }
        // Botones alineados al ancho restante
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        if (GUILayout.Toggle(targetBool.boolValue, "On", "Button", GUILayout.ExpandWidth(true))) targetBool.boolValue = true;

        if (GUILayout.Toggle(!targetBool.boolValue, "Off", "Button", GUILayout.ExpandWidth(true))) targetBool.boolValue = false;

        GUILayout.EndHorizontal(); GUILayout.EndHorizontal();

        return targetBool.boolValue;
    }

    public static bool GetBool(SerializedObject objectproperty, string property, string enable = "", string disable = "")
    {
        SerializedProperty targetBool = GetProperty(objectproperty, property);

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        // Botones alineados al ancho restante
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));



        if (GUILayout.Toggle(!targetBool.boolValue, enable == "" ? "Off" : enable, "Button", GUILayout.ExpandWidth(true))) targetBool.boolValue = false;
        if (GUILayout.Toggle(targetBool.boolValue, disable == "" ? "On" : disable, "Button", GUILayout.ExpandWidth(true))) targetBool.boolValue = true;

        GUILayout.EndHorizontal(); GUILayout.EndHorizontal();

        return targetBool.boolValue;
    }

    public static int SectionUI(List<string> labels, int currentSection, List<int> disableList = null)
    {
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        for (int i = 0; i < labels.Count; i++)
        {
            bool isActive = (currentSection == i);
            bool isDisabled = disableList != null && disableList.Contains(i);

            GUI.enabled = !isDisabled; // Disable if it's in disableList
            if (GUILayout.Toggle(isActive, labels[i], "Button", GUILayout.ExpandWidth(true)))
            {
                currentSection = i;
            }
            GUI.enabled = true; // Reset GUI state
        }

        GUILayout.EndHorizontal();
        return currentSection;
    }



    public static void ImageToggle(Component target, string property)
    {
        if (target == null) return;

        FieldInfo field = target.GetType().GetField(property, BindingFlags.Public | BindingFlags.Instance);
        if (field == null) return;

        Image sd = field.GetValue(target) as Image;
        if (sd == null) return;

        EditorGUILayout.BeginHorizontal();

        sd.enabled = EditorGUILayout.Toggle(sd.enabled, GUILayout.Width(20)); // Ajuste del ancho del toggle

        GUILayout.Space(5); // Espacio entre el Toggle y el LabelField

        EditorGUILayout.LabelField(property, GUILayout.Width(EditorGUIUtility.labelWidth - 25)); // Ajustar para mantener alineación

        sd.sprite = (Sprite)EditorGUILayout.ObjectField(sd.sprite, typeof(Sprite), false, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();
    }


    #region STRUCTURE

    private static Dictionary<string, (bool decreaseButton, bool isEditing, bool increaseButton, bool edit_button, bool isAnyKey, string inputText)> float_buttonStates = new();
    public static float FloatField(float target, string label)
    {
        GUILayout.BeginHorizontal();

        Event e = Event.current;
        string key = label + "_" + GUIUtility.GetControlID(FocusType.Passive);


        // -------------------------- Rects ---------------------------//

        Rect[] rects =
        {
                GUILayoutUtility.GetRect(20, 20, GUILayout.Width(30)), // Botón -
                GUILayoutUtility.GetRect(80, 20),                      // Campo Medio
                GUILayoutUtility.GetRect(20, 20, GUILayout.Width(30)), // Botón +
            };

        // -------------------------- DEFAULT --------------------------- //

        if (!float_buttonStates.TryGetValue(key, out var state)) state = (false, false, false, false, false, "");

        if (e.type == EventType.MouseDown)
        {
            // -------------------------- DEFAULT --------------------------- //

            state.isAnyKey = false;
            state.edit_button = false;
            state.increaseButton = false;
            state.decreaseButton = false;

            // -------------------------- MIDDLE BUTTON CLICK --------------------------- //

            if (rects[1].Contains(e.mousePosition) && e.button == 0)
            {
                state.isEditing = true;
                state.inputText = Mathf.Round(target).ToString();
                state.isAnyKey = true;
            }

            // -------------------------- OTHER BUTTONS CLICK --------------------------- //

            state =
            (
                 decreaseButton: rects[0].Contains(e.mousePosition),
                 isEditing: state.isEditing,
                 increaseButton: rects[2].Contains(e.mousePosition),
                 edit_button: state.edit_button,
                 isAnyKey: state.isAnyKey,
                 inputText: state.inputText
           );

            buttonStates[key] = state;


            if (state.decreaseButton || state.increaseButton)
            {
                state.isAnyKey = true;
                state.edit_button = true;
            }
        }

        if (e.type == EventType.MouseUp)
        {
            state.edit_button = false;
            state.increaseButton = false;
            state.decreaseButton = false;

            if (!state.isEditing) state.isAnyKey = false;
        }

        if (!state.isAnyKey)
        {
            if (state.isEditing)
            {
                state.isEditing = false;

                if (float.TryParse(state.inputText.Trim(), out float newValue))
                {
                    target = newValue;
                }
            }
        }
        else /// make maths
        {
            if (state.decreaseButton)
            {
                float sensitivity = 0.1f;

                target -= sensitivity;
                state.inputText = Mathf.Round(target).ToString();
            }
            else if (state.increaseButton)
            {
                float sensitivity = 0.1f;

                target += sensitivity;
                state.inputText = Mathf.Round(target).ToString();
            }

            // ------------------------ TEXTFIELD ---------------------//

            GUI.SetNextControlName(key);

            if (e.isKey && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Escape))
            {
                if (e.keyCode == KeyCode.Return)
                {
                    state.isAnyKey = false;
                }

                if (e.keyCode == KeyCode.Escape)
                {
                    state.isAnyKey = false;
                    state.isEditing = false;
                }

                GUI.FocusControl(null);
            }
        }

        if (state.isEditing)
        {
            GUIStyle centeredTextField = new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter };
            state.inputText = GUI.TextField(rects[1], state.inputText, centeredTextField); // show textfield
        }
        else
        {
            if (GUI.Button(rects[1], label, EditorStyles.miniButtonMid)) // show middle button
            {
                state.isEditing = true;
                state.inputText = Mathf.Round(target).ToString();
                GUI.FocusControl(key);
            }
        }

        if (GUI.Button(rects[0], "-", EditorStyles.miniButtonLeft)) target -= 0.15f;
        if (GUI.Button(rects[2], "+", EditorStyles.miniButtonLeft)) target += 0.15f;

        float_buttonStates[key] = state;

        GUILayout.EndHorizontal();

        return target;
    }

    private static Dictionary<string, (bool xButton, bool isEditing, bool yButton, bool edit_button, bool isAnyKey, string inputText)> buttonStates = new();
    public static Vector2 Vector2Field(Vector2 target, string label)
    {
        GUILayout.BeginHorizontal();

        Event e = Event.current;
        string key = label + "_" + GUIUtility.GetControlID(FocusType.Passive);

        // -------------------------- rects ---------------------------//

        Rect[] rects =
        {
                GUILayoutUtility.GetRect(20, 20, GUILayout.Width(30)), // Botón Izq X
                GUILayoutUtility.GetRect(80, 20),                      // Campo Medio
                GUILayoutUtility.GetRect(20, 20, GUILayout.Width(30)), // Botón Izq Y
        };

        // -------------------------- DEFAULT --------------------------- //

        if (!buttonStates.TryGetValue(key, out var state)) state = (false, false, false, false, false, "");

        if (e.type == EventType.MouseDown)
        {

           
            // -------------------------- DEFAULT --------------------------- //

            state.isAnyKey = false;
            state.edit_button = false;

            // -------------------------- MIDDLE BUTTON CLICK --------------------------- //

            if (rects[1].Contains(e.mousePosition) && e.button == 0)
            {
                state.isEditing = true;
                state.inputText = $"{Mathf.Round(target.x)},{Mathf.Round(target.y)}";
                state.isAnyKey = true;
            }

            // -------------------------- OTHERS BUTTON CLICK --------------------------- //

            state =
            (
                xButton: rects[0].Contains(e.mousePosition),
                isEditing: state.isEditing,
                yButton: rects[2].Contains(e.mousePosition),
                edit_button: state.edit_button,
                isAnyKey: state.isAnyKey,
                inputText: state.inputText
            );

            buttonStates[key] = state;


            if (state.xButton || state.yButton)
            {
                state.isAnyKey = true;
                state.edit_button = true;
            }
        }

        if (e.type == EventType.MouseUp)
        {
            state.edit_button = false;
            if (!state.isEditing) state.isAnyKey = false;
        }

        if (!state.isAnyKey)
        {
            if(state.isEditing)
            {
                state.isEditing = false;

                string[] values = state.inputText.Split(',');

                if (values.Length == 2 && float.TryParse(values[0].Trim(), out float newX) && float.TryParse(values[1].Trim(), out float newY))
                {
                    target = new Vector2(newX, newY);
                }
                else if (values.Length == 1 && float.TryParse(values[0].Trim(), out float singleValue))
                {
                    target = new Vector2(singleValue, singleValue);
                }
            }
           
        }
        else /// make maths
        {
            //  change vector 2 values 

            if (state.edit_button && e.type == EventType.MouseDrag)
            {
                float sensitivity = 0.5f;
                Vector2 mouseDelta = e.delta;

                if (mouseDelta.sqrMagnitude > 0.01f)
                {
                    bool shiftHeld = (Event.current.modifiers & EventModifiers.Shift) != 0;

                    if (shiftHeld) 
                    {
                        float scaleAmount = mouseDelta.x * sensitivity;
                        target.x += scaleAmount;
                        target.y += scaleAmount;
                    }
                    else 
                    {
                        if (state.xButton)
                            target.x += mouseDelta.x * sensitivity;

                        if (state.yButton)
                            target.y += mouseDelta.x * sensitivity;
                    }

                    state.inputText = $"{Mathf.Round(target.x)},{Mathf.Round(target.y)}";


                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);

                    // Teletransportar el cursor para evitar restricciones en los bordes
                    WarpMouseInEditor();
                }
            }



            GUI.SetNextControlName(key);

            if ((e.isKey && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Escape)))
            {
                if (e.keyCode == KeyCode.Return)
                {
                    state.isAnyKey = false;
                }

                if (e.keyCode == KeyCode.Escape)
                {
                    state.isAnyKey = false;
                    state.isEditing = false;
                }

                GUI.FocusControl(null);
            }
        }


        if (state.isEditing)
        {
            GUIStyle centeredTextField = new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter };
            state.inputText = GUI.TextField(rects[1], state.inputText, centeredTextField);
        }
        else
        {
            if (GUI.Button(rects[1], label, EditorStyles.miniButtonMid))
            {
                state.isEditing = true;
                state.inputText = $"{Mathf.Round(target.x)},{Mathf.Round(target.y)}";
                GUI.FocusControl(key);
            }
        }

        if (GUI.Button(rects[0], "x", EditorStyles.miniButtonLeft)) target.x -= 0.15f;
        if (GUI.Button(rects[2], "y", EditorStyles.miniButtonLeft)) target.y -= 0.15f;

        buttonStates[key] = state;

        GUILayout.EndHorizontal(); 

        return target;
    }

    #endregion
    private static void WarpMouseInEditor()
    {
        Vector3 mousePos = Event.current.mousePosition;
        bool cursorMoved = false;

        // Verificar si el cursor está cerca de los bordes
        if (mousePos.x <= 1)
        {
            mousePos.x = Screen.width - 2;
            cursorMoved = true;
        }
        else if (mousePos.x >= Screen.width - 1)
        {
            mousePos.x = 2;
            cursorMoved = true;
        }

        if (mousePos.y <= 1)
        {
            mousePos.y = Screen.height - 2;
            cursorMoved = true;
        }
        else if (mousePos.y >= Screen.height - 1)
        {
            mousePos.y = 2;
            cursorMoved = true;
        }

        // Si el cursor ha sido movido, lo "teletransportamos" para evitar restricciones
        if (cursorMoved)
        {
            EditorGUIUtility.SetWantsMouseJumping(1);
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
        }
    }


    #region LAYOUT 
    private static Dictionary<List<Sprite>, int[]> pickerIDs = new Dictionary<List<Sprite>, int[]>();
    public static List<Sprite> MostrarSpritesEnHorizontal(List<Sprite> spriteList, List<string> labelList, float PADDING)
    {
        if (spriteList == null || labelList == null || spriteList.Count != labelList.Count)
            return null;

        float viewWidth = EditorGUIUtility.currentViewWidth - 20;
        float spriteSize = Mathf.Min(80, viewWidth / Mathf.Max(1, spriteList.Count) - PADDING);
        float padding = spriteSize * 0.2f;
        float labelHeight = 16f;
        float spacing = 5f;
        Color edgeColor = new Color(0, 0, 0, 0.2f);
        float thickness = 2;

        // Generar un ID único para esta instancia de la función
        int controlIDBase = GUIUtility.GetControlID(FocusType.Passive);

        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < spriteList.Count; i++)
        {
            Rect rect = GUILayoutUtility.GetRect(spriteSize, spriteSize + labelHeight + spacing, GUILayout.ExpandWidth(true), GUILayout.Height(spriteSize));
            Rect spriteRect = new Rect(rect.x, rect.y, rect.width, spriteSize);
            Rect labelRect = new Rect(rect.x, rect.y + 5 + spriteSize + spacing, rect.width, labelHeight);

            SpriteGroup_Rect(spriteRect, edgeColor, thickness);
            GUI.Box(spriteRect, GUIContent.none);

            Texture2D tex = spriteList[i] != null ? AssetPreview.GetAssetPreview(spriteList[i]) : null;
            if (tex != null)
                GUI.DrawTexture(new Rect(spriteRect.x + padding, spriteRect.y + padding, spriteRect.width - 2 * padding, spriteRect.height - 2 * padding), tex, ScaleMode.ScaleToFit);
            else
                SpriteGroup_Draw_None(spriteRect);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            EditorGUI.LabelField(labelRect, labelList[i], labelStyle);

            //  Generar un ID único para este sprite en particular
            int pickerID = controlIDBase + i;

            // Manejo de clic izquierdo (abrir Object Picker)
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && spriteRect.Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.ShowObjectPicker<Sprite>(spriteList[i], false, "", pickerID);
                Event.current.Use();
            }
            // Manejo de clic derecho (ping al objeto)
            else if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && spriteRect.Contains(Event.current.mousePosition))
            {
                if (spriteList[i] != null)
                {
                    EditorGUIUtility.PingObject(spriteList[i]);
                    Event.current.Use();
                }
            }

            // Captura el sprite seleccionado asegurando que el ID sea el correcto
            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == pickerID)
            {
                spriteList[i] = (Sprite)EditorGUIUtility.GetObjectPickerObject();
            }

            SpriteGroup_DragDrop(spriteRect, spriteList, i);
        }

        EditorGUILayout.EndHorizontal();
        return spriteList;
    }

    private static void SpriteGroup_Rect(Rect rect, Color color, float thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color); // Arriba
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color); // Abajo
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color); // Izquierda
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color); // Derecha
    }
    private static void SpriteGroup_Draw_None(Rect rect)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20
        };

        Color setColor = new Color(1, 1, 1, 0.14f);
        style.normal.textColor = style.hover.textColor = style.active.textColor = style.focused.textColor = setColor;

        GUI.Label(rect, "None", style);
    }
    private static void SpriteGroup_DragDrop(Rect rect, List<Sprite> spriteList, int index)
    {
        Event evt = Event.current;
        if (rect.Contains(evt.mousePosition))
        {
            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
            else if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                if (DragAndDrop.objectReferences.Length > 0)
                {
                    UnityEngine.Object draggedObject = DragAndDrop.objectReferences[0];

                    Sprite newSprite = draggedObject as Sprite;
                    if (newSprite == null && draggedObject is Texture2D texture)
                    {
                        // Intenta obtener un sprite de la textura
                        string path = AssetDatabase.GetAssetPath(texture);
                        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

                        foreach (UnityEngine.Object asset in assets)
                        {
                            if (asset is Sprite foundSprite)
                            {
                                newSprite = foundSprite;
                                break; // Solo tomamos el primero encontrado
                            }
                        }
                    }

                    if (newSprite != null)
                    {
                        spriteList[index] = newSprite;
                        GUI.changed = true;
                    }
                }
                Event.current.Use();
            }
        }
    }

    #endregion
}
#endif
