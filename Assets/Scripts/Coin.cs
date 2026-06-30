using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour, IPausable
{
    public int value = 10;
    public float lifeTimeCounter = 0f;
    float lifeTime = 60f;
    bool isPaused = false;
    public int coinType;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused) return;
        lifeTimeCounter += Time.deltaTime;
        if (lifeTimeCounter >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }
    }

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
}
