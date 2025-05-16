using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class InstructionPanelGenerator
{
    [MenuItem("Tools/Generate Instruction Panel")]
    public static void GenerateInstructionPanel()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("InstructionPanelCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.GetComponent<RectTransform>().sizeDelta = new Vector2(959, 1172);
        canvasGO.transform.position = Vector3.zero;

        // Create Panel
        GameObject panelGO = new GameObject("InstructionPanel", typeof(Image), typeof(CanvasGroup));
        panelGO.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 600);
        Image panelImage = panelGO.GetComponent<Image>();
        panelImage.color = new Color32(10, 10, 10, 200);

        // Create TextMeshPro Text
        GameObject textGO = new GameObject("InstructionText", typeof(TextMeshProUGUI));
        textGO.transform.SetParent(panelGO.transform, false);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(40, 80);
        textRect.offsetMax = new Vector2(-40, -40);
        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text =
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
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.enableWordWrapping = true;

        // Add InstructionPanel script and wire up references
        var instructionPanel = panelGO.AddComponent<InstructionPanel>();
        instructionPanel.panelCanvasGroup = panelGO.GetComponent<CanvasGroup>();
        instructionPanel.instructionText = tmp;

        // Create Help Button
        GameObject helpBtnGO = CreateButton("HelpButton", "?", new Color32(51, 119, 204, 255));
        helpBtnGO.transform.SetParent(panelGO.transform, false);
        RectTransform helpBtnRect = helpBtnGO.GetComponent<RectTransform>();
        helpBtnRect.anchorMin = new Vector2(0, 0);
        helpBtnRect.anchorMax = new Vector2(0, 0);
        helpBtnRect.anchoredPosition = new Vector2(40, 40);
        helpBtnRect.sizeDelta = new Vector2(60, 60);

        // Connect help button to show panel
        helpBtnGO.GetComponent<Button>().onClick.AddListener(() => instructionPanel.ShowHelp());

        // Create Close Button
        GameObject closeBtnGO = CreateButton("CloseButton", "✕", new Color32(204, 51, 51, 255));
        closeBtnGO.transform.SetParent(panelGO.transform, false);
        RectTransform closeBtnRect = closeBtnGO.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(1, 0);
        closeBtnRect.anchorMax = new Vector2(1, 0);
        closeBtnRect.anchoredPosition = new Vector2(-40, 40);
        closeBtnRect.sizeDelta = new Vector2(60, 60);

        // Wire up close button
        instructionPanel.closeButton = closeBtnGO.GetComponent<Button>();

        // Select the canvas in the editor
        Selection.activeGameObject = canvasGO;
    }

    private static GameObject CreateButton(string name, string label, Color32 color)
    {
        GameObject btnGO = new GameObject(name, typeof(Image), typeof(Button));
        Image img = btnGO.GetComponent<Image>();
        img.color = color;
        Button btn = btnGO.GetComponent<Button>();

        GameObject txtGO = new GameObject("Text", typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return btnGO;
    }
}

