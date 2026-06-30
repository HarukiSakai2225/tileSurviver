using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Digger : MonoBehaviour, ILvUpable, IPausable
{
    public float diggingSpeed;
    float currentTime;

    public GameObject itemPrefab;

    public float dropForce = 1f;           // 横方向に飛び出す力   // 上方向の力
    public bool randomDirection = true;

    public float bonusPerObject = 0.1f;   
    public string targetTag;  
    public float detectionRadius = 1f;    

    private bool isPaused = false;



    void OnEnable()
    {
        PauseManager.Register(this);
    }
    
    void OnDisable()
    {
        PauseManager.Unregister(this);
    }

    public void OnPause()
    {
        isPaused = true;
    }
    
    public void OnResume()
    {
        isPaused = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        int nearbyCount = CountNearbyObjects();
        if(nearbyCount == 0)
        {
           currentTime = 1.0f / (diggingSpeed * 0.01f); 
        }
        else
        {
            float speedMultiplier = 1f + (nearbyCount * bonusPerObject);
        currentTime = 1.0f / (diggingSpeed * speedMultiplier);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused) return;
        int nearbyCount = CountNearbyObjects();
        currentTime -= Time.deltaTime;
        if(currentTime <= 0 && nearbyCount != 0)
        {
            DropItem();
            float speedMultiplier = 1f + (nearbyCount * bonusPerObject);
            currentTime = 1.0f / (diggingSpeed * speedMultiplier);
            Debug.Log(speedMultiplier);
        }
        else if(currentTime <= 0 && nearbyCount == 0)
        {
            DropItem();
            currentTime = 1.0f / (diggingSpeed * 0.01f);
        }
    }

    public void DropItem()
    {
        if (itemPrefab != null)
        {
            GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            
            Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float xDirection = randomDirection ? Random.Range(-1f, 1f) : 0f;
                float yDirection = randomDirection ? Random.Range(-1f, 1f) : 0f;
                
                Vector2 velocity = new Vector2(
                    xDirection * dropForce,
                    yDirection * dropForce
                );
                
                rb.velocity = velocity;
            }
        }
    }

    public void LvUp(int num, float percent)
    {
        switch (num)
        {
            case 0: diggingSpeed *= percent; break;
            default: break;
        }
    }

    int CountNearbyObjects()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        
        int count = 0;
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(targetTag))
            {
                count++;
            }
        }
        return count;
    }
}
