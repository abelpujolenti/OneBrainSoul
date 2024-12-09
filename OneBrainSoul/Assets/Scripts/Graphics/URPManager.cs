using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : Singleton<PostProcessingManager>
{
    public Dictionary<string, FullScreenPassRendererFeature> fullscreenPasses = new Dictionary<string, FullScreenPassRendererFeature>();

    [SerializeField] AnimationCurve braincellSwitchCurve;
    [SerializeField] AnimationCurve chargeRunCurve;
    [SerializeField] AnimationCurve chargeBounceCurve;
    [SerializeField] AnimationCurve switchModeCurve;
    [SerializeField] AnimationCurve switchModeOutCurve;
    [SerializeField] AnimationCurve damageCurve;

    Coroutine chargeRunCoroutine;
    bool switchMode = false;

    private void Start()
    {
        GetFullscreenPasses();
    }

    public void DamageEffect(float t)
    {
        var pass = fullscreenPasses["Damage"];
        StartCoroutine(EffectCoroutine(pass, t, damageCurve));
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
            t += Time.fixedUnscaledDeltaTime;
        }
        pass.passMaterial.SetFloat("_Fade", fade);
        pass.passMaterial.SetColor("_CellColor", cellColor);

        pass.SetActive(false);
    }

    public void EnableSwitchMode(float duration, float outDuration)
    {
        switchMode = true;
        StartCoroutine(SwitchModeCoroutine(switchModeCurve, switchModeOutCurve, duration, outDuration));
    }

    public void DisableSwitchMode()
    {
        switchMode = false;
    }

    private IEnumerator SwitchModeCoroutine(AnimationCurve inCurve, AnimationCurve outCurve, float duration, float outDuration)
    {
        var bPass = fullscreenPasses["BraincellSwitch"];
        bPass.SetActive(true);
        var sPass = fullscreenPasses["SwitchMode"];
        sPass.SetActive(true);
        float bFade = bPass.passMaterial.GetFloat("_Fade");
        float sFade = sPass.passMaterial.GetFloat("_Fade");

        float t = 0f;
        while (t < duration && switchMode)
        {
            bPass.SetActive(true);
            float c = inCurve.Evaluate(t / duration);
            bPass.passMaterial.SetFloat("_Progress", c);
            bPass.passMaterial.SetFloat("_Fade", Mathf.Lerp(bFade, 0f, c));
            sPass.passMaterial.SetFloat("_Progress", c);
            sPass.passMaterial.SetFloat("_Fade", Mathf.Lerp(sFade, 0f, c));
            yield return new WaitForSecondsRealtime(0.02f);
            t += 0.02f;
        }
        yield return new WaitUntil(() => !switchMode);

        bPass.passMaterial.SetFloat("_Fade", bFade);
        sPass.passMaterial.SetFloat("_Fade", sFade);
        bPass.SetActive(false);
        t = 0f;
        while (t < outDuration && !switchMode)
        {
            float c = outCurve.Evaluate(t / outDuration);
            sPass.passMaterial.SetFloat("_Progress", c);
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
        }
        if (t >= outDuration) {
            sPass.SetActive(false);
        }
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
