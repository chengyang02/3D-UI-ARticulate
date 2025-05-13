using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenAIController : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    public static OpenAIController Instance;

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this); 
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // This line gets your API key (and could be slightly different on Mac/Linux)
        // api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User));
        api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User));
    }

    public async Task<string> GetResponse(string userInput)
    {
        Debug.Log("Getting response...");
        // define system message 
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, @"You are a strict command parser. You must extract commands using the action schema below. Your output must follow the exact key-value format:

            action_type: <action type>
            <argument name 1>: <value>
            <argument name 2>: <value>
            ...
            <argument name N>: <value>

            Rules:
            - Only use action types from this list: selection, translation, rotation, scale.
            - Only use object types from the list provided in the prompt (e.g., cube, sphere, table, chair).
            - Always include all required arguments for the selected action type.
            - Include optional arguments if they are clearly present in the user's command.
            - Never add explanations, summaries, or extra text before or after the output.
            -  You must use only 'action_type' and argument names from the schema such as 'object_type' as keyword.
            - If the command uses synonyms like pick, grab, or filter, map them to the closest valid action (e.g., selection).
            - You must only use argument keys listed in the schema (e.g., object_type, location). Do not invent or rename keys such as 'place', 'position', etc.
            - If the user refers to a location (e.g., 'on the right', 'to the left'), it must be mapped to the argument 'location'.
            - Always express the value of 'quantity' as a number (e.g., 1, 2, 3), not as a word (e.g., one, two, three).
            - Always express the value of 'distance' as a number only (e.g., 1, 2.5). Do not include units like 'meters' or 'm'.
            - Always express 'angle' as a number only (e.g., 45, 90). Do not include units like 'degrees' or 'deg'.
            - Always express 'scale_factor' as a number only (e.g., 1.5, 2). Do not include words like 'times', 'x', or 'scale'.
            - The value of 'axis' must be one of: x, y, or z. Do not use full words like 'vertical', 'horizontal', or 'up'.



            - All keys must be from these possible action arguments:
            ACTION SCHEMA:
                selection:
                required_args: object_type
                optional_args: color, quantity, location, size, name

                translation:
                required_args: object_type, direction
                optional_args: color, quantity, location, size, name, distance

                rotation:
                required_args: object_type, axis
                optional_args: color, quantity, location, size, name, angle

                scale:
                required_args: object_type
                optional_args: color, quantity, location, size, name, scale_factor, axis

            You must output the structured template only — no full sentences or commentary.")

            // Do not explain or generalize. Action type can only be these values: selection, translation, rotation, scale. If a command uses a different word (like filter or pick), map it to the closest valid action. Do not invent new actions. You must extract all relevant arguments for the identified action type based on the user's command. Do not omit valid optional arguments if they are mentioned. You must use only 'action_type' and argument names from the schema such as 'object_type'. Only output the structure exactly as shown in the examples."
        };

        if (userInput.Length < 1)
        {
            return "";
        }

        // Fill the user message
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = userInput;
        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        // Add the message to the list
        messages.Add(userMessage);

        // Send the entire chat to OpenAI to get the next message
        try {
            var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.1,
                MaxTokens = 4096,
                Messages = messages
            });
            // Get the response message
            ChatMessage responseMessage = new ChatMessage();
            responseMessage.Role = chatResult.Choices[0].Message.Role;
            responseMessage.Content = chatResult.Choices[0].Message.Content;
            Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));

            return responseMessage.Content; 
        } catch (System.Exception ex) {
            Debug.Log(ex.ToString());
            return "error";
        }
    }
}
