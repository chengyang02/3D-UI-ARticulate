using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstructionPanel : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup panelCanvasGroup;
    public TextMeshProUGUI instructionText;
    public Button closeButton;
    
    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float autoHideDelay = 10f; // Set to 0 to disable auto-hide
    [SerializeField] public string welcomeText = 
@"<color=#69B3E7><size=36>Welcome to ARticulate!</size></color>

<color=#FFCC33>□ SELECTION:</color>
• Ray Selection (Right Controller): Press Y to activate, aim at object, press G to select
• Sphere Selection (Right Controller): Press Y to activate, press N to spawn selection sphere
• Adjust Sphere Size (Left Controller): Press T to activate, B to shrink, N to enlarge

<color=#FFCC33>□ VOICE COMMANDS:</color>
• Record: Click Record button or press B to start/stop recording
• Try saying: <i>""Color the building blue""</i> or <i>""Create a red cube""</i>

<color=#FFCC33>□ WAYPOINTS:</color>
• Press T to activate left controller, left-click when ray is white to create waypoint

<color=#FFCC33>□ TELEPORTING:</color>
• Press Y to activate right controller, press G when ray is white and spinning icon appears

<color=#FFCC33>□ UNDO/REDO:</color>
• Press Ctrl+Z to undo or Ctrl+Y to redo actions";
    
    [SerializeField] public string helpText = 
@"[Voice] Example Voice Commands

[Selection Commands]
""Select the red building.""
""Select all trees.""
""Select the tallest car.""
""Select the closest building.""
<i>Implementation:</i> <color=#FFCC33>FilterObjectsByCommonArgs</color> method in ActionExecutioner.cs handles filtering.

[Manipulation Commands]
""Scale the trees by a factor of 1.5.""
""Rotate the car 90 degrees around the Y axis.""
""Move the bench forward by 2 meters.""
""Color the building blue.""
<i>Implementation:</i> Each operation is handled by dedicated methods in ActionExecutioner.cs.

[Creation Commands]
""Create a red cube.""
""Add a tree near the building.""
""Generate a blue sphere.""
<i>Implementation:</i> <color=#FFCC33>ExecuteCreation</color> and <color=#FFCC33>CreateObject</color> methods in ActionExecutioner.cs.

[Color Commands]
""Color the cube red.""
""Make the building blue.""
""Change the car to green.""
<i>Implementation:</i> <color=#FFCC33>ExecuteColor</color> method in ActionExecutioner.cs.
Supported Colors: red, green, blue, yellow, white, black, gray, cyan, magenta, orange, purple

[Voice] Natural Language Prompts
""I want the tree to be a little taller.""
""Turn the car upside down.""
""Make the bench smaller.""
""Move the trees closer to the buildings.""
""I want the buildings to rotate to the left.""
""Flip the bench backward.""
";
    
    private Coroutine autoHideCoroutine;
    private bool showingHelp = false;
    
    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        
        if (instructionText != null)
        {
            // Set welcomeText to whatever is in the UI at design time
            welcomeText = instructionText.text;
        }
        
        // Make sure panel is hidden at start
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
    }
    
    private void Start()
    {
        // Show panel when scene starts
        ShowPanel();
    }
    
    public void ShowPanel()
    {
        if (panelCanvasGroup == null) return;
        
        // Cancel any existing auto-hide coroutine
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }
        
        // Start fade in animation
        StartCoroutine(FadeIn());
        
        // Start auto-hide timer if enabled
        if (autoHideDelay > 0)
        {
            autoHideCoroutine = StartCoroutine(AutoHidePanel());
        }
    }
    
    public void HidePanel()
    {
        if (panelCanvasGroup == null) return;
        
        // Cancel any existing auto-hide coroutine
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
        
        // Start fade out animation
        StartCoroutine(FadeOut());
    }
    
    private IEnumerator FadeIn()
    {
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
        
        float startTime = Time.time;
        float startAlpha = panelCanvasGroup.alpha;
        
        while (Time.time < startTime + fadeInDuration)
        {
            float t = (Time.time - startTime) / fadeInDuration;
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);
            yield return null;
        }
        
        panelCanvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        float startTime = Time.time;
        float startAlpha = panelCanvasGroup.alpha;
        
        while (Time.time < startTime + fadeOutDuration)
        {
            float t = (Time.time - startTime) / fadeOutDuration;
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
    }
    
    private IEnumerator AutoHidePanel()
    {
        yield return new WaitForSeconds(autoHideDelay);
        HidePanel();
    }
    
    public void UpdateInstructionText(string newText)
    {
        if (instructionText != null)
        {
            instructionText.text = newText;
        }
    }

    public void ShowHelp()
    {
        if (instructionText != null)
        {
            if (showingHelp)
            {
                instructionText.text = welcomeText;
            }
            else
            {
                instructionText.text = helpText;
                instructionText.fontSize = 15;
            }
            showingHelp = !showingHelp;
        }
        ShowPanel();
    }

    public void ShowWelcome()
    {
        if (instructionText != null)
        {
            instructionText.text = welcomeText;
        }
        ShowPanel();
    }
} 