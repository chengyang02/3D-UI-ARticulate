using UnityEngine;

public class InstructionUIController : MonoBehaviour
{
    public GameObject instructionPanel;

    void Start()
    {
        instructionPanel.SetActive(true); // Show on scene load
    }

    public void ClosePanel()
    {
        instructionPanel.SetActive(false);
    }

    public void ShowPanel()
    {
        instructionPanel.SetActive(true);
    }
}
