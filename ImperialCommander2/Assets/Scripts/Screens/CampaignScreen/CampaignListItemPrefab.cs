using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CampaignListItemPrefab : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Action<string> clickCallback;
	public TextMeshProUGUI nameText;
	public Image bgImage;

	Color normalColor;

	public void InitItem( string n, Action<string> onClick )
	{
		normalColor = bgImage.color;
		Init( n, onClick );
	}

	public void InitSkill( string n, Action<string> onClick )
	{
		normalColor = new Vector3( 1f, 0.5586207f, 0 ).ToColor();
		bgImage.color = normalColor;
		Init( n, onClick );
	}

	public void InitVillain( string n, Action<string> onClick )
	{
		normalColor = new Vector3( 1f, 0.5586207f, 0 ).ToColor();
		bgImage.color = normalColor;
		Init( n, onClick );
	}

	public void InitAlly( string n, Action<string> onClick )
	{
		normalColor = bgImage.color;
		Init( n, onClick );
	}

	public void InitGeneralItem( string n, Action<string> onClick )
	{
		normalColor = bgImage.color;
		Init( n, onClick );
	}

	void Init( string n, Action<string> onClick )
	{
		nameText.text = n;
		clickCallback = onClick;
	}

	public void OnPointerClick( PointerEventData eventData )
	{
		if ( eventData.clickCount == 1 )
		{
			if ( eventData.button == PointerEventData.InputButton.Left )
				clickCallback?.Invoke( nameText.text );
			else if ( eventData.pointerId == 1 )//touch
				clickCallback?.Invoke( nameText.text );
		}
	}

	public void OnPointerEnter( PointerEventData eventData )
	{
		bgImage.color = new Vector3( 1f, 0.1568628f, 0 ).ToColor();
	}

	public void OnPointerExit( PointerEventData eventData )
	{
		bgImage.color = normalColor;
	}
}
