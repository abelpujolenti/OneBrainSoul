using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menus
{
    public class MyTextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private Color _textColorOnHover;
        [SerializeField] private Color _textColorOnPress;
        [SerializeField] private Color _textColorOnExitHover;

        private void OnEnable()
        {
            _text.color = _textColorOnExitHover;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _text.color = _textColorOnHover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnEnable();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _text.color = _textColorOnPress;
        }
    }
}