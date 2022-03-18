using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DGPrefab : MonoBehaviour
{
	public Toggle[] countToggles;
	public Image colorPip, iconImage;
	public Outline outline;
	public Color eliteColor;
	public Color[] colors;
	public GameObject exhaustedOverlay;
	public Button selfButton;

	public bool IsExhausted { get { return exhaustedOverlay.activeInHierarchy; } }
	public CardDescriptor Card { get { return cardDescriptor; } }

	CardDescriptor cardDescriptor;
	int colorIndex = 0;

	private void Awake()
	{
		Transform tf = transform.GetChild( 0 );
		tf.localScale = Vector3.zero;
	}

	/// <summary>
	/// Takes an enemy, villain, or ally
	/// </summary>
	public void Init( CardDescriptor cd )
	{
		Debug.Log( "DEPLOYED: " + cd.name );
		cardDescriptor = cd;
		for ( int i = 0; i < cd.size; i++ )
			countToggles[i].gameObject.SetActive( true );
		selfButton.interactable = true;

		ToggleExhausted( cd.hasActivated );

		if ( DataStore.deploymentCards.cards.Any( x => x.id == cd.id ) )
		{
			iconImage.sprite = Resources.Load<Sprite>( $"Cards/Enemies/{cd.expansion}/{cd.id.Replace( "DG", "M" )}" );
		}
		else if ( DataStore.villainCards.cards.Any( x => x.id == cd.id ) )
		{
			iconImage.sprite = Resources.Load<Sprite>( $"Cards/Villains/{cd.id.Replace( "DG", "M" )}" );
			outline.effectColor = eliteColor;
		}
		else if ( cd.id == "DG070" )//handle custom group
		{
			iconImage.sprite = Resources.Load<Sprite>( "Cards/Enemies/Other/M070" );
		}
		else//otherwise it's an ally
		{
			//Debug.Log( "ally" );
			iconImage.sprite = Resources.Load<Sprite>( $"Cards/Allies/{cd.id.Replace( "DG", "M" )}" );
		}

		if ( cd.isElite )
			outline.effectColor = eliteColor;

		SetColorIndex();

		Transform tf = transform.GetChild( 0 );
		tf.localScale = Vector3.zero;
		tf.DOScale( 1, 1f ).SetEase( Ease.OutBounce );
	}

	public void OnCount1( Toggle t )
	{
		if ( !t.gameObject.activeInHierarchy )
			return;

		if ( t.isOn )
			cardDescriptor.currentSize += 1;
		else
			cardDescriptor.currentSize -= 1;

		for ( int i = 0; i < 3; i++ )
		{
			countToggles[i].gameObject.SetActive( false );
			countToggles[i].isOn = false;
		}
		for ( int i = 0; i < cardDescriptor.currentSize; i++ )
			countToggles[i].isOn = true;
		for ( int i = 0; i < cardDescriptor.size; i++ )
			countToggles[i].gameObject.SetActive( true );

		if ( cardDescriptor.currentSize == 0 )
			RemoveSelf();
		//Debug.Log( "SIZE: " + cardDescriptor.currentSize );
	}
	public void OnCount2( Toggle t )
	{
		if ( !t.gameObject.activeInHierarchy )
			return;

		if ( t.isOn )
			cardDescriptor.currentSize += 1;
		else
			cardDescriptor.currentSize -= 1;

		for ( int i = 0; i < 3; i++ )
		{
			countToggles[i].gameObject.SetActive( false );
			countToggles[i].isOn = false;
		}
		for ( int i = 0; i < cardDescriptor.currentSize; i++ )
			countToggles[i].isOn = true;
		for ( int i = 0; i < cardDescriptor.size; i++ )
			countToggles[i].gameObject.SetActive( true );

		if ( cardDescriptor.currentSize == 0 )
			RemoveSelf();
		//Debug.Log( "SIZE: " + cardDescriptor.currentSize );
	}
	public void OnCount3( Toggle t )
	{
		if ( !t.gameObject.activeInHierarchy )
			return;

		if ( t.isOn )
			cardDescriptor.currentSize += 1;
		else
			cardDescriptor.currentSize -= 1;

		for ( int i = 0; i < 3; i++ )
		{
			countToggles[i].gameObject.SetActive( false );
			countToggles[i].isOn = false;
		}
		for ( int i = 0; i < cardDescriptor.currentSize; i++ )
			countToggles[i].isOn = true;
		for ( int i = 0; i < cardDescriptor.size; i++ )
			countToggles[i].gameObject.SetActive( true );

		if ( cardDescriptor.currentSize == 0 )
			RemoveSelf();
		//Debug.Log( "SIZE: " + cardDescriptor.currentSize );
	}

	public void RemoveSelf()
	{
		for ( int i = 0; i < 3; i++ )
			countToggles[i].gameObject.SetActive( false );
		selfButton.interactable = false;

		Transform tf = transform.GetChild( 0 );
		tf.DOScale( 0, 1f ).SetEase( Ease.InBounce ).OnComplete( () =>
		 {
			 //add card back to dep hand ONLY IF IT'S NOT THE CUSTOM GROUP
			 //AND if it's NOT a villain
			 if ( cardDescriptor.id != "DG070" && !DataStore.villainCards.cards.Contains( cardDescriptor ) )
				 DataStore.deploymentHand.Add( cardDescriptor );
			 //remove it from deployed list
			 DataStore.deployedEnemies.Remove( cardDescriptor );
			 //if it is an EARNED villain, add it back into manual deploy list
			 if ( DataStore.sessionData.EarnedVillains.Contains( cardDescriptor ) && !DataStore.manualDeploymentList.Contains( cardDescriptor ) )
			 {
				 DataStore.manualDeploymentList.Add( cardDescriptor );
				 DataStore.SortManualDeployList();
			 }

			 if ( DataStore.sessionData.useAdaptiveDifficulty )
			 {
				 //add fame value
				 DataStore.sessionData.gameVars.fame += cardDescriptor.fame;
				 //reimburse some Threat
				 DataStore.sessionData.ModifyThreat( cardDescriptor.reimb );
				 //show fame popup
				 GlowEngine.FindObjectsOfTypeSingle<QuickMessage>().Show( $"{DataStore.uiLanguage.uiMainApp.fameIncreasedUC}: <color=\"red\">{cardDescriptor.fame}</color>" );
			 }

			 Object.Destroy( gameObject );
		 } );
	}

	public void ToggleColor()
	{
		//red black purple blue green gray
		colorIndex = colorIndex == 5 ? 0 : colorIndex + 1;
		colorPip.color = DataStore.pipColors[colorIndex].ToColor();
		cardDescriptor.colorIndex = colorIndex;
	}

	public void OnClickSelf()
	{
		exhaustedOverlay.SetActive( !exhaustedOverlay.activeInHierarchy );
		cardDescriptor.hasActivated = exhaustedOverlay.activeInHierarchy;
	}

	public void OnActivateSelf()
	{
		//if ( !exhaustedOverlay.activeInHierarchy )
		FindObjectOfType<MainGameController>().ActivateEnemy( cardDescriptor );
	}

	public void UpdateCount()
	{
		//Debug.Log( cardDescriptor.currentSize );
		for ( int i = 0; i < cardDescriptor.currentSize; i++ )
		{
			countToggles[i].gameObject.SetActive( false );
			countToggles[i].isOn = true;
			countToggles[i].gameObject.SetActive( true );
		}
	}

	public void ToggleExhausted( bool isExhausted )
	{
		//mark as NOT activated for this turn so it rolls up new Activation data
		if ( !isExhausted )
			cardDescriptor.hasActivated = false;
		exhaustedOverlay.SetActive( isExhausted );
	}

	public void OnPointerClick()
	{
		CardViewPopup cardViewPopup = GlowEngine.FindObjectsOfTypeSingle<CardViewPopup>();
		cardViewPopup.Show( cardDescriptor );
	}

	public void SetGroupSize( int size )
	{
		cardDescriptor.currentSize = size;
		//disable pips so callback will bail out
		for ( int i = 0; i < cardDescriptor.size; i++ )
			countToggles[i].gameObject.SetActive( false );
		for ( int i = 0; i < cardDescriptor.size; i++ )
		{
			if ( i < size )
				countToggles[i].isOn = true;
			else
				countToggles[i].isOn = false;
		}
		//re-eneable pips
		for ( int i = 0; i < cardDescriptor.size; i++ )
			countToggles[i].gameObject.SetActive( true );
	}

	public void SetColorIndex()
	{
		colorIndex = cardDescriptor.colorIndex;
		colorPip.color = DataStore.pipColors[colorIndex].ToColor();
	}
}
