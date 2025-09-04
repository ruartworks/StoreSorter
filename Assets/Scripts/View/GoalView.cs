using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text progressText;

    public RectTransform IconRect => icon.rectTransform;

    public void Bind(Sprite sprite, int current, int target)
    {
        icon.sprite = sprite;
        UpdateProgress(current, target);
    }

    public void UpdateProgress(int current, int target)
    {
        progressText.text = $"{current}/{target}";
    }
}
