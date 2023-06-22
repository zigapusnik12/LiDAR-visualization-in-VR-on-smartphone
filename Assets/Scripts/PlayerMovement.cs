using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    Button move;
    [SerializeField]
    int playerSpeed;
    [SerializeField]
    bool isActive;

    // Start is called before the first frame update
    void Start()
    {
        isActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (XRCardboardController.vrActive)
        {
            if (Input.GetButton("Fire1"))
            {
                transform.position = transform.position + Camera.main.transform.forward * playerSpeed * Time.deltaTime;
            }
        }
        else
        {
            if (isActive)
            {
                transform.position = transform.position + Camera.main.transform.forward * playerSpeed * Time.deltaTime;
            }
        }

    }
    public void onPress()
    {
        isActive = true;
    }

    public void onRelease()
    {
        isActive = false;
    }


}
