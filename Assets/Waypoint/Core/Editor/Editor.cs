using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using TMPro;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using static EditorUtilities;
using static FormatGames.UIUtilities;
#endif

namespace FormatGames.WayPoint
{

#if UNITY_EDITOR

    [CustomEditor(typeof(Manager))]
    public class Editor : UnityEditor.Editor
    {
        Manager manager;

        const string EditorPrefKey = "ShowHiddenReferences"; // Clave para EditorPrefs
        private string inputText = "";
        private int inputLayerID = -1;

        bool showHidenReferences;
        private SerializedProperty layersProperty;

        private bool isOnSceneView = false;
        private bool isOnInspector = false;

        private bool mouseDown = false;
        private bool mouseUp = false;

        void OnEnable()
        {
            manager = (target) as Manager;
            // manager.SetRef();
            // Cargar el valor de EditorPrefs al iniciar el editor
            showHidenReferences = EditorPrefs.GetBool(EditorPrefKey, false);
            layersProperty = serializedObject.FindProperty("layers");

            Manager.refs = manager;


            isOnSceneView = true;
        }

        void OnDisable()
        {
            // Guardar el valor de showHidenReferences en EditorPrefs al cerrar el editor
            EditorPrefs.SetBool(EditorPrefKey, showHidenReferences);
        }
      
        private void OnSceneGUI()
        {
            manager = (target) as Manager;

            if (!manager.showGizmos) return;

            if (manager.mainCanvas)
            {
                if (manager.screenOffset < 0) return;

                RectTransform rectTransform = manager.mainCanvas.GetComponent<RectTransform>();

                // Obtener las dimensiones del RectTransform
                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);

                // Calcular el margen en función del tamaño del rectángulo
                float width = corners[2].x - corners[0].x;
                float height = corners[2].y - corners[0].y;

                float marginX = width * manager.screenOffset;
                float marginY = width * manager.screenOffset;

                Vector3 min = corners[0] + new Vector3(marginX, marginY, 0);
                Vector3 max = corners[2] - new Vector3(marginX, marginY, 0);

                // Definir los puntos del rectángulo con margen
                Vector3[] rectPoints =
                {
                new Vector3(min.x, min.y, 0), // Inferior izquierdo
                new Vector3(max.x, min.y, 0), // Inferior derecho
                new Vector3(max.x, max.y, 0), // Superior derecho
                new Vector3(min.x, max.y, 0)  // Superior izquierdo
            };

                // Dibujar rectángulo con margen

                Handles.DrawSolidRectangleWithOutline(rectPoints, new Color(0, 1, 0, 0.01f), Color.green);
            }

            int groupCount = manager.layers.Count;

