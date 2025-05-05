using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionExecutioner : MonoBehaviour
{
    public class ActionCommand
    {
        public string ActionType;
        public Dictionary<string, string> Arguments = new Dictionary<string, string>();
    }
    public static ActionExecutioner Instance = null; 

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
        if (command.ActionType == "selection") {
            ExecuteSelection(command); 
        } else {

        }
    }

    public void ExecuteSelection(ActionCommand actionCommand) {
        string objectType = actionCommand.Arguments["object_type"]; 
        List<GameObject> list_temp = new List<GameObject>();
        foreach (var obj in SelectorManager.Instance.currentTargets) {
            if (!obj.CompareTag(objectType)) {
                ObjectController objectController = obj.GetComponent<ObjectController>();
                objectController.ToggleHighlight(); 
                list_temp.Add(obj);
            }
        }
        foreach (var obj in list_temp) {
            SelectorManager.Instance.RemoveFromSelection(obj);
        }
    }

    public ActionCommand ParseLLMResponse(string response)
    {
        var command = new ActionCommand();
        var lines = response.Split('\n');
        int num = 0; 
        foreach (var line in lines)
        {
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
        return command;
    }
}
