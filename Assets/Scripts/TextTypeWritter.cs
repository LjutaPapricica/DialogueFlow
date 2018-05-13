using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DialogueFlow {
  [RequireComponent(typeof(TextMeshProUGUI))]
  public class TextTypeWritter : MonoBehaviour, ITextAnimator {
    [Tooltip("Delay between letters in seconds")]
    public float letterDelay;

    private int textLength = 0;
    private bool animating = false;
    private float timer;
    private int frame = 0;

    private TextMeshProUGUI messageText;

    public System.Action OnComplete {
      get; set;
    }

    private void Awake() {
      messageText = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string text) {
      messageText.text = text;
      messageText.maxVisibleCharacters = textLength = 0;
    }

    public void StartAnimating() {
      messageText.maxVisibleCharacters = textLength = 0;
      animating = true;
      frame = 0;
    }

    private void Update() {
      if (!animating) {
        return;
      }
      while (animating) {
        if (frame++ > 1 && Input.GetButtonDown("Submit")) {
          messageText.maxVisibleCharacters = messageText.textInfo.characterCount;
          animating = false;
          break;
        }
        if (timer > 0.0f) {
          timer -= Time.deltaTime;
          break;
        } else {
          timer = letterDelay;
        }
        messageText.maxVisibleCharacters = ++textLength;
        animating = textLength < messageText.textInfo.characterCount;
      }
      if (!animating && OnComplete != null) {
        OnComplete();
      }
    }
  }
}