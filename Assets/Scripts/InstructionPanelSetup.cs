using UnityEngine;

public class InstructionPanelSetup : MonoBehaviour
{
    [SerializeField] private GameObject instructionPanelPrefab;
    [SerializeField] private bool showOnlyOnFirstRun = true;
    
    private const string FirstRunPrefKey = "InstructionPanel_FirstRun";
    
    private void Start()
    {
        bool isFirstRun = !PlayerPrefs.HasKey(FirstRunPrefKey);
        
        if (!showOnlyOnFirstRun || isFirstRun)
        {
            ShowInstructionPanel();
            
            // Mark as seen
            if (isFirstRun)
            {
                PlayerPrefs.SetInt(FirstRunPrefKey, 1);
                PlayerPrefs.Save();
            }
        }
    }
    
    private void ShowInstructionPanel()
    {
        if (instructionPanelPrefab != null)
        {
            // If we're in XR, instantiate the panel in front of the camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 position = mainCamera.transform.position + mainCamera.transform.forward * 0.5f;
                Quaternion rotation = Quaternion.LookRotation(position - mainCamera.transform.position);
                
                Instantiate(instructionPanelPrefab, position, rotation);
            }
            else
            {
                // Fallback if no camera is found
                Instantiate(instructionPanelPrefab, Vector3.zero, Quaternion.identity);
            }
        }
    }
    
    // Public method to show the panel again (can be triggered by a button)
    public void ShowInstructionsAgain()
    {
        ShowInstructionPanel();
    }
} 