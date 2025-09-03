
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CellView2D : MonoBehaviour
{
    [Header("Visible slots (3)")]
    public SpriteRenderer[] visibleSR = new SpriteRenderer[3];
    public BoxCollider2D[]  visibleHotspots = new BoxCollider2D[3];

    [Header("Waiting slots (3) - non interactable")]
    public SpriteRenderer[] waitingSR = new SpriteRenderer[3];

    [HideInInspector] public int cellIndex;

    public void Init(int index) => cellIndex = index;

    public void Bind(CellModel model, System.Func<int, Sprite> getSprite)
    {
        for (int i = 0; i < 3; i++)
        {
            bool has = model.visible[i] != -1;
            visibleSR[i].enabled = has;
            visibleSR[i].sprite  = has ? getSprite(model.visible[i]) : null;
        }
        for (int i = 0; i < 3; i++)
        {
            bool has = model.waiting[i] != -1;
            waitingSR[i].enabled = has;
            waitingSR[i].sprite  = has ? getSprite(model.waiting[i]) : null;
        }
    }
}
