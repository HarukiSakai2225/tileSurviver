using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public bool isHovering;
    public bool isClicked;
    [SerializeField] GameObject ItemInv;

    [SerializeField] float enterAnimationSpeed;
    [SerializeField] float exitAnimationSpeed;
    [SerializeField] float scaleFactor;

    private Vector3 originalScale;
    private Vector3 targetScale;
    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale * scaleFactor;
    }

    void Update()
    {
        if (isHovering)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * enterAnimationSpeed);
        }
        else if(!isClicked)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * exitAnimationSpeed);
        }

        if(isClicked)
        {
            ItemInv.SetActive(true);
        }
        else
        {
            ItemInv.SetActive(false);
        }
    }
}
