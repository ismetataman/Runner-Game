using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Current; //static degiskenler herhangi bir sinif tarafindan erisebilirler. 
                                            // yaratilan her sinifin playercontroller a erisebilmesi icin bu degisken tanimlandi
    public float limitX;
    public float runningSpeed;
    public float xSpeed;
    public GameObject ridingCylinderPrefab;
    public List<RidingCylinder> cylinders;
    private float _currentRunningSpeed;

    private bool _spawningBridge;
    public GameObject bridgePiecePrefab;
    private BridgeSpawner _bridgeSpawner;
    private float _creatingBridgeTimer;
    public Animator animator;
    private bool _finished;
    private float _scoreTimer = 0;
    private float _lastTouchedX;
     public AudioSource cylinderAudioSource,triggerAudioSource, itemAudioSource;
     public AudioClip gatherAudioClip, dropAudioClip, coinAudioClip, buyAudioClip,equipItemAudioClip,unequipItemAudioClip;
     private float _dropSoundTimer;
     public List<GameObject> wearSpots;
    // Start is called before the first frame update
   

    // Update is called once per frame
    void Update()
    {
        if(LevelController.Current == null || !LevelController.Current.gameActive)
        {
            return;
        }
        float newX = 0;
        float touchXDelta = 0;
        if(Input.touchCount > 0) // ekrana dokunulursanin kontrolu
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _lastTouchedX = Input.GetTouch(0).position.x;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                 touchXDelta = 5 * (Input.GetTouch(0).position.x - _lastTouchedX) / Screen.width;
                 _lastTouchedX = Input.GetTouch(0).position.x;
            }
           
        }
        else if(Input.GetMouseButton(0)) // mouse ile tiklanirsanin kontrolu
        {
            touchXDelta = Input.GetAxis("Mouse X");
        }
        newX = transform.position.x + xSpeed * touchXDelta * Time.deltaTime;
        newX = Mathf.Clamp(newX,-limitX,limitX); //x duzleminde alabilecegi maks ve min degerleri ayarlamak icin


        Vector3 newPosition = new Vector3(newX,transform.position.y,transform.position.z + _currentRunningSpeed * Time.deltaTime);
        transform.position = newPosition; 

        if(_spawningBridge)
        {
            PlayDropSound();
            _creatingBridgeTimer -= Time.deltaTime;
            if (_creatingBridgeTimer < 0)
            {
                _creatingBridgeTimer = 0.01f;
                IcrementClinderVolume(-0.01f);
                GameObject createdBridgePicece = Instantiate(bridgePiecePrefab,this.transform);
                createdBridgePicece.transform.SetParent(null);
                Vector3 direction = _bridgeSpawner.endReference.transform.position - _bridgeSpawner.startReference.transform.position;
                float distance = direction.magnitude; // yon vektorunun agirligina erismek icin magnitude kullaniliyor.
                // yon vektorunun agirlgi iki referans noktasi arasindaki mesafeyi vericek
                direction = direction.normalized;
                createdBridgePicece.transform.forward = direction;
                float characterDistance = transform.position.z - _bridgeSpawner.startReference.transform.position.z;
                characterDistance = Mathf.Clamp(characterDistance,0,distance);// yukarida cikarilan degeri 0 ve maks uzaklikla sinirlandirmak icin clamp fonksiyonu kullaniliyor
                Vector3 newPiecePosition = _bridgeSpawner.startReference.transform.position + direction * characterDistance;
                newPiecePosition.x = transform.position.x;
                createdBridgePicece.transform.position = newPiecePosition;
                if(_finished)
                {
                    _scoreTimer -= Time.deltaTime;
                    if(_scoreTimer <0)
                    {
                        _scoreTimer = 0.3f;
                        LevelController.Current.ChangeScore(1);
                    }
                }
                
            }
        }
    }
    public void ChangeSpeed(float value)
    {
        _currentRunningSpeed = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "AddCylinder")
        {
            cylinderAudioSource.PlayOneShot(gatherAudioClip,0.1f);
            IcrementClinderVolume(0.1f);
            Destroy(other.gameObject);
        }
        else if(other.tag== "SpawnBridge")
        {
            StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if(other.tag == "StopSpawnBridge")
        {
            StopSpawningBridge();
            if(_finished)
            {   
                LevelController.Current.FinishGame();
            }
        }
        else if(other.tag == "Finish")
        {
            _finished = true;
            StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if(other.tag == "Coin")
        {
            triggerAudioSource.PlayOneShot(coinAudioClip,0.1f);
            other.tag = "Untagged"; // karakterimizin childi olan silindirlerimizden iki tanesi ayni anda degerse ontrigger 2 kez calisip
            //bug olusturuyor. Bunu engellemek icin bir kere degdikten sonra coinin tag ini degistirdik.
            LevelController.Current.ChangeScore(10); // 10 puan skoru gunceller
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other) 
    {
        if(LevelController.Current.gameActive)
        {
             if(other.tag == "Trap")
        {
            PlayDropSound();
            IcrementClinderVolume(-Time.fixedDeltaTime); // - var cunku silindirleri zamanda azaltmak istiyoruz
        }

        }
       
    }
    public void IcrementClinderVolume(float value)
    {
        if (cylinders.Count == 0)
        {
            if (value >0)
            {
                CreateCylinder(value);
            }
            else
            {
               if(_finished)
               {
                   LevelController.Current.FinishGame();
               }else
               {
                   Die();
               }
                //Gameover
            }
        }
        else
        {
            cylinders[cylinders.Count -1].IncrementCylinderVolume(value); //en asagidaki silindiirn boyutunu guncellemek icin 
                                                                         // cylinders.count -1 ile son indexe erisiyorsun(en asagidaki silindir)

        }
    }
    public void Die() 
        {
            animator.SetBool("dead", true);
            gameObject.layer= 6;
            Camera.main.transform.SetParent(null);
            LevelController.Current.GameOver();
        }
    

    public void CreateCylinder(float value)
    {
        RidingCylinder createdCylinder = Instantiate(ridingCylinderPrefab,transform).GetComponent<RidingCylinder>();
        cylinders.Add(createdCylinder);
        createdCylinder.IncrementCylinderVolume(value);
    }

    public void DestroyCylinder(RidingCylinder cylinder)
    {
        cylinders.Remove(cylinder);
        Destroy(cylinder.gameObject);
    }

    public void StartSpawningBridge(BridgeSpawner spawner)
    {
        _bridgeSpawner = spawner; // fonksiyonu sinifin kendi BridgeSpawnerinin parametresine esitlicek ki refererans noktalarina erisebilmek icin
        _spawningBridge = true;

    }
    public void StopSpawningBridge()
    {
        _spawningBridge = false;
    }
    public void PlayDropSound()
    {
        _dropSoundTimer -= Time.deltaTime;
        if(_dropSoundTimer <0 )
        {
            _dropSoundTimer = 0.15f;
            cylinderAudioSource.PlayOneShot(dropAudioClip,0.1f);
        }
    }
}
