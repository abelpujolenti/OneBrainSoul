using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URPManager : MonoBehaviour
{
    public Dictionary<string, FullScreenPassRendererFeature> fullscreenPasses = new Dictionary<string, FullScreenPassRendererFeature>();

    [SerializeField] AnimationCurve braincellSwitchCurve;
    [SerializeField] AnimationCurve chargeRunCurve;
    [SerializeField] AnimationCurve chargeBounceCurve;

    Coroutine chargeRunCoroutine;

    private void Start()
    {
        GetFullscreenPasses();
    }

    public void ChargeRunEffect(float t)
    {
        var pass = fullscreenPasses["Charge"];
        chargeRunCoroutine = StartCoroutine(EffectCoroutine(pass, t, chargeRunCurve));
    }

    public void ChargeCollideEffect(float t)
    {
        var pass = fullscreenPasses["Charge"];
        if (chargeRunCoroutine != null) StopCoroutine(chargeRunCoroutine);
        StartCoroutine(ChargeCollideCoroutine(pass, t, chargeBounceCurve));
    }

    private IEnumerator ChargeCollideCoroutine(FullScreenPassRendererFeature pass, float duration, AnimationCurve curve)
    {
        pass.SetActive(true);
        Color cellColor = pass.passMaterial.GetColor("_CellColor");
        Color intenseCellColor = new Color(1f, .01f, 0f);
        float fade = pass.passMaterial.GetFloat("_Fade");
        float intenseFade = .8f;
        float t = 0f;
        while (t < duration)
        {
            float p = curve.Evaluate(t / duration);
            pass.passMaterial.SetFloat("_Progress", p);
            pass.passMaterial.SetColor("_CellColor", new Color(Mathf.Lerp(intenseCellColor.r, cellColor.r, p), Mathf.Lerp(intenseCellColor.g, cellColor.g, p), Mathf.Lerp(intenseCellColor.b, cellColor.b, p), 1f));
            pass.passMaterial.SetFloat("_Fade", Mathf.Lerp(intenseFade, fade, p));
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
        }
        pass.passMaterial.SetFloat("_Fade", fade);
        pass.passMaterial.SetColor("_CellColor", cellColor);

        pass.SetActive(false);
    }

    public void BraincellSwitchTransition(float t)
    {
        var pass = fullscreenPasses["BraincellSwitch"];
        StartCoroutine(EffectCoroutine(pass, t, braincellSwitchCurve));
    }

    private IEnumerator EffectCoroutine(FullScreenPassRendererFeature pass, float duration, AnimationCurve curve)
    {
        pass.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            pass.passMaterial.SetFloat("_Progress", curve.Evaluate(t / duration));
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
        }

        pass.SetActive(false);
    }

    private void GetFullscreenPasses()
    {
        var handledDataObjects = new List<ScriptableRendererData>();

        int levels = QualitySettings.names.Length;
        for (int level = 0; level < levels; level++)
        {
            var asset = QualitySettings.GetRenderPipelineAssetAt(level) as UniversalRenderPipelineAsset;
            var data = getDefaultRenderer(asset);

            if (handledDataObjects.Contains(data))
            {
                continue;
            }
            handledDataObjects.Add(data);

            foreach (var feature in data.rendererFeatures)
            {
                if (feature is FullScreenPassRendererFeature)
                {
                    if (!fullscreenPasses.ContainsKey(feature.name))
                    {
                        fullscreenPasses.Add(feature.name, feature as FullScreenPassRendererFeature);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the default renderer index.
    /// Thanks to: https://discussions.unity.com/t/842637/2
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    static int getDefaultRendererIndex(UniversalRenderPipelineAsset asset)
    {
        return (int)typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(asset);
    }

    /// <summary>
    /// Gets the renderer from the current pipeline asset that's marked as default.
    /// Thanks to: https://discussions.unity.com/t/842637/2
    /// </summary>
    /// <returns></returns>
    static ScriptableRendererData getDefaultRenderer(UniversalRenderPipelineAsset asset)
    {
        if (asset)
        {
            ScriptableRendererData[] rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset)
                    .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(asset);
            int defaultRendererIndex = getDefaultRendererIndex(asset);

            return rendererDataList[defaultRendererIndex];
        }
        else
        {
            Debug.LogError("No Universal Render Pipeline is currently active.");
            return null;
        }
    }
}
