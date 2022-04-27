using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeSpawner : MonoBehaviour
{
    public GameObject startReference, endReference;
    public BoxCollider hiddenPlatform;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 direction = endReference.transform.position - startReference.transform.position;
        float distance = direction.magnitude; // yon vektorunun agirligina erismek icin magnitude kullaniliyor.
        // yon vektorunun agirlgi iki referans noktasi arasindaki mesafeyi vericek
        direction = direction.normalized; //yon vektorunu islemlerde kullanabilmek icin birim vektore donusturmek gerekiyor.
        hiddenPlatform.transform.forward = direction;
        hiddenPlatform.size = new Vector3(hiddenPlatform.size.x, hiddenPlatform.size.y,distance);

        hiddenPlatform.transform.position = startReference.transform.position + (direction * distance /2) + (new Vector3(0,-direction.z,direction.y)* hiddenPlatform.size.y /2) ; 
    }

}
