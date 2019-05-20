using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

public class Gercek_Man : MonoBehaviour
{

    public static class Eklem_Degerleri
    {
        public static Vector3 Sol_Bacak     = new Vector3(-90, 0, 0);
        public static Vector3 Sag_Bacak     = new Vector3(-90, 0, 0);
        public static Vector3 Sol_Diz       = new Vector3(90, 0, 0);
        public static Vector3 Sag_Diz       = new Vector3(90, 0, 0);

        public static Vector3 Sol_Omuz      = new Vector3(0, 0, 0);
        public static Vector3 Sag_Omuz      = new Vector3(0, 0, 0);
        public static Vector3 Sol_Dirsek    = new Vector3(45, 90, -90);
        public static Vector3 Sag_Dirsek    = new Vector3(45, -90, 90);

        public static Vector3 Govde         = new Vector3(0, 150, 0);
    }
    
    public GameObject   Deger_Gosterici;

    public int          Port = 4545;

    private Thread      Paket_Yakalayici_Thread;
    private UdpClient   UDP_Client;
    private Animator    Animator;
    private string[]    Degerler;

    private string Kalan_Veri;
    private string Veri_Tamponu;
    public char Paket_Baslangici = '!';
    public char Paket_Sonu = '#';


    public void Start()
    {
        Animator = GetComponent<Animator>();

        Thread_Baslat();
    }


    private void Thread_Baslat()
    {
        Paket_Yakalayici_Thread = new Thread(new ThreadStart(Paket_Yakalayici));
        Paket_Yakalayici_Thread.IsBackground = true;
        Paket_Yakalayici_Thread.Start();
    }

    private void Paket_Yakalayici()
    {
        UDP_Client = new UdpClient(Port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, Port);
                byte[] Okunan_Veri = UDP_Client.Receive(ref anyIP);
                string Okunan_Mesaj = Encoding.UTF8.GetString(Okunan_Veri);

                Veri_Tamponu += Okunan_Mesaj;

                Kalan_Veri = Veri_Tamponu.Split(Paket_Sonu).Last();
                Regex Veri_Arayicisi = new Regex("(?<=!)(.*?)(?=#)");
                MatchCollection Bulunanlar = Veri_Arayicisi.Matches(Veri_Tamponu);


                Match Son_Bulunan = Bulunanlar.OfType<Match>().LastOrDefault();
                Veri_Tamponu = Kalan_Veri;

            

                if (Son_Bulunan == null || !Son_Bulunan.Success)
                {
                    return;
                }

                Degerler = Son_Bulunan.Value.Split(',');

                Eklem_Degerleri.Sol_Diz.x     = float.Parse(Degerler[0].Replace('.', ','));
                Eklem_Degerleri.Sag_Diz.x     = float.Parse(Degerler[1].Replace('.', ','));
                Eklem_Degerleri.Sol_Dirsek.x  = float.Parse(Degerler[2].Replace('.', ','));
                Eklem_Degerleri.Sag_Dirsek.x  = float.Parse(Degerler[3].Replace('.', ','));

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Animator.SetBoneLocalRotation(HumanBodyBones.Hips, Quaternion.Euler(Eklem_Degerleri.Govde));

        Animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperLeg,  Quaternion.Euler(Eklem_Degerleri.Sol_Bacak));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightUpperLeg, Quaternion.Euler(Eklem_Degerleri.Sag_Bacak));
        Animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerLeg,  Quaternion.Euler(Eklem_Degerleri.Sol_Diz));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightLowerLeg, Quaternion.Euler(Eklem_Degerleri.Sag_Diz));

        Animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperArm,  Quaternion.Euler(Eklem_Degerleri.Sol_Omuz));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightUpperArm, Quaternion.Euler(Eklem_Degerleri.Sag_Omuz));
        Animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerArm,  Quaternion.Euler(Eklem_Degerleri.Sol_Dirsek));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightLowerArm, Quaternion.Euler(Eklem_Degerleri.Sag_Dirsek));
    }


}
