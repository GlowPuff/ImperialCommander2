using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class SagaHGPrefab : MonoBehaviour
	{
		public Toggle woundToggle, activationToggle1, activationToggle2;
		public Image iconImage;
		public Outline outline;
		public Color eliteColor;
		public GameObject exhaustedOverlay;

		DeploymentCard cardDescriptor;
		//bool isAlly = false;
		//bool isHero = false;
		[HideInInspector]
		public bool isConfirming = false;

		private void Awake()
		{
			Transform tf = transform.GetChild( 0 );
			tf.localScale = Vector3.zero;
		}

		public void Init( DeploymentCard cd, bool useGenericMugshot = false )
		{
			Debug.Log( "DEPLOYED: " + cd.name );
			cardDescriptor = cd;

			if ( !cd.isDummy )
			{
				if ( DataStore.heroCards.Any( x => x.id == cd.id ) )
				{
					//isHero = true;
					iconImage.sprite = Resources.Load<Sprite>( $"Cards/Heroes/{cd.id}" );
				}
				else if ( DataStore.allyCards.Any( x => x.id == cd.id ) )
				{
					//isAlly = true;
					if ( !useGenericMugshot )
						iconImage.sprite = Resources.Load<Sprite>( $"Cards/Allies/{cd.id.Replace( "A", "M" )}" );
					else
						iconImage.sprite = Resources.Load<Sprite>( $"Cards/Allies/M002" );
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
				cd.heroState.Init( DataStore.sagaSessionData.MissionHeroes.Count );
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
			OnClickSelf();

			//cardDescriptor.heroState.isHealthy = t.isOn;
			//if ( cardDescriptor.heroState.isHealthy )
			//{
			//	cardDescriptor.heroState.heroHealth = HeroHealth.Healthy;
			//	exhaustedOverlay.SetActive( false );
			//}
			//else
			//{
			//	if ( isHero )
			//	{
			//		cardDescriptor.heroState.heroHealth = HeroHealth.Wounded;

			//		FindObjectOfType<SagaController>().eventManager.CheckIfEventsTriggered();
			//	}
			//	else
			//	{
			//		exhaustedOverlay.SetActive( !cardDescriptor.heroState.isHealthy );
			//		cardDescriptor.heroState.heroHealth = HeroHealth.Defeated;
			//	}
			//}

			//if it's an ally, mark it defeated
			//if ( cardDescriptor.id[0] == 'A' )
			//{
			//	exhaustedOverlay.SetActive( !cardDescriptor.heroState.isHealthy );
			//	cardDescriptor.heroState.heroHealth = HeroHealth.Defeated;
			//}

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

		public void OnClickSelf()
		{
			if ( cardDescriptor.isDummy
				|| cardDescriptor.heroState.heroHealth == HeroHealth.Defeated
				|| FindObjectOfType<SagaEventManager>().IsUIHidden )
				return;

			if ( !isConfirming && FindObjectOfType<ConfirmPopup>() == null )
			{
				FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( false );
				FindObjectOfType<SagaEventManager>()?.transform.Find( "HGConfirmPopup" )
					.GetComponent<ConfirmPopup>().ShowRight(
					transform,
					this,
					cardDescriptor.name,
					cardDescriptor.isHero,
					cardDescriptor.heroState.heroHealth == HeroHealth.Wounded,
					OnDefeat,
					OnWound );
			}
			else
			{
				FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
				FindObjectOfType<ConfirmPopup>()?.Hide();
			}
		}

		//popup card view, excluding Heroes
		public void OnPointerClick()
		{
			//new - don't show popup for ANY hero/ally since ally might be a generic regel

			//if ( cardDescriptor.isDummy || cardDescriptor.isHero )
			//	return;
			//CardViewPopup cardViewPopup = GlowEngine.FindUnityObject<CardViewPopup>();
			//cardViewPopup.Show( cardDescriptor );
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

			if ( DataStore.sagaSessionData.MissionHeroes.Count <= 2 && cardDescriptor.isHero )
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

			if ( DataStore.sagaSessionData.MissionHeroes.Count <= 2 && cardDescriptor.isHero )
			{
				activationToggle2.isOn = false;
				cardDescriptor.heroState.hasActivated[1] = false;
				activationToggle2.gameObject.SetActive( true );
			}
		}

		public void OnDefeat()
		{
			var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( cardDescriptor.id );
			FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );

			cardDescriptor.heroState.isHealthy = false;
			cardDescriptor.heroState.heroHealth = HeroHealth.Defeated;

			FindObjectOfType<ConfirmPopup>().Hide();
			woundToggle.gameObject.SetActive( false );
			woundToggle.isOn = false;
			woundToggle.gameObject.SetActive( true );
			woundToggle.interactable = false;
			activationToggle1.interactable = false;
			activationToggle2.interactable = false;
			exhaustedOverlay.SetActive( true );

			FindObjectOfType<SagaController>().eventManager.CheckIfEventsTriggered();

			//trigger on defeated Trigger, if it exists
			//(?? heroes and allies don't have a defeated trigger ??)
			if ( ovrd != null && !cardDescriptor.isHero )//not really necessary to check isAlly
			{
				FindObjectOfType<SagaController>().triggerManager.FireTrigger( ovrd.setTrigger );
			}
		}

		public void OnWound()
		{
			FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );

			cardDescriptor.heroState.isHealthy = false;
			cardDescriptor.heroState.heroHealth = HeroHealth.Wounded;

			FindObjectOfType<ConfirmPopup>().Hide();
			woundToggle.gameObject.SetActive( false );
			woundToggle.isOn = false;
			woundToggle.gameObject.SetActive( true );

			FindObjectOfType<SagaController>().eventManager.CheckIfEventsTriggered();
		}
	}
}