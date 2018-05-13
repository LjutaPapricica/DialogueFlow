# Dependenices
- [Text Mesh Pro](https://assetstore.unity.com/packages/essentials/beta-projects/textmesh-pro-84126)

# How to setup DialogueCanvas
Create a GameObject with the `DialogueController` component. This GameObject requires 2 children; 1 named `MessageBox` and the other named `Choices`. 

- The `MessageBox` GameObject needs 2 children; 1 named `MessageText` and the other named `Name`.
If `MessageBox` has an `Animator` component, it will set a parameter named `Open` to true/false when openning/closing.
  - `MessageText` requires a component that inherits from `ITextAnimator` and a `Text MeshPro UGUI` component
  - `Name` requires a `Text MeshPro UGUI` component
- The `Choices` GameObject needs 1 child.
  - The child needs a Button component and a child with a `Text MeshPro UGUI` component

A prefab of this is already setup in the Prefabs folder.

# How to create a Dialogue Flow
First create a Dialogue Flow click on `Assets > Create > Dialogue Flow > Dialogue Flow Asset`. Then open the Dialogue Flow Window `Window > Dialogue Flow` now select your Dialogue Flow Asset if it isn't selected and you should see a node labeled "Start".
To create new nodes, just right click and select the type of node to add; Dialogue, Choice or Event.

- Dialogue - Shows the message in the message box
- Choice - Shows a list of choices to select from
- Event - Calls the `OnFlowEvent` message and passes the name value as the parameter

The nodes values can be modified in the inspector.

# How to run a Dialogue Flow
To run a dialogue you just need to call a function in the `DialogueController` singleton:
~~~csharp
// where flowAsset is type DialogueFlow.DialogueFlowAsset
DialogueFlow.DialogueController.current.SetFlow(flowAsset);
~~~
If a flow is already running it will be replaced with the new one.

# Dialogue Flow Messages
Dialogue Flow sends 3 messages to a GameObject
- `OnFlowChange` - Is called when the flow changes index
  ~~~csharp
  void OnFlowChange(int index) {
    Debug.Log("Now starting index " + index);
  }
  ~~~
- `OnFlowEvent` - Is called when the flow reaches an `Event` node
  ~~~csharp
  void OnFlowEvent(string name) {
    Debug.Log("The event name was " + name);
  }
  ~~~
- `OnFlowEnded` - Is called when the flow ended or was replaced
  ~~~csharp
  void OnFlowEnded(DialogueFlow.DialogueFlowAsset flow) {
    Debug.Log(flow.name + " just ended");
  }
  ~~~

In order to receive these messages you need to pass call the `SetFlow` function like:
~~~csharp
// where flowAsset is type DialogueFlowAsset
DialogueFlow.DialogueController.current.SetFlow(flowAsset, gameObject);
~~~
The messages will be sent to the gameObject passed in the 2nd parameter.