using System;
using System.Collections.Generic;
using System.Linq;
using Saga;
using UnityEngine;
using UnityEngine.EventSystems;

public class HelpPanel : MonoBehaviour
{
	public string helpOverlayID;//the help ID of the object this panel provides help for
	public GameObject helpButtonPrefab;
	public GameObject textBoxPrefab;
	public Transform[] rootPanels;//panels we want help for, whose UI children have a HelpMeta attached

	PopupBase popupBase, textBoxPopupBase;
	RectTransform closeButton;//this panel's "close help" button
	Canvas theCanvas;
	Transform customRegionsRoot;

	Action closeCallback;

	private void Awake()
	{
		theCanvas = transform.parent.parent.GetComponent<Canvas>();
		customRegionsRoot = transform.parent.Find( "Custom Regions" );
		textBoxPopupBase = transform.parent.Find( "Textbox PopupBase" ).GetComponent<PopupBase>();
		closeButton = transform.parent.Find( "close help Button" ).GetComponent<RectTransform>();
	}

	public void Show( Action callback = null )
	{
		popupBase = transform.parent.GetComponent<PopupBase>();
		popupBase.ShowNoZoom();

		closeCallback = callback;

		List<Transform> controlList = new List<Transform>();
		List<Transform> customRegions = new List<Transform>();
		Transform helpActivationButton = null;

		foreach ( Transform rootPanel in rootPanels )
		{
			//discover the controls that have a HelpMeta attached
			controlList = controlList.Concat( rootPanel.GetComponentsInChildren<HelpMeta>().Select( x => x.transform ) ).ToList();
			//find the help activation button on the other panel (should only be 1, even with multiple panels)
			//the panel you want help for should have a "Help Activation Button" prefab on it
			if ( rootPanel.GetComponentsInChildren<Transform>().Where( x => x.name == "Help Activation Button" ).FirstOr( null ) != null )
			{
				helpActivationButton = rootPanel.GetComponentsInChildren<Transform>().Where( x => x.name == "Help Activation Button" ).FirstOr( null );
			}
		}

		//collect any custom regions
		customRegions = customRegionsRoot.GetComponentsInChildren<HelpMeta>().Select( x => x.transform ).ToList();

		//match this object's "close help" button size/position to the other panel's "activate help" button
		if ( helpActivationButton != null )
		{
			var closeBounds = RectTransformUtility.CalculateRelativeRectTransformBounds( theCanvas.transform, helpActivationButton );
			closeButton.anchoredPosition = closeBounds.center;
			closeButton.sizeDelta = closeBounds.size;
		}

		Debug.Log( $"Help Panel Found: {controlList.Count} HelpMeta UI elements" );
		Debug.Log( $"Help Panel Found: {customRegions.Count} custom regions" );

		//create a help button for each control added to this Help Panel and custom regions
		foreach ( var control in controlList.Concat( customRegions ) )
		{
			//var r = control.GetComponent<CanvasRenderer>();
			if ( !control.gameObject.activeInHierarchy )
				continue;
			var bounds = control.GetComponent<RectTransform>();

			GameObject helpButton = Instantiate( helpButtonPrefab, transform );
			//assign help metadata that contains the translation GUID
			var meta = helpButton.GetComponent<HelpMeta>();
			meta.elementID = control.GetComponent<HelpMeta>().elementID;
			meta.helpPanel = this;
			meta.isHidden = control.GetComponent<HelpMeta>().isHidden;
			meta.showIcon = control.GetComponent<HelpMeta>().showIcon;

			if ( !meta.isHidden )
			{
				var goRect = helpButton.GetComponent<RectTransform>();
				var relBounds = RectTransformUtility.CalculateRelativeRectTransformBounds( theCanvas.transform, bounds.transform );
				goRect.anchoredPosition = relBounds.center;
				goRect.sizeDelta = relBounds.size;

				if ( !meta.showIcon )
				{
					helpButton.transform.Find( "icon" ).gameObject.SetActive( false );
				}
			}
			else
				Destroy( helpButton );
		}
	}

	public void OnHelpRequest( string elementID )
	{
		Debug.Log( $"Help requested for: {elementID}" );

		EventSystem.current.SetSelectedGameObject( null );

		var panelHelp = DataStore.uiLanguage.uiHelpOverlay.helpOverlayPanels.Where( x => x.panelHelpID == helpOverlayID ).FirstOr( null );

		if ( panelHelp != null )
		{
			string helpText = panelHelp.helpItems.Where( x => x.id == elementID ).FirstOr( null )?.helpText;
			//format the text
			//if ( !string.IsNullOrEmpty( helpText ) )
			//{
			//	string[] t = helpText.Split( ':' );
			//	if ( t.Length == 2 )
			//	{
			//		t[0] = $"<b><color=yellow>{t[0]}</color></b>:";
			//		helpText = t[0] + t[1];
			//	}
			//}

			if ( helpText != null )
			{
				var go = Instantiate( textBoxPrefab, textBoxPopupBase.transform );
				var tb = go.transform.Find( "TextBox" ).GetComponent<TextBox>();
				textBoxPopupBase.ShowNoZoom();
				tb.Show( helpText, CloseTB );
			}
			else
				Utils.LogWarning( $"OnHelpRequest()::panelHelp [{helpOverlayID}]::helpText [{elementID}] is null" );
		}
		else
			Utils.LogWarning( $"OnHelpRequest()::panelHelp [{helpOverlayID}] is null" );
	}

	/// <summary>
	/// Fired from the overlay Close button
	/// </summary>
	public void Close()
	{
		popupBase.CloseNoZoom();
		closeCallback?.Invoke();
		foreach ( Transform t in transform )
		{
			Destroy( t.gameObject );
		}
	}

	/// <summary>
	/// Fired from textbox Close button
	/// </summary>
	public void CloseTB()
	{
		textBoxPopupBase.CloseNoZoom();
	}
}