            for (int g = 0; g < groupCount; g++)
            {
                IndicatorLayer group = manager.layers[g];

                int indicatorCount = group.indicators.Count;

                for (int i = 0; i < indicatorCount; i++)
                {
                    Indicator indicator = group.indicators[i];

                    if (indicator.marker == null) continue; // Evita errores si el marker no está asignado

                    if (indicator.unfold)
                    {
                        // Obtiene la posición del marcador en el mundo
                        Vector3[] corners = new Vector3[4];
                        indicator.marker.GetWorldCorners(corners);

                        // MAIN RECTANGLE
                        Handles.DrawSolidRectangleWithOutline(
                            corners,
                            new Color(0, 0, 0, 0.3f), // fondo semitransparente
                            new Color(1, 1, 1, 0.3f) // borde blanco
                        );

                        // Calcular el centro del rectángulo
                        Vector3 center = (corners[0] + corners[2]) / 2f;

                        //cross

                        Color crossColor = new Color(1, 1, 1, 0.3f);


                        Handles.color = crossColor; // Cambia esto al color que quieras
                        Handles.DrawLine(new Vector3(center.x, corners[1].y, center.z), new Vector3(center.x, corners[0].y, center.z));

                        Handles.color = crossColor; // Otro color si deseas líneas de diferentes colores
                        Handles.DrawLine(new Vector3(corners[0].x, center.y, center.z), new Vector3(corners[2].x, center.y, center.z));


                        //  Añadir pequeño cuadrado transparente con bordes negros
                        float smallSize = 3; // Tamaño basado en el marcador
                        Vector3[] smallSquare = new Vector3[4]
                        {
                            center + new Vector3(-smallSize, -smallSize, 0),
                            center + new Vector3(smallSize, -smallSize, 0),
                            center + new Vector3(smallSize, smallSize, 0),
                            center + new Vector3(-smallSize, smallSize, 0)
                        };

                        Handles.DrawSolidRectangleWithOutline(
                            smallSquare,
                            new Color(0, 0, 0, 0), // Fondo transparente
                            new Color(1, 1, 1, 0.3f) // Bordes blancos
                        );

                        //  Mostrar el texto si está en edición
                        if (indicator.markerScript && indicator.markerScript.isEditingOffset)
                        {
                            Vector2 markerSize = indicator.marker.sizeDelta;

                            // Texto vertical (altura del rectángulo) en el lado izquierdo
                           
                            GUIStyle verticalTextStyle = new GUIStyle
                            {
                                normal = new GUIStyleState { textColor = Color.white },
                                fontSize = 14,
                                alignment = TextAnchor.MiddleCenter
                            };

                            Handles.Label((corners[0] + corners[1]) / 2f, markerSize.y.ToString("0"), verticalTextStyle);

                            // Texto horizontal (ancho del rectángulo) en la parte inferior
                            GUIStyle horizontalTextStyle = new GUIStyle(verticalTextStyle);
                            Handles.Label((corners[0] + corners[3]) / 2f, markerSize.x.ToString("0"), horizontalTextStyle);
                        }
                    }


                    // -------------------------------------------------------- MOUSE ON SCENE VIEW -------------------------------------------------------------//

                    CheckMouseWindow("sceneView");
                }
            }
        }

