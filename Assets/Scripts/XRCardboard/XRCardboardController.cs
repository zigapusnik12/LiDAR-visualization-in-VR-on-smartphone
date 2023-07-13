#if !UNITY_EDITOR
using Google.XR.Cardboard;
using UnityEngine.XR.Management;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;

public class XRCardboardController : MonoBehaviour
{
    [SerializeField]
    Transform cameraTransform = default;
    [SerializeField]
    GameObject vrGroup = default;
    [SerializeField]
    GameObject teleportGroup = default;
    [SerializeField]
    GameObject standardGroup = default;
    [SerializeField]
    GameObject settingsGroup = default;
    [SerializeField]
    Material pointMaterial = default;
    [SerializeField]
    Button closeBtn = default;
    [SerializeField]
    XRCardboardInputModule vrInputModule = default;
    [SerializeField]
    StandaloneInputModule standardInputModule = default;
    [SerializeField, Range(.05f, 2)]
    float dragRate = .2f;

    TrackedPoseDriver poseDriver;
    Camera cam;
    Quaternion initialRotation;
    Quaternion attitude;
    Vector2 dragDegrees;
    float defaultFov;
    public static bool vrActive;
    public static bool settingsOpened;
    public static bool isPointerOverSettings;
    public static bool LASColorActive;
    public static bool teleportActive;

#if UNITY_EDITOR
    Vector3 lastMousePos;
#endif

    void Awake()
    {
        cam = cameraTransform.GetComponent<Camera>();
        poseDriver = cameraTransform.GetComponent<TrackedPoseDriver>();
        defaultFov = cam.fieldOfView;
        initialRotation = cameraTransform.rotation;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        settingsOpened = false;
        isPointerOverSettings = false;
        LASColorActive = true;
        teleportActive = false;
#if UNITY_EDITOR
    SetObjects(vrActive);
#else
        vrActive = UnityEngine.XR.XRSettings.enabled;
        SetObjects(vrActive);
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!vrActive)
                StopApplication();
            else
#else
        if (Api.IsCloseButtonPressed)
        {
#endif
                DisableVR();
        }

        if (!(settingsOpened && isPointerOverSettings))
        {
#if UNITY_EDITOR
            if (vrActive)
                SimulateVR();
            else
                SimulateDrag();
#else
            vrActive = UnityEngine.XR.XRSettings.enabled;
            if (vrActive)
                return;

            CheckDrag();
#endif

            attitude = initialRotation * Quaternion.Euler(dragDegrees.x, 0, 0);
            cameraTransform.rotation = Quaternion.Euler(0, -dragDegrees.y, 0) * attitude;
        }
    }

    public void ResetCamera()
    {
        cameraTransform.rotation = initialRotation;
        dragDegrees = Vector2.zero;
    }

    public void DisableVR()
    {
        initialRotation = cameraTransform.rotation;
#if UNITY_EDITOR
        vrActive = false;
#else
        var xrManager = XRGeneralSettings.Instance.Manager;
        if (xrManager.isInitializationComplete)
        {
            xrManager.StopSubsystems();
            xrManager.DeinitializeLoader();
        }
#endif
        cam.ResetAspect();
        cam.fieldOfView = defaultFov;
        cam.ResetProjectionMatrix();
        cam.ResetWorldToCameraMatrix();
        SetObjects(false);
        ResetCamera();
        RecenterCamera();
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    public void EnableVR() => EnableVRCoroutine();

    public Coroutine EnableVRCoroutine()
    {
        return StartCoroutine(enableVRRoutine());

        IEnumerator enableVRRoutine()
        {
            SetObjects(true);
#if UNITY_EDITOR
            yield return null;
            vrActive = true;
#else
            var xrManager = XRGeneralSettings.Instance.Manager;
            if (!xrManager.isInitializationComplete)
                yield return xrManager.InitializeLoader();
            xrManager.StartSubsystems();
#endif
            RecenterCamera();
            initialRotation = cameraTransform.rotation;
            ResetCamera();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }

    void RecenterCamera()
    {
        if (vrActive)
        {
            cameraTransform.parent.localEulerAngles = cameraTransform.localEulerAngles;
            cameraTransform.localEulerAngles = Vector3.zero;
        }
        else
        {
            cameraTransform.localEulerAngles = cameraTransform.parent.localEulerAngles + cameraTransform.localEulerAngles;
            cameraTransform.parent.localEulerAngles = Vector3.zero;
        }
    }

    void SetObjects(bool vrActive)
    {
        if (!standardGroup.activeSelf)
            DisableCloseBtnCoroutine();
        standardGroup.SetActive(!vrActive);
        vrGroup.SetActive(vrActive);
        standardInputModule.enabled = !vrActive;
        vrInputModule.enabled = vrActive;
        poseDriver.enabled = vrActive;

        settingsOpened = false;
        settingsGroup.SetActive(settingsOpened);
        teleportGroup.SetActive(vrActive && teleportActive);
    }

    private Coroutine DisableCloseBtnCoroutine()
    {
        return StartCoroutine(disableCloseBtnRoutine(0.1f));

        IEnumerator disableCloseBtnRoutine(float duration)
        {
            closeBtn.interactable = false;
            yield return new WaitForSeconds(duration);
            closeBtn.interactable = true;
        }
    }

    public void CloseApp() => StopApplication();

    private void StopApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ToggleSettingsPanel() => ToggleSettings();
    private void ToggleSettings()
    {
        settingsOpened = !settingsOpened;
        settingsGroup.SetActive(settingsOpened);
    }

    public void ToggleTouchToMove() => ToggleTeleport();
    private void ToggleTeleport()
    {
        teleportActive = !teleportActive;
    }

    public void ToggleLASColor() => ToggleShaderKeyword();
    private void ToggleShaderKeyword()
    {
        LASColorActive = !LASColorActive;
        if (LASColorActive)
            pointMaterial.EnableKeyword("_LAS_ON");
        else
            pointMaterial.DisableKeyword("_LAS_ON");
    }

    public void onPointerOverSettingsEnter()
    {
        isPointerOverSettings = true;
    }

    public void onPointerOverSettingsExit()
    {
        isPointerOverSettings = false;
    }

    void CheckDrag()
    {
        if (Input.touchCount <= 0)
            return;

        Touch touch = Input.GetTouch(0);
        dragDegrees.x += touch.deltaPosition.y * dragRate;
        dragDegrees.y += touch.deltaPosition.x * dragRate;
    }

#if UNITY_EDITOR
    void SimulateVR()
    {
        var mousePos = Input.mousePosition;
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            var delta = mousePos - lastMousePos;
            dragDegrees.x -= delta.y * dragRate;
            dragDegrees.y -= delta.x * dragRate;
        }
        lastMousePos = mousePos;
    }

    void SimulateDrag()
    {
        var mousePos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            var delta = mousePos - lastMousePos;
            dragDegrees.x += delta.y * dragRate;
            dragDegrees.y += delta.x * dragRate;
        }
        lastMousePos = mousePos;
    }
#endif
}