using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    public class StaminaBar : MonoBehaviour
    {
        [SerializeField] private GlideStamina glideStamina; 
        [SerializeField] private Image currentGlideBar;   

        private void Update()
        {
            if (glideStamina&& currentGlideBar)
            {
                currentGlideBar.fillAmount = glideStamina.GetStaminaFraction();
            }
        }
    }
}