        public override void OnInspectorGUI()
        {
            manager = (target) as Manager;


            Settings();


            if (!manager.isAddingLayer && !manager.isEditingLayer && !manager.isAddingIndicator)
            {
                MainWindow();
            }
            else
            {
                EditionWindow();

            }


            serializedObject.ApplyModifiedProperties();

        }
        public void MainWindow()
        {
            // ------------------------------------- ADD BUTTON --------------------------------------//

            AddButton("Add Layer", () => { manager.isAddingLayer = true; });

            // -------------------------------------- EDITOR --------------------------------------//

            FoldGroup(serializedObject, "unfoldEditor", "Editor", () =>
            {
                GetProperty(serializedObject, "showGizmos", "Gizmos");
                GetProperty(serializedObject, "showDefaultInspector", "Debug Inspector");
                GetProperty(serializedObject, "collapseIndicators", "Collapse Indicators");
                GetProperty(serializedObject, "editAtPlayMode", "Edit At PlayMode");
                GetProperty(serializedObject, "hideMarkerOnFold", "Hide Marker On Fold");
                //GetProperty(serializedObject, "color", "color");
               // GetProperty(serializedObject, "floattest", "color");
            });

            // -------------------------------------- SETTINGS --------------------------------------//

            FoldGroup(serializedObject, "unfoldSettings", "Settings", () =>
            {
                GetProperty(serializedObject, "player", "Player");
                GetProperty(serializedObject, "mainCamera", "Camera");
                GetProperty(serializedObject, "mainCanvas", "Canvas"); GUILayout.Space(10);

                manager.screenOffset = EditorGUILayout.Slider("Screen Offset", manager.screenOffset, 0, 0.1f);

            });

            // -------------------------------------- LAYERS --------------------------------------//


            if (!manager.editAtPlayMode && Application.isPlaying) { GUILayout.Space(20); EditorGUILayout.HelpBox("    Can not edit at play mode", MessageType.Error); GUILayout.Space(20); return; }


            for (int i = 0; i < manager.layers.Count; i++)
            {
                IndicatorLayer group = manager.layers[i];

                SerializedProperty groupPropertys = layersProperty.GetArrayElementAtIndex(i);

                Event e = Event.current;

                if(!Application.isPlaying)
                {
                    for (int f = 0; f < manager.layers[i].indicators.Count; f++)
                    {
                        IndicatorLayer IndicatorLayer = manager.layers[i];
                        Indicator indicator = IndicatorLayer.indicators[f];

                        if (indicator.markerScript != null)
                        {
                            if(indicator.markerScript.marker.anchoredPosition != indicator.markerScript.lastPosition && !indicator.markerScript.isEditingOffset)
                            {
                                indicator.markerScript.lastPosition = indicator.markerScript.marker.anchoredPosition;
                                indicator.markerScript.AdjustOffset("syncSize");

                                continue;
                            }

                            if (isOnInspector)
                            {
                                indicator.markerScript.AdjustOffset("applySize");
                            }
                        }
                    }
                }
              

                FoldGroup(groupPropertys, "unfold", "enabled", manager.layers[i].name, () =>
                {
                    IndicatorLayer IndicatorLayer = manager.layers[i];

                    GetRelative(groupPropertys, "clampStyle", "Clamp Style", new EditorLayout { });

                    Slider(groupPropertys, "radiusFromCenter", "Radious from Center", 0, 300);
                    Slider(groupPropertys, "minDistance", "Min Distance", 0, 20);
                    Slider(groupPropertys, "maxDistance", "Max Distance", 0, 999);
                    Slider(groupPropertys, "fadeSpeed", "Fade Speed", 1, 10);
                    IndicatorLayer.focusAngle = EditorGUILayout.Slider("Focus Angle", IndicatorLayer.focusAngle, 0, 360);
                    EditorGUILayout.Space(10);

                    for (int f = 0; f < manager.layers[i].indicators.Count; f++)
                    {
                      
                        Indicator indicator = IndicatorLayer.indicators[f];
                        IndicatorLayer.index = i;
                        indicator.SetData(i, f);
                        IndicatorFold(IndicatorLayer, indicator);
                        IndicatorInterface(groupPropertys, IndicatorLayer, indicator);
                    }

                    EditorGUILayout.Space(20);

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Edit"))
                    {
                        manager.isEditingLayer = true;
                        manager.editingIndicatorLayer = manager.layers[i];
                        manager.editingIndicatorLayer.index = i;
                    }

                    if (GUILayout.Button("Add"))
                    {
                        manager.isAddingIndicator = true;
                        manager.editingIndicatorLayer = manager.layers[i];
                    }

                    if (manager.isCopiedStyle)
                    {
                        if (GUILayout.Button("Paste"))
                        {
                            for (int f = 0; f < manager.layers[i].indicators.Count; f++)
                            {
                                Indicator indicator = IndicatorLayer.indicators[f];

                                if (indicator.markerScript)
                                {
                                    indicator.sectionIndex = 0;
                                    indicator.markerScript.PasteStyle();
                                    indicator.markerScript.UpdateConfigurations();
                                }
                            }

                            manager.isCopiedStyle = false;
                        }
                    }

                    GUILayout.EndHorizontal();

                });
            }

        }
        public void EditionWindow()
        {
            void AdLayer()
            {
                EditorGUILayout.Space(20);

                inputText = EditorGUILayout.TextField("Name:", inputText);

                EditorGUILayout.Space(10); bool isValid = !string.IsNullOrWhiteSpace(inputText);

                if (GUILayout.Button(isValid ? "Submit" : "Cancel"))
                {
                    manager.isAddingLayer = false;

                    if (isValid)
                    {
                        manager.AddLayer(inputText);
                    }

                    inputText = "";
                }

                EditorGUILayout.Space(20);
            }

            void EditLayer()
            {
                EditorGUILayout.Space(20);

                inputText = EditorGUILayout.TextField("Name:", inputText);

                manager.editingIndicatorLayer.index = EditorGUILayout.IntField("index:", manager.editingIndicatorLayer.index);
                EditorGUILayout.IntField("id:", manager.editingIndicatorLayer.id);
                inputLayerID = EditorGUILayout.IntField("Edit Index:", inputLayerID);
                bool isValid = inputText != "" && inputText != manager.editingIndicatorLayer.name || inputLayerID != -1;

                EditorGUILayout.Space(10);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(isValid ? "Save" :"cancel"))
                {
                    manager.isEditingLayer = false;

                    manager.editingIndicatorLayer.name = !string.IsNullOrWhiteSpace(inputText) ? inputText : manager.editingIndicatorLayer.name;

                    manager.MoveLayer(manager.editingIndicatorLayer.index, inputLayerID);

                    manager.editingIndicatorLayer = null;
                    inputText = "";
                    inputLayerID = -1;
                }

                if (GUILayout.Button("Delete"))
                {
                    manager.isEditingLayer = false;

                    inputText = "";

                    manager.layers.Remove(manager.editingIndicatorLayer);
                    manager.editingIndicatorLayer = null;
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(20);
            }

            void AddIndicator()
            {
                EditorGUILayout.Space(20);

                bool isValid = false;

                SerializedProperty isAddMultiple = GetProperty(serializedObject, "addMultiple", "");

                if (isAddMultiple.boolValue)
                {
                    GetProperty(serializedObject, "markers", "Markers");
                    GetProperty(serializedObject, "targets", "Targets");

                    isValid = manager.markers.Count != 0 && manager.targets.Count != 0 && manager.markers.Count == manager.targets.Count;
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();

                    manager.markerAdd = (RectTransform)EditorGUILayout.ObjectField(GUIContent.none, manager.markerAdd,
                        typeof(RectTransform), true);

                    manager.targetAdd = (Transform)EditorGUILayout.ObjectField(GUIContent.none, manager.targetAdd,
                        typeof(Transform), true);

                    EditorGUILayout.EndHorizontal();

                    isValid = manager.markerAdd != null && manager.targetAdd != null;
                }

                EditorGUILayout.Space(10);

                isAddMultiple.boolValue = GUILayout.Toggle(isAddMultiple.boolValue, "Add Multiple", "Button");

                EditorGUILayout.Space(10);

                if (GUILayout.Button(isValid ? "Submit" : "Cancel"))
                {
                    manager.isAddingIndicator = false;

                    if (isValid)
                    {
                        if (isAddMultiple.boolValue)
                        {
                            manager.AddIndicatorList();
                        }
                        else
                        {
                            manager.AddIndicator(manager.editingIndicatorLayer, manager.markerAdd, manager.targetAdd);
                        }
                    }


                    manager.editingIndicatorLayer = null;
                    manager.markerAdd = null;
                    manager.targetAdd = null;
                }

                EditorGUILayout.Space(20);
            }

            if (manager.isAddingLayer)
            {
                AdLayer();
            }

            if (manager.isEditingLayer)
            {
                EditLayer();
            }

            if (manager.isAddingIndicator)
            {
                AddIndicator();
            }
        }
        public void IndicatorInterface(SerializedProperty groupProperty, IndicatorLayer IndicatorLayer, Indicator indicator)
        {
            void IndicatorReferences()
            {
                indicator.marker = (RectTransform)EditorGUILayout.ObjectField(GUIContent.none, indicator.marker, typeof(RectTransform), true);
                indicator.target = (Transform)EditorGUILayout.ObjectField(GUIContent.none, indicator.target, typeof(Transform), true);
                indicator.markerScript = (Marker)EditorGUILayout.ObjectField(GUIContent.none, indicator.markerScript, typeof(Marker), true);
            }

            if (indicator.markerScript)
            {
                SerializedProperty indicators = layersProperty.GetArrayElementAtIndex(IndicatorLayer.index).FindPropertyRelative("indicators").GetArrayElementAtIndex(indicator.index);
                SerializedObject markerScript = GetMarker(indicator);
                SerializedProperty StageReference = markerScript.FindProperty("stage").FindPropertyRelative(indicator.markerScript.isOnEdgeStage ? "onEdgeReference" : "onFreeReference");
                indicator.markerScript.indicatorReference = new IndicatorReference(IndicatorLayer, indicator);

                void MarkerReferences()
                {
                    ComponentField(markerScript, "conteiner", typeof(Image));
                    ComponentField(markerScript, "conteiner_label", typeof(TextMeshProUGUI));
                    ComponentField(markerScript, "conteiner_icon", typeof(Image));
                    ComponentField(markerScript, "label", typeof(TextMeshProUGUI));
                    ComponentField(markerScript, "directional_arrow", typeof(Image));
                    ComponentField(null, "canvasGroup", typeof(CanvasGroup), indicators);

                    GUILayout.Space(20);

                    GUI.enabled = false; EditorGUILayout.TextField("ID : " + indicator.id); GUI.enabled = true;
                }

                if (indicator.markerScript.referencesMissingCheck())
                {
                    GUILayout.Space(20);

                    EditorGUILayout.HelpBox("    Marker Script added but some componets are missing", MessageType.Error);

                    GUILayout.Space(20);

                    IndicatorReferences();

                    GUILayout.Space(20);

                    MarkerReferences();

                    GUILayout.Space(20);

                    if (GUILayout.Button("Delete"))
                    {
                        indicator.marker?.gameObject.SetActive(false);
                        IndicatorLayer.indicators.Remove(indicator);
                    }
                }
                else
                {
                    if (indicator.unfold)
                    {

                        GUILayout.Space(20);

                        StageReference currentStage = indicator.markerScript.stage.currenReference;

                        indicator.sectionIndex = SectionUI(new List<string> { "Free", "On Edge", "Advanced" }, indicator.sectionIndex , new List<int> { indicator.markerScript.disableOnEdge ? 1 : -1 });

                        if (indicator.sectionIndex == 0 || indicator.sectionIndex == 1)
                        {

                            indicator.markerScript.isOnEdgeStage = indicator.sectionIndex == 1;

                            GUILayout.Space(20);

                            Section(() =>
                            {

                                List<Sprite> lista = new List<Sprite>() { currentStage.center_sprite, currentStage.icon_conteiner_sprite, currentStage.arrow_sprite };

                                lista = MostrarSpritesEnHorizontal(lista, new List<string> { "Conteiner", "Icon", "Arrow" }, 80);

                                currentStage.center_sprite = lista[0];
                                currentStage.icon_conteiner_sprite = lista[1];
                                currentStage.arrow_sprite = lista[2];


                                GUILayout.Space(20);
                            });
                            Section("Color", () =>
                            {
                                GetRelative(StageReference, "center_color", "Conteiner");
                                GetRelative(StageReference, "conteiner_label_color", "Conteiner Label");
                                GetRelative(StageReference, "conteiner_icon_color", "Conteiner Icon");
                                GetRelative(StageReference, "label_color", "label");

                            });

                            Section("String", () =>
                            {
                                GetRelative(StageReference, "label", "Label");
                                GetRelative(StageReference, "center_label", "Conteiner Label");

                            });

                            Section("Font Size", () =>
                            {
                                currentStage.icon_conteiner_label_size = FloatField(currentStage.icon_conteiner_label_size, "Conteiner Label");
                                currentStage.label_font_size = FloatField(currentStage.label_font_size, "LabeL");

                            });

                            Section("Marker Offset", () =>
                            {
                                Vector2 horizontal = new Vector2(currentStage.offset.x, currentStage.offset.y);
                                Vector2 vertical = new Vector2(currentStage.offset.z, currentStage.offset.w);

                                horizontal = Vector2Field(horizontal, "Horizontal");
                                vertical = Vector2Field(vertical, "Vertical");


                                currentStage.offset = new(horizontal.x, horizontal.y, vertical.x, vertical.y);


                            });

                            Section("Size", () =>
                            {
                                currentStage.center_size = Vector2Field(currentStage.center_size, "Conteiner");
                                currentStage.icon_conteiner_icon_size = Vector2Field(currentStage.icon_conteiner_icon_size, "Conteiner icon");
                                currentStage.arrow_size = Vector2Field(currentStage.arrow_size, "Arrow");
                                currentStage.label_size = Vector2Field(currentStage.label_size, "Label");
                            });

                            Section("Offset", () =>
                            {
                                indicator.offset = Vector2Field(indicator.offset, "Target");
                                currentStage.center_offset = Vector2Field(currentStage.center_offset, "Conteiner");
                                currentStage.ic_conteiner_label_offset = Vector2Field(currentStage.ic_conteiner_label_offset, "Conteiner label");
                                currentStage.ic_conteiner_icon_offset = Vector2Field(currentStage.ic_conteiner_icon_offset, "conteiner icon");
                                currentStage.arrow_offset = Vector2Field(currentStage.arrow_offset, "Arrow");
                                currentStage.label_offset = Vector2Field(currentStage.label_offset, "Label");

                            });

                            Section("Layout", () =>
                            {
                                GetRelative(StageReference, "use_label", "Label");
                                GetRelative(StageReference, "use_distance", "Distance");
                                GetRelative(StageReference, "use_center_label", "Conteiner Label");
                                GetRelative(StageReference, "use_center_icon", "Conteiner Icon");
                                GetRelative(StageReference, "use_arrow", "Arrow");
                            });

                            Section("Behaviour", () =>
                            {
                                GetRelative(StageReference, "rotateCenter", "Directional Conteiner");
                                GetRelative(StageReference, "rotateLabel", "Directional Label");
                            });

                            GUILayout.Space(10);
                        }

                        else if (indicator.sectionIndex == 2)
                        {
                            indicator.markerScript.isOnEdgeStage = false;

                            int space = 20;

                            GUILayout.Space(space);


                            indicator.marker = (RectTransform)EditorGUILayout.ObjectField(GUIContent.none, indicator.marker, typeof(RectTransform), true);
                            indicator.target = (Transform)EditorGUILayout.ObjectField(GUIContent.none, indicator.target, typeof(Transform), true);
                            indicator.markerScript = (Marker)EditorGUILayout.ObjectField(GUIContent.none, indicator.markerScript, typeof(Marker), true);


                            GUILayout.Space(space);

                            MarkerReferences();


                            GUILayout.Space(space);


                            indicator.markerScript.disableOnEdge = ButtonToggle(indicator.markerScript.disableOnEdge, "Disable OnEdge");

                            if (GUILayout.Button(manager.isCopiedStyle ? "Paste" : "Copy"))
                            {
                                if (manager.isCopiedStyle)
                                {
                                    indicator.markerScript.PasteStyle();
                                    manager.isCopiedStyle = false;
                                    indicator.sectionIndex = 0;
                                }
                                else
                                {
                                    indicator.markerScript.CopyStyle();
                                    manager.isCopiedStyle = true;
                                }
                            }

                            if (GUILayout.Button("Delete"))
                            {
                                indicator.marker?.gameObject.SetActive(false);
                                IndicatorLayer.indicators.Remove(indicator);
                            }
                            GUILayout.Space(space);
                        }
                    }
                    else
                    {
                        if (indicator.markerScript.isOnEdgeStage)
                        {
                            indicator.markerScript.isOnEdgeStage = false;
                            //indicator.markerScript.UpdateConfigurations();
                        }
                    }


                    indicator.markerScript.UpdateConfigurations();

                    UndoRecord("markerScript", indicator.markerScript);

                }

                markerScript.ApplyModifiedProperties();
            }

            else if (indicator.unfold)
            {
                GUILayout.Space(20);

                IndicatorReferences();

                GUILayout.Space(20);

                if (GUILayout.Button("Delete"))
                {
                    indicator.marker?.gameObject.SetActive(false);
                    IndicatorLayer.indicators.Remove(indicator);
                }
            }
        }
       


