using TMPro;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using static FormatGames.UIUtilities;
// using Newtonsoft.Json;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using static EditorUtilities;
#endif

namespace FormatGames.WayPoint
{
    [DefaultExecutionOrder(1)]
    public class Marker : MonoBehaviour
    {
        [HideInInspector] public RectTransform marker;

        [HideInInspector] public Image conteiner;
        [HideInInspector] public TextMeshProUGUI conteiner_label;
        [HideInInspector] public Image conteiner_icon;
        [HideInInspector] public Image directional_arrow;
        [HideInInspector] public TextMeshProUGUI label;


        [HideInInspector] public bool isOnEdgeStage;
        [HideInInspector] public bool referencesMissing;
        [HideInInspector] public bool disableOnEdge;
        [HideInInspector] public bool callFromInspector;
        [HideInInspector] public bool isApplyOffset;

        [HideInInspector] public Stage stage = new();
        [HideInInspector] public IndicatorReference indicatorReference;

        [HideInInspector] public Vector2 minSize = new Vector2(40f, 40f);
        [HideInInspector] public Vector2 maxSize = new Vector2(40f, 40f);
        [HideInInspector] public Vector2 lastPosition;
        [HideInInspector] public List<RectTransform> ignoreOnEdge;

        // ----------------------- Default --------------------------//
        [HideInInspector] public Sprite default_indicator_sprite;
        [HideInInspector] public Sprite default_arrow_sprite;

        private void Reset()
        {
            marker = GetComponent<RectTransform>();

            default_indicator_sprite = GetSpritesFromAssets("WaypointTextures", "waypoint_1");
            default_arrow_sprite = GetSpritesFromAssets("ArrowTextures", "arrow_1");

            stage.onFreeReference.center_sprite = default_indicator_sprite;
            stage.onEdgeReference.center_sprite = default_indicator_sprite;

            stage.onFreeReference.arrow_sprite = default_arrow_sprite;
            stage.onEdgeReference.arrow_sprite = default_arrow_sprite;

            marker = transform.GetComponent<RectTransform>();

            minSize = marker.sizeDelta;
            maxSize = marker.sizeDelta;

            Vector4 marker_offset = new(marker.offsetMin.x, marker.offsetMax.x, marker.offsetMin.y, marker.offsetMax.y);

            stage.currenReference.offset = marker_offset;
            stage.onEdgeReference.offset = marker_offset;
            stage.onFreeReference.offset = marker_offset;

            stage.onEdgeReference.last_offset = marker_offset;
            stage.onFreeReference.last_offset = marker_offset;

        }

        void Start()
        {
            referencesMissingCheck();

            if (Manager.refs && marker != null)
            {
                indicatorReference = Manager.refs.GetIndicator(marker);
                stage.refs = indicatorReference;
            }

            stage.isOnEdge = false;

            if (!referencesMissing)
            {
                label.gameObject.SetActive(true);
                directional_arrow.gameObject.SetActive(true);
            }
        }


        void Update()
        {
            DuringGame();
        }
        public bool referencesMissingCheck()
        {
            referencesMissing = conteiner == null || conteiner_label == null || conteiner_icon == null || label == null || directional_arrow == null || (indicatorReference != null && indicatorReference.indicator != null && indicatorReference.indicator.canvasGroup == null);

            return referencesMissing;
        }

       
        public void UpdateConfigurations()
        {
            if ((stage == null || referencesMissing || indicatorReference == null) || Application.isPlaying)
                return;

            if (Manager.refs && marker != null) stage.refs = indicatorReference;

          
           
            stage.isOnEdge = isOnEdgeStage;
            stage.currenReference = isOnEdgeStage ? stage.onEdgeReference : stage.onFreeReference;

            StageReference reference = stage.currenReference;

            conteiner_icon.gameObject.SetActive(reference.use_center_icon);
            conteiner_label.gameObject.SetActive(reference.use_center_label);
            label.gameObject.SetActive(reference.use_label || reference.use_distance);
            directional_arrow.gameObject.SetActive(reference.use_arrow);

            if (!isEditingOffset)
            {
                label.rectTransform.localPosition = reference.label_offset;
            }


            UpdateAppearance();

            (isOnEdgeStage ? ref maxSize : ref minSize) = marker.sizeDelta;
        }

