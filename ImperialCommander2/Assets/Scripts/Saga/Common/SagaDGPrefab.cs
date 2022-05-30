using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class SagaDGPrefab : MonoBehaviour
	{
		public Toggle[] countToggles;
		public Image colorPip, iconImage, outlineColor;
		//public Outline outline;
		public Color eliteColor;
		public Color[] colors;
		public GameObject exhaustedOverlay, modifierBox;
		public Button selfButton;
		public TextMeshProUGUI modText;

		public bool IsExhausted { get { return exhaustedOverlay.activeInHierarchy; } }
		public DeploymentCard Card { get { return cardDescriptor; } }

		DeploymentCard cardDescriptor;
		int colorIndex = 0;
		[HideInInspector]
		public bool isConfirming = false;

		private void Awake()
		{
			Transform tf = transform.GetChild( 0 );
			tf.localScale = Vector3.zero;
		}

		/// <summary>
		/// Takes an enemy, villain, or ally
		/// </summary>
		public void Init( DeploymentCard cd )
		{
			Debug.Log( "DEPLOYED: " + cd.name );
			cardDescriptor = cd;
			for ( int i = 0; i < cd.size; i++ )
				countToggles[i].gameObject.SetActive( true );
			selfButton.interactable = true;

			ToggleExhausted( cd.hasActivated );

			if ( DataStore.deploymentCards.Any( x => x.id == cd.id ) )
			{
				iconImage.sprite = Resources.Load<Sprite>( $"Cards/Enemies/{cd.expansion}/{cd.id.Replace( "DG", "M" )}" );
				cd.hasDeployed = true;
			}
			else if ( DataStore.villainCards.Any( x => x.id == cd.id ) )
			{
				iconImage.sprite = Resources.Load<Sprite>( $"Cards/Villains/{cd.id.Replace( "DG", "M" )}" );
				//outline.effectColor = eliteColor;
				outlineColor.color = eliteColor;
				cd.hasDeployed = true;
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

			//check for using generic mugshot
			var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( cd.id );
			if ( ovrd != null && !ovrd.useGenericMugshot )
				iconImage.sprite = Resources.Load<Sprite>( $"Cards/genericEnemy" );

			if ( cd.isElite )
				outlineColor.color = eliteColor;
			//outline.effectColor = eliteColor;

			SetColorIndex();

			//check for modifier text override
			if ( ovrd != null && ovrd.showMod && !string.IsNullOrEmpty( ovrd.modification.Trim() ) )
			{
				modifierBox.SetActive( true );
				modText.text = Utils.ReplaceGlyphs( ovrd.modification );
			}
			else
				modifierBox.SetActive( false );

			Transform tf = transform.GetChild( 0 );
			Transform tf2 = transform.GetChild( 1 );
			tf.localScale = Vector3.zero;
			tf2.localScale = Vector3.zero;
			tf.DOScale( 1, 1f ).SetEase( Ease.OutBounce );
			tf2.DOScale( 1, 1f ).SetEase( Ease.OutBounce );
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
				ShowConfirm();

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
				ShowConfirm();

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
				ShowConfirm();

			//Debug.Log( "SIZE: " + cardDescriptor.currentSize );
		}

		/// <summary>
		/// Visually remove, adds card back to hand (if not villain/custom)
		/// </summary>
		public void RemoveSelf()
		{
			for ( int i = 0; i < 3; i++ )
				countToggles[i].gameObject.SetActive( false );
			selfButton.interactable = false;

			Transform tf = transform.GetChild( 0 );
			tf.DOScale( 0, .35f ).SetEase( Ease.InCirc ).OnComplete( () =>
			{
				//add card back to dep hand ONLY IF IT'S NOT THE CUSTOM GROUP
				//AND if it's NOT a villain
				if ( cardDescriptor.id != "DG070" && !DataStore.villainCards.ContainsCard( cardDescriptor ) )
					DataStore.deploymentHand.Add( cardDescriptor );
				//remove it from deployed list
				DataStore.deployedEnemies.Remove( cardDescriptor );
				//if it is an EARNED villain, add it back into manual deploy list
				if ( DataStore.sagaSessionData.EarnedVillains.ContainsCard( cardDescriptor ) && !DataStore.manualDeploymentList.ContainsCard( cardDescriptor ) )
				{
					DataStore.manualDeploymentList.Add( cardDescriptor );
					DataStore.SortManualDeployList();
				}

				if ( DataStore.deployedEnemies.Count == 0 )
					FindObjectOfType<SagaController>().eventManager.CheckIfEventsTriggered();

				if ( DataStore.sagaSessionData.setupOptions.useAdaptiveDifficulty )
				{
					//add fame value
					DataStore.sagaSessionData.gameVars.fame += cardDescriptor.fame;
					//reimburse some Threat
					DataStore.sagaSessionData.ModifyThreat( cardDescriptor.reimb );
					//show fame popup
					GlowEngine.FindUnityObject<QuickMessage>().Show( $"{DataStore.uiLanguage.uiMainApp.fameIncreasedUC}: <color=\"green\">{cardDescriptor.fame}</color>" );
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
			if ( FindObjectOfType<SagaEventManager>().IsUIHidden )
				return;

			if ( !isConfirming && FindObjectOfType<ConfirmPopup>() == null )
				ShowConfirm();
			else
			{
				FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
				FindObjectOfType<ConfirmPopup>()?.Hide();
			}
		}

		public void OnActivateSelf()
		{
			//if ( !exhaustedOverlay.activeInHierarchy )
			FindObjectOfType<MainGameController>().ActivateEnemy( cardDescriptor );
		}

		void ShowConfirm()
		{
			FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( false );
			FindObjectOfType<SagaEventManager>()?.transform.Find( "DGConfirmPopup" ).GetComponent<ConfirmPopup>().ShowLeft( transform, this, OnDefeat, OnExhaust );
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
			CardViewPopup cardViewPopup = GlowEngine.FindUnityObject<CardViewPopup>();
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

		public void OnDefeat()
		{
			var ovrd = DataStore.sagaSessionData.gameVars.GetDeploymentOverride( cardDescriptor.id );
			FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );

			if ( ovrd == null || (ovrd != null && ovrd.canBeDefeated) )
			{
				FindObjectOfType<ConfirmPopup>().Hide( () =>
				{
					RemoveSelf();
				} );
			}
			else
				GlowEngine.FindUnityObject<QuickMessage>().Show( "This group cannot be defeated." );

			//trigger on defeated Trigger, if it exists
			if ( ovrd != null )
			{
				FindObjectOfType<SagaController>().triggerManager.FireTrigger( ovrd.setTrigger );
				FindObjectOfType<SagaController>().eventManager.DoEvent( ovrd.setEvent );
				//soft reset of override
				ovrd.SoftReset();
			}
		}

		public void OnExhaust()
		{
			exhaustedOverlay.SetActive( !exhaustedOverlay.activeInHierarchy );
			FindObjectOfType<ConfirmPopup>().Hide();
			FindObjectOfType<SagaController>().ToggleNavAndEntitySelection( true );
		}
	}
}
