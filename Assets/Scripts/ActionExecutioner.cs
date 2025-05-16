using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using EPOOutline;

public class ActionExecutioner : MonoBehaviour
{
    public class ActionCommand
    {
        public string ActionType;
        public Dictionary<string, string> Arguments = new Dictionary<string, string>();
        public string RawInput;
    }

    // For undo/redo functionality
    public class CommandState
    {
        public ActionCommand Command;
        public List<ObjectState> ObjectStates = new List<ObjectState>();
    }

    public class ObjectState
    {
        public GameObject GameObject;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public Color Color;
        public bool Exists;  // For creation/deletion tracking

        public static ObjectState CaptureState(GameObject obj)
        {
            if (obj == null) return null;

            ObjectState state = new ObjectState
            {
                GameObject = obj,
                Position = obj.transform.position,
                Rotation = obj.transform.rotation,
                Scale = obj.transform.localScale,
                Exists = true
            };

            // Capture color if there's a renderer
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                state.Color = renderer.material.color;
            }
            else
            {
                // Try to find renderer in children
                renderer = obj.GetComponentInChildren<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    state.Color = renderer.material.color;
                }
            }

            return state;
        }
    }

    public static ActionExecutioner Instance = null;
    
    // Command history for undo/redo
    private List<CommandState> commandHistory = new List<CommandState>();
    private int currentHistoryIndex = -1;
    private const int MAX_HISTORY_SIZE = 20;

    private Dictionary<string, Color> namedColors = new Dictionary<string, Color>()
    {
        { "red", new Color(1.0f, 0.0f, 0.0f, 1.0f) },
        { "green", new Color(0.0f, 1.0f, 0.0f, 1.0f) },
        { "blue", new Color(0.0f, 0.2f, 1.0f, 1.0f) },  // Slightly adjusted to be more visible
        { "yellow", new Color(1.0f, 0.9f, 0.0f, 1.0f) },
        { "white", new Color(1.0f, 1.0f, 1.0f, 1.0f) },
        { "black", new Color(0.0f, 0.0f, 0.0f, 1.0f) },
        { "gray", new Color(0.5f, 0.5f, 0.5f, 1.0f) },
        { "cyan", new Color(0.0f, 1.0f, 1.0f, 1.0f) },
        { "magenta", new Color(1.0f, 0.0f, 1.0f, 1.0f) },
        { "orange", new Color(1.0f, 0.6f, 0.0f, 1.0f) },
        { "purple", new Color(0.5f, 0.0f, 1.0f, 1.0f) }
    };

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) {
            Instance = this; 
        } else {
            Destroy(Instance);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Execute(string response) {
        ActionCommand command = ParseLLMResponse(response);
        
        // Capture state before executing the command
        CommandState commandState = new CommandState { Command = command };
        
        switch (command.ActionType)
        {
            case "selection":
                ExecuteSelection(command);
                break;
            case "translation":
                // Capture state of affected objects before translation
                List<GameObject> translationCandidates = FilterObjectsByCommonArgs(command);
                foreach (var obj in translationCandidates)
                {
                    commandState.ObjectStates.Add(ObjectState.CaptureState(obj));
                }
                ExecuteTranslation(command);
                break;
            case "rotation":
                // Capture state of affected objects before rotation
                List<GameObject> rotationCandidates = FilterObjectsByCommonArgs(command);
                foreach (var obj in rotationCandidates)
                {
                    commandState.ObjectStates.Add(ObjectState.CaptureState(obj));
                }
                ExecuteRotation(command);
                break;
            case "scale":
                // Capture state of affected objects before scaling
                List<GameObject> scaleCandidates = FilterObjectsByCommonArgs(command);
                foreach (var obj in scaleCandidates)
                {
                    commandState.ObjectStates.Add(ObjectState.CaptureState(obj));
                }
                ExecuteScale(command);
                break;
            case "color":
                // Capture state of affected objects before color change
                List<GameObject> colorCandidates = FilterObjectsByCommonArgs(command);
                foreach (var obj in colorCandidates)
                {
                    commandState.ObjectStates.Add(ObjectState.CaptureState(obj));
                }
                ExecuteColor(command);
                break;
            case "creation":
                // For creation, we'll capture the state after the object is created
                ExecuteCreation(command);
                
                // After creation, find and capture the newly created object
                // Get the object type from the command
                if (command.Arguments.TryGetValue("object_type", out string objectType))
                {
                    // Wait briefly for the object to be fully initialized
                    StartCoroutine(CaptureCreatedObjectState(commandState, objectType));
                }
                return; // Skip adding to history here, will be done in coroutine
            default:
                Debug.LogWarning($"Unknown action type: {command.ActionType}");
                return; // Don't add to history if command type is unknown
        }
        
        // Add to command history (only for non-creation commands)
        AddToHistory(commandState);
    }
    
    // Add a command to the history
    private void AddToHistory(CommandState commandState)
    {
        // If we're not at the end of history (user did undo), remove anything after current position
        if (currentHistoryIndex < commandHistory.Count - 1)
        {
            commandHistory.RemoveRange(currentHistoryIndex + 1, commandHistory.Count - currentHistoryIndex - 1);
        }
        
        // Add new command
        commandHistory.Add(commandState);
        currentHistoryIndex = commandHistory.Count - 1;
        
        // Trim history if it gets too long
        if (commandHistory.Count > MAX_HISTORY_SIZE)
        {
            commandHistory.RemoveAt(0);
            currentHistoryIndex--;
        }
        
        Debug.Log($"Added command to history. History size: {commandHistory.Count}, Current index: {currentHistoryIndex}");
    }
    
    // Undo the last action
    public void Undo()
    {
        if (currentHistoryIndex < 0 || commandHistory.Count == 0)
        {
            Debug.Log("Nothing to undo");
            return;
        }
        
        CommandState state = commandHistory[currentHistoryIndex];
        Debug.Log($"Undoing action: {state.Command.ActionType}");
        
        // Restore object states
        foreach (var objState in state.ObjectStates)
        {
            if (objState.GameObject != null)
            {
                Debug.Log($"Restoring state for: {objState.GameObject.name}");
                objState.GameObject.transform.position = objState.Position;
                objState.GameObject.transform.rotation = objState.Rotation;
                objState.GameObject.transform.localScale = objState.Scale;
                
                // Restore color if possible
                Renderer renderer = objState.GameObject.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = objState.Color;
                }
                else
                {
                    // Try children
                    var renderers = objState.GameObject.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                    {
                        if (r != null && r.material != null)
                        {
                            r.material.color = objState.Color;
                        }
                    }
                }
                
                // Handle creation/deletion
                if (state.Command.ActionType == "creation" && !objState.Exists)
                {
                    // If it was a creation command, destroy the object
                    Debug.Log($"Removing created object: {objState.GameObject.name}");
                    Destroy(objState.GameObject);
                }
            }
        }
        
        currentHistoryIndex--;
        Debug.Log($"Undo complete. Current history index: {currentHistoryIndex}");
    }
    
    // Redo the previously undone action
    public void Redo()
    {
        if (currentHistoryIndex >= commandHistory.Count - 1)
        {
            Debug.Log("Nothing to redo");
            return;
        }
        
        currentHistoryIndex++;
        CommandState state = commandHistory[currentHistoryIndex];
        Debug.Log($"Redoing action: {state.Command.ActionType}");
        
        // INSTEAD of re-executing the command, which might have parsing issues,
        // directly apply the inverse of the Undo operation
        if (state.ObjectStates != null && state.ObjectStates.Count > 0)
        {
            // For each affected object, restore to the "after" state
            foreach (var objState in state.ObjectStates)
            {
                if (objState.GameObject != null)
                {
                    // Handle object state changes based on action type
                    switch (state.Command.ActionType)
                    {
                        case "color":
                            // Apply the color that was set by the original command
                            Debug.Log($"Restoring color for: {objState.GameObject.name}");
                            if (state.Command.Arguments.TryGetValue("color", out string colorName))
                            {
                                if (TryGetColorFromName(colorName, out Color color))
                                {
                                    ColorObjectAndChildren(objState.GameObject, color);
                                    Debug.Log($"Applied color {colorName} to {objState.GameObject.name}");
                                }
                                else
                                {
                                    Debug.LogWarning($"Could not parse color name: {colorName}");
                                }
                            }
                            break;
                            
                        case "translation":
                            // Use the position that was originally set
                            if (state.Command.Arguments.TryGetValue("direction", out string dirStr) &&
                                state.Command.Arguments.TryGetValue("distance", out string distStr))
                            {
                                float distance = 1f;
                                float.TryParse(distStr.Replace("meters", "").Trim(), out distance);
                                Vector3 direction = ParseDirection(dirStr);
                                objState.GameObject.transform.position += direction.normalized * distance;
                            }
                            break;
                            
                        case "rotation":
                            // Use the rotation that was originally set
                            if (state.Command.Arguments.TryGetValue("axis", out string axisStr) &&
                                state.Command.Arguments.TryGetValue("angle", out string angleStr))
                            {
                                float angle = 90f;
                                float.TryParse(angleStr.Replace("degrees", "").Trim(), out angle);
                                Vector3 axis = ParseAxis(axisStr);
                                objState.GameObject.transform.Rotate(axis, angle);
                            }
                            break;
                            
                        case "scale":
                            // Use the scale that was originally set
                            if (state.Command.Arguments.TryGetValue("scale_factor", out string factorStr))
                            {
                                if (float.TryParse(factorStr, out float factor))
                                {
                                    Vector3 scale = Vector3.one;
                                    
                                    if (state.Command.Arguments.TryGetValue("axis", out string scaleAxisStr))
                                    {
                                        Vector3 axis = ParseAxis(scaleAxisStr);
                                        scale += axis * (factor - 1);
                                    }
                                    else
                                    {
                                        scale *= factor;
                                    }
                                    
                                    objState.GameObject.transform.localScale = Vector3.Scale(objState.GameObject.transform.localScale, scale);
                                }
                            }
                            break;
                            
                        // Handle other action types as needed
                        default:
                            Debug.LogWarning($"Action type '{state.Command.ActionType}' not implemented for direct redo");
                            break;
                    }
                }
            }
        }
        
        Debug.Log($"Redo complete. Current history index: {currentHistoryIndex}");
    }
    
    // Check if undo is available
    public bool CanUndo()
    {
        return currentHistoryIndex >= 0;
    }
    
    // Check if redo is available
    public bool CanRedo()
    {
        return currentHistoryIndex < commandHistory.Count - 1;
    }

    public void ExecuteTranslation(ActionCommand actionCommand)
    {
        // ensure required argument exists
        if (!actionCommand.Arguments.ContainsKey("direction"))
        {
            Debug.LogWarning("Missing 'direction' for translation.");
            return;
        }

        // initial filter based on common arguments 
        List<GameObject> candidates = FilterObjectsByCommonArgs(actionCommand);

        // parse direction argument 
        Vector3 direction = ParseDirection(actionCommand.Arguments["direction"]);

        // parse distance argument 
        float distance = 1f;
        if (actionCommand.Arguments.TryGetValue("distance", out string distStr))
        {
            float.TryParse(distStr.Replace("meters", "").Trim(), out distance);
        }

        foreach (var obj in candidates)
        {
            obj.transform.position += direction.normalized * distance;
        }
    }

    public void ExecuteSelection(ActionCommand actionCommand) {
        List<GameObject> list_temp = new List<GameObject>();
        List<GameObject> candidates = FilterObjectsByCommonArgs(actionCommand);

        foreach (var obj in SelectorManager.Instance.currentTargets)
        {
            if (!candidates.Contains(obj))
            {
                obj.GetComponent<ObjectController>()?.ToggleHighlight();
                list_temp.Add(obj);
            }
        }

        foreach (var obj in list_temp) {
            SelectorManager.Instance.RemoveFromSelection(obj);
        }
    }

    public void ExecuteRotation(ActionCommand actionCommand)
    {
        // ensure required argument exists
        if (!actionCommand.Arguments.ContainsKey("axis"))
        {
            Debug.LogWarning("Missing 'axis' for rotation.");
            return;
        }

        // intial object filtering 
        List<GameObject> candidates = FilterObjectsByCommonArgs(actionCommand);

        // parse axis argument 
        Vector3 axis = ParseAxis(actionCommand.Arguments["axis"]);

        // parse angle argument 
        float angle = 90f;
        if (actionCommand.Arguments.TryGetValue("angle", out string angleStr))
        {
            float.TryParse(angleStr.Replace("degrees", "").Trim(), out angle);
        }

        foreach (var obj in candidates)
        {
            obj.transform.Rotate(axis, angle);
        }
    }

    public void ExecuteScale(ActionCommand actionCommand)
    {
        // parse scale factor argument 
        Vector3 scale = Vector3.one;
        
        if (actionCommand.Arguments.TryGetValue("scale_factor", out string factorStr) &&
            float.TryParse(factorStr, out float factor))
        {
            // check if a specific axis is specified 
            if (actionCommand.Arguments.TryGetValue("axis", out string axisStr))
            {
                Vector3 axis = ParseAxis(axisStr);
                scale += axis * (factor - 1);
            }
            else // uniform scale
            {
                scale *= factor;
            }
        }

        List<GameObject> candidates = FilterObjectsByCommonArgs(actionCommand);

        foreach (var obj in candidates)
        {
            obj.transform.localScale = Vector3.Scale(obj.transform.localScale, scale);
        }
    }

    public void ExecuteColor(ActionCommand actionCommand)
    {
        Debug.Log("🎨 ExecuteColor: Starting color change process");
        
        // ensure required argument exists
        if (!actionCommand.Arguments.ContainsKey("color"))
        {
            Debug.LogWarning("🚫 Missing 'color' for color change.");
            return;
        }
        
        // parse color argument
        string colorName = actionCommand.Arguments["color"].ToLower();
        
        // Apply color name correction for speech recognition errors
        colorName = CorrectMisrecognizedColor(colorName);
        
        if (!TryGetColorFromName(colorName, out Color newColor))
        {
            Debug.LogWarning($"🚫 Invalid color name: {colorName}");
            return;
        }
        
        // Get candidates - should be a single building if we're using camera-based selection
        List<GameObject> candidates = FilterObjectsByCommonArgs(actionCommand);
        
        if (candidates.Count == 0)
        {
            Debug.LogWarning("🚫 No objects found to change color!");
            return;
        }

        Debug.Log($"🎯 Found {candidates.Count} candidates for color change");
        
        foreach (var obj in candidates)
        {
            Debug.Log($"🔄 Coloring object: {obj.name}");
            ColorObjectAndChildren(obj, newColor);
        }
        
        Debug.Log($"🏁 Color change to {colorName} complete");
    }
    
    // New helper method to recursively color an object and all its children
    private void ColorObjectAndChildren(GameObject obj, Color color)
    {
        int coloredCount = 0;
        HashSet<Material> processedMaterials = new HashSet<Material>();
        
        // Make the color more vivid/saturated for better visibility
        Color enhancedColor = new Color(
            Mathf.Clamp01(color.r * 1.3f), 
            Mathf.Clamp01(color.g * 1.3f), 
            Mathf.Clamp01(color.b * 1.3f), 
            1.0f);  // Full opacity
        
        // Process all renderers in this object and its children
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"Found {renderers.Length} renderers in {obj.name} and its children");
        
        foreach (var renderer in renderers)
        {
            // Color all materials in the renderer
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material material = mats[i];
                // Avoid processing the same material multiple times
                if (!processedMaterials.Contains(material))
                {
                    material.color = enhancedColor;
                    
                    // Also set emission for more visible effect
                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", enhancedColor * 0.3f);
                    }
                    
                    // Apply change back to the renderer
                    renderer.materials = mats;
                    
                    processedMaterials.Add(material);
                    coloredCount++;
                    
                    Debug.Log($"✅ Colored material on {renderer.gameObject.name} (part of {obj.name})");
                }
            }
        }
        
        Debug.Log($"Total materials colored for {obj.name}: {coloredCount}");
    }

    private Vector3 ParseAxis(string axis)
    {
        axis = axis.ToLower();
        if (axis == "x") return Vector3.right;
        if (axis == "y") return Vector3.up;
        if (axis == "z") return Vector3.forward;
        return Vector3.zero;
    }

    private Vector3 ParseDirection(string dir)
    {
        dir = dir.ToLower();
        if (dir.Contains("forward") || dir.Contains("further") || dir.Contains("farther")) return Camera.main.transform.forward;
        if (dir.Contains("back") || dir.Contains("close") || dir.Contains("near")) return -Camera.main.transform.forward;
        if (dir.Contains("left")) return -Camera.main.transform.right;
        if (dir.Contains("right")) return Camera.main.transform.right;
        if (dir.Contains("up")) return Vector3.up;
        if (dir.Contains("down")) return Vector3.down;
        return Vector3.zero;
    }

    private List<GameObject> FilterObjectsByCommonArgs(ActionCommand actionCommand)
    {
        string objectType = actionCommand.Arguments["object_type"];
        
        // Apply speech recognition correction
        objectType = CorrectMisrecognizedObjectType(objectType);
        actionCommand.Arguments["object_type"] = objectType;
        
        Debug.Log($"🔍 FilterObjectsByCommonArgs - Finding objects of type: {objectType}");

        // Analyze the raw input if available to detect if it's a generic command
        bool hasSpecificQualifier = false;
        bool isGenericCommand = true;
        
        // Check if command has specific qualifiers in the arguments
        hasSpecificQualifier = actionCommand.Arguments.ContainsKey("location") || 
                              actionCommand.Arguments.ContainsKey("quantity") ||
                              actionCommand.Arguments.ContainsKey("size") ||
                              actionCommand.Arguments.ContainsKey("name");
        
        // Special case: for color commands, the 'color' argument doesn't count as a qualifier
        // because it's the new color, not a selection filter
        if (actionCommand.ActionType.ToLower() == "color" && 
            actionCommand.Arguments.ContainsKey("color"))
        {
            hasSpecificQualifier = false;
        }
        
        // If the command has "the" before the object type, it's likely specific
        string rawInput = actionCommand.RawInput?.ToLower() ?? "";
        bool hasThePrefix = rawInput.Contains("the " + objectType.ToLower()) || 
                           rawInput.Contains("this " + objectType.ToLower());
        
        isGenericCommand = !hasSpecificQualifier && !hasThePrefix;
                               
        bool useCameraBasedSelection = !isGenericCommand && 
                                      (actionCommand.ActionType.ToLower() == "color" || 
                                       actionCommand.ActionType.ToLower() == "rotation");
        
        Debug.Log($"Command analysis: RawInput='{rawInput}', Generic={isGenericCommand}, HasThe={hasThePrefix}, Camera-based={useCameraBasedSelection}");
        
        List<GameObject> candidates = new List<GameObject>();
        
        // For generic commands like "color all buildings red" or "color cars blue", find all matching objects
        if (isGenericCommand)
        {
            Debug.Log($"🌟 Generic command detected - will find all {objectType}s");
            
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (MatchesObjectType(obj, objectType))
                {
                    candidates.Add(obj);
                    Debug.Log($"  + Added {objectType}: {obj.name}");
                }
            }
            
            Debug.Log($"Found {candidates.Count} {objectType}s for generic command");
        }
        // Use camera-based selection for specific commands
        else if (useCameraBasedSelection)
        {
            Debug.Log($"🎯 Using camera-based selection for {objectType}");
            
            // Try to find the most relevant object (in front of the camera or closest to center of view)
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Get all potential objects of the requested type
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                List<GameObject> potentialObjects = new List<GameObject>();
                
                // First collect all objects matching the type (by name, tag, etc.)
                foreach (var obj in allObjects)
                {
                    // Match by name, tag, or component name
                    if (obj.name.ToLower().Contains(objectType.ToLower()) || 
                        obj.CompareTag(objectType) || 
                        ComponentNameMatches(obj, objectType))
                    {
                        potentialObjects.Add(obj);
                        Debug.Log($"+ Found potential {objectType}: {obj.name}");
                    }
                }
                
                Debug.Log($"Found {potentialObjects.Count} potential {objectType}s to filter");
                
                if (potentialObjects.Count > 0)
                {
                    // Calculate which object is most likely in focus
                    GameObject bestObject = null;
                    float bestScore = float.MinValue;
                    
                    Vector3 cameraPosition = mainCamera.transform.position;
                    Vector3 cameraForward = mainCamera.transform.forward;
                    
                    foreach (var obj in potentialObjects)
                    {
                        // Skip very small objects
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer == null && obj.GetComponentInChildren<Renderer>() == null)
                            continue;
                            
                        // Get direction to object
                        Vector3 dirToObject = (obj.transform.position - cameraPosition).normalized;
                        
                        // Calculate dot product (higher values mean more directly in front)
                        float forwardAlignment = Vector3.Dot(cameraForward, dirToObject);
                        
                        // Calculate distance (closer is better)
                        float distance = Vector3.Distance(cameraPosition, obj.transform.position);
                        
                        // Screen position - objects in center of view get bonus points
                        Vector3 screenPos = mainCamera.WorldToViewportPoint(obj.transform.position);
                        float centeralignment = 0;
                        if (screenPos.z > 0) // Only if in front of camera
                        {
                            centeralignment = 1f - 2f * Vector2.Distance(
                                new Vector2(screenPos.x, screenPos.y), 
                                new Vector2(0.5f, 0.5f));
                        }
                        
                        // Combined score: favor objects that are:
                        // 1. Directly in front of camera (forwardAlignment)
                        // 2. Close to the center of the screen (centeralignment)
                        // 3. Not too far away (distance)
                        float score = (forwardAlignment * 5) + (centeralignment * 10) - (distance * 0.1f);
                        
                        Debug.Log($"{objectType} {obj.name}: alignment={forwardAlignment:F2}, center={centeralignment:F2}, distance={distance:F2}, score={score:F2}");
                        
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestObject = obj;
                        }
                    }
                    
                    if (bestObject != null)
                    {
                        candidates.Add(bestObject);
                        Debug.Log($"✅ Selected best {objectType}: {bestObject.name} with score {bestScore:F2}");
                    }
                }
                else
                {
                    Debug.Log($"⚠️ No {objectType}s found");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Main camera not found");
                
                // Fallback: try to find objects by tag/name
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name.ToLower().Contains(objectType.ToLower()) || obj.CompareTag(objectType))
                    {
                        candidates.Add(obj);
                        Debug.Log($"  + Added {objectType} by name/tag (fallback): {obj.name}");
                    }
                }
            }
        }
        else
        {
            // Normal case: use SelectorManager and tags
            Debug.Log($"🔍 Using tag-based filtering: tag={objectType}");
            
            // Step 1: Gather all matching objects by tag
            foreach (var obj in SelectorManager.Instance.currentTargets)
            {
                if (obj.CompareTag(objectType))
                {
                    candidates.Add(obj);
                    Debug.Log($"  + Added object by tag: {obj.name}");
                }
            }
        }
        
        // Apply optional filters (color, location, quantity)
        ApplyOptionalFilters(actionCommand, ref candidates);
        
        Debug.Log($"🏁 Final candidates: {candidates.Count} for {actionCommand.ActionType}");
        return candidates;
    }
    
    // Helper method to check if any component name matches the object type
    private bool ComponentNameMatches(GameObject obj, string typeName)
    {
        Component[] components = obj.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component != null && component.GetType().Name.ToLower().Contains(typeName.ToLower()))
                return true;
        }
        return false;
    }
    
    // Helper to check if an object matches the requested type
    private bool MatchesObjectType(GameObject obj, string typeName)
    {
        bool matches = false;
        string reasons = "";
        
        // Match by name
        if (obj.name.ToLower().Contains(typeName.ToLower()))
        {
            matches = true;
            reasons += "name;";
        }
            
        // Match by tag
        if (obj.CompareTag(typeName))
        {
            matches = true;
            reasons += "tag;";
        }
            
        // Match by component name
        if (ComponentNameMatches(obj, typeName))
        {
            matches = true;
            reasons += "component;";
        }
        
        // Special case for cars
        if (typeName.ToLower() == "car" || typeName.ToLower() == "cars")
        {
            // Check for car-like names
            if (obj.name.ToLower().Contains("car") || 
                obj.name.ToLower().Contains("auto") || 
                obj.name.ToLower().Contains("vehicle") ||
                obj.name.ToLower().Contains("truck") ||
                obj.name.ToLower().Contains("van"))
            {
                matches = true;
                reasons += "car-name;";
            }
            
            // Has car-like structure (check for wheels, etc.)
            var childCount = obj.transform.childCount;
            if (childCount >= 4)
            {
                int wheelCount = 0;
                foreach (Transform child in obj.transform)
                {
                    if (child.name.ToLower().Contains("wheel") || 
                        child.name.ToLower().Contains("tire"))
                    {
                        wheelCount++;
                    }
                }
                
                if (wheelCount >= 3)
                {
                    matches = true;
                    reasons += "wheels;";
                }
            }
        }
        
        // Skip objects without renderers unless already matched
        if (!matches && obj.GetComponent<Renderer>() == null && obj.GetComponentInChildren<Renderer>() == null)
        {
            return false;
        }
        
        // For debugging
        if (matches)
        {
            Debug.Log($"Object {obj.name} matched {typeName} by: {reasons}");
        }
            
        return matches;
    }
    
    // Apply optional filters like color, location, quantity
    private void ApplyOptionalFilters(ActionCommand actionCommand, ref List<GameObject> candidates)
    {
        // Filter by color if specified
        if (actionCommand.Arguments.TryGetValue("color", out string colorFilter))
        {
            Debug.Log($"🔍 Filtering by color: {colorFilter}");
            if (TryGetColorFromName(colorFilter, out Color expectedColor))
            {
                const float colorTolerance = 0.1f; // Allow slight variation
                
                // For color actions, we skip the color filter (we're changing the color, not selecting by current color)
                if (actionCommand.ActionType.ToLower() != "color")
                {
                    candidates = candidates.FindAll(obj =>
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer == null) return false;

                        Color actualColor = renderer.material.color;
                        bool matched = ColorsApproximatelyEqual(actualColor, expectedColor, colorTolerance);
                        Debug.Log($"  - Color check for {obj.name}: {(matched ? "✅ matched" : "❌ didn't match")}");
                        return matched;
                    });
                }
            }
        }

        // Filter by location if specified
        if (actionCommand.Arguments.TryGetValue("location", out string location))
        {
            Debug.Log($"🔍 Filtering by location: {location}");
            candidates = ApplySpatialFilter(candidates, location);

            // If location is specified, limit to 1 unless quantity is explicitly set
            if (!actionCommand.Arguments.ContainsKey("quantity")) {
                actionCommand.Arguments["quantity"] = "1";
            }
        }

        // Limit by quantity if specified
        if (actionCommand.Arguments.TryGetValue("quantity", out string quantityStr) && int.TryParse(quantityStr, out int quantity))
        {
            Debug.Log($"🔍 Limiting to quantity: {quantity}");
            candidates = candidates.Take(quantity).ToList();
        }
    }

    private List<GameObject> ApplySpatialFilter(List<GameObject> objects, string filter)
    {
        Transform player = Camera.main.transform; 
        string lower = filter.ToLower();

        if (lower.Contains("left"))
        {
            return objects.OrderByDescending(obj =>
                Vector3.Dot((obj.transform.position - player.position).normalized, -player.right)).ToList();
        }
        else if (lower.Contains("right"))
        {
            return objects.OrderByDescending(obj =>
                Vector3.Dot((obj.transform.position - player.position).normalized, player.right)).ToList();
        }
        else if (lower.Contains("closest") || lower.Contains("nearest"))
        {
            return objects.OrderBy(obj =>
                Vector3.Distance(obj.transform.position, player.position)).ToList();
        }
        else if (lower.Contains("farthest") || lower.Contains("furthest"))
        {
            return objects.OrderByDescending(obj =>
                Vector3.Distance(obj.transform.position, player.position)).ToList();
        }
        else if (lower.Contains("highest") || lower.Contains("top"))
        {
            return objects.OrderByDescending(obj =>
                obj.transform.position.y).ToList();
        }
        else if (lower.Contains("lowest") || lower.Contains("bottom"))
        {
            return objects.OrderBy(obj =>
                obj.transform.position.y).ToList();
        }
        return objects;
    }

    private bool TryGetColorFromName(string name, out Color color)
    {
        name = name.ToLower();
        return namedColors.TryGetValue(name, out color);
    }

    private bool ColorsApproximatelyEqual(Color a, Color b, float tolerance)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
            Mathf.Abs(a.g - b.g) < tolerance &&
            Mathf.Abs(a.b - b.b) < tolerance;
    }

    public ActionCommand ParseLLMResponse(string response)
    {
        var command = new ActionCommand();
        command.RawInput = response;  // Store the full raw response
        
        // Look for original command in comment format
        string[] lines = response.Split('\n');
        string originalCommand = "";
        if (lines.Length > 0 && lines[0].StartsWith("// Original command:"))
        {
            originalCommand = lines[0].Replace("// Original command:", "").Trim();
            command.RawInput = originalCommand;  // Update to use just the voice command
            Debug.Log($"📢 Original voice command: '{originalCommand}'");
        }
        
        // Process the response lines
        int num = 0; 
        foreach (var line in lines)
        {
            // Skip comment lines
            if (line.StartsWith("//")) continue;
            
            num++; 
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (num == 1 && !line.Contains("action_type")) {
                if (line.Contains("select")) {
                    command.ActionType = "selection"; 
                } else if (line.Contains("rotation")) {
                    command.ActionType = "rotation";
                } else if (line.Contains("translation")) {
                    command.ActionType = "translation";
                } else if (line.Contains("scale")) {
                    command.ActionType = "scale";
                } else if (line.Contains("color")) {
                    command.ActionType = "color";
                } else if (line.Contains("create") || line.Contains("generate") || line.Contains("make") || line.Contains("add")) {
                    command.ActionType = "creation";
                }
                continue; 
            }
            var parts = line.Split(new[] { ':' }, 2);
            if (parts.Length != 2) continue;

            string key = parts[0].Trim().ToLower();
            string value = parts[1].Trim().ToLower();

            if (key == "action_type")
                command.ActionType = value;
            else
                command.Arguments[key] = value;
        }
        
        // Log what we've extracted for debugging
        Debug.Log($"Parsed Command: Type={command.ActionType}, Args={string.Join(", ", command.Arguments.Select(kv => $"{kv.Key}={kv.Value}"))}");
        
        return command;
    }

    // New method to handle object creation
    public void ExecuteCreation(ActionCommand actionCommand)
    {
        Debug.Log("🏗️ ExecuteCreation: Starting object creation process");
        
        // Get the object type to create
        if (!actionCommand.Arguments.ContainsKey("object_type"))
        {
            Debug.LogWarning("🚫 Missing 'object_type' for creation.");
            return;
        }
        
        string objectType = actionCommand.Arguments["object_type"].ToLower();
        Debug.Log($"Creating object of type: {objectType}");
        
        // Extract color if specified in the command
        Color? initialColor = null;
        if (actionCommand.Arguments.TryGetValue("color", out string colorName))
        {
            if (TryGetColorFromName(colorName, out Color color))
            {
                initialColor = color;
                Debug.Log($"Will create {objectType} with initial color: {colorName}");
            }
        }
        
        // Find reference object if specified
        GameObject referenceObject = null;
        if (actionCommand.Arguments.ContainsKey("reference_type"))
        {
            string referenceType = actionCommand.Arguments["reference_type"].ToLower();
            Debug.Log($"Looking for reference object of type: {referenceType}");
            
            // Try to find the reference object (selected or by camera)
            List<GameObject> referenceObjects = new List<GameObject>();
            
            // First check if there's a selected object
            if (SelectorManager.Instance != null && SelectorManager.Instance.currentTargets.Count > 0)
            {
                foreach (var obj in SelectorManager.Instance.currentTargets)
                {
                    if (MatchesObjectType(obj, referenceType))
                    {
                        referenceObjects.Add(obj);
                        Debug.Log($"Found selected reference object: {obj.name}");
                    }
                }
            }
            
            // If no selected object matches, use camera-based selection
            if (referenceObjects.Count == 0)
            {
                Camera mainCamera = Camera.main;
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                
                // Get all matching reference objects
                foreach (var obj in allObjects)
                {
                    if (MatchesObjectType(obj, referenceType))
                    {
                        referenceObjects.Add(obj);
                    }
                }
                
                // Find the most relevant (in view, close to center)
                if (referenceObjects.Count > 0 && mainCamera != null)
                {
                    float bestScore = float.MinValue;
                    foreach (var obj in referenceObjects)
                    {
                        Vector3 screenPos = mainCamera.WorldToViewportPoint(obj.transform.position);
                        float centerDistance = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), new Vector2(0.5f, 0.5f));
                        float score = -centerDistance; // Higher score = closer to center
                        
                        if (screenPos.z > 0 && score > bestScore) // Object is in front of camera
                        {
                            bestScore = score;
                            referenceObject = obj;
                        }
                    }
                }
            }
            else
            {
                // Use the first selected object that matches
                referenceObject = referenceObjects[0];
            }
            
            if (referenceObject == null)
            {
                Debug.LogWarning($"⚠️ No reference object of type {referenceType} found.");
            }
            else
            {
                Debug.Log($"✅ Selected reference object: {referenceObject.name}");
            }
        }
        
        // Calculate position for new object
        Vector3 spawnPosition = Vector3.zero;
        
        if (referenceObject != null)
        {
            // Get position relative to reference object
            spawnPosition = CalculatePositionRelativeToReference(
                referenceObject, 
                actionCommand.Arguments.TryGetValue("position", out string pos) ? pos : "beside",
                actionCommand.Arguments.TryGetValue("distance", out string distStr) ? ParseFloat(distStr, 2f) : 2f
            );
        }
        else
        {
            // Default position is in front of the camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 3f;
                spawnPosition.y = 0; // Place on ground level
            }
        }
        
        // Create the object with the specified initial color
        GameObject newObject = CreateObject(objectType, spawnPosition, initialColor);
        if (newObject != null)
        {
            // Apply optional properties (except color, which is already applied during creation)
            
            // Scale
            if (actionCommand.Arguments.TryGetValue("scale", out string scaleStr))
            {
                float scale = ParseFloat(scaleStr, 1f);
                newObject.transform.localScale *= scale;
                Debug.Log($"✅ Applied scale {scale} to new {objectType}");
            }
            
            // Make sure all required components are fully initialized
            StartCoroutine(SelectAfterCreation(newObject, objectType));
            
            Debug.Log($"🎉 Successfully created {objectType} at position {spawnPosition}");
        }
    }
    
    // Coroutine to select the object after a short delay to ensure components are initialized
    private IEnumerator SelectAfterCreation(GameObject newObject, string objectType)
    {
        // Wait for a frame to let components initialize
        yield return null;
        
        if (SelectorManager.Instance != null)
        {
            // Clear the current selection by removing all objects
            try {
                List<GameObject> objectsToRemove = new List<GameObject>(SelectorManager.Instance.currentTargets);
                foreach (var obj in objectsToRemove)
                {
                    // Make sure we have an ObjectController and it's initialized
                    ObjectController objController = obj.GetComponent<ObjectController>();
                    if (objController != null)
                    {
                        try
                        {
                            objController.ToggleHighlight();
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"Error toggling highlight on {obj.name}: {e.Message}");
                        }
                    }
                    SelectorManager.Instance.RemoveFromSelection(obj);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error clearing selection: {e.Message}");
            }
            
            // Add the new object to selection after ensuring components are properly set up
            ObjectController newObjController = newObject.GetComponent<ObjectController>();
            if (newObjController == null)
            {
                newObjController = newObject.AddComponent<ObjectController>();
            }
            
            // Give a moment for Start to run
            yield return null;
            
            try
            {
                // Make sure the object has an Outlinable component
                EPOOutline.Outlinable outlinable = newObject.GetComponent<EPOOutline.Outlinable>();
                if (outlinable == null)
                {
                    outlinable = newObject.AddComponent<EPOOutline.Outlinable>();
                }
                outlinable.enabled = true; // Directly enable outline instead of toggling
                
                // Add to selection
                SelectorManager.Instance.AddToSelection(newObject);
                Debug.Log($"✅ Selected newly created {objectType}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error selecting newly created object: {e.Message}");
            }
        }
    }
    
    // Create an object of the specified type with an optional initial color
    private GameObject CreateObject(string objectType, Vector3 position, Color? initialColor = null)
    {
        GameObject prefab = null;
        
        // First try to find an existing prefab by tag - only if the tag exists
        if (TagExists(objectType.ToLower()))
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(objectType.ToLower());
            if (taggedObjects.Length > 0)
            {
                Debug.Log($"Found {taggedObjects.Length} objects with tag '{objectType.ToLower()}'");
                // Use the first one as our template
                prefab = taggedObjects[0];
                
                // Instantiate a new object based on the template
                GameObject obj = GameObject.Instantiate(prefab, position, Quaternion.identity);
                obj.name = objectType.ToLower();
                
                // Apply initial color if specified
                if (initialColor.HasValue)
                {
                    // Apply color to all renderers
                    foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                    {
                        ApplyColorToRenderer(renderer, initialColor.Value);
                    }
                    
                    // Update VoiceControllable component with the color name
                    var voiceControl = obj.GetComponent<VoiceControllable>();
                    if (voiceControl != null)
                    {
                        voiceControl.color = GetColorName(initialColor.Value);
                    }
                }
                
                Debug.Log($"Successfully created {objectType} from tagged template");
                
                // Set tag appropriately
                try {
                    // Try to set the specific object type tag if it exists in the tag manager
                    if (TagExists(objectType.ToLower()))
                    {
                        obj.tag = objectType.ToLower();
                        Debug.Log($"Tagged object as '{objectType.ToLower()}'");
                    }
                    else
                    {
                        // Default to Interactable tag
                        obj.tag = "Interactable";
                    }
                }
                catch (UnityException)
                {
                    // If tag setting fails, use Interactable as fallback
                    obj.tag = "Interactable";
                }
                
                // Add ObjectController for highlighting and selection
                if (!obj.GetComponent<ObjectController>())
                {
                    var objController = obj.AddComponent<ObjectController>();
                    
                    // If SelectorManager is not available, make the object not selectable by disabling XR interactable
                    var interactable = obj.GetComponent<XRGrabInteractable>();
                    if (SelectorManager.Instance == null && interactable != null)
                    {
                        Debug.LogWarning($"SelectorManager not available, temporarily disabling interactable on {obj.name}");
                        interactable.enabled = false;
                        
                        // Start a coroutine to enable it later when SelectorManager might be available
                        StartCoroutine(EnableInteractionWhenReady(interactable));
                    }
                }
                
                // Place at position
                obj.transform.position = position;
                
                return obj;
            }
        }
        
        // If no tagged object found, try Resources folder
        if (prefab == null)
        {
            try
            {
                prefab = Resources.Load<GameObject>($"Prefabs/{objectType}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Error loading prefab: {e.Message}");
            }
        }
        
        // If still no prefab found, create a primitive
        if (prefab == null)
        {
            Debug.Log($"No prefab found for {objectType}, creating primitive");
            
            GameObject obj;
            GameObject body;
            GameObject windshield;
            Renderer windshieldRenderer;
            
            switch (objectType.ToLower())
            {
                case "cube":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case "sphere":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case "cylinder":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                case "capsule":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    break;
                case "plane":
                    obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    break;
                case "suv":
                    // Create a simple SUV from primitives
                    obj = new GameObject("SUV");
                    
                    // Body - make it taller and shorter than car
                    body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    body.transform.SetParent(obj.transform);
                    body.transform.localPosition = new Vector3(0, 0.7f, 0);
                    body.transform.localScale = new Vector3(2.2f, 1.4f, 3.5f);
                    body.name = "Body";
                    
                    // Wheels
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        wheel.transform.SetParent(obj.transform);
                        wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                        
                        float x = (i % 2 == 0) ? -1.2f : 1.2f;
                        float z = (i < 2) ? 1f : -1f;
                        wheel.transform.localPosition = new Vector3(x, 0.4f, z);
                        wheel.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
                        wheel.name = $"Wheel_{i}";
                        
                        // Apply black color to wheels
                        var wheelRenderer = wheel.GetComponent<Renderer>();
                        if (wheelRenderer != null)
                        {
                            wheelRenderer.material.color = Color.black;
                        }
                    }
                    
                    // SUV has a more uniform body (less separate cabin)
                    GameObject roofrack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    roofrack.transform.SetParent(obj.transform);
                    roofrack.transform.localPosition = new Vector3(0, 1.9f, 0);
                    roofrack.transform.localScale = new Vector3(2f, 0.2f, 3f);
                    roofrack.name = "RoofRack";
                    
                    // Windows (make them darker colored)
                    windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    windshield.transform.SetParent(obj.transform);
                    windshield.transform.localPosition = new Vector3(0, 1.3f, 1.2f);
                    windshield.transform.localScale = new Vector3(2f, 0.8f, 0.1f);
                    windshield.name = "Windshield";
                    
                    windshieldRenderer = windshield.GetComponent<Renderer>();
                    if (windshieldRenderer != null)
                    {
                        windshieldRenderer.material.color = new Color(0.1f, 0.1f, 0.3f, 0.8f);
                    }
                    
                    // Set tag to 'suv'
                    obj.tag = "suv";
                    
                    // Apply the initial color to body and roofrack if specified
                    if (initialColor.HasValue)
                    {
                        ApplyColorToRenderer(body.GetComponent<Renderer>(), initialColor.Value);
                        ApplyColorToRenderer(roofrack.GetComponent<Renderer>(), initialColor.Value);
                    }
                    break;
                case "car":
                    // Create a simple car from primitives
                    obj = new GameObject("Car");
                    
                    // Body
                    body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    body.transform.SetParent(obj.transform);
                    body.transform.localPosition = new Vector3(0, 0.5f, 0);
                    body.transform.localScale = new Vector3(2f, 1f, 4f);
                    body.name = "Body";
                    
                    // Wheels
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        wheel.transform.SetParent(obj.transform);
                        wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                        
                        float x = (i % 2 == 0) ? -1.1f : 1.1f;
                        float z = (i < 2) ? 1f : -1f;
                        wheel.transform.localPosition = new Vector3(x, 0.3f, z);
                        wheel.transform.localScale = new Vector3(0.6f, 0.2f, 0.6f);
                        wheel.name = $"Wheel_{i}";
                        
                        // Apply black color to wheels
                        var wheelRenderer = wheel.GetComponent<Renderer>();
                        if (wheelRenderer != null)
                        {
                            wheelRenderer.material.color = Color.black;
                        }
                    }
                    
                    // Cabin
                    GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cabin.transform.SetParent(obj.transform);
                    cabin.transform.localPosition = new Vector3(0, 1.25f, 0.5f);
                    cabin.transform.localScale = new Vector3(1.8f, 0.5f, 1.5f);
                    cabin.name = "Cabin";
                    
                    // Windows (make them darker colored)
                    windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    windshield.transform.SetParent(obj.transform);
                    windshield.transform.localPosition = new Vector3(0, 1.25f, 1.3f);
                    windshield.transform.localScale = new Vector3(1.7f, 0.4f, 0.1f);
                    windshield.name = "Windshield";
                    
                    windshieldRenderer = windshield.GetComponent<Renderer>();
                    if (windshieldRenderer != null)
                    {
                        windshieldRenderer.material.color = new Color(0.1f, 0.1f, 0.3f, 0.8f);
                    }
                    
                    // Set tag to 'car'
                    obj.tag = "car";
                    
                    // Apply the initial color to body and cabin if specified
                    if (initialColor.HasValue)
                    {
                        ApplyColorToRenderer(body.GetComponent<Renderer>(), initialColor.Value);
                        ApplyColorToRenderer(cabin.GetComponent<Renderer>(), initialColor.Value);
                    }
                    break;
                case "tree":
                    // Create a simple tree from primitives
                    obj = new GameObject("Tree");
                    
                    // Trunk
                    GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    trunk.transform.SetParent(obj.transform);
                    trunk.transform.localPosition = new Vector3(0, 1.5f, 0);
                    trunk.transform.localScale = new Vector3(0.5f, 3f, 0.5f);
                    trunk.name = "Trunk";
                    
                    // Apply brown color to trunk
                    Renderer trunkRenderer = trunk.GetComponent<Renderer>();
                    if (trunkRenderer != null)
                    {
                        trunkRenderer.material.color = new Color(0.5f, 0.25f, 0f);
                    }
                    
                    // Leaves
                    GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    leaves.transform.SetParent(obj.transform);
                    leaves.transform.localPosition = new Vector3(0, 4f, 0);
                    leaves.transform.localScale = new Vector3(3f, 3f, 3f);
                    leaves.name = "Leaves";
                    
                    // Apply green color to leaves unless a specific color is requested
                    Renderer leavesRenderer = leaves.GetComponent<Renderer>();
                    if (leavesRenderer != null)
                    {
                        if (initialColor.HasValue)
                        {
                            leavesRenderer.material.color = initialColor.Value;
                        }
                        else
                        {
                            leavesRenderer.material.color = Color.green;
                        }
                    }
                    
                    // Set tag to 'tree'
                    obj.tag = "tree";
                    break;
                default:
                    // Default to a cube for unknown types
                    obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    
                    // Apply initial color if specified
                    if (initialColor.HasValue)
                    {
                        ApplyColorToRenderer(obj.GetComponent<Renderer>(), initialColor.Value);
                    }
                    break;
            }
            
            // Set the name
            obj.name = objectType.ToLower();
            
            // Add rigidbody and collider for physics
            if (!obj.GetComponent<Rigidbody>())
            {
                Rigidbody rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = true; // Make it static by default
            }
            
            // Make sure it has the VoiceControllable component
            if (!obj.GetComponent<VoiceControllable>())
            {
                VoiceControllable voiceControl = obj.AddComponent<VoiceControllable>();
                voiceControl.objectType = objectType.ToLower();
                
                // Set the color property if we applied a color
                if (initialColor.HasValue)
                {
                    voiceControl.color = GetColorName(initialColor.Value);
                }
            }
            
            // Add Outlinable component for highlighting
            if (!obj.GetComponent<EPOOutline.Outlinable>())
            {
                var outlinable = obj.AddComponent<EPOOutline.Outlinable>();
                outlinable.enabled = false; // Start with outline disabled
            }
            
            // Add XR Grab Interactable component for interaction
            if (!obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>())
            {
                var interactable = obj.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
            }
            
            // Add ObjectController for highlighting and selection
            if (!obj.GetComponent<ObjectController>())
            {
                var objController = obj.AddComponent<ObjectController>();
                
                // If SelectorManager is not available, make the object not selectable by disabling XR interactable
                var interactable = obj.GetComponent<XRGrabInteractable>();
                if (SelectorManager.Instance == null && interactable != null)
                {
                    Debug.LogWarning($"SelectorManager not available, temporarily disabling interactable on {obj.name}");
                    interactable.enabled = false;
                    
                    // Start a coroutine to enable it later when SelectorManager might be available
                    StartCoroutine(EnableInteractionWhenReady(interactable));
                }
            }
            
            // Set tag appropriately
            try {
                // Try to set the specific object type tag if it exists in the tag manager
                if (TagExists(objectType.ToLower()))
                {
                    obj.tag = objectType.ToLower();
                    Debug.Log($"Tagged object as '{objectType.ToLower()}'");
                }
                else
                {
                    // Default to Interactable tag
                    obj.tag = "Interactable";
                }
            }
            catch (UnityException)
            {
                // If tag setting fails, use Interactable as fallback
                obj.tag = "Interactable";
            }
            
            // Place at position
            obj.transform.position = position;
            
            return obj;
        }
        else
        {
            // Instantiate prefab
            GameObject obj = GameObject.Instantiate(prefab, position, Quaternion.identity);
            obj.name = objectType.ToLower();
            
            // Apply initial color if specified
            if (initialColor.HasValue)
            {
                // Apply color to all renderers
                foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                {
                    ApplyColorToRenderer(renderer, initialColor.Value);
                }
                
                // Update VoiceControllable component with the color name
                var voiceControl = obj.GetComponent<VoiceControllable>();
                if (voiceControl != null)
                {
                    voiceControl.color = GetColorName(initialColor.Value);
                }
            }
            
            // Make sure it has the VoiceControllable component
            if (!obj.GetComponent<VoiceControllable>())
            {
                VoiceControllable voiceControl = obj.AddComponent<VoiceControllable>();
                voiceControl.objectType = objectType.ToLower();
                
                // Set the color property if we applied a color
                if (initialColor.HasValue)
                {
                    voiceControl.color = GetColorName(initialColor.Value);
                }
            }
            
            // Add Outlinable component for highlighting
            if (!obj.GetComponent<EPOOutline.Outlinable>())
            {
                var outlinable = obj.AddComponent<EPOOutline.Outlinable>();
                outlinable.enabled = false; // Start with outline disabled
            }
            
            // Add XR Grab Interactable component for interaction
            if (!obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>())
            {
                var interactable = obj.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
            }
            
            // Add ObjectController for highlighting and selection
            if (!obj.GetComponent<ObjectController>())
            {
                var objController = obj.AddComponent<ObjectController>();
                
                // If SelectorManager is not available, make the object not selectable by disabling XR interactable
                var interactable = obj.GetComponent<XRGrabInteractable>();
                if (SelectorManager.Instance == null && interactable != null)
                {
                    Debug.LogWarning($"SelectorManager not available, temporarily disabling interactable on {obj.name}");
                    interactable.enabled = false;
                    
                    // Start a coroutine to enable it later when SelectorManager might be available
                    StartCoroutine(EnableInteractionWhenReady(interactable));
                }
            }
            
            // Set tag appropriately
            try {
                // Try to set the specific object type tag if it exists in the tag manager
                if (TagExists(objectType.ToLower()))
                {
                    obj.tag = objectType.ToLower();
                    Debug.Log($"Tagged object as '{objectType.ToLower()}'");
                }
                else
                {
                    // Default to Interactable tag
                    obj.tag = "Interactable";
                }
            }
            catch (UnityException)
            {
                // If tag setting fails, use Interactable as fallback
                obj.tag = "Interactable";
            }
            
            return obj;
        }
    }
    
    // Helper to apply color to a renderer
    private void ApplyColorToRenderer(Renderer renderer, Color color)
    {
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
    
    // Helper to get the name of a color
    private string GetColorName(Color color)
    {
        // Find the closest named color
        string bestMatch = "unknown";
        float bestDistance = float.MaxValue;
        
        foreach (var pair in namedColors)
        {
            float distance = ColorDistance(color, pair.Value);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestMatch = pair.Key;
            }
        }
        
        return bestMatch;
    }
    
    // Calculate distance between colors
    private float ColorDistance(Color a, Color b)
    {
        return Mathf.Sqrt(
            Mathf.Pow(a.r - b.r, 2) +
            Mathf.Pow(a.g - b.g, 2) +
            Mathf.Pow(a.b - b.b, 2)
        );
    }
    
    // Calculate position relative to a reference object
    private Vector3 CalculatePositionRelativeToReference(GameObject reference, string position, float distance)
    {
        Vector3 referencePosition = reference.transform.position;
        Vector3 resultPosition = referencePosition;
        
        // Get the extents of the reference object
        Bounds referenceBounds = CalculateObjectBounds(reference);
        float referenceWidth = referenceBounds.size.x;
        float referenceDepth = referenceBounds.size.z;
        
        // Adjust position based on the specified relation
        switch (position.ToLower())
        {
            case "left":
            case "to the left":
                resultPosition += -reference.transform.right * (referenceWidth / 2 + distance);
                break;
            case "right":
            case "to the right":
                resultPosition += reference.transform.right * (referenceWidth / 2 + distance);
                break;
            case "front":
            case "in front":
            case "in front of":
                resultPosition += reference.transform.forward * (referenceDepth / 2 + distance);
                break;
            case "back":
            case "behind":
            case "in back of":
                resultPosition += -reference.transform.forward * (referenceDepth / 2 + distance);
                break;
            case "beside":
            case "next to":
                // Default to right side
                resultPosition += reference.transform.right * (referenceWidth / 2 + distance);
                break;
            // Add more positions as needed
            default:
                // Default to right side
                resultPosition += reference.transform.right * (referenceWidth / 2 + distance);
                break;
        }
        
        // Ensure the object is placed on the ground
        resultPosition.y = 0;
        
        return resultPosition;
    }
    
    // Calculate the bounds of an object including all its renderers
    private Bounds CalculateObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            // If no renderers, use a default size
            return new Bounds(obj.transform.position, new Vector3(1f, 1f, 1f));
        }
        
        // Start with the first renderer's bounds
        Bounds bounds = renderers[0].bounds;
        
        // Encapsulate all other renderers
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        
        return bounds;
    }
    
    // Parse float with fallback value (moved to a common method)
    private float ParseFloat(string input, float fallback)
    {
        if (string.IsNullOrWhiteSpace(input)) return fallback;
        input = input.ToLower().Replace("meters", "").Replace("m", "").Replace("x", "").Trim();
        return float.TryParse(input, out float value) ? value : fallback;
    }

    // Coroutine to enable interaction when SelectorManager becomes available
    private IEnumerator EnableInteractionWhenReady(XRGrabInteractable interactable)
    {
        // Wait a bit to give SelectorManager time to initialize
        yield return new WaitForSeconds(2f);
        
        // Only enable if SelectorManager is now available
        if (SelectorManager.Instance != null && interactable != null)
        {
            interactable.enabled = true;
            Debug.Log($"Re-enabled interactable on {interactable.gameObject.name} now that SelectorManager is available");
        }
    }

    // Helper method to check if a tag exists
    private bool TagExists(string tagName)
    {
        try
        {
            GameObject.FindGameObjectsWithTag(tagName);
            return true;
        }
        catch (UnityException)
        {
            return false;
        }
    }

    // Coroutine to capture state of newly created object after a brief delay
    private IEnumerator CaptureCreatedObjectState(CommandState commandState, string objectType)
    {
        // Wait two frames to ensure the object is fully created and positioned
        yield return null;
        yield return null;
        
        // Try to find the most recently created object of this type
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Interactable");
        GameObject newestObject = null;
        
        // Look for objects with the specific type tag if it exists
        if (TagExists(objectType.ToLower()))
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(objectType.ToLower());
            if (taggedObjects.Length > 0)
            {
                newestObject = taggedObjects[taggedObjects.Length - 1];
            }
        }
        
        // If not found by tag, look for objects with the right type in VoiceControllable
        if (newestObject == null)
        {
            foreach (var obj in allObjects)
            {
                VoiceControllable controllable = obj.GetComponent<VoiceControllable>();
                if (controllable != null && controllable.objectType.ToLower() == objectType.ToLower())
                {
                    newestObject = obj;
                    break;
                }
            }
        }
        
        // If found, capture its state
        if (newestObject != null)
        {
            ObjectState state = ObjectState.CaptureState(newestObject);
            state.Exists = false; // Mark as "didn't exist before" for undo
            commandState.ObjectStates.Add(state);
            Debug.Log($"Captured state of newly created {objectType}: {newestObject.name}");
        }
        else
        {
            Debug.LogWarning($"Could not find newly created object of type {objectType} for history");
        }
        
        // Add to history after the creation state is captured
        AddToHistory(commandState);
    }

    // Helper method to correct common speech recognition mismatches
    private string CorrectMisrecognizedObjectType(string recognizedType)
    {
        if (string.IsNullOrEmpty(recognizedType)) return recognizedType;
        
        // Define common misrecognitions and their corrections
        Dictionary<string, string> corrections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "beauty", "building" },
            { "beauties", "buildings" },
            { "beautiful", "building" },
            { "billing", "building" },
            { "buildin", "building" },
            { "bildung", "building" },
            { "cube", "cube" },  // Keep correctly recognized types
            { "sphere", "sphere" },
            { "car", "car" },
            { "suv", "suv" },
            { "tree", "tree" }
        };
        
        // Check for similar words using Levenshtein distance for more flexibility
        string closestMatch = recognizedType;
        int shortestDistance = int.MaxValue;
        
        foreach (var validType in new[] { "building", "cube", "sphere", "car", "suv", "tree" })
        {
            int distance = LevenshteinDistance(recognizedType.ToLower(), validType);
            if (distance < shortestDistance && distance < 3) // Max distance of 3
            {
                shortestDistance = distance;
                closestMatch = validType;
            }
        }
        
        // First check if we have an exact correction match
        if (corrections.TryGetValue(recognizedType, out string correctedType))
        {
            Debug.Log($"Speech recognition correction: '{recognizedType}' → '{correctedType}'");
            return correctedType;
        }
        // Otherwise use the closest match if it's close enough
        else if (shortestDistance < 3)
        {
            Debug.Log($"Speech recognition correction (fuzzy): '{recognizedType}' → '{closestMatch}' (distance: {shortestDistance})");
            return closestMatch;
        }
        
        return recognizedType;
    }
    
    // Helper method to calculate Levenshtein distance for fuzzy matching
    private int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a))
        {
            return string.IsNullOrEmpty(b) ? 0 : b.Length;
        }
        
        if (string.IsNullOrEmpty(b))
        {
            return a.Length;
        }
        
        int[,] distance = new int[a.Length + 1, b.Length + 1];
        
        for (int i = 0; i <= a.Length; i++)
        {
            distance[i, 0] = i;
        }
        
        for (int j = 0; j <= b.Length; j++)
        {
            distance[0, j] = j;
        }
        
        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }
        
        return distance[a.Length, b.Length];
    }

    // Helper method to correct misrecognized color names
    private string CorrectMisrecognizedColor(string recognizedColor)
    {
        if (string.IsNullOrEmpty(recognizedColor)) return recognizedColor;
        
        // Define common color misrecognitions and their corrections
        Dictionary<string, string> colorCorrections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "read", "red" },
            { "rid", "red" },
            { "bread", "red" },
            { "rat", "red" },
            { "fred", "red" },
            { "grain", "green" },
            { "screen", "green" },
            { "clean", "green" },
            { "cream", "green" },
            { "grin", "green" },
            { "blu", "blue" },
            { "blew", "blue" },
            { "blow", "blue" },
            { "bloo", "blue" },
            { "yello", "yellow" },
            { "yallow", "yellow" },
            { "jello", "yellow" },
            { "hello", "yellow" },
            { "weight", "white" },
            { "wide", "white" },
            { "light", "white" },
            { "whine", "white" }
        };
        
        // Check for exact matches in our correction dictionary
        if (colorCorrections.TryGetValue(recognizedColor, out string correctedColor))
        {
            Debug.Log($"Color correction: '{recognizedColor}' → '{correctedColor}'");
            return correctedColor;
        }
        
        // Check for closest match using Levenshtein distance
        string[] validColors = { "red", "green", "blue", "yellow", "white", "black", "gray", "cyan", "magenta" };
        string closestMatch = recognizedColor;
        int shortestDistance = int.MaxValue;
        
        foreach (var color in validColors)
        {
            int distance = LevenshteinDistance(recognizedColor.ToLower(), color);
            if (distance < shortestDistance && distance < 3) // Max distance of 3
            {
                shortestDistance = distance;
                closestMatch = color;
            }
        }
        
        if (shortestDistance < 3)
        {
            Debug.Log($"Color correction (fuzzy): '{recognizedColor}' → '{closestMatch}' (distance: {shortestDistance})");
            return closestMatch;
        }
        
        return recognizedColor;
    }
}
