
namespace DialogueFlow {
  public interface ITextAnimator {
    System.Action OnComplete { get; set; }
    void SetText(string text);
    void StartAnimating();
  }
}