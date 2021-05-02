using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResSetter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Screen.SetResolution(960, 540, false);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            Screen.SetResolution(1920, 1080, true);

        }
    }
}
