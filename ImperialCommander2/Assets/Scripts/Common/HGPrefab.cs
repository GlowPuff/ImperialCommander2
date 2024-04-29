using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DEPRECATED, CLASSIC MODE IS NOW REMOVED
/// </summary>

public class HGPrefab : MonoBehaviour
{
	public Toggle woundToggle, activationToggle1, activationToggle2;
	public Image iconImage;
	public Outline outline;
	public Color eliteColor;
	public GameObject exhaustedOverlay;

	DeploymentCard cardDescriptor;

	private void Awake()
	{
		Transform tf = transform.GetChild( 0 );
		tf.localScale = Vector3.zero;
	}

	public void Init( DeploymentCard cd )
	{
		Debug.Log( "DEPLOYED: " + cd.name );
		cardDescriptor = cd;

		if ( !cd.isDummy )
		{
			iconImage.sprite = Resources.Load<Sprite>( cd.mugShotPath );
			outline.effectColor = Saga.Utils.String2UnityColor( cardDescriptor.deploymentOutlineColor );
		}
		else
		{
			iconImage.sprite = Resources.Load<Sprite>( "CardThumbnails/bonus" );
			woundToggle.gameObject.SetActive( false );
		}

		if ( cd.heroState == null )
		{
			cd.heroState = new HeroState();
			cd.heroState.Init();
		}

		SetHealth( cd.heroState );
		SetActivation();

		Transform tf = transform.GetChild( 0 );
		tf.localScale = Vector3.zero;
		tf.DOScale( 1, 1f ).SetEase( Ease.OutBounce );
	}

	/// <summary>
	/// Red status pip clicked
	/// </summary>
	public void OnCount1( Toggle t )
	{
		if ( !woundToggle.gameObject.activeInHierarchy )
			return;

		cardDescriptor.heroState.isWounded = t.isOn;
		if ( cardDescriptor.heroState.isHealthy )
			exhaustedOverlay.SetActive( false );
		else
			cardDescriptor.heroState.isWounded = true;

		if ( exhaustedOverlay.activeInHierarchy )
		{
			cardDescriptor.heroState.isDefeated = true;
			cardDescriptor.heroState.isWounded = true;
		}

		//if it's an ally, mark it defeated
		if ( cardDescriptor.id[0] == 'A' )
		{
			exhaustedOverlay.SetActive( !cardDescriptor.heroState.isHealthy );
			cardDescriptor.heroState.isDefeated = true;
			//cardDescriptor.heroState.heroHealth = HeroHealth.Defeated;
		}

		//Debug.Log( "HEALTHY: " + cardDescriptor.isHealthy );
	}

	public void OnActivation1()
	{
		if ( !activationToggle1.gameObject.activeInHierarchy )
			return;
		cardDescriptor.heroState.hasActivated[0] = activationToggle1.isOn;
	}

	public void OnActivation2()
	{
		if ( !activationToggle2.gameObject.activeInHierarchy )
			return;
		cardDescriptor.heroState.hasActivated[1] = activationToggle2.isOn;
	}

	/// <summary>
	/// Toggle DEFEATED (dimmed overlay)
	/// </summary>
	public void OnClickSelf()
	{
		if ( cardDescriptor.isDummy )
			return;
		exhaustedOverlay.SetActive( !exhaustedOverlay.activeInHierarchy );
		woundToggle.isOn = !exhaustedOverlay.activeInHierarchy;
		if ( exhaustedOverlay.activeInHierarchy )
			cardDescriptor.heroState.isDefeated = true;
	}

	public void OnPointerClick()
	{
		if ( cardDescriptor.isDummy || cardDescriptor.characterType == Saga.CharacterType.Hero )//isHero )
			return;
		CardViewPopup cardViewPopup = GlowEngine.FindUnityObject<CardViewPopup>();
		cardViewPopup.Show( cardDescriptor );
	}

	public void SetHealth( HeroState heroState )
	{
		if ( cardDescriptor.isDummy )
			return;

		cardDescriptor.heroState = heroState;
		woundToggle.gameObject.SetActive( false );//skip callback

		if ( heroState.isWounded || heroState.isDefeated )
			woundToggle.isOn = false;

		if ( cardDescriptor.heroState.isDefeated )
			exhaustedOverlay.SetActive( true );

		woundToggle.gameObject.SetActive( true );
	}

	private void SetActivation()
	{
		//skip callbacks
		activationToggle1.gameObject.SetActive( false );
		activationToggle2.gameObject.SetActive( false );

		//if ( DataStore.sessionData.MissionHeroes.Count <= 2 && cardDescriptor.characterType != Saga.CharacterType.Ally )
		//{
		//	activationToggle1.isOn = cardDescriptor.heroState.hasActivated[0];
		//	activationToggle2.isOn = cardDescriptor.heroState.hasActivated[1];
		//	activationToggle1.gameObject.SetActive( true );
		//	activationToggle2.gameObject.SetActive( true );
		//}
		//else
		//{
		//	activationToggle1.isOn = cardDescriptor.heroState.hasActivated[0];
		//	activationToggle1.gameObject.SetActive( true );
		//}
	}

	public void ResetActivation()
	{
		//skip callbacks
		activationToggle1.gameObject.SetActive( false );
		activationToggle2.gameObject.SetActive( false );

		activationToggle1.isOn = false;
		cardDescriptor.heroState.hasActivated[0] = false;
		activationToggle1.gameObject.SetActive( true );

		//if ( DataStore.sessionData.MissionHeroes.Count <= 2 && cardDescriptor.characterType != Saga.CharacterType.Ally )
		//{
		//	activationToggle2.isOn = false;
		//	cardDescriptor.heroState.hasActivated[1] = false;
		//	activationToggle2.gameObject.SetActive( true );
		//}
	}
}
