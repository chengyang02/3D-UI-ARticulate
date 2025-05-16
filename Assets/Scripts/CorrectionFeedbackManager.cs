using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This class manages feedback to users about speech recognition corrections
public class CorrectionFeedbackManager : MonoBehaviour
{
    public static CorrectionFeedbackManager Instance;
    
    public TextMeshProUGUI feedbackText;
    public float displayDuration = 5f;
    public Color feedbackColor = new Color(0, 0.75f, 1f, 1f); // Light blue
    private Coroutine clearFeedbackCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Show correction feedback to the user
    public void ShowCorrectionFeedback(string originalInput, string correctedOutput)
    {
        if (feedbackText == null) return;
        
        // Generate appropriate feedback message
        string feedbackMessage = GetCorrectionFeedback(originalInput, correctedOutput);
        
        if (!string.IsNullOrEmpty(feedbackMessage))
        {
            // Save original color
            Color originalColor = feedbackText.color;
            
            // Set feedback text and color
            feedbackText.text = feedbackMessage;
            feedbackText.color = feedbackColor;
            
            // Clear any existing coroutine to avoid timing issues
            if (clearFeedbackCoroutine != null)
            {
                StopCoroutine(clearFeedbackCoroutine);
            }
            
            // Start new coroutine
            clearFeedbackCoroutine = StartCoroutine(ClearFeedbackAfterDelay(displayDuration, originalColor));
        }
    }
    
    // Helper to generate appropriate feedback about speech recognition corrections
    private string GetCorrectionFeedback(string originalInput, string correctedOutput)
    {
        // Check for object type corrections
        if (originalInput.ToLower().Contains("beauty") && correctedOutput.Contains("building"))
        {
            return "I heard 'beauty' but understood you meant 'building'";
        }
        
        // Check for color corrections
        if (originalInput.ToLower().Contains("read") && correctedOutput.Contains("color") && correctedOutput.Contains("red"))
        {
            return "I heard 'read' but understood you meant 'red'";
        }
        
        // Check for other common patterns
        foreach (var pair in new Dictionary<string, string> {
            { "beautif", "building" },
            { "bild", "building" },
            { "bil", "building" },
            { "buil", "building" },
            { "rid", "red" },
            { "bread", "red" },
            { "fred", "red" },
            { "grain", "green" },
            { "blu", "blue" },
            { "yellow", "yellow" },
            { "weight", "white" }
        })
        {
            if (originalInput.ToLower().Contains(pair.Key) && correctedOutput.Contains(pair.Value))
            {
                return $"I heard '{pair.Key}' but understood you meant '{pair.Value}'";
            }
        }
        
        return "";
    }
    
    // Coroutine to clear feedback text after a delay
    private IEnumerator ClearFeedbackAfterDelay(float delay, Color originalColor)
    {
        yield return new WaitForSeconds(delay);
        
        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.color = originalColor;
        }
        
        clearFeedbackCoroutine = null;
    }
} 