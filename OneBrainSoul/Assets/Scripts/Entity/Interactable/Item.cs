using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    private SpriteRenderer sprite;
    private Light pointLight;
    Vector3 startSpritePosition;

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        pointLight = GetComponent<Light>();
        startSpritePosition = sprite.transform.localPosition;
    }
    void LateUpdate()
    {
        sprite.transform.localPosition = startSpritePosition + Vector3.up * Mathf.Sin(Time.time * 1.5f) * 0.4f;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Pickup(other.gameObject);
            StartCoroutine(PickupDeleteCoroutine());
        }
    }
    protected virtual void Pickup(GameObject collider)
    {
        //AudioManager.instance.PlayOneShot(FMODEvents.instance.itemPickup, transform.position);
    }

    private IEnumerator PickupDeleteCoroutine()
    {
        yield return SetScaleCoroutine(0f, .3f);
        StartCoroutine(SetLightIntensityCoroutine(0f, .3f));
        Destroy(this.gameObject);
    }

    private IEnumerator SetScaleCoroutine(float end, float t)
    {
        float start = sprite.transform.localScale.y;
        float elapsedT = 0f;
        while (elapsedT <= t)
        {
            float s = Mathf.SmoothStep(start, end, Mathf.Pow(elapsedT / t, 2f));
            sprite.transform.transform.localScale = new Vector3(s, s, s);
            yield return new WaitForFixedUpdate();
            elapsedT += Time.fixedDeltaTime;
        }
        sprite.transform.transform.localScale = new Vector3(end, end, end);
    }

    private IEnumerator SetLightIntensityCoroutine(float end, float t)
    {
        float start = pointLight.intensity;
        float elapsedT = 0f;
        while (elapsedT <= t)
        {
            pointLight.intensity = Mathf.SmoothStep(start, end, elapsedT / t);
            yield return new WaitForFixedUpdate();
            elapsedT += Time.fixedDeltaTime;
        }
        pointLight.intensity = end;
    }
}
