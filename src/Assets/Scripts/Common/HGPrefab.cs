using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HGPrefab : MonoBehaviour
{
	public Toggle woundToggle, activationToggle1, activationToggle2;
	public Image iconImage;
	public Outline outline;
	public Color eliteColor;
	public GameObject exhaustedOverlay;

	CardDescriptor cardDescriptor;
	bool isAlly = false;
	bool isHero = false;

	private void Awake()
	{
		Transform tf = transform.GetChild( 0 );
		tf.localScale = Vector3.zero;
	}

	public void Init( CardDescriptor cd )
	{
		Debug.Log( "DEPLOYED: " + cd.name );
		cardDescriptor = cd;

		if ( !cd.isDummy )
		{
			if ( DataStore.heroCards.cards.Any( x => x.id == cd.id ) )
			{
				isHero = true;
				iconImage.sprite = Resources.Load<Sprite>( $"Cards/Heroes/{cd.id}" );
			}
			else if ( DataStore.allyCards.cards.Any( x => x.id == cd.id ) )
			{
				isAlly = true;
				iconImage.sprite = Resources.Load<Sprite>( $"Cards/Allies/{cd.id.Replace( "A", "M" )}" );
			}

			if ( cd.id[0] == 'A' )
				outline.effectColor = eliteColor;
		}
		else
		{
			iconImage.sprite = Resources.Load<Sprite>( "Cards/Heroes/bonus" );
			woundToggle.gameObject.SetActive( false );
		}

		if ( cd.heroState == null )
		{
			cd.heroState = new HeroState();
			cd.heroState.Init( DataStore.sessionData.MissionHeroes.Count );
		}

		SetHealth( cd.heroState.heroHealth );
		SetActivation();

		Transform tf = transform.GetChild( 0 );
		tf.localScale = Vector3.zero;
		tf.DOScale( 1, 1f ).SetEase( Ease.OutBounce );
	}

	public void OnCount1( Toggle t )
	{
		if ( !woundToggle.gameObject.activeInHierarchy )
			return;

		cardDescriptor.heroState.isHealthy = t.isOn;
		if ( cardDescriptor.heroState.isHealthy )
		{
			cardDescriptor.heroState.heroHealth = HeroHealth.Healthy;
			exhaustedOverlay.SetActive( false );
		}
		else
			cardDescriptor.heroState.heroHealth = HeroHealth.Wounded;

		if ( exhaustedOverlay.activeInHierarchy )
			cardDescriptor.heroState.heroHealth = HeroHealth.Defeated;

		//if it's an ally, mark it defeated
		if ( cardDescriptor.id[0] == 'A' )
		{
			exhaustedOverlay.SetActive( !cardDescriptor.heroState.isHealthy );
			cardDescriptor.heroState.heroHealth = HeroHealth.Defeated;
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
			cardDescriptor.heroState.heroHealth = HeroHealth.Defeated;
	}

	public void OnPointerClick()
	{
		if ( cardDescriptor.isDummy || isHero )
			return;
		CardViewPopup cardViewPopup = GlowEngine.FindObjectsOfTypeSingle<CardViewPopup>();
		cardViewPopup.Show( cardDescriptor );
	}

	public void SetHealth( HeroHealth heroHealth )
	{
		if ( cardDescriptor.isDummy )
			return;

		cardDescriptor.heroState.heroHealth = heroHealth;
		woundToggle.gameObject.SetActive( false );//skip callback

		if ( heroHealth == HeroHealth.Wounded || heroHealth == HeroHealth.Defeated )
		{
			woundToggle.isOn = false;
		}
		if ( cardDescriptor.heroState.heroHealth == HeroHealth.Defeated )
			exhaustedOverlay.SetActive( true );

		woundToggle.gameObject.SetActive( true );
	}

	private void SetActivation()
	{
		//skip callbacks
		activationToggle1.gameObject.SetActive( false );
		activationToggle2.gameObject.SetActive( false );

		if ( DataStore.sessionData.MissionHeroes.Count <= 2 && !isAlly )
		{
			activationToggle1.isOn = cardDescriptor.heroState.hasActivated[0];
			activationToggle2.isOn = cardDescriptor.heroState.hasActivated[1];
			activationToggle1.gameObject.SetActive( true );
			activationToggle2.gameObject.SetActive( true );
		}
		else
		{
			activationToggle1.isOn = cardDescriptor.heroState.hasActivated[0];
			activationToggle1.gameObject.SetActive( true );
		}
	}

	public void ResetActivation()
	{
		//skip callbacks
		activationToggle1.gameObject.SetActive( false );
		activationToggle2.gameObject.SetActive( false );

		activationToggle1.isOn = false;
		cardDescriptor.heroState.hasActivated[0] = false;
		activationToggle1.gameObject.SetActive( true );

		if ( DataStore.sessionData.MissionHeroes.Count <= 2 && !isAlly )
		{
			activationToggle2.isOn = false;
			cardDescriptor.heroState.hasActivated[1] = false;
			activationToggle2.gameObject.SetActive( true );
		}
	}
}
