using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menus
{
    public class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private Color _normalTextColor;
        [SerializeField] private Color _hoverTextColor;
        [SerializeField] private Color _pressingTextColor;
        [SerializeField] private Color _selectedTextColor;

        private bool _isPressing;

        private bool _isSelected;

        private void Awake()
        {
            ShowAsNormal();
        }

        private void OnDisable()
        {
            if (!_isPressing)
            {
                return;
            }

            _isPressing = false;
            
            ShowAsNormal();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _text.color = _hoverTextColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPressing = false;
            if (_isSelected)
            {
                ShowAsSelected();
                return;
            }
            ShowAsNormal();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressing = true;
            _text.color = _pressingTextColor;
        }

        public void ShowAsNormal()
        {
            _text.color = _normalTextColor;
            _isSelected = false;
        }

        public void ShowAsSelected()
        {
            _text.color = _selectedTextColor;
            _isSelected = true;
        }
    }
}