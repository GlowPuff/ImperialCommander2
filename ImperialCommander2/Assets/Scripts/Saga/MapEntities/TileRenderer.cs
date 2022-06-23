using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Saga
{
	public class TileRenderer : MonoBehaviour
	{
		public SpriteRenderer spriteRenderer;
		public bool isLoaded = false;

		[HideInInspector]
		public MapTile mapTile;
		TileDescriptor tileDescriptor;
		//handle used to load and release tile asset
		AsyncOperationHandle<Sprite> loadHandle;

		public void LoadTile( MapTile t, TileDescriptor td )
		{
			mapTile = t;
			tileDescriptor = td;
			//Debug.Log( "LoadTile()::" + mapTile.tileID );
			var textname = $"{t.expansion}_{t.tileID}{t.tileSide}";
			loadHandle = Addressables.LoadAssetAsync<Sprite>( textname );
			loadHandle.Completed += TileRenderer_Completed;
		}

		private void TileRenderer_Completed( AsyncOperationHandle<Sprite> tex )
		{
			if ( tex.Result != null )
			{
				spriteRenderer.sprite = tex.Result;
				//convert from ICE editor coords to Unity coords
				transform.position = new Vector3( mapTile.entityPosition.X / 10, 0, -mapTile.entityPosition.Y / 10 );
				transform.rotation = Quaternion.Euler( 90, 0, -mapTile.entityRotation );
				float s = Mathf.Max( tileDescriptor.width, tileDescriptor.height );
				transform.localScale = new Vector3( s, s, s );
				isLoaded = true;
			}
			else
				FindObjectOfType<SagaController>().ShowError( $"LoadTile()::{mapTile.textureName} not found" );
		}

		/// <summary>
		/// ONLY changes visibility, does NOT modify isActive
		/// </summary>
		public void ShowTile( bool immediate = false )
		{
			//Debug.Log( $"SHOWING TILE::{tileDescriptor.id}" );
			if ( mapTile.entityProperties.isActive )
			{
				if ( immediate )
					spriteRenderer.color = Color.white;
				else
					spriteRenderer.DOFade( 1, 1.5f );
			}
		}

		/// <summary>
		/// ONLY changes visibility, does NOT modify isActive
		/// </summary>
		public void HideTile( bool immediate = false )
		{
			Debug.Log( $"HIDING TILE::{tileDescriptor.id}" );
			if ( immediate )
				spriteRenderer.color = new Color( 1, 1, 1, 0 );
			else
				spriteRenderer.DOFade( 0, 1f );
		}

		public void ModifyVisibility( bool vis )
		{
			mapTile.entityProperties.isActive = vis;
			if ( vis )
				ShowTile();
			else
				HideTile();
		}

		private void OnDestroy()
		{
			Addressables.Release( loadHandle );
		}
	}
}
