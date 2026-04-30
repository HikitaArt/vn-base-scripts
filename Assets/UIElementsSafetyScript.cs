using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIElementsSafetyScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Reader readerScript;
    public void Start()
    {
        readerScript = Camera.main.GetComponent<Reader>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        readerScript.pointerUnderUI = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        readerScript.pointerUnderUI = false;
    }
}
