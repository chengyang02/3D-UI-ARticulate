using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FormatGames.WayPoint
{
    [AddComponentMenu("UI/Waypoint (Manager)"), DefaultExecutionOrder(-1)]
    public class Manager : MonoBehaviour
    {
        // ----------------------- REFERENCES ------------------------//

        public static Manager refs;
        public List<IndicatorLayer> layers = new();

        [Header("References"), Space(10)]
        public Transform player;
        public Camera mainCamera;
        public Canvas mainCanvas;
        private RectTransform canvasRect;


        // ----------------------- OPTIONS ------------------------//


        [HideInInspector] public float screenOffset = 0.0f;
        [HideInInspector] public float SmoothClamp = 40f;
        [HideInInspector] public float screenWidth, screenHeight;
        [HideInInspector] public float radiusX, radiusY, halfScreenWidth, halfScreenHeight;
        [HideInInspector] public float minX, maxX, minY, maxY;
        [HideInInspector] public Vector2 lastScreenSize;
        [HideInInspector] public Vector2 forwardPlayer;

        // ------------------- EDITOR OPTIONS --------------------//

        [HideInInspector] public bool showDefaultInspector;
        [HideInInspector] public bool showGizmos = true;
        [HideInInspector] public bool collapseIndicators = true;
        [HideInInspector] public bool editAtPlayMode = true;
        [HideInInspector] public bool hideMarkerOnFold = false;

        [HideInInspector] public Color color = Color.white;
        [HideInInspector] public float floattest = 0;

        // ------------------- EDITOR STATE ---------------------//

        public bool unfoldEditor;
        public bool unfoldSettings;

        public bool isAddingLayer;
        public bool isAddingIndicator;
        public bool isEditingLayer;
        public bool addMultiple;
        public bool isCopiedStyle;

        // ---------------- EDITOR REFERENCES ------------------//

        [HideInInspector] public IndicatorLayer editingIndicatorLayer;
        [HideInInspector] public RectTransform markerAdd;
        [HideInInspector] public Transform targetAdd;
        [HideInInspector] public List<RectTransform> markers = new();
        [HideInInspector] public List<Transform> targets = new();
        [HideInInspector] public IndicatorStyle indicatorStyleCopy;



        // ---------------------------------- WAYPOINT INDICATOR SYSTEM ---------------------------///



        void Awake()
        {
            refs = this;
            canvasRect = mainCanvas.GetComponent<RectTransform>();
        }
        private void Start()
        {
            EnabledManager();
        }

        private void LateUpdate()
        {
            if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
            {
                lastScreenSize = new Vector2(Screen.width, Screen.height);
                ScreenManager();
            }

            int layerCount = layers.Count; for (int g = 0; g < layerCount; g++)
            {
                IndicatorLayer group = layers[g];

                int indicatorCount = group.indicators.Count;  for (int i = 0; i < indicatorCount; i++)
                {
                    Indicator indicator = group.indicators[i];

                    float distance = Vector3.Distance(player.position,indicator.target.position);
                    bool isFocusingTarget = IsTargetInFront(indicator.target, group.focusAngle);
                    bool showIndicator = distance > group.minDistance && distance < group.maxDistance && group.enabled && indicator.enabled && isFocusingTarget;

                    Vector3 centerPosition = new Vector3(screenWidth / 2, screenHeight / 2, 0);
                    indicator.targetDirection = centerPosition - indicator.marker.position;
                    indicator.isInRange = distance < group.maxDistance && isFocusingTarget;
                    indicator.distance = distance;
                    


                    if (indicator.canvasGroup)
                    {
                        CanvasGroup canvasGroup = indicator.canvasGroup;
                        RectTransform marker = indicator.marker;

                        float targetAlpha = showIndicator ? 1f : 0f;
                        float currentAlpha = canvasGroup.alpha;
                        float minAlpha = 0.001f;

                        // Fade
                        if (!Mathf.Approximately(currentAlpha, targetAlpha))
                        {
                            canvasGroup.enabled = true;
                            indicator.canvasGroup.alpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * group.fadeSpeed);

                            if (canvasGroup.alpha <= minAlpha)
                            {
                                marker.anchoredPosition = new Vector2(9999, 9999);
                                canvasGroup.enabled = false;
                            }
                        }

                        bool canTrack = canvasGroup.alpha > minAlpha;

                        // tracking
                        if (canTrack)
                        {
                            GetScreenPosition(group, indicator);
                            ClampManager(group, indicator);
                        }

                        indicator.canTrack = canTrack;
                    }
                }
            }
        }
        public void UpdateConfigurations()
        {
            EnabledManager();
        }
        private void ScreenManager()
        {
            screenWidth = Screen.width;
            screenHeight = Screen.height;

            halfScreenWidth = screenWidth / 2;
            radiusX = halfScreenWidth;

            halfScreenHeight = screenHeight / 2;
            radiusY = halfScreenHeight;

            minX = 0;
            maxX = screenWidth;
            minY = 0;
            maxY = screenHeight;
        }
        private void ClampManager(IndicatorLayer IndicatorLayer, Indicator indicator)
        {
            RectTransform makerTransform = indicator.marker;

            Vector2 markersize = makerTransform.sizeDelta * mainCanvas.scaleFactor / 2f;
            Vector2 screen = new(screenOffset * screenWidth, screenOffset * screenWidth);
            Vector2 GUI_targetPosition = (Vector2)indicator.screenPosition - new Vector2(halfScreenWidth, halfScreenHeight);

            float newradX = (radiusX - markersize.x) - screen.x;
            float newradY = (radiusY - markersize.y) - screen.y;

            float newMinX = (minX + markersize.x) + screen.x;
            float newMaxX = (maxX - markersize.x) - screen.x;
            float newMinY = (minY + markersize.y) + screen.y;
            float newMaxY = (maxY - markersize.y) - screen.y;

            switch (IndicatorLayer.clampStyle)
            {
                case IndicatorLayer.ClampStyle.Elipse:

                    float ovalValue = Mathf.Pow(GUI_targetPosition.x / newradX, 2) + Mathf.Pow(GUI_targetPosition.y / newradY, 2);
                    GUI_targetPosition = ovalValue >= 1f ? Vector2.ClampMagnitude(GUI_targetPosition, 1f) * new Vector2(newradX, newradY) : GUI_targetPosition;
                    makerTransform.position = Vector3.Lerp(makerTransform.position, GUI_targetPosition + new Vector2(halfScreenWidth, halfScreenHeight), Time.fixedDeltaTime * SmoothClamp);

                    ScreenData(IndicatorLayer, indicator, new Vector4(ovalValue, 0, 0, 0));

                    break;

                case IndicatorLayer.ClampStyle.Centered:

                    if(indicator.rawIsOnEdge)
                    {
                        Vector2 direction = (indicator.screenPosition - new Vector3(halfScreenWidth, halfScreenHeight)).normalized;

                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                        Vector2 newPosition = new(
                        halfScreenWidth + Mathf.Cos(Mathf.Deg2Rad * angle) * IndicatorLayer.radiusFromCenter,
                        halfScreenHeight + Mathf.Sin(Mathf.Deg2Rad * angle) * IndicatorLayer.radiusFromCenter
                        );


                        makerTransform.position = newPosition;
                        makerTransform.rotation = Quaternion.Euler(0f, 0f, angle);

                        ScreenData(IndicatorLayer, indicator, new Vector4(minX, maxX, minY, maxY));
                    }
                    else
                    {
                        makerTransform.localPosition = new Vector3(9999,9999,0);

                    }
                   

                    break;

                case IndicatorLayer.ClampStyle.Laterals:

                    bool isLeft = indicator.screenPosition.x < screenWidth * 0.5f;
                    bool isAtTopOrBottom = indicator.screenPosition.y <= newMinY || indicator.screenPosition.y >= newMaxY;
                    bool isOnEdgeLaterals = indicator.screenPosition.x <= newMinX || indicator.screenPosition.x >= newMaxX;

                    float clampedY = Mathf.Clamp(indicator.screenPosition.y, newMinY, newMaxY);
                    float clampedX = isAtTopOrBottom ? (isLeft ? newMinX : newMaxX) : Mathf.Clamp(indicator.screenPosition.x, newMinX, newMaxX);

                    makerTransform.position = new Vector3(clampedX, clampedY, 0f);

                    ScreenData(IndicatorLayer, indicator, new Vector4(newMinX, newMaxX, newMinY, newMaxY));

                    break;

                case IndicatorLayer.ClampStyle.Rectangle:

                    makerTransform.position = new Vector3(
                    Mathf.Clamp(indicator.screenPosition.x, newMinX, newMaxX),
                    Mathf.Clamp(indicator.screenPosition.y, newMinY, newMaxY),
                    0f);

                    ScreenData(IndicatorLayer, indicator, new Vector4(newMinX, newMaxX, newMinY, newMaxY));

                    break;


                case IndicatorLayer.ClampStyle.None:


                    ScreenData(IndicatorLayer, indicator, new Vector4(minX, maxX, minY, maxY));

                    makerTransform.position = indicator.freescreenPosition;

                    break;
            }
        }

        private void GetScreenPosition(IndicatorLayer IndicatorLayer, Indicator indicator)
        {
            Transform target = indicator.target;
            Vector3 targetPosition = target.position;
            Vector3 playerPosition = player.position;

            float distance = Vector3.Distance(playerPosition, targetPosition);
            float scaleFactor = 1f / (1f + (distance * 0.05f));

            // Factores de normalización de pantalla y escala del canvas
            float screenFactor = screenWidth / 1920f;
            Vector2 canvasScale = canvasRect.localScale;
            Vector2 canvasSize = canvasRect.rect.size * canvasScale;

            indicator.screenPosition = mainCamera.WorldToScreenPoint(targetPosition) + (Vector3)(indicator.offset * scaleFactor * screenFactor);
            indicator.freescreenPosition = indicator.screenPosition;

            if (indicator.screenPosition.z >= 0f &&
                indicator.screenPosition.x >= 0f && indicator.screenPosition.x <= canvasSize.x &&
                indicator.screenPosition.y >= 0f && indicator.screenPosition.y <= canvasSize.y)
            {
                indicator.screenPosition.z = 0f;
                return;
            }

            // Si está fuera de la vista en z, ajustamos la posición
            indicator.screenPosition *= (indicator.screenPosition.z < 0f) ? -1f : 1f;

            Vector3 indicatorPosition = indicator.screenPosition;
            indicatorPosition.z = 0f;

            Vector3 canvasCenter = canvasSize * 0.5f;
            indicatorPosition -= canvasCenter;

            float halfWidth = canvasSize.x * 0.5f - screenOffset;
            float halfHeight = canvasSize.y * 0.5f - screenOffset;

            float divX = halfWidth / Mathf.Abs(indicatorPosition.x);
            float divY = halfHeight / Mathf.Abs(indicatorPosition.y);

            float angle;

            if (divX < divY)
            {
                angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
                indicatorPosition.x = Mathf.Sign(indicatorPosition.x) * halfWidth;
                indicatorPosition.y = Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.x;
            }
            else
            {
                angle = Vector3.SignedAngle(Vector3.up, indicatorPosition, Vector3.forward);
                indicatorPosition.y = Mathf.Sign(indicatorPosition.y) * halfHeight;
                indicatorPosition.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.y;
            }

            indicator.screenPosition = indicatorPosition + canvasCenter;

            // Verificación de bordes con offset
            Vector2 screenMin = Vector2.one * screenOffset;
            Vector2 screenMax = canvasSize - Vector2.one * screenOffset;

            indicator.rawIsOnEdge = indicator.screenPosition.x <= screenMin.x || indicator.screenPosition.x >= screenMax.x ||
                                    indicator.screenPosition.y <= screenMin.y || indicator.screenPosition.y >= screenMax.y;
        }
        private bool IsTargetInFront(Transform target, float maxAngle)
        {
            Vector3 playerToTarget = (target.position - player.position).normalized;

            if (mainCamera.orthographic) // 2D
            {
                Vector2 playerForward = forwardPlayer == Vector2.zero ? player.right : forwardPlayer;
                float angle = Vector2.Angle(playerForward, playerToTarget);
                return angle < maxAngle;
            }
            else // 3D
            {
                Vector3 playerForward = player.forward;

                // Proyectar en el plano XZ para ignorar altura
                Vector3 flatForward = new Vector3(playerForward.x, 0, playerForward.z).normalized;
                Vector3 flatToTarget = new Vector3(playerToTarget.x, 0, playerToTarget.z).normalized;

                float angle = Vector3.Angle(flatForward, flatToTarget);
                return angle < maxAngle;
            }
        }

        public void EnabledManager()
        {
            foreach (var layer in layers)
            {
                foreach (var indicator in layer.indicators)
                {
                    if (!indicator.marker) continue;

                    bool isActive = layer.enabled && indicator.enabled;

                    if (!Application.isPlaying && hideMarkerOnFold)
                    {
                        isActive &= indicator.unfold && layer.unfold;
                    }

                    indicator.marker.gameObject.SetActive(isActive);
                }
            }
        }
        public void ScreenData(IndicatorLayer IndicatorLayer, Indicator indic, Vector4 screenValues)
        {
            float minX = screenValues.x;
            float maxX = screenValues.y;
            float minY = screenValues.z;
            float maxY = screenValues.w;

            Indicator indicator = indic;
            Transform marker = indicator.marker;

            // ---------------------------------------- On Edge --------------------------------------------//

            float tolerance = IndicatorLayer.clampStyle == IndicatorLayer.ClampStyle.None ? 0.1f : 0.01f; // Tolerancia estándar para detección

            bool isOnEdgeX = Mathf.Abs(marker.position.x - minX) < tolerance || Mathf.Abs(marker.position.x - maxX) < tolerance;
            bool isOnEdgeY = Mathf.Abs(marker.position.y - minY) < tolerance || Mathf.Abs(marker.position.y - maxY) < tolerance;

            Vector2 edgeDirection = Vector2.zero;

            if (Mathf.Abs(marker.position.x - minX) < tolerance)
            {
                edgeDirection.x = -1; // Izquierda
            }
            else if (Mathf.Abs(marker.position.x - maxX) < tolerance)
            {
                edgeDirection.x = 1; // Derecha
            }

            if (Mathf.Abs(marker.position.y - minY) < tolerance)
            {
                edgeDirection.y = -1; // Abajo
            }
            else if (Mathf.Abs(marker.position.y - maxY) < tolerance)
            {
                edgeDirection.y = 1; // Arriba
            }

            // Asigna el resultado
            indicator.screenEdgePosition = edgeDirection;




            switch (IndicatorLayer.clampStyle)
            {
                case IndicatorLayer.ClampStyle.Elipse:
                    indicator.isOnEdge = minX >= 1f;
                    break;

                case IndicatorLayer.ClampStyle.Centered:
                    indicator.isOnEdge = indicator.rawIsOnEdge;
                    break;

                case IndicatorLayer.ClampStyle.Laterals:
                    indicator.isOnEdge = isOnEdgeX;
                    break;

                case IndicatorLayer.ClampStyle.Rectangle:
                    indicator.isOnEdge = isOnEdgeX || isOnEdgeY;
                    break;

                case IndicatorLayer.ClampStyle.None:
                    indicator.isOnEdge = isOnEdgeX || isOnEdgeY;
                    break;
            }



            // ---------------------------------------- Screen Direction --------------------------------------------//



            if (IndicatorLayer.clampStyle == IndicatorLayer.ClampStyle.Rectangle || IndicatorLayer.clampStyle == IndicatorLayer.ClampStyle.Laterals)
            {

                if (IndicatorLayer.clampStyle == IndicatorLayer.ClampStyle.Laterals)
                {
                    bool isLeft = indicator.screenPosition.x < screenWidth * 0.5f;
                    bool isOnEdgeLaterals = indicator.screenPosition.x <= minX || indicator.screenPosition.x >= maxX;

                    indicator.targetAngleDirection = isOnEdgeLaterals ? (isLeft ? 180 : 0) : (indicator.screenPosition.x < screenWidth / 2) ? 180 : 0;
                }
                else if (IndicatorLayer.clampStyle == IndicatorLayer.ClampStyle.Rectangle)
                {
                    indicator.targetAngleDirection =
                    indicator.screenPosition.x <= minX ? 180f :
                    indicator.screenPosition.x >= maxX ? 0f :
                    indicator.screenPosition.y <= minY ? 270f :
                    indicator.screenPosition.y >= maxY ? 90f :
                    indicator.targetAngleDirection;
                }
            }
            else
            {
                indicator.targetAngleDirection = Mathf.Atan2(indicator.targetDirection.y, indicator.targetDirection.x) * Mathf.Rad2Deg + 180;
            }
        }



        // ------------------------------------------ USER ------------------------------------------///

        public void AddIndicator(int layerId, RectTransform marker, Transform target)
        {
            // Buscar directamente en la lista sin crear un diccionario temporal
            IndicatorLayer layerFound = layers.Find(layer => layer.id == layerId);

            if (layerFound == null)
            {
                Debug.LogWarning($"Could not find the layer with Id:  {layerId}");
                return;
            }

            // Añadir el indicador
            Indicator newIndicator = CreateIndicator(marker, target);
            layerFound.indicators.Add(newIndicator);

            // Asignar el script Marker si existe
            Marker markerScript = marker.GetComponent<Marker>();
            if (markerScript)
            {
                newIndicator.markerScript = markerScript;
            }
        }

        public IndicatorReference GetIndicator(object identifier = null)
        {
            foreach (var group in layers)
            {
                foreach (var indicator in group.indicators)
                {
                    if (identifier == null || // returns the first one of the list
                        identifier is RectTransform marker && indicator.marker == marker || // returns by looking for a rect
                        identifier is int id && indicator.id == id) // returns by looking for a id
                    {
                        return new IndicatorReference(group, indicator);
                    }
                }
            }
            return null;
        }

        public void RemoveIndicator(RectTransform marker)
        {
            foreach (var indicGroup in layers)
            {
                for (int i = indicGroup.indicators.Count - 1; i >= 0; i--)
                {
                    if (indicGroup.indicators[i].marker == marker)
                    {
                        CanvasGroup canvasGroup = indicGroup.indicators[i].canvasGroup;
                        indicGroup.indicators[i].marker.gameObject.SetActive(false);
                        indicGroup.indicators.RemoveAt(i);
                        DestroyImmediate(canvasGroup);
                    }
                }
            }
        }

        // ----------------------------------------- EDITOR ------------------------------------------///

        public void AddLayer(string name)
        {
            bool isValid = true;

            foreach (var IndicatorLayer in layers)
            {
                if (IndicatorLayer.name == name)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                IndicatorLayer instanceLayer = new();
                instanceLayer.name = name;
                instanceLayer.index = layers.Count - 1;

                // Generar un ID de 5 dígitos basado en tiempo (mmssM)
                System.DateTime now = System.DateTime.Now;
                int randomPart = UnityEngine.Random.Range(0, 100);
                instanceLayer.id = ((now.Minute * 10000 + now.Second * 100 + now.Millisecond / 10) + randomPart) % 100000;

                layers.Add(instanceLayer);
            }
            else
            {
                Debug.Log("There is already a layer with this name");
            }
        }
        public void AddIndicatorList()
        {
            for (int i = 0; i < markers.Count; i++)
            {
                editingIndicatorLayer.indicators.Add(CreateIndicator(markers[i], targets[i]));
            }

            markers.Clear();
            targets.Clear();
        }
        public void AddIndicator(IndicatorLayer IndicatorLayer, RectTransform marker, Transform target)
        {
            bool alreadyUsed = false;


            foreach (IndicatorLayer layer in layers)
            {
                foreach (Indicator indicator in layer.indicators)
                {
                    if (indicator.target == target)
                    {
                        alreadyUsed = true;
                        Debug.LogWarning("TThis target is already used by " + "indicator: " + indicator.index + " in layer: " + layer.name);
                    }

                    if (indicator.marker == marker)
                    {
                        alreadyUsed = true;
                        Debug.LogWarning("TThis marker is already used by " + "indicator: " + indicator.index + " in layer: " + layer.name);
                    }
                }
            }

            if(!alreadyUsed)
            {
                IndicatorLayer.indicators.Add(CreateIndicator(marker, target));
            }

        }
        private Indicator CreateIndicator(RectTransform marker, Transform target)
        {
            CanvasGroup canvasG = marker.GetComponent<CanvasGroup>();

            if(canvasG) DestroyImmediate(canvasG);

            Indicator newIndicator = new()
            {
                marker = marker,
                target = target,
                canvasGroup = marker.gameObject.AddComponent<CanvasGroup>()
            };

            Marker markerScript = marker.GetComponent<Marker>();

            newIndicator.markerScript = markerScript;

            newIndicator.canvasGroup.enabled = false;
            marker.gameObject.SetActive(true);
            System.DateTime now = System.DateTime.Now;
            int randomPart = UnityEngine.Random.Range(0, 100);
            newIndicator.id = ((now.Minute * 10000 + now.Second * 100 + now.Millisecond / 10) + randomPart) % 100000;

            return newIndicator;
        }
        public void MoveLayer(int currentIndex, int newIndex)
        {
            if (newIndex == -1) return;

            if(newIndex >= layers.Count)
            {
                newIndex = layers.Count - 1;
            }

            if (newIndex < 0 || newIndex >= layers.Count || currentIndex < 0 || currentIndex >= layers.Count)
            {
                Debug.LogWarning($"out of range currentIndex={currentIndex}, newIndex={newIndex}, layers.Count={layers.Count}");
                return;
            }

            IndicatorLayer movingElement = layers[currentIndex];

            layers.RemoveAt(currentIndex);
            layers.Insert(newIndex, movingElement);
        }

    }

    [Serializable]
    public class IndicatorLayer
    {
        public enum ClampStyle { Rectangle, Laterals, Elipse, Centered, None }
        public ClampStyle clampStyle;

        [SerializeField] public List<Indicator> indicators = new();

        public float maxDistance = 100f;
        public float minDistance = 5f;
        public float offset;
        public float radiusFromCenter = 150;
        public float fadeSpeed = 3f;
        public bool unfold = false;
        public bool enabled = true;
        public float focusAngle = 360f;

        //-------- USER ---------//

        public string name;
        public int index;
        public int id;

    }

    [Serializable]
    public class Indicator
    {
        public Marker markerScript;
        public CanvasGroup canvasGroup;
        public RectTransform marker;
        public Transform target;

        public Vector3 targetDirection;
        public Vector3 screenPosition;
        public Vector3 freescreenPosition;
        public Vector2 screenEdgePosition;
        public Vector2 offset;


        public bool canTrack;
        public bool isOnEdge;
        public bool rawIsOnEdge;     
        public float targetAngleDirection;
       

        //-------- EDITOR ---------//

        public bool unfold = false;
        public int sectionIndex;

        //-------- USER ---------//

        public float distance;
        public bool enabled = true;
        public int layer_index;
        public int id;
        public int index;
        public bool isInRange;

        public void SetData(int layerIndex, int indicatorIndex)
        {
            index = indicatorIndex;
            layer_index = layerIndex;
        }
    }

    [Serializable]
    public class IndicatorReference
    {
        public IndicatorLayer IndicatorLayer { get; }
        public Indicator indicator { get; }

        public IndicatorReference(IndicatorLayer layer, Indicator ind)
        {
            IndicatorLayer = layer;
            indicator = ind;
        }
    }
}