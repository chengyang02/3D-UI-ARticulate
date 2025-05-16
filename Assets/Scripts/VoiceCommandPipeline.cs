using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Samples;
using TMPro;

public class VoiceCommandPipeline : MonoBehaviour
{
    public TextMeshProUGUI transcriptText;
    public TextMeshProUGUI feedbackText;
    public StreamingSampleMic whisperManager;
    public ActionClassifier classifier;
    
    private string latestTranscript = "";
    private bool pipelineStarted = false;

    void Awake()
    {
        // Try to find components if not assigned
        if (whisperManager == null)
            whisperManager = FindObjectOfType<StreamingSampleMic>();
            
        if (classifier == null)
            classifier = FindObjectOfType<ActionClassifier>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize with empty transcript
        if (whisperManager != null && whisperManager.text != null)
            latestTranscript = whisperManager.text.text;
        else
            latestTranscript = "";
            
        // Initialize feedback components
        InitializeComponents();
    }
    
    // Initialize UI and other components
    private void InitializeComponents()
    {
        // Make sure we have a CorrectionFeedbackManager
        if (CorrectionFeedbackManager.Instance == null && feedbackText != null)
        {
            GameObject feedbackManagerObj = new GameObject("CorrectionFeedbackManager");
            CorrectionFeedbackManager manager = feedbackManagerObj.AddComponent<CorrectionFeedbackManager>();
            manager.feedbackText = feedbackText;
        }
        
        // If we have a CorrectionFeedbackManager but no assigned feedback text, connect it
        if (CorrectionFeedbackManager.Instance != null && 
            CorrectionFeedbackManager.Instance.feedbackText == null && 
            feedbackText != null)
        {
            CorrectionFeedbackManager.Instance.feedbackText = feedbackText;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if whisperManager and its text component exist
        if (whisperManager != null && whisperManager.text != null)
        {
            string currentTranscript = whisperManager.text.text;
            
            // Check if transcript has changed and is not empty
            if (!string.IsNullOrEmpty(currentTranscript) && currentTranscript != latestTranscript)
            {
                latestTranscript = currentTranscript;
                Debug.Log("transcript is: " + latestTranscript);
                
                if (transcriptText != null)
                {
                    transcriptText.text = latestTranscript;
                }
                
                if (pipelineStarted)
                    return;
                    
                pipelineStarted = true;
                StartPipeline();
            }
        }
    }

    private async void StartPipeline()
    {
        try
        {
            Debug.Log($"🗣️ Voice command: {latestTranscript}");
            
            if (classifier == null)
            {
                Debug.LogError("ActionClassifier not assigned to VoiceCommandPipeline!");
                pipelineStarted = false;
                return;
            }
            
            string response = await classifier.ClassifyText(latestTranscript);
            Debug.Log($"🤖 LLM response: {response}");
            
            // Check if the response contains color action
            bool hasColorAction = response.ToLower().Contains("action_type: color") || 
                                 response.ToLower().Contains("color the") || 
                                 response.ToLower().Contains("change color");
            if (hasColorAction) {
                Debug.Log("🎨 Detected color change request in: " + response);
            }
            
            // Add the original voice command as a comment at the top of the response
            // This helps our detection of generic vs. specific commands
            string augmentedResponse = $"// Original command: {latestTranscript}\n{response}";
            
            // Display correction feedback if the CorrectionFeedbackManager is available
            if (CorrectionFeedbackManager.Instance != null)
            {
                CorrectionFeedbackManager.Instance.ShowCorrectionFeedback(latestTranscript, response);
            }

            // Call execution if ActionExecutioner exists
            if (ActionExecutioner.Instance != null)
            {
                ActionExecutioner.Instance.Execute(augmentedResponse); 
            }
            else
            {
                Debug.LogError("ActionExecutioner.Instance is null! Cannot execute command.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in VoiceCommandPipeline.StartPipeline: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            pipelineStarted = false;
        }
    }
}