        public void Serialize()
        {
            serializedObject.Update();

            UnityEngine.Object[] objectsToRecord =
            {
                manager // script and all the properties and subproperties
            };

            if (objectsToRecord.Length > 0)
            {
                Undo.RecordObjects(objectsToRecord, "manager");
            }

        }
        public void CheckMouseWindow(string type)
        {
            if(type == "inspector")
            {
                bool isMouseInInspector = EditorWindow.mouseOverWindow != null &&
                            EditorWindow.mouseOverWindow.titleContent != null &&
                            EditorWindow.mouseOverWindow.titleContent.text == "Inspector";

                Event e = Event.current;

                bool isMouseDown = e.type == EventType.MouseDown;


                // Registrar si el clic comenzó en el inspector
                if (isMouseDown)
                {
                    isOnSceneView = false;
                }


                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.I)
                {
                    for (int i = 0; i < manager.layers.Count; i++)
                    {
                        IndicatorLayer group = manager.layers[i];

                        SerializedProperty groupPropertys = layersProperty.GetArrayElementAtIndex(i);

                        for (int f = 0; f < manager.layers[i].indicators.Count; f++)
                        {
                            IndicatorLayer IndicatorLayer = manager.layers[i];
                            Indicator indicator = IndicatorLayer.indicators[f];

                            System.DateTime now = System.DateTime.Now;
                            IndicatorLayer.id = (now.Minute * 10000 + now.Second * 100 + now.Millisecond / 10) % 100000;

                        }
                    }
                }

                isOnInspector = isMouseInInspector || !isOnSceneView;

            }
            else
            {
                Event e = Event.current;

                bool isMouseDown = e.type == EventType.MouseDown;
                bool isMouseUp = e.type == EventType.MouseUp;

                if (isMouseDown)
                {
                    isOnSceneView = true;
                }

                if (isMouseUp)
                {
                    isOnSceneView = false;
                }
            }
        }
        public void Settings()
        {
            // Copy Manager

            Event e = Event.current;

            mouseDown = e.type == EventType.MouseDown;
            mouseUp = e.type == EventType.MouseUp;

            if (manager.isCopiedStyle)
            {
                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
                {
                    manager.isCopiedStyle = false;
                    GUI.FocusControl(null);
                    Repaint(); 
                }
            }

            // Mouse On Inspector Check

            CheckMouseWindow("inspector");


            Serialize();

            if (manager.showDefaultInspector) { DrawDefaultInspector(); }

            if (!Application.isPlaying)
            {
                manager.UpdateConfigurations();
                EditorUtility.SetDirty(manager);
                Repaint();
            }

        }
        public void UndoRecord(string type, MonoBehaviour script)
        {
            if (type == "markerScript")
            {
                Marker indicatorScript = script as Marker;
                Undo.RecordObject(indicatorScript, "markerScript");
                Undo.RecordObject(indicatorScript.conteiner.rectTransform, "marker_conteiner");
                Undo.RecordObject(indicatorScript.label.rectTransform, "marker_label");
                Undo.RecordObject(indicatorScript.directional_arrow.rectTransform, "marker_directional_arrow");
                Undo.RecordObject(indicatorScript.conteiner_label.rectTransform, "marker_conteiner_label");
                Undo.RecordObject(indicatorScript.conteiner_icon.rectTransform, "marker_conteiner_icon");

                EditorUtility.SetDirty(indicatorScript);
            }


        }
        public void print(string text)
        {
            Debug.Log(text);
        }
        public SerializedProperty GetIndicator(Indicator indicator)
        {
            return serializedObject.FindProperty("layers").GetArrayElementAtIndex(indicator.layer_index).FindPropertyRelative("indicators").GetArrayElementAtIndex(indicator.index);
        }
        public SerializedObject GetMarker(Indicator indicator)
        {
            SerializedObject markerScript = new SerializedObject(indicator.markerScript); markerScript.Update();
            return markerScript;
        }
        public void ComponentField(SerializedObject serializedObject, string property, Type type, SerializedProperty serializedProperty_2 = null)
        {
            if(serializedObject != null)
            {
                SerializedProperty serializedProperty = serializedObject.FindProperty(property);

                GUILayout.BeginHorizontal();

                serializedProperty.objectReferenceValue = EditorGUILayout.ObjectField(
                        GUIContent.none,
                        serializedProperty.objectReferenceValue,
                        type,
                        true
                    ) as UnityEngine.Object;

                GUILayout.EndHorizontal();
            }
            else
            {
                SerializedProperty serializedProperty = serializedProperty_2.FindPropertyRelative(property);

                GUILayout.BeginHorizontal();

                serializedProperty.objectReferenceValue = EditorGUILayout.ObjectField(
                        GUIContent.none,
                        serializedProperty.objectReferenceValue,
                        type,
                        true
                    ) as UnityEngine.Object;

                GUILayout.EndHorizontal();
            }
          
        }
        public void AddButton(string label, Action content)
        {
            GUILayout.Space(10); EditorGUILayout.BeginHorizontal();

            Rect rect = GUILayoutUtility.GetRect(100, 17);
            rect.y += 3;

            // Dibujar un fondo sólido con un color similar al botón de Unity
            EditorGUI.DrawRect(rect, new Color32(88, 88, 88, 255));

            Handles.color = new Color32(0, 0, 0, 255);
            Handles.DrawAAPolyLine(1, new Vector3(rect.x, rect.y), new Vector3(rect.xMax, rect.y)); // Superior
            Handles.DrawAAPolyLine(1, new Vector3(rect.x, rect.yMax), new Vector3(rect.xMax, rect.yMax)); // Inferior
            Handles.DrawAAPolyLine(1, new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.yMax)); // Izquierda
            Handles.DrawAAPolyLine(1, new Vector3(rect.xMax, rect.y), new Vector3(rect.xMax, rect.yMax)); // Derecha

            // Dibujar el texto dentro del rectángulo
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = new Color32(200, 200, 200, 255); // Color de texto similar al de los botones en Unity

            GUI.Label(rect, label, labelStyle);

            if(GUILayout.Button("+", GUILayout.Width(50)))
            {
                content?.Invoke();
            }

            EditorGUILayout.EndHorizontal(); GUILayout.Space(10);
        }
        public float Slider(SerializedProperty serializedProperty, string property, string label, float min, float max)
        {
            SerializedProperty value = GetRelative(serializedProperty, property);
            value.floatValue = EditorGUILayout.Slider(label, value.floatValue, min, max);

            return value.floatValue;
        }
        public void IndicatorFold(IndicatorLayer IndicatorLayer, Indicator indicator, float backgroundHeight = 30f)
        {
            #region BACKGROUND

            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            Rect backgroundRect = GUILayoutUtility.GetRect(inspectorWidth, backgroundHeight);

            backgroundRect.x = 0;
            backgroundRect.width = inspectorWidth;

            Color backgroundColor = new Color32(44, 20, 55, 91);
            Color backgroundColor2 = manager.color;
            EditorGUI.DrawRect(backgroundRect, backgroundColor);
            Rect LineRectTop = EditorGUILayout.GetControlRect(false, 1);
            Color Line = new Color32(0,0,0, 100);  // Línea predeterminada
            EditorGUI.DrawRect(new Rect(0, backgroundRect.y + 30, EditorGUIUtility.currentViewWidth, 0.5f), Line);
            GUILayout.Space(-backgroundHeight + 2);

            

            //-------------------- BACKGROUND ---------------------//
            // Top Line
            

            #endregion

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);

            // Foldout
            Rect foldRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
            indicator.unfold = EditorGUI.Foldout(new Rect(foldRect.x, foldRect.y - 1, foldRect.width, foldRect.height), indicator.unfold, GUIContent.none, true);

            // Toggle
            Rect toggleRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
            indicator.enabled = EditorGUI.Toggle(new Rect(toggleRect.x, toggleRect.y - 1, toggleRect.width, toggleRect.height), indicator.enabled);

            // LabelField con tamaño dinámico
            GUIContent labelContent = new GUIContent(indicator.marker.name);
            Vector2 labelSize = EditorStyles.label.CalcSize(labelContent);
            EditorGUI.LabelField(new Rect(foldRect.xMax + 30, foldRect.y + 1, labelSize.x, labelSize.y), labelContent);

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);

            // Captura de eventos
            if (Event.current.type == EventType.MouseDown && backgroundRect.Contains(Event.current.mousePosition) &&
                !foldRect.Contains(Event.current.mousePosition) && !toggleRect.Contains(Event.current.mousePosition))
            {
                if(manager.collapseIndicators)
                {
                    for (int i = 0; i < manager.layers.Count; i++)
                    {
                        for (int f = 0; f < manager.layers[i].indicators.Count; f++)
                        {
                            Indicator indicator_ = manager.layers[i].indicators[f];

                            if (indicator_ == indicator)
                            {
                                indicator.unfold = !indicator_.unfold; // Alterna el estado
                                indicator_.sectionIndex = !indicator_.unfold ? 0 : indicator_.sectionIndex;
                            }
                            else
                            {
                                indicator_.unfold = false;
                                indicator_.sectionIndex = 0;
                            }
                        }
                    }
                }
                else
                {
                    indicator.unfold = !indicator.unfold; // Alterna el estado
                   
                }

                GUI.changed = true; // Forzar redibujo


            }
        }

        bool ButtonToggle(bool target, string label)
        {
            bool value = GUILayout.Toggle(target, label, "Button", GUILayout.ExpandWidth(true));

            return value;
        }
       
        }
    }

#endif