        public void DuringGame()
        {
            if (indicatorReference == null || !indicatorReference.indicator.canTrack || referencesMissing) return;

            stage.currenReference = disableOnEdge ? stage.onFreeReference : (indicatorReference.indicator.isOnEdge ? stage.onEdgeReference : stage.onFreeReference);

            label.color = new Color(label.color.r, label.color.g, label.color.b, stage.currenReference.use_label || stage.currenReference.use_distance ? 1 : 0);
            directional_arrow.color = new Color(directional_arrow.color.r, directional_arrow.color.g, directional_arrow.color.b, stage.currenReference.use_arrow ? 1 : 0);

            stage.isOnEdge = indicatorReference.indicator.isOnEdge;

            UpdateAppearance();
        }

        public void UpdateAppearance()
        {
            if (isEditingOffset) return;

            stage.MarkerConteiner(marker, indicatorReference.indicator.isOnEdge, minSize, maxSize);
            stage.MainConteiner(conteiner);
            stage.IconConteiner(conteiner_icon, conteiner_label);
            stage.RotateArrow(directional_arrow);
            stage.Label(label);
        }


        // Editor

        #region RESIZE_MARKER

        [HideInInspector] public List<Vector3> ChildrenPositions = new();

        [HideInInspector] public bool isEditingOffset;
        [HideInInspector] private float lastEditTime;
        [HideInInspector] private bool pendingEdit;
        [HideInInspector] public Vector4 last_free_Offset;
        [HideInInspector] public Vector4 last_edge_Offset;



        public bool IncorrectOffset()
        {
            bool value = false;


            RectTransform target = marker;

            Vector4 target_offset = new(target.offsetMin.x, target.offsetMax.x, target.offsetMin.y, target.offsetMax.y);

            StageReference currentStage = isOnEdgeStage ? stage.onEdgeReference : stage.onFreeReference;

            if (target_offset != currentStage.offset)
            {
                value = true;
            }

            return value;
        }

        public Vector4 EqualPosition(Vector4 from, Vector4 to)
        {
            Vector4 A_position = from;
            Vector4 B_position = to;

            Vector4 A_center = new((A_position.x + A_position.y) / 2f, (A_position.z + A_position.w) / 2f);
            Vector4 B_center = new((B_position.x + B_position.y) / 2f, (B_position.z + B_position.w) / 2f);

            Vector2 offsetDelta = A_center - B_center;

            return new Vector4(offsetDelta.x, offsetDelta.x, offsetDelta.y, offsetDelta.y);

        }

