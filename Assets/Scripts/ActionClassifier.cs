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

        prompt = "You are an intelligent assistant that classifies user voice commands into structured action templates. Here are the possible classification categories:\n\nACTION SCHEMA:\n";
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
        prompt += $@"Now, classify the following user command into an action type and all possible arguments.

            FORMAT:
            action_type: <action type>
            <argument name>: <value>

            EXAMPLE 1
            User Command: select the red cube
            Output:
            action_type: selection
            object_type: cube
            color: red

            EXAMPLE 2
            User Command: move the blue table forward by 2 meters
            Output:
            action_type: translation
            object_type: table
            color: blue
            direction: forward
            distance: 2 meters

            EXAMPLE 3
            User Command: rotate the green chair 90 degrees around Y
            Output:
            action_type: rotation
            object_type: chair
            color: green
            axis: Y
            angle: 90 degrees

            Now classify this:
            User Command: {userInput}
            Output:
        ";

        Debug.Log("Prompt is" + prompt);

        // run OPENAI API to get the output
        response = await OpenAIController.Instance.GetResponse(userInput);
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
