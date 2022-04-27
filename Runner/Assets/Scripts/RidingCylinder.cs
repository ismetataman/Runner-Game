using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RidingCylinder : MonoBehaviour
{
    private bool _filled; //silindir tamamen doldu mu dolmadi mi onu tutan degisken
    private float _value; // slilindirin sayisal olarak ne kadar doldugunu tutan degisken

    public void IncrementCylinderVolume(float value)
    {
        _value += value; // aldigimiz valueyu silindirin boyutuna ekleme
        if(_value >1)
        {
            float leftValue = _value -1;
            int cylinderCount = PlayerController.Current.cylinders.Count;
            transform.localPosition = new Vector3(transform.localPosition.x, -0.5f *(cylinderCount-1) - 0.25f,transform.localPosition.z); //silindirin boyutunu tam olarak 1 yap
            transform.localScale = new Vector3(0.5f,transform.localScale.y,0.5f);
            PlayerController.Current.CreateCylinder(leftValue); // 1 den ne kadar buyukse o buyuklukte yeni bir silindir yarat
        }
        else if (_value < 0)
        {
            PlayerController.Current.DestroyCylinder(this); // Karakterimize bu silindiri yok etmesini soyle
            
        }
        else
        {
            int cylinderCount = PlayerController.Current.cylinders.Count;
            transform.localPosition = new Vector3(transform.localPosition.x,-0.5f *(cylinderCount-1) - 0.25f *_value,transform.localPosition.z); 
            transform.localScale = new Vector3(0.5f * _value,transform.localScale.y,0.5f* _value);
            // Silindirin boyutunu guncelle
        }
    }
}
