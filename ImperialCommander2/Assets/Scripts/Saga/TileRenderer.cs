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

		public void LoadTile( MapTile t, TileDescriptor td )
		{
			mapTile = t;
			tileDescriptor = td;
			//Debug.Log( "LoadTile()::" + mapTile.tileID );
			Addressables.LoadAssetAsync<Sprite>( mapTile.textureName ).Completed += TileRenderer_Completed;
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

		public void ShowTile( bool immediate = false )
		{
			Debug.Log( $"SHOWING TILE::{tileDescriptor.id}" );
			mapTile.entityProperties.isActive = true;
			if ( immediate )
				spriteRenderer.color = Color.white;
			else
				spriteRenderer.DOFade( 1, 1f );
		}

		public void HideTile( bool immediate = false )
		{
			Debug.Log( $"HIDING TILE::{tileDescriptor.id}" );
			mapTile.entityProperties.isActive = false;
			if ( immediate )
				spriteRenderer.color = new Color( 1, 1, 1, 0 );
			else
				spriteRenderer.DOFade( 0, 1f );
		}
	}
}
