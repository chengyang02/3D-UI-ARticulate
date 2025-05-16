using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Whisper.Samples;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WhisperUIPrefabGenerator : MonoBehaviour
{
    // This script is intended to be used only within the Unity Editor to regenerate
    // the WhisperUI prefab with the correct structure and components
    
#if UNITY_EDITOR
    [MenuItem("Tools/Regenerate WhisperUI Prefab")]
    public static void RegenerateWhisperUIPrefab()
    {
        RegenerateWhisperUIPrefabInternal(false);
    }
    
    [MenuItem("Tools/Regenerate WhisperUI Prefab (Debug Mode)")]
    public static void RegenerateWhisperUIPrefabDebug()
    {
        RegenerateWhisperUIPrefabInternal(true);
    }
    
    private static void RegenerateWhisperUIPrefabInternal(bool includeVerifier)
    {
        // Create a new GameObject for the Canvas
        GameObject canvasObj = new GameObject("WhisperUI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add required Canvas components
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Add the WhisperManager component
        WhisperManager whisperManager = canvasObj.AddComponent<WhisperManager>();
        
        // Add the UndoRedoManager component
        UndoRedoManager undoRedoManager = canvasObj.AddComponent<UndoRedoManager>();
        
        // Add the UndoRedoButtonConnector component for runtime connection
        UndoRedoButtonConnector buttonConnector = canvasObj.AddComponent<UndoRedoButtonConnector>();
        
        // Optionally add the UndoRedoVerifier for debugging
        if (includeVerifier)
        {
            canvasObj.AddComponent<UndoRedoVerifier>();
        }
        
        // Create Window GameObject
        GameObject window = new GameObject("Window");
        window.transform.SetParent(canvasObj.transform, false);
        RectTransform windowRect = window.AddComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0, 0);
        windowRect.anchorMax = new Vector2(1, 1);
        windowRect.sizeDelta = Vector2.zero;
        
        // Add Background Image
        Image windowImage = window.AddComponent<Image>();
        windowImage.color = new Color(1f, 1f, 1f, 0.4f);
        
        // Create Background to make UI more readable
        GameObject background = new GameObject("Background");
        background.transform.SetParent(window.transform, false);
        RectTransform backgroundRect = background.AddComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0, 0);
        backgroundRect.anchorMax = new Vector2(1, 1);
        backgroundRect.sizeDelta = Vector2.zero;
        
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(1f, 1f, 1f, 0.05f);
        
        // Create ScrollView for Transcript
        GameObject scrollView = new GameObject("Scroll View");
        scrollView.transform.SetParent(window.transform, false);
        RectTransform scrollViewRect = scrollView.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollViewRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollViewRect.sizeDelta = new Vector2(400, 350);
        
        Image scrollViewImage = scrollView.AddComponent<Image>();
        scrollViewImage.color = new Color(0.23f, 0.28f, 0.49f, 0.39f);
        
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        
        // Create Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0, 0);
        viewportRect.anchorMax = new Vector2(0, 0);
        viewportRect.sizeDelta = new Vector2(0, 0);
        viewportRect.pivot = new Vector2(0, 1);
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.white;
        
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        // Create Transcript
        GameObject transcript = new GameObject("Transcript");
        transcript.transform.SetParent(viewport.transform, false);
        RectTransform transcriptRect = transcript.AddComponent<RectTransform>();
        transcriptRect.anchorMin = new Vector2(0.5f, 0.5f);
        transcriptRect.anchorMax = new Vector2(0.5f, 0.5f);
        transcriptRect.sizeDelta = new Vector2(400, 350);
        
        // Use TextMeshProUGUI for better text rendering
        TextMeshProUGUI transcriptTMPro = transcript.AddComponent<TextMeshProUGUI>();
        transcriptTMPro.text = "Transcript";
        transcriptTMPro.fontSize = 36;
        transcriptTMPro.alignment = TextAlignmentOptions.TopLeft;
        transcriptTMPro.enableWordWrapping = true;
        
        // Also add a legacy Text component for compatibility with existing StreamingSampleMic
        Text transcriptText = transcript.AddComponent<Text>();
        transcriptText.text = "Transcript";
        transcriptText.fontSize = 36;
        transcriptText.alignment = TextAnchor.UpperLeft;
        
        // Reference for WhisperManager
        if (whisperManager != null)
        {
            whisperManager.transcript = transcriptText;
        }
        
        // Create Record Button
        GameObject recordButton = new GameObject("Record");
        recordButton.transform.SetParent(window.transform, false);
        RectTransform recordButtonRect = recordButton.AddComponent<RectTransform>();
        recordButtonRect.anchorMin = new Vector2(0.5f, 0);
        recordButtonRect.anchorMax = new Vector2(0.5f, 0);
        recordButtonRect.anchoredPosition = new Vector2(0, 74);
        recordButtonRect.sizeDelta = new Vector2(250, 80);
        
        Image recordButtonImage = recordButton.AddComponent<Image>();
        recordButtonImage.color = Color.white;
        
        Button recordButtonComponent = recordButton.AddComponent<Button>();
        
        // Create Record Button Text
        GameObject recordButtonText = new GameObject("Text (Legacy)");
        recordButtonText.transform.SetParent(recordButton.transform, false);
        RectTransform recordButtonTextRect = recordButtonText.AddComponent<RectTransform>();
        recordButtonTextRect.anchorMin = new Vector2(0, 0);
        recordButtonTextRect.anchorMax = new Vector2(1, 1);
        recordButtonTextRect.sizeDelta = Vector2.zero;
        
        Text recordButtonTextComponent = recordButtonText.AddComponent<Text>();
        recordButtonTextComponent.text = "Record";
        recordButtonTextComponent.fontSize = 36;
        recordButtonTextComponent.alignment = TextAnchor.MiddleCenter;
        
        // Create Undo Button
        GameObject undoButton = new GameObject("UndoButton");
        undoButton.transform.SetParent(window.transform, false);
        RectTransform undoButtonRect = undoButton.AddComponent<RectTransform>();
        undoButtonRect.anchorMin = new Vector2(0, 0);
        undoButtonRect.anchorMax = new Vector2(0, 0);
        undoButtonRect.anchoredPosition = new Vector2(40, 180);
        undoButtonRect.sizeDelta = new Vector2(80, 30);
        
        Image undoButtonImage = undoButton.AddComponent<Image>();
        undoButtonImage.color = new Color(0.32f, 0.32f, 0.32f, 0.6f);
        
        Button undoButtonComponent = undoButton.AddComponent<Button>();
        
        // Create Undo Button Text
        GameObject undoButtonText = new GameObject("Text (Legacy)");
        undoButtonText.transform.SetParent(undoButton.transform, false);
        RectTransform undoButtonTextRect = undoButtonText.AddComponent<RectTransform>();
        undoButtonTextRect.anchorMin = new Vector2(0, 0);
        undoButtonTextRect.anchorMax = new Vector2(1, 1);
        undoButtonTextRect.sizeDelta = Vector2.zero;
        
        Text undoButtonTextComponent = undoButtonText.AddComponent<Text>();
        undoButtonTextComponent.text = "Undo";
        undoButtonTextComponent.fontSize = 12;
        undoButtonTextComponent.color = Color.white;
        undoButtonTextComponent.alignment = TextAnchor.MiddleCenter;
        
        // Create Redo Button
        GameObject redoButton = new GameObject("RedoButton");
        redoButton.transform.SetParent(window.transform, false);
        RectTransform redoButtonRect = redoButton.AddComponent<RectTransform>();
        redoButtonRect.anchorMin = new Vector2(0, 0);
        redoButtonRect.anchorMax = new Vector2(0, 0);
        redoButtonRect.anchoredPosition = new Vector2(130, 180);
        redoButtonRect.sizeDelta = new Vector2(80, 30);
        
        Image redoButtonImage = redoButton.AddComponent<Image>();
        redoButtonImage.color = new Color(0.32f, 0.32f, 0.32f, 0.6f);
        
        Button redoButtonComponent = redoButton.AddComponent<Button>();
        
        // Create Redo Button Text
        GameObject redoButtonText = new GameObject("Text (Legacy)");
        redoButtonText.transform.SetParent(redoButton.transform, false);
        RectTransform redoButtonTextRect = redoButtonText.AddComponent<RectTransform>();
        redoButtonTextRect.anchorMin = new Vector2(0, 0);
        redoButtonTextRect.anchorMax = new Vector2(1, 1);
        redoButtonTextRect.sizeDelta = Vector2.zero;
        
        Text redoButtonTextComponent = redoButtonText.AddComponent<Text>();
        redoButtonTextComponent.text = "Redo";
        redoButtonTextComponent.fontSize = 14;
        redoButtonTextComponent.color = Color.white;
        redoButtonTextComponent.alignment = TextAnchor.MiddleCenter;
        
        // Assign buttons to UndoRedoManager
        undoRedoManager.undoButton = undoButtonComponent;
        undoRedoManager.redoButton = redoButtonComponent;
        
        // Make sure the scrollRect is set up correctly
        scrollRect.content = transcriptRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10f;
        
        // Create Text Panel with Scroll Rect
        GameObject textPanelObj = new GameObject("TranscriptPanel");
        textPanelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform textPanelRect = textPanelObj.AddComponent<RectTransform>();
        textPanelRect.anchorMin = new Vector2(0.1f, 0.3f);
        textPanelRect.anchorMax = new Vector2(0.9f, 0.9f);
        textPanelRect.anchoredPosition = Vector2.zero;
        textPanelRect.sizeDelta = Vector2.zero;
        
        Image textPanelImage = textPanelObj.AddComponent<Image>();
        textPanelImage.color = new Color(0, 0, 0, 0.7f);
        
        // Create a feedback text for speech recognition corrections
        GameObject feedbackTextObj = new GameObject("SpeechCorrectionFeedback");
        feedbackTextObj.transform.SetParent(canvasObj.transform, false);
        RectTransform feedbackTextRect = feedbackTextObj.AddComponent<RectTransform>();
        feedbackTextRect.anchorMin = new Vector2(0.1f, 0.2f);
        feedbackTextRect.anchorMax = new Vector2(0.9f, 0.25f);
        feedbackTextRect.anchoredPosition = Vector2.zero;
        feedbackTextRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI feedbackText = feedbackTextObj.AddComponent<TextMeshProUGUI>();
        feedbackText.text = "";
        feedbackText.color = new Color(0, 0.75f, 1f, 1f); // Light blue
        feedbackText.fontSize = 16;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.enableWordWrapping = true;
        feedbackText.overflowMode = TextOverflowModes.Truncate;
        
        // Add UISpeechCorrectionSetup component and configure it
        UISpeechCorrectionSetup correctionSetup = canvasObj.AddComponent<UISpeechCorrectionSetup>();
        correctionSetup.transcriptText = transcriptTMPro;
        correctionSetup.correctionFeedbackText = feedbackText;
        correctionSetup.feedbackColor = new Color(0, 0.75f, 1f, 1f);
        correctionSetup.displayDuration = 5f;
        
        // Add VoiceCommandPipeline component and configure it
        VoiceCommandPipeline pipeline = canvasObj.AddComponent<VoiceCommandPipeline>();
        
        // Try to find existing components needed by the pipeline
        StreamingSampleMic streamingSample = FindObjectOfType<StreamingSampleMic>();
        ActionClassifier actionClassifier = FindObjectOfType<ActionClassifier>();
        
        // Connect the pipeline if components are found
        if (pipeline != null)
        {
            pipeline.transcriptText = transcriptTMPro;
            pipeline.feedbackText = feedbackText;
            
            if (streamingSample != null)
                pipeline.whisperManager = streamingSample;
                
            if (actionClassifier != null)
                pipeline.classifier = actionClassifier;
        }
        
        // Create the prefab
        string prefabPath = "Assets/Prefabs/WhisperUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);
        
        // Cleanup
        Object.DestroyImmediate(canvasObj);
        
        Debug.Log("WhisperUI prefab has been successfully regenerated at " + prefabPath + (includeVerifier ? " (Debug Mode)" : ""));
    }
#endif
} 