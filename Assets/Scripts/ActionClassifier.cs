using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ActionClassifier : MonoBehaviour
{
    private string prompt; 
    public string userInput; 
    private string response; 
    public static ActionClassifier Instance = null;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) {
            Instance = this; 
        } else {
            Destroy(this); 
        }

        prompt = "You are a strict command parser. You must extract commands using the action schema below. Your output must be a single valid JSON object, with all keys and string values in double quotes, and no extra text.\n\n";
        prompt += "For action_type: selection, you may flexibly interpret user intent, including superlatives (top, bottom, closest, farthest), quantities, and locations (on my right, on my left, etc). For all other action types (translation, rotation, scale, color), you must be strict and only output the action if the user command is clear and matches the schema exactly. Never guess or invent parameters for non-selection actions.\n\n";
        prompt += "Examples for selection (flexible):\n";
        prompt += "User Command: I want the red cubes on my right\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"selection\",\n  \"target\": {\n    \"object_type\": \"cube\",\n    \"color\": \"red\"\n  },\n  \"location\": \"right\"\n}\n\n";
        prompt += "User Command: take one cube on my left\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"selection\",\n  \"target\": {\n    \"object_type\": \"cube\"\n  },\n  \"quantity\": \"1\",\n  \"location\": \"left\"\n}\n\n";
        prompt += "User Command: take the top cube\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"selection\",\n  \"target\": {\n    \"object_type\": \"cube\"\n  },\n  \"location\": \"top\"\n}\n\n";
        prompt += "User Command: give me the 3 cubes on my right\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"selection\",\n  \"target\": {\n    \"object_type\": \"cube\"\n  },\n  \"quantity\": \"3\",\n  \"location\": \"right\"\n}\n\n";
        prompt += "User Command: take only the blue cube\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"selection\",\n  \"target\": {\n    \"object_type\": \"cube\",\n    \"color\": \"blue\"\n  }\n}\n\n";
        prompt += "For all other action types (translation, rotation, scale, color), you must be strict and only output the action if the user command is clear and matches the schema exactly. Never guess or invent parameters for these actions.\n\n";
        prompt += "Examples for other actions (strict):\n";
        prompt += "User Command: move the blue cube forward by 2 meters\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"translation\",\n  \"target\": {\n    \"object_type\": \"cube\",\n    \"color\": \"blue\"\n  },\n  \"direction\": \"forward\",\n  \"distance\": \"2\"\n}\n\n";
        prompt += "User Command: move the red cube to the right by 1 meters\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"translation\",\n  \"target\": {\n    \"object_type\": \"cube\",\n    \"color\": \"red\"\n  },\n  \"direction\": \"right\",\n  \"distance\": \"1\"\n}\n\n";
        prompt += "User Command: rotate the grey sphere 90 degrees around Y\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"rotation\",\n  \"target\": {\n    \"object_type\": \"sphere\",\n    \"color\": \"grey\"\n  },\n  \"axis\": \"Y\",\n  \"angle\": \"90\"\n}\n\n";
        prompt += "User Command: scale the green cube by 0.8\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"scale\",\n  \"target\": {\n    \"object_type\": \"cube\",\n    \"color\": \"green\"\n  },\n  \"scale_factor\": \"0.8\"\n}\n\n";
        prompt += "User Command: color the blue cube green\n";
        prompt += "Output:\n";
        prompt += "{\n  \"action_type\": \"color\",\n  \"target\": {\n    \"object_type\": \"cube\",\n    \"color\": \"blue\"\n  },\n  \"color\": \"green\"\n}\n\n";
        prompt += "Rules:\n";
        prompt += "- Only use action types from this list: selection, translation, rotation, scale, color.\n";
        prompt += "- Only use object types from the list provided in the prompt (e.g., cube, sphere, table, chair).\n";
        prompt += "- Always include all required arguments for the selected action type.\n";
        prompt += "- Include optional arguments if they are clearly present in the user's command.\n";
        prompt += "- Never add explanations, summaries, or extra text before or after the output.\n";
        prompt += "- You must use only 'action_type' and argument names from the schema such as 'object_type' as keyword.\n";
        prompt += "- If the command uses synonyms like pick, grab, or filter, map them to the closest valid action (e.g., selection).\n";
        prompt += "- You must only use argument keys listed in the schema (e.g., object_type, location). Do not invent or rename keys such as 'place', 'position', etc.\n";
        prompt += "- If the user refers to a location (e.g., 'on the right', 'to the left'), it must be mapped to the argument 'location'.\n";
        prompt += "- Always express the value of 'quantity' as a number (e.g., 1, 2, 3), not as a word (e.g., one, two, three).\n";
        prompt += "- All keys must be from these possible action arguments:\n";
        prompt += "ACTION SCHEMA:\n";
        prompt += GenerateActionSchemaPrompt() + "\n";

        var objectTypesLine = $"Valid object types: {string.Join(", ", ActionSchemaRegistry.Instance.ObjectTypes)}.";
        prompt += objectTypesLine + "\n";
        // ClassifyText(userInput); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async Task<string> ClassifyText(string userInput) {
        string fullPrompt = prompt + $@"
Now, classify the following user command as a single valid JSON object, with all keys and string values in double quotes, and no extra text.
User Command: {userInput}
Output:
";
        Debug.Log("Prompt is" + fullPrompt);
        response = await OpenAIController.Instance.GetResponse(fullPrompt);
        Debug.Log("response: " + response);
        return response;
    }

    // IEnumerator GetLLMResponse(string userInput) {
    //     var task = 
    //     while (!task.IsCompleted) {
    //         yield return null; 
    //     }
        
    //     response = task.Result;
    //     Debug.Log("Response returned");
    // }

    string GenerateActionSchemaPrompt()
    {
        var sb = new StringBuilder();
        foreach (var action in ActionSchemaRegistry.Instance.ListActions())
        {
            var schema = ActionSchemaRegistry.Instance.GetSchema(action);
            var required = string.Join(", ", schema.GetRequiredArguments());
            var optional = string.Join(", ", schema.GetOptionalArguments());
            sb.AppendLine($"{action}:\n  required_args: {required}\n  optional_args: {optional}\n");
        }
        return sb.ToString();
    }
}
