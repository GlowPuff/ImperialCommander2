using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Saga
{
	public class CampaignHeroPrefab : MonoBehaviour
	{
		public GameObject addPanel, heroPanel, listItem;
		public CampaignHero campaignHero;
		public Transform contentContainer;
		public Button addItemButton;
		//UI
		public Image mug;
		public TextMeshProUGUI xpLabelText, xpAmountText;
		public MWheelHandler mWheelHandler;

		SagaCampaign sagaCampaign;

		public void AddHero()
		{
			GlowEngine.FindUnityObject<AddCampaignItemPopup>().AddHero( OnHeroAdded );
		}

		public void RemoveHero()
		{
			addPanel.SetActive( true );
			heroPanel.SetActive( false );

			FindObjectOfType<CampaignManager>().RemoveHeroFromCampaign( campaignHero );
			campaignHero = null;
		}

		public void AddSkill()
		{
			GlowEngine.FindUnityObject<AddCampaignItemPopup>().AddSkill( campaignHero.heroID, OnSkillAdded );
		}

		public void AddItem()
		{
			GlowEngine.FindUnityObject<AddCampaignItemPopup>().AddItem( OnItemAdded, true );
		}

		void OnHeroAdded( DeploymentCard card )
		{
			sagaCampaign = FindObjectOfType<CampaignManager>().sagaCampaign;

			mWheelHandler.valueAdjuster = FindObjectOfType<CampaignManager>().valueAdjuster;

			addPanel.SetActive( false );
			heroPanel.SetActive( true );

			foreach ( Transform child in contentContainer )
				Destroy( child.gameObject );

			campaignHero = new CampaignHero()
			{
				heroID = card.id,
				mugShotPath = card.mugShotPath
			};
			FindObjectOfType<CampaignManager>().AddHeroToCampaign( campaignHero );
			mug.sprite = Resources.Load<Sprite>( card.mugShotPath );
			xpLabelText.text = DataStore.uiLanguage.uiCampaign.xpUC;
		}

		void OnItemAdded( CampaignItem item )
		{
			if ( !campaignHero.campaignItems.Contains( item ) )
			{
				campaignHero.campaignItems.Add( item );
				AddHeroToUI( campaignHero );
			}
		}

		void OnSkillAdded( CampaignSkill skill )
		{
			if ( !campaignHero.campaignSkills.Contains( skill ) )
			{
				campaignHero.xpAmount = Mathf.Max( 0, campaignHero.xpAmount - skill.cost );
				mWheelHandler.ResetWheeler( campaignHero.xpAmount );
				campaignHero.campaignSkills.Add( skill );
				AddHeroToUI( campaignHero );
			}
		}

		public void OnXPChanged()
		{
			campaignHero.xpAmount = mWheelHandler.wheelValue;
		}

		private void Update()
		{
			addItemButton.interactable = sagaCampaign?.campaignItems.Count > 0;
		}

		//resets/adds the hero along with all items/skills
		public void AddHeroToUI( CampaignHero hero )
		{
			sagaCampaign = FindObjectOfType<CampaignManager>().sagaCampaign;

			addPanel.SetActive( false );
			heroPanel.SetActive( true );

			foreach ( Transform child in contentContainer )
				Destroy( child.gameObject );

			xpAmountText.text = "0";

			//hero
			campaignHero = hero;
			mug.sprite = Resources.Load<Sprite>( hero.mugShotPath );
			mWheelHandler.ResetWheeler( hero.xpAmount );
			xpLabelText.text = DataStore.uiLanguage.uiCampaign.xpUC;
			//skills
			var skills = SagaCampaign.campaignDataSkills.Concat( DataStore.globalImportedCharacters.Where( x => x.deploymentCard.characterType == CharacterType.Hero ).SelectMany( x => x.heroSkills ) );

			foreach ( var skill in campaignHero.campaignSkills.OrderBy( x => x.name ) )
			{
				var go = Instantiate( listItem, contentContainer );
				var selectedSkill = skills.FirstOrDefault( x => x.id == skill.id );
				if ( selectedSkill != null )
				{
					go.GetComponent<CampaignListItemPrefab>().InitSkill( selectedSkill.name, ( n ) =>
					{
						campaignHero.xpAmount += skill.cost;
						mWheelHandler.ResetWheeler( hero.xpAmount );
						campaignHero.campaignSkills.Remove( skill );
						Destroy( go );
					} );
				}
			}
			//items
			foreach ( var item in campaignHero.campaignItems.OrderBy( x => x.name ) )
			{
				var go = Instantiate( listItem, contentContainer );
				var selectedItem = SagaCampaign.campaignDataItems.FirstOrDefault( x => x.id == item.id );
				if ( selectedItem != null )
				{
					go.GetComponent<CampaignListItemPrefab>().InitItem( selectedItem.name, ( n ) =>
					{
						campaignHero.campaignItems.Remove( item );
						Destroy( go );
					} );
				}
			}
		}
	}
}