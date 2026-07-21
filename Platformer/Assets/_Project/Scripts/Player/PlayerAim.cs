using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    // Spider-Man style aim mode for the blast attack: hold Aim to zoom the camera,
    // face the camera direction, and show a reticle. PlayerCombat asks GetAimPoint()
    // so fired blasts fly to the reticle instead of straight ahead.
    public class PlayerAim : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] InputReader input;

        [Header("Aim Settings")]
        [SerializeField] float aimFOV = 42f;
        [SerializeField] float fovLerpSpeed = 10f;
        [SerializeField] float aimRotationSpeed = 720f;
        [SerializeField] float aimRange = 60f;
        [SerializeField] LayerMask aimMask = ~0;

        [Header("Shoulder Offset")]
        [Tooltip("Camera-space offset while aiming: X = right, Y = up. Puts the character off-center for a clear reticle view.")]
        [SerializeField] Vector3 aimCameraOffset = new Vector3(0.7f, 0.15f, 0f);
        [SerializeField] float offsetLerpSpeed = 10f;

        public bool IsAiming { get; private set; }

        Camera mainCam;
        CinemachineCamera zoomedVcam;
        CinemachineCameraOffset cameraOffset;
        float defaultFOV;
        Image reticle;

        void Awake()
        {
            mainCam = Camera.main;
        }

        void OnEnable()
        {
            if (input != null) input.Aim += OnAimInput;
        }

        void OnDisable()
        {
            if (input != null) input.Aim -= OnAimInput;
            SetAiming(false);
            RestoreFOV();
        }

        void OnAimInput(bool pressed) => SetAiming(pressed);

        void SetAiming(bool aiming)
        {
            if (IsAiming == aiming) return;
            IsAiming = aiming;

            if (aiming && reticle == null) CreateReticle();
            if (reticle != null) reticle.enabled = aiming;
        }

        void Update()
        {
            UpdateCameraZoom();

            if (!IsAiming) return;

            // Face the camera's yaw so shots go where the player is looking
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam != null)
            {
                Quaternion yaw = Quaternion.Euler(0f, mainCam.transform.eulerAngles.y, 0f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, yaw, aimRotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>World position under the reticle (screen center), or a far point along the view ray.</summary>
        public Vector3 GetAimPoint()
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return transform.position + transform.forward * aimRange;

            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, aimRange, aimMask, QueryTriggerInteraction.Ignore)
                && hit.transform.root != transform.root)
            {
                return hit.point;
            }
            return ray.GetPoint(aimRange);
        }

        void UpdateCameraZoom()
        {
            if (IsAiming)
            {
                var brain = CinemachineBrain.GetActiveBrain(0);
                var vcam = brain != null ? brain.ActiveVirtualCamera as CinemachineCamera : null;

                if (vcam != null && vcam != zoomedVcam)
                {
                    RestoreFOV();
                    zoomedVcam = vcam;
                    defaultFOV = vcam.Lens.FieldOfView;

                    // Shoulder offset lives in a Cinemachine extension on the vcam
                    cameraOffset = vcam.GetComponent<CinemachineCameraOffset>();
                    if (cameraOffset == null) cameraOffset = vcam.gameObject.AddComponent<CinemachineCameraOffset>();
                }

                if (zoomedVcam != null)
                {
                    var lens = zoomedVcam.Lens;
                    lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, aimFOV, fovLerpSpeed * Time.deltaTime);
                    zoomedVcam.Lens = lens;
                }

                if (cameraOffset != null)
                    cameraOffset.Offset = Vector3.Lerp(cameraOffset.Offset, aimCameraOffset, offsetLerpSpeed * Time.deltaTime);
            }
            else if (zoomedVcam != null)
            {
                var lens = zoomedVcam.Lens;
                lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, defaultFOV, fovLerpSpeed * Time.deltaTime);
                zoomedVcam.Lens = lens;

                if (cameraOffset != null)
                    cameraOffset.Offset = Vector3.Lerp(cameraOffset.Offset, Vector3.zero, offsetLerpSpeed * Time.deltaTime);

                if (Mathf.Abs(lens.FieldOfView - defaultFOV) < 0.05f) RestoreFOV();
            }
        }

        void RestoreFOV()
        {
            if (zoomedVcam == null) return;
            var lens = zoomedVcam.Lens;
            lens.FieldOfView = defaultFOV;
            zoomedVcam.Lens = lens;
            zoomedVcam = null;

            if (cameraOffset != null)
            {
                cameraOffset.Offset = Vector3.zero;
                cameraOffset = null;
            }
        }

        // Self-contained screen-space reticle — no scene wiring or assets needed.
        void CreateReticle()
        {
            var canvasGo = new GameObject("AimReticleCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var imgGo = new GameObject("Reticle", typeof(Image));
            imgGo.transform.SetParent(canvasGo.transform, false);

            reticle = imgGo.GetComponent<Image>();
            reticle.sprite = CreateReticleSprite();
            reticle.color = new Color(1f, 1f, 1f, 0.9f);
            reticle.raycastTarget = false;

            var rt = reticle.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(28f, 28f);
            rt.anchoredPosition = Vector2.zero;
        }

        Sprite CreateReticleSprite()
        {
            const int size = 64;
            float center = (size - 1) / 2f;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    bool ring = d >= 24f && d <= 28f;
                    bool dot = d <= 4f;
                    tex.SetPixel(x, y, ring || dot ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
