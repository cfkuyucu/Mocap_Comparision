using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;

public class Web_Man : MonoBehaviour
{
    private class Kullanici
    {
        public int Kullanici_ID;
        public string Kullanici_Adi;
        public double Sol_Diz;
        public double Sag_Diz;
        public double Son_Veri_Zamani;
    }


    public int Secili_Kullanici_ID;

    public string Veri_Merkezi_Adresi   = "127.0.0.1";
    public int Veri_Merkezi_Port        = 8888;

    private Animator Animator;
    private Kullanici Kullanici_Bilgisi;

    private string Kayit_Dosyasi_Adi = Directory.GetCurrentDirectory() + "\\Assets\\Others\\Web_Man_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".motion";
    private StreamWriter Kayit_Dosyasi;

    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();

        Kayit_Dosyasi = File.CreateText(Kayit_Dosyasi_Adi);
        Kayit_Dosyasi.WriteLine("Tarih,Sag_Aci,Sol_Aci");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        StartCoroutine(Karakter_Verilerini_Webden_Al());
    }

    void OnAnimatorIK()
    {
        if(Kullanici_Bilgisi == null)
        {
            return;
        }

        Animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperLeg, Quaternion.Euler(new Vector3(0, 0, 0)));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightUpperLeg, Quaternion.Euler(new Vector3(0, 0, 0)));


        Animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerLeg,  Quaternion.Euler(new Vector3((float)Kullanici_Bilgisi.Sol_Diz, 0, 0)));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightLowerLeg, Quaternion.Euler(new Vector3((float)Kullanici_Bilgisi.Sag_Diz, 0, 0)));

        Kayit_Dosyasi.WriteLine("{0};{1};{2}", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_ffffff"), Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).localEulerAngles[0].ToString("000"), Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).localEulerAngles[0].ToString("000"));
    }

    void OnApplicationQuit()
    {
        Kayit_Dosyasi.Close();
    }

    IEnumerator Karakter_Verilerini_Webden_Al()
    {
        string Kullanici_Veri_URL = "http://" + Veri_Merkezi_Adresi + ":" + Veri_Merkezi_Port + "/Karakter_Bilgisi_Al?Kullanici_ID=" + Secili_Kullanici_ID.ToString();

        UnityWebRequest Web_Talebi = UnityWebRequest.Get(Kullanici_Veri_URL);
        yield return Web_Talebi.SendWebRequest();

        
        if (Web_Talebi.isNetworkError)
        {
            Debug.Log("Sunucu baglanti  hatasi: " + Web_Talebi.error);
        }
        else
        {
            string Sunucu_Cevabi = Web_Talebi.downloadHandler.text;

            Kullanici_Bilgisi = JsonUtility.FromJson<Kullanici>(Sunucu_Cevabi);
        }
    }



}
