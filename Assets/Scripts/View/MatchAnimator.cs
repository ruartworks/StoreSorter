using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchAnimator : MonoBehaviour
{
    [SerializeField] private AnimationCurve travelCurve;
    [SerializeField] private float travelDuration = 0.6f;
    [SerializeField] private GameObject matchPfxPrefab;

    public IEnumerator AnimateMatchTo(List<SpriteRenderer> matchedSRs, Vector3 endWorldPos, System.Action onComplete)
    {
        var sources = new List<SpriteRenderer>(3);
        foreach (var sr in matchedSRs)
            if (sr != null && sr.enabled && sr.sprite != null) sources.Add(sr);

        if (sources.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        if (travelCurve == null) travelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        int remaining = sources.Count;
        foreach (var sr in sources)
        {
            sr.enabled = false;

            var ghostGO = new GameObject("MatchGhost");
            var ghostSR = ghostGO.AddComponent<SpriteRenderer>();
            ghostSR.sprite = sr.sprite;
            ghostSR.sortingLayerID = sr.sortingLayerID;
            ghostSR.sortingOrder = Mathf.Max(sr.sortingOrder + 1000, 1000);
            ghostGO.transform.position = sr.transform.position;
            ghostGO.transform.localScale = sr.transform.lossyScale;

            StartCoroutine(FlyGhost(
                ghostSR,
                sr.transform.position,
                endWorldPos,
                () =>
                {
                    Destroy(ghostGO);
                    if (--remaining == 0)
                    {
                        if (matchPfxPrefab != null)
                            Instantiate(matchPfxPrefab, endWorldPos, Quaternion.identity);
                        onComplete?.Invoke();
                    }
                }
            ));
        }

        yield return null;
    }

    private IEnumerator FlyGhost(SpriteRenderer ghost, Vector3 start, Vector3 end, System.Action onDone)
    {
        Vector3 control = (start + end) * 0.5f + Vector3.up * 1.25f;
        float t = 0f;
        float dur = Mathf.Max(0.05f, travelDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float e = travelCurve.Evaluate(Mathf.Clamp01(t));

            Vector3 a = Vector3.Lerp(start, control, e);
            Vector3 b = Vector3.Lerp(control, end, e);
            ghost.transform.position = Vector3.Lerp(a, b, e);

            float pulse = 1f + 0.12f * Mathf.Sin(e * Mathf.PI);
            ghost.transform.localScale = new Vector3(pulse, pulse, 1f);

            yield return null;
        }

        onDone?.Invoke();
    }
}
