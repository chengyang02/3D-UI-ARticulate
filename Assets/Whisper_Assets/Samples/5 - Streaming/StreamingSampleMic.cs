using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

namespace Whisper.Samples
{
    /// <summary>
    /// Stream transcription from microphone input.
    /// </summary>
    public class StreamingSampleMic : MonoBehaviour
    {
        public WhisperController whisper;
        public MicrophoneRecord microphoneRecord;
    
        [Header("UI")] 
        public Button button;
        public Text buttonText;
        public Text text;
        public ScrollRect scroll;
        private WhisperStream _stream;
        public bool isRecording = false; 
        public static StreamingSampleMic Instance; 

        private async void Start()
        {
            if (Instance == null) {
                Instance = this; 
            } else {
                Destroy(this);
            }

            _stream = await whisper.CreateStream(microphoneRecord);
            _stream.OnResultUpdated += OnResult;
            _stream.OnSegmentUpdated += OnSegmentUpdated;
            _stream.OnSegmentFinished += OnSegmentFinished;
            _stream.OnStreamFinished += OnFinished;

            microphoneRecord.OnRecordStop += OnRecordStop;
            button.onClick.AddListener(OnButtonPressed);
        }

        private void OnButtonPressed()
        {
            if (!microphoneRecord.IsRecording)
            {
                _stream.StartStream();
                microphoneRecord.StartRecord();
            }
            else
                microphoneRecord.StopRecord();
        
            buttonText.text = microphoneRecord.IsRecording ? "Stop" : "Record";
        }

        public void StartRecording() {
            if (!microphoneRecord.IsRecording)
            {
                _stream.StartStream();
                microphoneRecord.StartRecord();
            }
        
            isRecording = true; 
            buttonText.text = microphoneRecord.IsRecording ? "Stop" : "Record";
        }

        public void EndRecording() {
            if (microphoneRecord.IsRecording)
            {
                microphoneRecord.StopRecord();
            }

            isRecording = false; 
            buttonText.text = microphoneRecord.IsRecording ? "Stop" : "Record";
        }
    
        private void OnRecordStop(AudioChunk recordedAudio)
        {
            buttonText.text = "Record";
        }
    
        private void OnResult(string result)
        {
            text.text = result;
            UiUtils.ScrollDown(scroll);
        }
        
        private void OnSegmentUpdated(WhisperResult segment)
        {
            print($"Segment updated: {segment.Result}");
        }
        
        private string _lastCmd = ""; 

        private void OnSegmentFinished(WhisperResult segment)
        {
            // 注释掉命令处理，避免多次触发
            // var manager = VoiceCommandManager.Instance;
            // if (manager != null)
            //     manager.ProcessVoiceCommand();
        }

        private void OnFinished(string finalResult)
        {
            Debug.Log($"🎤 Final transcription: {finalResult}");

            string cleaned = finalResult.Split('.')[0].Trim();
            if (string.IsNullOrWhiteSpace(cleaned)) return;

            var manager = VoiceCommandManager.Instance;
            if (manager != null)
                manager.ProcessVoiceCommand();
        }
    }
}
