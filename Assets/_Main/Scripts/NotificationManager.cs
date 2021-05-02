using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NotificationManager : MonoBehaviour
{
    public Text notificationText;
    public GameObject textHolder;
    public GameObject notifHolder;
    public static NotificationManager instance = null;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void Notification(string s)
    {
        notifHolder.SetActive(true);
        notificationText.text = s;
        textHolder.transform.localScale = Vector3.zero;
        StartCoroutine(ScaleUp());
    }

    IEnumerator ScaleUp()
    {
        while (textHolder.transform.localScale.x < 1f)
        {
            yield return new WaitForSeconds(0.01f);
            textHolder.transform.localScale += Vector3.one * 0.05f;
        }
        yield return new WaitForSeconds(2f);
        notifHolder.SetActive(false);
    }

}
