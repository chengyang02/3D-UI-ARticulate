using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Samples;

public class VoiceCommandPipeline : MonoBehaviour
{
    public Text transcript; 
    private string lastTranscript; 

    // Start is called before the first frame update
    void Start()
    {
        lastTranscript = transcript.text; 
    }

    // Update is called once per frame
    void Update()
    {
        if (transcript.text != lastTranscript && !StreamingSampleMic.Instance.isRecording) {
            StartPipeline(); 

            // update
            lastTranscript = transcript.text;
        }
    }

    public async void StartPipeline() {
        // call classification 
        string response = await ActionClassifier.Instance.ClassifyText(transcript.text); 
        Debug.Log("response: " + response);

        // call execution 
        ActionExecutioner.Instance.Execute(response); 

    }
}