        public void AdjustOffset(string situation)
        {
            RectTransform target = marker;

            if (situation == "syncSize")
            {
                OffsetHandler offset = new();
                offset.GetOffset(this);


                // Objetivo: igualar en posicion ambos stages en cuanto a posicion tomando como referencia el actual. al final cada uno debe mantener su tama�o en pantalla.

                StageReference currentStage = isOnEdgeStage ? stage.onEdgeReference : stage.onFreeReference;
                StageReference theOppositeStage = isOnEdgeStage ? stage.onFreeReference : stage.onEdgeReference;

                currentStage.offset = offset.GetTransForm().offset;

                theOppositeStage.offset += EqualPosition(offset.GetTransForm().offset, offset.GetOppositeStage().offset);

            }
            else if (situation == "pasteSize")
            {

                OffsetHandler offset = new();
                offset.GetOffset(this);

                StageReference currentStage = isOnEdgeStage ? stage.onEdgeReference : stage.onFreeReference;
                StageReference theOppositeStage = isOnEdgeStage ? stage.onFreeReference : stage.onEdgeReference;

                Vector2 offset_def = offset.GetOldOffset().center - offset.GetCurrentStage().center;
                Vector4 newOffset = new(offset.GetFreeStage().offset.x + offset_def.x, offset.GetFreeStage().offset.y + offset_def.x, offset.GetFreeStage().offset.z + offset_def.y, offset.GetFreeStage().offset.w + offset_def.y);


                currentStage.offset = newOffset;

                currentStage.last_offset = new(newOffset.z, newOffset.w);

                target.offsetMin = new Vector2(newOffset.x, newOffset.z);
                target.offsetMax = new Vector2(newOffset.y, newOffset.w);


                theOppositeStage.offset += EqualPosition(offset.GetTransForm().offset, offset.GetEdgeStage().offset);



            }
            else if (situation == "applySize")
            {

                OffsetHandler offset = new();
                offset.GetOffset(this);


                StageReference reference = isOnEdgeStage ? stage.onEdgeReference : stage.onFreeReference;

                target.offsetMin = offset.GetCurrentStage().offsetMin;
                target.offsetMax = offset.GetCurrentStage().offsetMax;

                reference.arrow_offset = directional_arrow.rectTransform.localPosition;
                reference.label_offset = label.rectTransform.localPosition;
                reference.center_offset = conteiner.rectTransform.localPosition;

                bool isEditingNow = isOnEdgeStage ? stage.onEdgeReference.last_offset != offset.GetCurrentStage().offset :
                                                    stage.onFreeReference.last_offset != offset.GetCurrentStage().offset;

                if (isOnEdgeStage)
                {
                    stage.onEdgeReference.last_offset = offset.GetCurrentStage().offset;
                }
                else
                {
                    stage.onFreeReference.last_offset = offset.GetCurrentStage().offset;
                }

                OnEditOffset(isEditingNow);
            }
        }

        public void OnEditOffset(bool isEditingNow)
        {
            if (isEditingNow)
            {
                if (!isEditingOffset)
                {
                    isEditingOffset = true;

                    ChildrenPositions.Clear();

                    foreach (RectTransform child in marker)
                    {
                        ChildrenPositions.Add(child.position);
                    }
                }

                SaveLastTimeEdited();

                for (int i = 0; i < marker.childCount; i++)
                {
                    marker.GetChild(i).position = ChildrenPositions[i];
                }
            }
        }

        private void SaveLastTimeEdited()
        {
            lastEditTime = Time.realtimeSinceStartup;
            pendingEdit = true;

#if UNITY_EDITOR
            EditorApplication.update -= CheckEditingStatus;
            EditorApplication.update += CheckEditingStatus;
#endif
        }

        private void CheckEditingStatus()
        {
            if (!pendingEdit) return;

            if (Time.realtimeSinceStartup - lastEditTime > 0.5f)
            {
                isEditingOffset = pendingEdit = false;
                isApplyOffset = false;
#if UNITY_EDITOR
                EditorApplication.update -= CheckEditingStatus;
#endif
            }
        }

        #endregion

        public void CopyStyle()
        {
            Manager.refs.indicatorStyleCopy = new IndicatorStyle
            {
                edge = stage.onEdgeReference.Clone(),
                free = stage.onFreeReference.Clone()
            };
        }
        public void PasteStyle()
        {
            last_free_Offset = stage.onFreeReference.offset;
            last_edge_Offset = stage.onEdgeReference.offset;

            IndicatorStyle style = Manager.refs.indicatorStyleCopy;

            stage.onFreeReference = style.free.Clone();
            stage.onEdgeReference = style.edge.Clone();

            AdjustOffset("pasteSize");

        }

    }
    public class OffsetHandler
    {
        public RectTransform marker;
        public Marker markerScript;

        public TransformOffset transformOffset;

        public CurrentStage currentStage;
        public OppositeStage oppositeStage;

        public FreeStage freeStage;
        public EdgeStage edgeStage;
        public OldOffset oldOffset;

        public struct TransformOffset
        {
            public Vector4 offset;
            public Vector4 center;
            public Vector4 offsetMin;
            public Vector4 offsetMax;

