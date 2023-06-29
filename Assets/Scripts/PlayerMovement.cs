using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    Button move;

    [SerializeField]
    bool isActive;

    [SerializeField]
    GameObject teleportGroup = default;

    public static float playerSpeed;

    // Start is called before the first frame update
    void Start()
    {
        isActive = false;
        playerSpeed = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (XRCardboardController.vrActive)
        {
            if (!XRCardboardController.teleportActive && Input.GetButton("Fire1"))
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

    public void OnPlayerSpeedSliderValueChanged(float value)
    {
        playerSpeed = value;
    }

    public void onPress()
    {
        isActive = true;
    }

    public void onRelease()
    {
        isActive = false;
    }

    public void RefreshTeleport() => RefreshTeleportCoroutine();

    private Coroutine RefreshTeleportCoroutine()
    {
        return StartCoroutine(refreshTeleportRoutine(0.1f));

        IEnumerator refreshTeleportRoutine(float duration)
        {
            teleportGroup.SetActive(false);
            yield return new WaitForSeconds(duration);
            teleportGroup.SetActive(true);
        }
    }
}
