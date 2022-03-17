using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public string tooltip;

	public void OnPointerEnter( PointerEventData eventData )
	{
		GlowEngine.FindObjectsOfTypeSingle<Tooltip>().Show( tooltip );
	}

	public void OnPointerExit( PointerEventData eventData )
	{
		GlowEngine.FindObjectsOfTypeSingle<Tooltip>().Hide();
	}
}
