# 🧠 XR Interaction System with LLMs

**Team Name**: *ARticulate*  
**Team Members**:  
- Cheng Yang (cy2748)  
- Linlin Zhang (lz2981)  
- Charlie Zou (jz3331)  
- Tans Rosen (njr2145)  

**Advisor**: Ben Yang

---

## 🔍 Overview

In this project, our team uses **Large Language Models (LLMs)** to improve user experience and create a more natural and intuitive way of interacting with virtual reality environments.

Users can use controllers (e.g., **ray selection** or **sphere selection**) or **voice commands** to:

- Select groups of objects  
- Refine their selections  
- Manipulate objects (e.g., **scale**, **rotate**, **move**)  

The system also allows users to **label** and **navigate** to different parts of the scene to support easier wayfinding. Our goal is to make interaction with complex 3D environments as seamless as having a conversation.

---

## 🎮 How to Use (Playmode Instructions)

### 🎯 Selection

#### 🔦 Ray Selection (Right Controller)
- Press **`Y`** to activate the right controller.
- Aim at an object.
- Press **`G`** to select it.

#### 🟠 Sphere Selection (Right Controller)
- Press **`Y`** to activate the right controller.
- Press **`N`** to spawn the selection sphere.

##### 🎚️ Adjust Sphere Size (Left Controller)
- Press **`T`** to activate the left controller.
- Press **`B`** to make the sphere bigger.
- Press **`N`** to make it smaller.

✅ Selected objects will have a **yellow highlight**.

---

### 🎙️ Voice Command Recording

After selecting objects, record your command in one of two ways:

#### Option 1: UI Buttons
- Click **Record** on the UI panel to begin.
- Click **Stop** to finish.

#### Option 2: Keyboard Shortcut
- Press **`B`** to **start recording**.
- Press **`B`** again to **stop recording**.

---

### 📍 Waypoint Creation (Left Controller)

- Press **`T`** to activate the left controller.
- When the ray turns **white**, **left-click** to create a waypoint.
- To cancel: click again without moving the ray.

---

### 🚀 Teleporting (Right Controller)

- Press **`Y`** to activate the right controller.
- When the ray turns **white** and a **spinning teleport icon** appears:
  - Press **`G`** to teleport.

⚠️ **First-time teleport bug**:  
If you fall underground, press **`Q`** to move up or **`E`** to move down. Once you move up and the **spinning teleport icon** appears again on the floor, teleporting will work normally

---

## 🗣️ Example Voice Commands

### ✅ Specific Object Manipulation
- “Scale the trees by a factor of 1.5.”
- “Rotate the car 90 degrees around the Y axis.”
- “Move the bench forward by 2 meters.”

### 🗣️ Natural Language Prompts
- “I want the tree to be a little taller.”
- “Turn the car upside down.”
- “Make the bench smaller.”
- “Move the trees closer to the buildings.”
- “I want the buildings to rotate to the left.”
- “Flip the bench backward.”

⚠️ **Note**: The system uses LLMs to interpret voice commands.  
The result **may not exactly match your spoken prompt**, as the model may fill in missing details like direction, amount, or axis based on context and past patterns.

---
