using UnityEngine;
using UnityEngine.UI;
public class SmoothSlider : MonoBehaviour
{
    public Slider slider; // Reference to your slider
    public float smoothSpeed = 10f; // How fast the slider lerps

    private float targetValue;

    private void Start()
    {
        // Ensure targetValue starts at the correct position
        targetValue = slider.value;
    }

    private void Update()
    {
        // Smoothly interpolate the slider's value towards the targetValue
        slider.value = Mathf.Lerp(slider.value, targetValue, Time.deltaTime * smoothSpeed);
    }

    // This function is triggered when the slider's value changes (through any input)
    public void OnSliderValueChanged(float newValue)
    {
        // Update the target value to lerp toward
        targetValue = newValue;
    }
}