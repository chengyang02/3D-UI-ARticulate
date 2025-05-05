# (May5 - voice command - manipulation)🎙️ Voice Interaction Extension – 3D-UI-ARticulate

This README documents the additions made to support **voice-based manipulation** of scene objects using OpenAI's Whisper in the Unity project `3D-UI-ARticulate`.

---

## ✅ Summary of What Was Added

- Added **voice command parsing** to manipulate objects in the scene (`move`, `rotate`, `scale`, `color`).
- Implemented `VoiceCommandHandler.cs` to process spoken commands like:  
  👉 “Rotate blue book” or “Scale red cube”
- Attached `VoiceControllable.cs` to each interactable object so they can be matched by type and color.
- Modified `StreamingSampleMic.cs` (Whisper sample) to call our `VoiceCommandHandler`.
- Verified end-to-end voice interaction from mic input → Whisper → scene manipulation.

---

## 🧩 File Changes

### 🆕 Scripts

- `Assets/Scripts/VoiceActions/VoiceCommandHandler.cs`  
  → Parses voice text & applies transformations.

- `Assets/Scripts/VoiceActions/VoiceControllable.cs`  
  → Attach to any object you want to manipulate via voice.

### 📝 Modified

- `Assets/Whisper_Assets/Samples/5 - Streaming/StreamingSampleMic.cs`  
  → Hooked up voice parsing after Whisper detects a segment.

---

## 🛠️ Setup Instructions

### 1. Add `VoiceManager` GameObject
- In your scene, create a new empty GameObject called **`VoiceManager`**
- Attach the `VoiceCommandHandler` script to it.
- Tag this object if needed (e.g., for reference from other scripts).

### 2. Connect to Whisper Sample
- Select the **Microphone** GameObject (from Whisper sample scene).
- In its **`StreamingSampleMic`** component, make sure the `text`, `button`, and `scroll` fields are set.
- Ensure the following line can find `VoiceManager` in scene:
  ```csharp
  var handlerGO = GameObject.Find("VoiceManager");

### 3. Prepare Interactable Objects
- For every object you want to control via voice:

-（1) Attach the VoiceControllable script.

- (2)Set the objectType and color in Inspector (e.g., book, blue)

- (2)Tag it as Interactable so it's picked up in search.

### 4. Supported Voice Commands
Format: <action> <color> <type>

Examples:

"rotate blue book"

"move red cube"

"scale green chair"

"color yellow plant"
