using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ActionClassifier : MonoBehaviour
{
    private string prompt; 
    public string userInput; 
    private string response; 

    // Start is called before the first frame update
    void Start()
    {
        prompt = "You are an intelligent assistant that classifies user voice commands into structured action templates using the following action schema:\n\nACTION SCHEMA:\n";
        prompt += GenerateActionSchemaPrompt(); 
        ClassifyText(userInput); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string ClassifyText(string userInput) {
        prompt += $@"Now, classify the following user command into an action type and arguments.

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
            User Command: move the table forward by 2 meters
            Output:
            action_type: translation
            target: table
            direction: forward
            distance: 2 meters

            EXAMPLE 3
            User Command: rotate the blue chair 90 degrees around Y
            Output:
            action_type: rotation
            target: blue chair
            axis: Y
            angle: 90 degrees

            Now classify this:
            User Command: {userInput}
            Output:
        ";

        Debug.Log("Prompt is" + prompt);

        // run OPENAI API to get the output
        GetLLMResponse(prompt); 
        return response; 
    }

    IEnumerator GetLLMResponse(string userInput) {
        var task = OpenAIController.Instance.GetResponse(userInput);
        while (!task.IsCompleted) {
            yield return null; 
        }
        
        response = task.Result;
        Debug.Log("Response returned");
    }

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
