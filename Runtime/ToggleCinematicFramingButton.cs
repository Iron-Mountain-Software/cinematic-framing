using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ToggleCinematicFramingButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float animationSeconds = 1;
    [SerializeField] private float aspectRatio = 1;
    
    [Header("Cache")]
    private Button _button;
    private CinematicFraming _cinematicFraming;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        if (!_cinematicFraming)
        {
            _cinematicFraming = CinematicFraming.SpawnInstance(aspectRatio, animationSeconds);
        }
        else
        {
            float seconds = _cinematicFraming.Disappear();
            Destroy(_cinematicFraming.gameObject, seconds);
            _cinematicFraming = null;
        } 
    }
}