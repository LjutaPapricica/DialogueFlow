using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

namespace DialogueFlow {
  public class DialogueController : MonoBehaviour {
    public static DialogueController current {
      get; private set;
    }

    public DialogueFlowAsset flow {
      get; private set;
    }
    public int flowIndex {
      get; private set;
    }

    private bool messageIsOpen = false;
    private bool messageIsWaiting = false;
    private bool requestMessageClose = false;

    private bool choicesIsOpen = false;

    private GameObject messageObj;
    private Animator messageAnimator;
    private ITextAnimator messageText;
    private TextMeshProUGUI messageName;

    private GameObject choiceObj;

    private GameObject emitter;

    private void Awake() {
      if (current == null) {
        current = this;
      } else if (current != this) {
        Destroy(this);
        return;
      }
      Init();
    }

    private void OnDestroy() {
      if (current == this) {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        current = null;
      }
    }

    private void Init() {
      messageObj = transform.Find("MessageBox").gameObject;
      messageAnimator = messageObj.GetComponent<Animator>();
      messageText = transform.Find("MessageBox/MessageText").GetComponent<ITextAnimator>();
      messageName = transform.Find("MessageBox/MessageName").GetComponent<TextMeshProUGUI>();
      messageObj.SetActive(false);
      choiceObj = transform.Find("Choices").gameObject;
      choiceObj.SetActive(false);
      flowIndex = -1;
      SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
      Reset();
    }

    private void Reset() {
      flow = null;
      flowIndex = -1;
      emitter = null;
      messageIsOpen = false;
      messageIsWaiting = false;
      requestMessageClose = false;
      choicesIsOpen = false;
      messageObj.SetActive(false);
      choiceObj.SetActive(false);
    }

    private void Update() {
      if (flow == null) {
        return;
      }
      if (messageIsOpen) {
        if (requestMessageClose) {
          StopCoroutine("EndFlow");
          StartCoroutine("EndFlow");
          return;
        }
        if (messageIsWaiting && !choicesIsOpen && Input.GetButtonDown("Submit")) {
          GoToNext();
        }
      }
    }

    public bool IsRunning() {
      return flow != null;
    }

    public void SetFlow(DialogueFlowAsset newFlow, GameObject caller = null) {
      if (flow != null) {
        if (emitter != null) {
          emitter.SendMessage("OnFlowEnded", flow, SendMessageOptions.DontRequireReceiver);
        }
        StopCoroutine("EndFlow");
      }
      requestMessageClose = false;
      flow = newFlow;
      flowIndex = -1;
      emitter = caller;
      GoToNext();
    }

    private void GoToNext(int outId = 0) {
      var currentNode = flow.Get(flowIndex);
      flowIndex = currentNode.connectionsOut[outId];
      ProcessNode(flow.Get(flowIndex));
    }

    private void ProcessNode(DialogueNode node) {
      if (node == null) {
        requestMessageClose = true;
        return;
      }
      if (emitter != null) {
        emitter.SendMessage("OnFlowChange", node.id, SendMessageOptions.DontRequireReceiver);
      }
      switch (node.type) {
        case DialogueNode.Type.Start:
          GoToNext();
          break;
        case DialogueNode.Type.Message:
          StartCoroutine(SetMessage(node));
          break;
        case DialogueNode.Type.Choice:
          SetChoices(node);
          break;
        case DialogueNode.Type.Event:
          if (emitter != null) {
            emitter.SendMessage("OnFlowEvent", node.eventMsg, SendMessageOptions.DontRequireReceiver);
          }
          GoToNext();
          break;
      }
    }

    private IEnumerator SetMessage(DialogueNode node) {
      messageObj.SetActive(true);
      messageText.SetText(node.text);
      messageName.text = node.name;
      if (messageAnimator && !messageIsOpen) {
        messageAnimator.SetBool("Open", true);
        float duration = messageAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);
      }
      messageIsOpen = true;
      messageIsWaiting = false;
      messageText.StartAnimating();
      messageText.OnComplete = () => {
        messageIsWaiting = true;
      };
    }

    private void SetChoices(DialogueNode node) {
      choiceObj.SetActive(true);
      choicesIsOpen = true;
      Transform choiceTr = choiceObj.transform;
      for (int i = 0; i < node.choices.Count; i++) {
        Transform buttonObj;
        if (i >= choiceTr.childCount) {
          buttonObj = Instantiate(choiceTr.GetChild(0), choiceTr);
        } else {
          buttonObj = choiceTr.GetChild(i);
        }
        buttonObj.gameObject.SetActive(true);
        Button button = buttonObj.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        int j = i;
        button.onClick.AddListener(() => {
          OnChoice(j);
        });
        var text = buttonObj.Find("Text").GetComponent<TextMeshProUGUI>();
        text.text = node.choices[i];
        if (i == 0) {
          EventSystem.current.SetSelectedGameObject(buttonObj.gameObject);
        }
      }
    }

    private void OnChoice(int i) {
      choiceObj.SetActive(false);
      choicesIsOpen = false;
      Transform choiceTr = choiceObj.transform;
      for (int j = 0; j < choiceTr.childCount; j++) {
        choiceTr.GetChild(j).gameObject.SetActive(false);
      }
      GoToNext(i);
    }

    private IEnumerator EndFlow() {
      if (emitter != null) {
        emitter.SendMessage("OnFlowEnded", flow, SendMessageOptions.DontRequireReceiver);
      }
      requestMessageClose = false;
      if (messageAnimator) {
        messageAnimator.SetBool("Open", false);
        float duration = messageAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);
      }
      messageObj.SetActive(false);
      messageIsOpen = false;
      messageIsWaiting = false;
      choicesIsOpen = false;
      flow = null;
      flowIndex = -1;
    }

  }
}
