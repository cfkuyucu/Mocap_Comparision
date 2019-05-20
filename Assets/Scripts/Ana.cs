using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ana : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Cikis_Kontrol();
    }

    private void Cikis_Kontrol()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
