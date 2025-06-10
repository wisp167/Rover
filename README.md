# Rover

This project features an autonomous Rover trained in a simulated 3D environment using Unity's ML-Agents toolkit. Through reinforcement learning, the Rover acquires behaviors like obstacle avoidance and target seeking. A key focus is the potential for "sim-to-real" transfer, allowing learned policies from the simulation to be applied to real-world robotic platforms. This repository provides the Unity project and configurations for the Rover's environment and AI training.

## Short Demonstration

https://github.com/user-attachments/assets/7c5aac23-71f2-43d6-bd8c-7eba948a6b43

## Installation

1. Clone the repo
> https://github.com/wisp167/Rover.git
2. Download and install Unity Hub and Editor
    * Download and install Unity Hub from https://unity.com/unity-hub 
    * Once Hub is installed, navigate to the "Installs" tab and install the Unity Editor. For the best compatibility with this project, I recommend version 6000.0.39f1.
    * Some systems may encounter bug with endless editor installation. In this case you can download the engine manually from https://unity.com/releases/editor/archive and then add the downloaded folder path in Unity Hub's "Installs" tab.
3. Add Project to Unity Hub In the "Projects" tab of Unity Hub, click "Add" and then "Add project from disk". Point it to the directory where you cloned the GitHub repository.
4. Open and Play the Scene Once the project opens in the Unity Editor, drag the Terrain and Post-process (optional) objects from the Assets/Prefabs folder into your main scene, click Play.

## Dependencies for Training
Project uses addon ml-agents for training as it provides API between Unity environment and python and also has implemented RL algorithms.

* For installation, please follow the official guide in the ML-Agents web documentation: https://unity-technologies.github.io/ml-agents/Installation/ 
   * I strongly recommend using python version 3.10.12
   * Ensure using version 1.0.0 of ML-Agents for full compatibility.
   * After installing the ML-Agents Python package, replace the files from the mlagents folder in this project with the corresponding files in your installed ml-agents Python package directory.

Replace files from mlagents folder of the project in installed ml-agents python package.

## Training

*Instructions coming soon!*
