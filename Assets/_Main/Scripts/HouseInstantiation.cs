using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseInstantiation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.transform.localScale = Vector3.zero;
        StartCoroutine(ScaleUp());
    }


    IEnumerator ScaleUp()
    {
        while (this.transform.localScale != Vector3.one * 0.1f)
        {
            yield return new WaitForSeconds(0.01f);
            transform.localScale += Vector3.one * 0.01f;
        }
    }
}