            public TransformOffset(RectTransform marker)
            {
                offset = new Vector4(marker.offsetMin.x, marker.offsetMax.x, marker.offsetMin.y, marker.offsetMax.y);
                center = new Vector4((marker.offsetMin.x + marker.offsetMax.x) / 2f, (marker.offsetMin.y + marker.offsetMax.y) / 2f, 0, 0);
                offsetMin = new Vector4(marker.offsetMin.x, marker.offsetMin.y, 0, 0);
                offsetMax = new Vector4(marker.offsetMax.x, marker.offsetMax.y, 0, 0);
            }
        }
        public struct CurrentStage
        {
            public Vector4 offset;
            public Vector4 center;
            public Vector4 offsetMin;
            public Vector4 offsetMax;

            public CurrentStage(Marker markerScript)
            {
                StageReference stageRef = markerScript.isOnEdgeStage ? markerScript.stage.onEdgeReference : markerScript.stage.onFreeReference;

                offset = stageRef.offset;
                center = new((stageRef.offset.x + stageRef.offset.y) / 2f, (stageRef.offset.z + stageRef.offset.w) / 2f);
                offsetMin = new(stageRef.offset.x, stageRef.offset.z, 0, 0);
                offsetMax = new(stageRef.offset.y, stageRef.offset.w, 0, 0);
            }
        }
        public struct OppositeStage
        {
            public Vector4 offset;
            public Vector4 center;
            public Vector4 offsetMin;
            public Vector4 offsetMax;

            public OppositeStage(Marker markerScript)
            {
                StageReference opposite = markerScript.isOnEdgeStage ? markerScript.stage.onFreeReference : markerScript.stage.onEdgeReference;

                offset = opposite.offset;
                center = new((opposite.offset.x + opposite.offset.y) / 2f, (opposite.offset.z + opposite.offset.w) / 2f);
                offsetMin = new(opposite.offset.x, opposite.offset.z, 0, 0);
                offsetMax = new(opposite.offset.y, opposite.offset.w, 0, 0);
            }
        }
        public struct FreeStage
        {
            public Vector4 offset;
            public Vector4 center;
            public Vector4 offsetMin;
            public Vector4 offsetMax;

            public FreeStage(Marker markerScript)
            {
                StageReference free = markerScript.stage.onFreeReference;

                offset = free.offset;
                center = new((free.offset.x + free.offset.y) / 2f, (free.offset.z + free.offset.w) / 2f);
                offsetMin = new(free.offset.x, free.offset.z, 0, 0);
                offsetMax = new(free.offset.y, free.offset.w, 0, 0);
            }
        }
        public struct EdgeStage
        {
            public Vector4 offset;
            public Vector4 center;
            public Vector4 offsetMin;
            public Vector4 offsetMax;

            public EdgeStage(Marker markerScript)
            {
                StageReference edge = markerScript.stage.onEdgeReference;

                offset = edge.offset;
                center = new((edge.offset.x + edge.offset.y) / 2f, (edge.offset.z + edge.offset.w) / 2f);
                offsetMin = new(edge.offset.x, edge.offset.z, 0, 0);
                offsetMax = new(edge.offset.y, edge.offset.w, 0, 0);
            }
        }
        public struct OldOffset
        {
            public Vector4 offset;
            public Vector4 center;
            public Vector4 offsetMin;
            public Vector4 offsetMax;

            public OldOffset(Marker markerScript)
            {
                Vector4 Old_Offset = markerScript.last_free_Offset;

                offset = Old_Offset;
                center = new((Old_Offset.x + Old_Offset.y) / 2f, (Old_Offset.z + Old_Offset.w) / 2f);
                offsetMin = new(Old_Offset.x, Old_Offset.z, 0, 0);
                offsetMax = new(Old_Offset.y, Old_Offset.w, 0, 0);
            }
        }

        public TransformOffset GetTransForm()
        {
            transformOffset = new TransformOffset(marker);
            return transformOffset;
        }
        public CurrentStage GetCurrentStage()
        {
            currentStage = new CurrentStage(markerScript);
            return currentStage;
        }
        public OppositeStage GetOppositeStage()
        {
            oppositeStage = new OppositeStage(markerScript);
            return oppositeStage;
        }
        public FreeStage GetFreeStage()
        {
            freeStage = new FreeStage(markerScript);
            return freeStage;
        }
        public EdgeStage GetEdgeStage()
        {
            edgeStage = new EdgeStage(markerScript);
            return edgeStage;
        }
        public OldOffset GetOldOffset()
        {
            oldOffset = new OldOffset(markerScript);
            return oldOffset;
        }

