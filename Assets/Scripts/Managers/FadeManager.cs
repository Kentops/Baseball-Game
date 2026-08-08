using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    //Controls the screen fade

    public static FadeManager i;
    private Image myImage;
    private void Start()
    {
        if(i == null)
        {
            i = this;
        }
        else
        {
            Destroy(gameObject);
        }

        myImage = GetComponent<Image>();
    }

    public void fadeIn(float seconds)
    {
        StopAllCoroutines(); //No fading in while the fading out routine is still going and vice versa.
        StartCoroutine(goBack(seconds));
    }
    public void fadeOut(float seconds)
    {
        StopAllCoroutines();
        StartCoroutine(toBlack(seconds));
    }

    private IEnumerator goBack (float s)
    {
        while (myImage.color.a > 0)
        {
            myImage.color = new Color(0, 0, 0, myImage.color.a - s * Time.deltaTime);
            yield return null;
        }
    }
    private IEnumerator toBlack(float s)
    {
        while (myImage.color.a < 1)
        {
            myImage.color = new Color(0, 0, 0, myImage.color.a + s * Time.deltaTime);
            yield return null;
        }
    }

}
