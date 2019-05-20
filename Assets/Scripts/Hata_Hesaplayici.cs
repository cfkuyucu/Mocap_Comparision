using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hata_Hesaplayici : MonoBehaviour
{
    public GameObject Referans_Value;
    public GameObject Textile_Value;
    public GameObject Kinect_Value;
    public GameObject RGB_Value;
    public GameObject PN_Value;

    private TextMesh Referans_Text;
    private TextMesh Textile_Text;
    private TextMesh Kinect_Text;
    private TextMesh RGB_Text;
    private TextMesh PN_Text;

    // Start is called before the first frame update
    void Start()
    {
        if (Referans_Value == null || Textile_Value == null || Kinect_Value == null || RGB_Value == null || PN_Value == null)
        {
            return;
        }

        Referans_Text   = Referans_Value.GetComponent<TextMesh>();
        Textile_Text    = Textile_Value.GetComponent<TextMesh>();
        Kinect_Text     = Kinect_Value.GetComponent<TextMesh>();
        RGB_Text        = RGB_Value.GetComponent<TextMesh>();
        PN_Text         = PN_Value.GetComponent<TextMesh>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Referans_Text == null || Textile_Text == null || Kinect_Text == null || RGB_Text == null || PN_Text == null)
        {
            return;
        }





    }
}
