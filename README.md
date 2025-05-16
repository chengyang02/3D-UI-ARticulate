# 🧠 XR Interaction System with LLMs

**Team Name**: *ARticulate*  
**Team Members**:  
- Charlie Zou (jz3331)
- Cheng Yang (cy2748)
- Linlin Zhang (lz2981)  
- Tans Rosen (njr2145)  

**Advisor**: Ben Yang

---

## 🔍 Overview

In this project, our team uses **Large Language Models (LLMs)** to improve user experience and create a more natural and intuitive way of interacting with virtual reality environments.

Users can use controllers (e.g., **ray selection** or **sphere selection**) or **voice commands** to:

- Select groups of objects  
- Refine their selections  
- Manipulate objects (e.g., **scale**, **rotate**, **move**, **color**)  
- Create new objects

The system also allows users to **label** and **navigate** to different parts of the scene to support easier wayfinding. Our goal is to make interaction with complex 3D environments as seamless as having a conversation.

---

## 🎥 Demo Video (narrated)
https://youtu.be/v5MnWmMCoeM

---

## 🎮 How to Use (Playmode Instructions & Quest)

### 🎯 Selection

#### 🔦 Ray Selection (Right Controller)
- Press **`Y`** to activate the right controller.
- Aim at an object.
- Press **`G`** to select it.
- **Implementation**: Uses `SelectorManager.cs` for ray-based selection and highlighting.

#### 🟠 Sphere Selection (Right Controller)
- Press **`Y`** to activate the right controller.
- Press **`N`** to spawn the selection sphere.
- **Implementation**: Uses sphere colliders to detect and select objects.

##### 🎚️ Adjust Sphere Size (Left Controller)
- Press **`T`** to activate the left controller.
- Press **`B`** to make the sphere smaller.
- Press **`N`** to make it bigger.

✅ Selected objects will have a **yellow highlight** (using `EPOOutline` system).

---

### 🎙️ Voice Command Recording

After selecting objects, record your command in one of two ways:

#### Option 1: UI Buttons
- Click **Record** on the UI panel to begin.
- Click **Stop** to finish.
- **Implementation**: Uses `WhisperManager.cs` for speech recognition.

#### Option 2: Keyboard Shortcut
- Press **`B`** to **start recording**.
- Press **`B`** again to **stop recording**.

---

### 📍 Waypoint Creation (Left Controller)

- Press **`T`** to activate the left controller.
- When the ray turns **white**, **left-click** to create a waypoint.
- To cancel: click again without moving the ray.
- **Implementation**: Uses waypoint prefabs and navigation system.

---

### 🚀 Teleporting (Right Controller)

- Press **`Y`** to activate the right controller.
- When the ray turns **white** and a **spinning teleport icon** appears:
  - Press **`G`** to teleport.
- **Implementation**: Uses XR Interaction Toolkit's teleportation system.

⚠️ **First-time teleport bug**:  
If you fall underground, press **`Q`** to move up or **`E`** to move down. Once you move up and the **spinning teleport icon** appears again on the floor, teleporting will work normally.

---

### ↩️ Undo & Redo Functionality

- **Undo**: Click the **Undo** button or press **Ctrl+Z** to revert the last action. If in quest, please use ray casting.
- **Redo**: Click the **Redo** button or press **Ctrl+Y** to reapply the undone action. If in quest, please use ray casting.
- **Implementation**: `UndoRedoManager.cs` manages UI connections while `ActionExecutioner.cs` tracks command history.

---

## 🗣️ Example Voice Commands

### ✅ Selection Commands
- "Select the red building."
- "Select all trees."
- "Select the tallest car."
- "Select the closest building."
- **Implementation**: `FilterObjectsByCommonArgs` method in `ActionExecutioner.cs` handles filtering.

### 🔄 Manipulation Commands
- "Scale the trees by a factor of 1.5."
- "Rotate the car 90 degrees around the Y axis."
- "Move the bench forward by 2 meters."
- "Color the building blue."
- **Implementation**: Each operation is handled by dedicated methods in `ActionExecutioner.cs`.

### 🏗️ Creation Commands
- "Create a red cube."
- "Add a tree near the building."
- "Generate a blue sphere."
- **Implementation**: `ExecuteCreation` and `CreateObject` methods in `ActionExecutioner.cs`.

### 🗣️ Natural Language Prompts
- "I want the tree to be a little taller."
- "Turn the car upside down."
- "Make the bench smaller."
- "Move the trees closer to the buildings."
- "I want the buildings to rotate to the left."
- "Flip the bench backward."

⚠️ **Note**: The system uses LLMs to interpret voice commands.  
The result **may not exactly match your spoken prompt**, as the model may fill in missing details like direction, amount, or axis based on context and past patterns.

---

## 🧩 Technical Implementation

The system consists of several key components:

1. **Voice Recognition**: Uses Whisper for accurate speech-to-text transcription
2. **Command Interpretation**: Uses OpenAI's models to parse natural language into actionable commands
3. **Speech Correction**: Handles common misrecognitions like "read" → "red" or "beauty" → "building"
4. **Action Execution**: `ActionExecutioner.cs` processes parsed commands and performs operations
5. **Selection System**: Multiple selection methods (ray, sphere, voice) with filtering capabilities
6. **History Management**: Tracks object states for undo/redo functionality

---
## ❤️UI Instruction Panel

General XR interaction shown when User Enter the Scene

1. **Blue - HelpButton**: User could see more detailed sample voice commands
2. **Red- CloseButton**: User could close the panel (it will also disappear if user click on scene or user did not react for some time)
