using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.RenderStreaming.Samples
{
    /// <summary>
    /// XR-friendly RenderTexture preview: draws a RenderTexture on a quad
    /// parented to the HMD camera (no OnGUI / ScreenSpaceOverlay).
    /// </summary>
    public class RenderTextureDisplay : MonoBehaviour
    {
        [Header("References")]
        public Camera targetCamera; // if null, uses Camera.main
        public RenderTexture renderTexture;

        [Header("World-space placement (meters)")]
        [Tooltip("Position in camera local-space (meters). Negative X = left, negative Y = down.")]
        public Vector3 localPosition = new Vector3(-0.18f, -0.16f, 0.60f);

        [Tooltip("Quad size in meters.")]
        public Vector2 sizeMeters = new Vector2(0.25f, 0.25f);

        [Header("Options")]
        [Tooltip("If the texture appears upside down, toggle this.")]
        public bool flipY = false;

        [Tooltip("Hide the quad if RenderTexture is null.")]
        public bool hideWhenNull = true;

        [Tooltip("Force the quad to render late (high renderQueue).")]
        public bool renderOnTop = true;

        private GameObject _quad;
        private MeshRenderer _mr;
        private Material _mat;
        private Texture _lastTex;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            CreateQuad();
        }

        private void CreateQuad()
        {
            if (_quad != null) return;

            _quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _quad.name = "RenderTextureDebugQuad";
            _quad.transform.SetParent(targetCamera != null ? targetCamera.transform : transform, false);

            // Remove collider (primitive adds one)
            var col = _quad.GetComponent<Collider>();
            if (col) Destroy(col);

            _quad.transform.localPosition = localPosition;
            _quad.transform.localRotation = Quaternion.identity;
            _quad.transform.localScale = new Vector3(sizeMeters.x, sizeMeters.y, 1f);

            _mr = _quad.GetComponent<MeshRenderer>();
            _mr.shadowCastingMode = ShadowCastingMode.Off;
            _mr.receiveShadows = false;

            // URP shader
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

            // Built-in fallback
            if (shader == null)
                shader = Shader.Find("Unlit/Texture");

            if (shader == null)
            {
                Debug.LogError("[RenderTextureScreenRectangle] Could not find Unlit shader.");
                shader = Shader.Find("Sprites/Default");
            }

            _mat = new Material(shader)
            {
                name = "RenderTextureDebug_Mat"
            };

            if (renderOnTop)
                _mat.renderQueue = 5000;

            _mr.sharedMaterial = _mat;

            Debug.Log("[RenderTextureScreenRectangle] Debug quad created (XR friendly).");
        }

        private void LateUpdate()
        {
            if (_mr == null) return;

            if (hideWhenNull)
                _quad.SetActive(renderTexture != null);

            if (renderTexture == null) return;

            if (!ReferenceEquals(renderTexture, _lastTex))
            {
                _lastTex = renderTexture;
                _mat.mainTexture = renderTexture;

                // Flip if needed
                _mat.mainTextureScale = flipY ? new Vector2(1, -1) : Vector2.one;
                _mat.mainTextureOffset = flipY ? new Vector2(0, 1) : Vector2.zero;

                Debug.Log($"[RenderTextureScreenRectangle] Bound RT: {renderTexture.width}x{renderTexture.height}");
            }

            // Keep anchored (allows runtime tweaking)
            _quad.transform.localPosition = localPosition;
            _quad.transform.localScale = new Vector3(sizeMeters.x, sizeMeters.y, 1f);
        }

        private void OnDestroy()
        {
            if (_quad) Destroy(_quad);
            if (_mat) Destroy(_mat);
        }
    }
}
