
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text progressText;

    public RectTransform IconRect => icon.rectTransform;

    public void Bind(Sprite sprite, int remaining)
    {
        icon.sprite = sprite;
        UpdateRemaining(remaining);
    }

    public void UpdateRemaining(int remaining)
    {
        progressText.text = remaining.ToString();// + " left"
    }
}
