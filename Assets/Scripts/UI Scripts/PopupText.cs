using System.Collections;
using TMPro;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    public void displayText(string text, int seconds)
    {
        StartCoroutine(displayCoroutine(text, seconds));
    }

    private IEnumerator displayCoroutine(string t, int s)
    {
        GetComponent<TextMeshProUGUI>().text = t;
        yield return new WaitForSeconds(s);
        Destroy(gameObject);
    }

    private void onReset() //Hide when the scene changes or plays start
    {
        Destroy(gameObject);
    }

    private void OnEnable() //To make sure these leave when we want them to
    {
        Ballpark.resetField += onReset;
        Ballpark.pitcherWinds += onReset;

        //If multiple exist, move previous up on screen
        PopupText[] otherTexts = FindObjectsByType<PopupText>(FindObjectsSortMode.InstanceID);
        for(int i = 1; i <otherTexts.Length; i++) //1 to not count ourself (we are first apparently)
        {
            otherTexts[i].GetComponent<RectTransform>().anchoredPosition += Vector2.up * 100;
        }
    }
    private void OnDisable()
    {
        Ballpark.resetField -= onReset;
        Ballpark.pitcherWinds -= onReset;
    }
}
