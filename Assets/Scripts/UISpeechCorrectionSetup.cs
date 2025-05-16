using UnityEngine;
using TMPro;

// Add this to your WhisperUI GameObject to auto-configure the feedback UI
public class UISpeechCorrectionSetup : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI transcriptText;
    public TextMeshProUGUI correctionFeedbackText;
    
    [Header("Customization")]
    public Color feedbackColor = new Color(0, 0.75f, 1f, 1f); // Light blue
    public float displayDuration = 5f; 
    
    void Start()
    {
        // Create correction feedback manager if it doesn't exist
        if (CorrectionFeedbackManager.Instance == null)
        {
            GameObject feedbackManagerObj = new GameObject("CorrectionFeedbackManager");
            CorrectionFeedbackManager manager = feedbackManagerObj.AddComponent<CorrectionFeedbackManager>();
            
            // Configure it
            if (correctionFeedbackText != null)
            {
                manager.feedbackText = correctionFeedbackText;
                manager.feedbackColor = feedbackColor;
                manager.displayDuration = displayDuration;
            }
            else
            {
                Debug.LogWarning("Correction feedback text is not assigned. Speech recognition correction feedback will not be displayed.");
            }
        }
        else
        {
            // If it already exists, update the text reference
            if (correctionFeedbackText != null && CorrectionFeedbackManager.Instance.feedbackText == null)
            {
                CorrectionFeedbackManager.Instance.feedbackText = correctionFeedbackText;
                CorrectionFeedbackManager.Instance.feedbackColor = feedbackColor;
                CorrectionFeedbackManager.Instance.displayDuration = displayDuration;
            }
        }
    }
} 