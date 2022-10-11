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

			addPanel.SetActive( false );
			heroPanel.SetActive( true );

			foreach ( Transform child in contentContainer )
				Destroy( child.gameObject );

			campaignHero = new CampaignHero()
			{
				heroID = card.id
			};
			FindObjectOfType<CampaignManager>().AddHeroToCampaign( campaignHero );
			mug.sprite = Resources.Load<Sprite>( $"Cards/Heroes/{card.id}" );
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
				campaignHero.campaignSkills.Add( skill );
				AddHeroToUI( campaignHero );
			}
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
			//hero
			campaignHero = hero;
			mug.sprite = Resources.Load<Sprite>( $"Cards/Heroes/{hero.heroID}" );
			//skills
			foreach ( var skill in campaignHero.campaignSkills )
			{
				var go = Instantiate( listItem, contentContainer );
				go.GetComponent<CampaignListItemPrefab>().InitSkill( skill.name, ( n ) =>
				{
					campaignHero.campaignSkills.Remove( skill );
					Destroy( go );
				} );
			}
			//items
			foreach ( var item in campaignHero.campaignItems )
			{
				var go = Instantiate( listItem, contentContainer );
				go.GetComponent<CampaignListItemPrefab>().InitItem( item.name, ( n ) =>
				{
					campaignHero.campaignItems.Remove( item );
					Destroy( go );
				} );
			}
		}
	}
}