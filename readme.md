# How to install
Copy the Assets folder to somewhere inside your projects Assets folder. You may rename the folder to whatever you'd like. The Examples folder is optional.
It also requires [Text Mesh Pro](https://assetstore.unity.com/packages/essentials/beta-projects/textmesh-pro-84126)

# About
**Dialogue Flow** is a tool to create quick dialogues in Unity 2018 (untested in older versions)

![Editor Gif](https://user-images.githubusercontent.com/9346563/39963438-ef18c51e-561f-11e8-866a-e06eb7b66246.gif)

This is just something I created just to have something I made with Unity. I wouldn't really recommend using this as is. I would recommend building off this and probably changing the `DialogueNode.eventMsg` to a struct with like an enum + some value. That way you could create 1 single `DialogueEmitter` with some sort of switch statement in the `OnFlowEvent` method so you wouldn't need to create a different emitter for each dialogue flow that has an event.

For more information [read here](Assets/quickGuide.md).