        public void GetOffset(Marker target)
        {
            markerScript = target;
            marker = markerScript.marker;
        }
    }

  
    [Serializable]
    public class Stage
    {
        [HideInInspector] public IndicatorReference refs;

        public StageReference currenReference = new();
        public StageReference onFreeReference = new();
        public StageReference onEdgeReference = new();

        public bool isOnEdge;

        public void MarkerConteiner(RectTransform target, bool caseType, Vector2 minSize, Vector2 maxSize)
        {
            if (Application.isPlaying)
            {
                target.sizeDelta = caseType ? maxSize : minSize;
            }
        }
        public void Label(TextMeshProUGUI target)
        {
            bool setLabel = (isOnEdge && currenReference.use_label) || (!isOnEdge && currenReference.use_label);
            bool setDistance = (isOnEdge && currenReference.use_distance) || (!isOnEdge && currenReference.use_distance);
            bool setOffset = (isOnEdge && !currenReference.rotateLabel) || (!isOnEdge && !currenReference.rotateLabel);
            bool orbit = Application.isPlaying ? currenReference.rotateLabel : false;


            if (setOffset)
            {
                Vector2 offset = setOffset ? currenReference.label_offset : target.rectTransform.localPosition;

                target.rectTransform.localPosition = offset;
            }

            if (orbit)
            {
                float orbit_Offset = currenReference.label_offset.magnitude;

                Vector2 direction = refs.indicator.screenEdgePosition;

                direction = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? new Vector2(Mathf.Sign(direction.x), 0) : new Vector2(0, Mathf.Sign(direction.y));

                Vector2 newoffset = direction * -orbit_Offset;

                target.rectTransform.localPosition = newoffset;
            }

            if (setLabel)
            {
                target.text = currenReference.label;
            }
            else if (setDistance)
            {
                target.text = refs.indicator.distance.ToString("0") + "m";
            }
            else
            {
                target.text = "";
            }

            target.rectTransform.sizeDelta = currenReference.label_size;
            target.fontSize = currenReference.label_font_size;
            target.color = currenReference.label_color;
        }
        public void IconConteiner(Image icon, TextMeshProUGUI label)
        {
            bool useIcon = currenReference.use_center_icon;
            bool useLabel = currenReference.use_center_label;

            Vector2 contentSize = currenReference.icon_conteiner_icon_size;
            Vector2 icon_offset = currenReference.ic_conteiner_icon_offset;
            Vector2 label_offset = currenReference.ic_conteiner_label_offset;
            Color conteiner_label_color = new(currenReference.conteiner_label_color.r, currenReference.conteiner_label_color.g, currenReference.conteiner_label_color.b, useLabel ? 1: 0);
            Color conteiner_icon_color = new(currenReference.conteiner_icon_color.r, currenReference.conteiner_icon_color.g, currenReference.conteiner_icon_color.b, useIcon ? 1 : 0);

            string label_string = useLabel ? (currenReference.center_label) : "";
            Sprite sprite = currenReference.icon_conteiner_sprite;
            float fontSize = currenReference.icon_conteiner_label_size;

            icon.color = conteiner_icon_color;
            icon.sprite = sprite;
            icon.rectTransform.sizeDelta = contentSize;
            icon.rectTransform.localPosition = icon_offset;

            label.color = conteiner_label_color;
            label.fontSize = fontSize;
            label.rectTransform.sizeDelta = contentSize;
            label.rectTransform.localPosition = label_offset;
            label.text = label_string;
        }
        public void MainConteiner(Image newImage)
        {
            bool rotate = currenReference.rotateCenter;
            float angle = rotate ? refs.indicator.targetAngleDirection : newImage.rectTransform.localRotation.z;

            Sprite sprite = currenReference.center_sprite;
            Color color = currenReference.center_color;
            Vector2 size = currenReference.center_size;
            Vector2 position = currenReference.center_offset;

            newImage.sprite = sprite;
            newImage.color = color;
            newImage.rectTransform.sizeDelta = size;
            newImage.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            newImage.rectTransform.localPosition = position;
        }
        public void RotateArrow(Image newImage)
        {
            Color imageColor = newImage.color;

            bool rotate = currenReference.use_arrow;
            imageColor.a = rotate ? 1 : 0;
            newImage.color = imageColor;
            newImage.sprite = currenReference.arrow_sprite;
            newImage.rectTransform.sizeDelta = currenReference.arrow_size;
            newImage.rectTransform.localPosition = currenReference.arrow_offset;

            if (rotate && Application.isPlaying)
            {
                Vector3 direction = new Vector3(Manager.refs.screenWidth / 2, Manager.refs.screenHeight / 2, 0) - refs.indicator.marker.position;
                FormatGames.UIUtilities.OrbitTarget(newImage.rectTransform, direction, currenReference.arrow_offset.x);
            }
        }
    }

