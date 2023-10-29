using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IronMountain.CinematicFraming
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class CinematicFraming : MonoBehaviour
    {
        private static readonly string ShaderName = "UI/Default";
        private static readonly int Tint = Shader.PropertyToID("_Color");
        private static readonly int StencilComparison = Shader.PropertyToID("_StencilComp");
        private static readonly int StencilID = Shader.PropertyToID("_Stencil");
        private static readonly int StencilOperation = Shader.PropertyToID("_StencilOp");
        private static readonly int StencilWriteMask = Shader.PropertyToID("_StencilWriteMask");
        private static readonly int StencilReadMask = Shader.PropertyToID("_StencilReadMask");
        private static readonly int ColorMask = Shader.PropertyToID("_ColorMask");
        private static readonly int UseAlphaClip = Shader.PropertyToID("_UseUIAlphaClip");

        public static CinematicFraming SpawnInstance(
            float aspectRatio = 16f / 9f,
            float animationSeconds = 1f,
            Transform parent = null)
        {
            GameObject cinematicFramingGameObject = new GameObject("Cinematic Framing");
            cinematicFramingGameObject.transform.parent = parent;
            CinematicFraming cinematicFraming = cinematicFramingGameObject.AddComponent<CinematicFraming>();
            cinematicFraming.Initialize(aspectRatio, animationSeconds);
            return cinematicFraming;
        }

        [Header("Settings")]
        private float _animationSeconds;
        private float _aspectRatio;

        public Canvas Canvas { get; private set; }
        public AspectRatioFitter AspectRatioFitter { get; private set; }
        
        public float AnimationSeconds => _animationSeconds;
        public float ScreenAspectRatio => (float) Screen.width / Screen.height;
        
        private void Awake()
        {
            Canvas = GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 2000;
        
            CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(transform, false);
            RectTransform backgroundRectTransform = background.GetComponent<RectTransform>();
            backgroundRectTransform.anchorMin = Vector2.zero;
            backgroundRectTransform.anchorMax = Vector2.one;
            backgroundRectTransform.offsetMin = Vector2.zero;
            backgroundRectTransform.offsetMax = Vector2.zero;
            backgroundRectTransform.localScale = Vector3.one;
            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = null;
            backgroundImage.color = Color.black;
            backgroundImage.raycastTarget = false;
            backgroundImage.material = GetMaskedMaterial();

            GameObject maskGameObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            maskGameObject.transform.SetParent(transform, false);
            RectTransform maskRectTransform = maskGameObject.GetComponent<RectTransform>();
            maskRectTransform.anchorMin = Vector2.zero;
            maskRectTransform.anchorMax = Vector2.one;
            maskRectTransform.offsetMin = Vector2.zero;
            maskRectTransform.offsetMax = Vector2.zero;
            maskRectTransform.localScale = Vector3.one;
            Image maskImage = maskGameObject.GetComponent<Image>();
            maskImage.sprite = null;
            maskImage.color = Color.white;
            maskImage.raycastTarget = false;
            maskImage.material = GetMaskMaterial();
            AspectRatioFitter = maskGameObject.GetComponent<AspectRatioFitter>();
            AspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            AspectRatioFitter.enabled = false;
        
            maskRectTransform.SetSiblingIndex(0);
            backgroundRectTransform.SetSiblingIndex(1);
        }

        private void Initialize(float aspectRatio, float seconds)
        {
            _aspectRatio = aspectRatio;
            _animationSeconds = seconds;
            AspectRatioFitter.aspectRatio = ScreenAspectRatio;
            AspectRatioFitter.enabled = true;
            Appear();
        }

        public float Appear()
        {
            StopAllCoroutines();
            float progress = Mathf.InverseLerp(ScreenAspectRatio, _aspectRatio, AspectRatioFitter.aspectRatio);
            float animationSeconds = (1 - progress) * _animationSeconds;
            StartCoroutine(Animate(
                AspectRatioFitter.aspectRatio,
                _aspectRatio,
                animationSeconds));
            return animationSeconds;
        }

        public float Disappear()
        {
            StopAllCoroutines();
            float progress = Mathf.InverseLerp(_aspectRatio, ScreenAspectRatio, AspectRatioFitter.aspectRatio);
            float animationSeconds = (1 - progress) * _animationSeconds;
            StartCoroutine(Animate(
                AspectRatioFitter.aspectRatio, 
                ScreenAspectRatio,
                animationSeconds));
            return animationSeconds;
        }

        private IEnumerator Animate(float startAspectRatio, float endAspectRatio, float seconds)
        {
            for (float timer = 0f; timer < seconds; timer += Time.unscaledDeltaTime)
            {
                float progress = timer / seconds;
                AspectRatioFitter.aspectRatio = Mathf.Lerp(startAspectRatio, endAspectRatio, progress);
                yield return null;
            }
            AspectRatioFitter.aspectRatio = endAspectRatio;
        }

        private Material GetMaskedMaterial()
        {
            Material material = new Material(Shader.Find(ShaderName));
            material.SetColor(Tint, Color.white);
            material.SetInt(StencilComparison, 3);
            material.SetInt(StencilID, 2);
            material.SetInt(StencilOperation, 0);
            material.SetInt(StencilWriteMask, 0);
            material.SetInt(StencilReadMask, 1);
            material.SetInt(ColorMask, 15);
            material.SetFloat(UseAlphaClip, 0);
            return material;
        }
    
        private Material GetMaskMaterial()
        {
            Material material = new Material(Shader.Find(ShaderName));
            material.SetColor(Tint, Color.white);
            material.SetInt(StencilComparison, 8);
            material.SetInt(StencilID, 1);
            material.SetInt(StencilOperation, 2);
            material.SetInt(StencilWriteMask, 255);
            material.SetInt(StencilReadMask, 255);
            material.SetInt(ColorMask, 0);
            material.SetFloat(UseAlphaClip, 1);
            return material;
        }
    }
}