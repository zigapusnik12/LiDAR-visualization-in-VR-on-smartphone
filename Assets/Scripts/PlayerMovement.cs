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

    public static bool isGazing;

    [SerializeField]
    GameObject teleportGroup = default;

    // Start is called before the first frame update
    void Start()
    {
        isActive = false;
        isGazing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (XRCardboardController.vrActive)
        {
            if (!isGazing && Input.GetButton("Fire1"))
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

    public void EnableGazing()
    {
        isGazing = true;
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