    [Serializable]
    public class IndicatorStyle
    {
        public StageReference free;
        public StageReference edge;
    }

    [Serializable]
    public class StageReference
    {
        public Vector4 last_offset = new(-50, 50, -50, 50);
        public Vector4 offset = new(-50, 50, -50, 50);

        public Vector2 center_size = new(40, 40);
        public Vector2 icon_conteiner_icon_size = new(20, 20);
        public Vector2 arrow_size = new(6, 12);
        public Vector2 label_size = new(100, 20);

        // Offsets
        public Vector2 center_offset = new(0, 0);
        public Vector2 arrow_offset = new(40f, 0);
        public Vector2 label_offset = new(0, -40);

        public Vector2 ic_conteiner_label_offset = new(0, 0);
        public Vector2 ic_conteiner_icon_offset = new(0, 0);

        // Colores
        public Color center_color = Color.white;
        public Color conteiner_label_color = Color.black;
        public Color conteiner_icon_color = Color.black;
        public Color label_color = Color.white;

        // Texto y Fuente
        public string center_label = "e";
        public string label = "Label";
        public float icon_conteiner_label_size = 17f;
        public float label_font_size = 17f;

        // Sprites
        public Sprite center_sprite;
        public Sprite icon_conteiner_sprite;
        public Sprite arrow_sprite;

        public bool use_label;
        public bool use_distance;
        public bool use_center_label;
        public bool use_center_icon;
        public bool use_arrow;
        public bool rotateCenter;
        public bool rotateLabel;

