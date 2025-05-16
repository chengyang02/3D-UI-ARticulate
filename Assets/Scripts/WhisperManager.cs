using System;
using UnityEngine;
using UnityEngine.UI;

public class WhisperManager : MonoBehaviour
{
    public Text transcript;
    
    // Events for transcription - suppress warnings as these are used by external components
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "CS0067:Event is never used", Justification = "Used by external systems")]
    public event Action<string> OnNewSegment;
    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "CS0067:Event is never used", Justification = "Used by external systems")]
    public event Action<int> OnProgress;

    // Reference to the transcript text component
    public void UpdateTranscript(string text)
    {
        if (transcript != null)
        {
            transcript.text = text;
        }
    }
    
    // Helper method that external systems might use to report progress
    public void ReportProgress(int progressPercent)
    {
        OnProgress?.Invoke(progressPercent);
    }
    
    // Helper method that external systems might use to report new segments
    public void ReportNewSegment(string segment)
    {
        OnNewSegment?.Invoke(segment);
        UpdateTranscript(segment);
    }
} 