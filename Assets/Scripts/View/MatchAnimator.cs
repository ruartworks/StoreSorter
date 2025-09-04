
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchAnimator : MonoBehaviour
{
    [Header("Travel")]
    [SerializeField] private AnimationCurve travelCurve;
    [SerializeField] private float travelDuration = 0.6f;

    [Header("PFX")]
    [SerializeField] private GameObject matchPfxPrefab;
    [SerializeField] private float pfxLifetime = 1.2f;

    [Header("Pick/Drop Feedback")]
    [SerializeField] private float pickScale = 1.12f;
    [SerializeField] private float dropPopScale = 1.08f;
    [SerializeField] private float pickDropDuration = 0.08f;

    [Header("Pre-Match Highlight")]
    [SerializeField] private float preHighlightDuration = 0.15f;
    [SerializeField] private float preHighlightScale = 1.1f;

    public IEnumerator AnimateMatchTo(List<SpriteRenderer> matchedSRs, Vector3 endWorldPos, System.Action onComplete)
    {
        var sources = new List<SpriteRenderer>(3);
        foreach (var sr in matchedSRs)
            if (sr != null && sr.sprite != null) sources.Add(sr);

        if (sources.Count == 0) { onComplete?.Invoke(); yield break; }
        if (travelCurve == null) travelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        yield return StartCoroutine(PreHighlight(sources));

        int remaining = sources.Count;
        foreach (var sr in sources)
        {
            if (matchPfxPrefab != null)
            {
                var p = Instantiate(matchPfxPrefab, sr.transform.position, Quaternion.identity);
                Destroy(p, pfxLifetime);
            }

            sr.enabled = false;

            var ghostGO = new GameObject("MatchGhost");
            var ghostSR = ghostGO.AddComponent<SpriteRenderer>();
            ghostSR.sprite = sr.sprite;
            ghostSR.sortingLayerID = sr.sortingLayerID;
            ghostSR.sortingOrder = Mathf.Max(sr.sortingOrder + 1000, 1000);
            ghostGO.transform.position = sr.transform.position;
            ghostGO.transform.localScale = sr.transform.lossyScale;

            StartCoroutine(FlyGhost(
                ghostSR, sr.transform.position, endWorldPos,
                () =>
                {
                    Destroy(ghostGO);
                    if (--remaining == 0) onComplete?.Invoke();
                }
            ));
        }
    }

    private IEnumerator PreHighlight(List<SpriteRenderer> srs)
    {
        float t = 0f;
        var origColors = new List<Color>(srs.Count);
        var origScales = new List<Vector3>(srs.Count);
        foreach (var sr in srs) { origColors.Add(sr.color); origScales.Add(sr.transform.localScale); }

        while (t < preHighlightDuration)
        {
            t += Time.deltaTime;
            float e = Mathf.Clamp01(t / preHighlightDuration);
            float scale = Mathf.Lerp(1f, preHighlightScale, e);
            float flash = Mathf.Sin(e * Mathf.PI);
            for (int i = 0; i < srs.Count; i++)
            {
                if (srs[i] == null) continue;
                srs[i].transform.localScale = origScales[i] * scale;
                srs[i].color = Color.Lerp(origColors[i], Color.white, flash);
            }
            yield return null;
        }
        for (int i = 0; i < srs.Count; i++) if (srs[i] != null) srs[i].color = origColors[i];
    }

    private IEnumerator FlyGhost(SpriteRenderer ghost, Vector3 start, Vector3 end, System.Action onDone)
    {
        Vector3 control = (start + end) * 0.5f + Vector3.up * 1.25f;
        float t = 0f, dur = Mathf.Max(0.05f, travelDuration);

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

    public IEnumerator PickPulse(SpriteRenderer sr)
    {
        if (sr == null) yield break;
        Vector3 baseScale = sr.transform.localScale;
        float t = 0f;
        while (t < pickDropDuration)
        {
            t += Time.deltaTime;
            float e = Mathf.Clamp01(t / pickDropDuration);
            sr.transform.localScale = baseScale * Mathf.Lerp(1f, pickScale, e);
            yield return null;
        }
    }

    public IEnumerator DropPulse(SpriteRenderer sr)
    {
        if (sr == null) yield break;
        Vector3 baseScale = sr.transform.localScale;
        float t = 0f;
        while (t < pickDropDuration)
        {
            t += Time.deltaTime;
            float e = Mathf.Clamp01(t / pickDropDuration);
            float s = 1f + (dropPopScale - 1f) * Mathf.Sin(e * Mathf.PI);
            sr.transform.localScale = baseScale * s;
            yield return null;
        }
        sr.transform.localScale = baseScale;
    }
}