        public StageReference Clone()
        {
            return (StageReference)this.MemberwiseClone();
        }
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(Marker))]
    public class Editor : UnityEditor.Editor
    {
        Marker marker;

        public override void OnInspectorGUI()
        {
            marker = (target) as Marker;

            if (marker.conteiner != null && marker.label != null && marker.label != null && marker.directional_arrow != null && marker.conteiner_label != null && marker.conteiner_icon != null)
            {
                DrawDefaultInspector();
            }
            else
            {
                GUILayout.Space(20);

                if (GUILayout.Button("Create Hierarchy"))
                {
                    GameObject instance = new GameObject();

                    // Crear elementos principales
                    GameObject mainContainerGO = CreateChild(instance, marker.transform, "Conteiner");
                    GameObject labelGO = CreateChild(instance, marker.transform, "Label");
                    GameObject arrowGO = CreateChild(instance, marker.transform, "Directional Arrow");

                    // Crear elementos secundarios
                    GameObject labelContainerGO = CreateChild(instance, mainContainerGO.transform, "conteiner_label");
                    GameObject iconGO = CreateChild(instance, mainContainerGO.transform, "conteiner_icon");

                    // Agregar RectTransforms
                    RectTransform mainContainerRect = mainContainerGO.AddComponent<RectTransform>();
                    RectTransform labelRect = labelGO.AddComponent<RectTransform>();
                    RectTransform arrowRect = arrowGO.AddComponent<RectTransform>();
                    RectTransform labelContainerRect = labelContainerGO.AddComponent<RectTransform>();
                    RectTransform iconRect = iconGO.AddComponent<RectTransform>();

                    // Agregar componentes
                    Image mainContainerImage = mainContainerRect.gameObject.AddComponent<Image>();
                    Image arrowImage = arrowRect.gameObject.AddComponent<Image>();
                    Image iconImage = iconRect.gameObject.AddComponent<Image>();

                    TextMeshProUGUI label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
                    TextMeshProUGUI labelContainerText = labelContainerRect.gameObject.AddComponent<TextMeshProUGUI>();


                   

                    // Configurar posiciones
                    SetLocalPosition(mainContainerRect, Vector2.zero);
                    SetLocalPosition(labelRect, new Vector2(0, -50));
                    SetLocalPosition(arrowRect, new Vector2(50, 0));
                    SetLocalPosition(labelContainerRect, Vector2.zero);
                    SetLocalPosition(iconRect, Vector2.zero);

                    // Configurar tama�os
                    SetSize(marker.transform.GetComponent<RectTransform>(), new Vector2(50, 50));
                    SetSize(mainContainerRect, new Vector2(50, 50));
                    SetSize(labelRect, new Vector2(100, 30));
                    SetSize(arrowRect, new Vector2(10, 20));
                    SetSize(iconRect, new Vector2(20, 20));

                    // Configurar anclajes
                    labelContainerRect.offsetMin = Vector2.zero;
                    labelContainerRect.offsetMax = Vector2.zero;

                    labelContainerRect.anchorMin = Vector2.zero;
                    labelContainerRect.anchorMax = Vector2.one;

                    // Configurar colores
                    iconImage.color = Color.black;
                    label.color = Color.white;
                    labelContainerText.color = Color.black;

                    // Configurar texto
                    label.text = "Label";
                    labelContainerText.text = "e";

                    // Configurar alineaci�n
                    label.alignment = TextAlignmentOptions.Center;
                    label.alignment = TextAlignmentOptions.Midline;
                    labelContainerText.alignment = TextAlignmentOptions.Center;
                    labelContainerText.alignment = TextAlignmentOptions.Midline;

                    // Configurar tama�o de fuente
                    label.fontSize = 20;
                    labelContainerText.fontSize = 40;

                    // Asignar referencias al marcador
                    marker.conteiner = mainContainerImage;
                    marker.label = label;
                    marker.directional_arrow = arrowImage;
                    marker.conteiner_label = labelContainerText;
                    marker.conteiner_icon = iconImage;

                    SetSprite(mainContainerImage, marker.default_indicator_sprite);
                    SetSprite(arrowImage, marker.default_arrow_sprite);

                    SetAlpha(typeof(TextMeshProUGUI), labelContainerText.rectTransform, 0);
                    SetAlpha(typeof(Image), iconImage.rectTransform, 0);

                    // Eliminar objeto temporal
                    DestroyImmediate(instance);
                }

                GUILayout.Space(20);

                // --------------------- M�todos Auxiliares ---------------------

                GameObject CreateChild(GameObject template, Transform parent, string name)
                {
                    GameObject obj = Instantiate(template, parent);
                    obj.name = name;
                    return obj;
                }

                void SetLocalPosition(RectTransform rect, Vector2 position)
                {
                    rect.localPosition = position;
                }

                void SetSize(RectTransform rect, Vector2 size)
                {
                    rect.sizeDelta = size;
                }

                void SetSprite(Image image, Sprite sprite)
                {
                    image.sprite = sprite;
                }

                void SetAlpha(Type type, RectTransform target, float alpha)
                {
                    if(type == typeof(Image))
                    {
                        Image image = target.GetComponent<Image>();

                        Color newcolor = image.color;

                        newcolor.a = alpha;

                        image.color = newcolor;
                    }
                    else if (type == typeof(TextMeshProUGUI))
                    {
                        TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();

                        Color newcolor = text.color;

                        newcolor.a = alpha;

                        text.color = newcolor;
                    }



                   
                }

            }


            serializedObject.ApplyModifiedProperties();
        }
     }
}
#endif