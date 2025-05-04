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
        api = new OpenAIAPI("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");
    }

    public async Task<string> GetResponse(string userInput)
    {
        Debug.Log("Getting response...");
        // define system message 
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, "You are a helpful assistant who classifies voice commands into structured actions.")
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
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.9,
            MaxTokens = 50,
            Messages = messages
        });

        // Get the response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));

        return responseMessage.Content; 
    }
}