using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static System.TimeZoneInfo;

public class BGScript : MonoBehaviour
{
    public float timeToBlack;
    public float timeToBeBlack;
    public float timeToBG;
    public float timeToInvisible;

    public Image bg1;
    public Image transparentImage;

    public void BlackTranstion(Sprite nextBG)
    {
        StartCoroutine(BlackTransitionCoroutine(nextBG));
        
    }
    IEnumerator BlackTransitionCoroutine(Sprite bg)
    {
        float timer = 0f;

        while (timer < timeToBlack)
        {
            timer += Time.deltaTime;
            float progress = timer / timeToBlack;

            bg1.color = new Color(1f - progress, 1f - progress, 1f - progress);

            yield return null;
        }
        yield return new WaitForSeconds(timeToBlack);
        bg1.sprite = bg;
        
        timer = 0f;

        while (timer < timeToBlack)
        {
            timer += Time.deltaTime;
            float progress = timer / timeToBlack;

            bg1.color = new Color(progress, progress, progress);

            yield return null;
        }

    }
    public void TransparentTransition(Sprite nextBG)
    {
        StartCoroutine(TransparentTransitionCoroutine(nextBG));
    }
    IEnumerator TransparentTransitionCoroutine(Sprite bg)
    {
        transparentImage.gameObject.SetActive(true);
        transparentImage.color = new Color(1, 1, 1, 0);
        transparentImage.sprite = bg;

        float timer = 0f;

        while (timer < timeToInvisible)
        {
            timer += Time.deltaTime;
            float progress = timer / timeToBlack;

            transparentImage.color = new Color(1f, 1f, 1f, progress);

            yield return null;
        }
        transparentImage.gameObject.SetActive(false);
        bg1.sprite = bg;
    }
    void Update()
    {
        
    }
}
