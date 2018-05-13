*Only documented public fields/methods*

# Dialogue Controller
This class is a singleton that is used to control the dialogue UI.

## Fields/Properties
- `DialogueController current` 
  
  Returns the current singleton instance

- `DialogueFlowAsset flow` 
  
  Returns the current flow that is running. Value is nulled when the flow ends

- `int flowIndex`
  
  Returns the index that the flow is currently at

## Methods
- `void SetFlow(DialogueFlowAsset newFlow, GameObject caller = null)`
  
  Starts a flow. If a flow was running it will be stopped. If a caller is passed in, it will receive Messages.
 
## Messages
Dialogue Controller will send messages to a gameObject that was passed in the `SetFlow` method call. The messages are:
- `OnFlowChange`

  Is called every time the flow index changes. It passes the new index as the parameter.

- `OnFlowEvent`

  Is called when the flow reaches an `Event` node type. It passes the event value as the parameter

- `OnFlowEnded`

  Is called when the flow ends. It passes the Flow Asset as the parameter.

# DialogueFlowAsset / DialogueNode
Should only be modified from the Editor scripts

# ITextAnimator
interface used for setting text inside a `TextMeshProUGUI`.
## Fields/Properties
- `OnComplete`

  A System Action that is needs to be called when it's done animating so the DialogueController will know.

## Methods
- `void SetText(string text)`

  The function that will be used to pass what text needs to be drawn.

- `void StartAnimating()`

  DialogueController will call this when it's ready to start drawing the text.