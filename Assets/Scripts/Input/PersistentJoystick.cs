using UnityEngine;
using UnityEngine.EventSystems;

namespace JannahGarden.Input
{
    /// <summary>Keeps the fixed movement control visible while dimming it when idle.</summary>
    public sealed class PersistentJoystick : Joystick
    {
        private CanvasGroup backgroundGroup;

        protected override void Start()
        {
            base.Start();
            background.gameObject.SetActive(true);
            backgroundGroup = background.GetComponent<CanvasGroup>();
            if (backgroundGroup == null)
                backgroundGroup = background.gameObject.AddComponent<CanvasGroup>();
            backgroundGroup.alpha = 0.5f;
            backgroundGroup.blocksRaycasts = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (backgroundGroup != null)
                backgroundGroup.alpha = 1f;
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            background.gameObject.SetActive(true);
            if (backgroundGroup != null)
                backgroundGroup.alpha = 0.5f;
        }
    }
}
