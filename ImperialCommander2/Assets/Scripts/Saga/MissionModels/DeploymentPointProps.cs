using System.Collections.Generic;
using System.Linq;

namespace Saga
{
	public class DeploymentPointProps
	{
		public bool incImperial;
		public bool incMercenary;
		public bool incSmall;
		public bool incMedium;
		public bool incLarge;
		public bool incHuge;
		public bool incBrawler;
		public bool incCreature;
		public bool incDroid;
		public bool incForceUser;
		public bool incGuardian;
		public bool incHeavyWeapon;
		public bool incHunter;
		public bool incLeader;
		public bool incSmuggler;
		public bool incSpy;
		public bool incTrooper;
		public bool incWookiee;
		public bool incVehicle;

		public DeploymentPointProps()
		{
			//incImperial = incMercenary = true;
			//incSmall = incMedium = incLarge = incHuge = true;
			//incBrawler = incCreature = incDroid = incForceUser = true;
			//incGuardian = incHeavyWeapon = incHunter = incLeader = true;
			//incSmuggler = incSpy = incTrooper = incWookiee = true;
		}

		/// <summary>
		/// Test if this DP should be filtered out, given the enemy deployment
		/// </summary>
		public bool IsFilteredOut( DeploymentCard enemyToAdd )
		{
			//Faction
			if ( !incMercenary && enemyToAdd.faction == "Mercenary" )
				return true;
			if ( !incImperial && enemyToAdd.faction == "Imperial" )
				return true;
			//figure size
			if ( !incSmall && enemyToAdd.miniSize == FigureSize.Small1x1 )
				return true;
			if ( !incMedium && enemyToAdd.miniSize == FigureSize.Medium1x2 )
				return true;
			if ( !incLarge && enemyToAdd.miniSize == FigureSize.Large2x2 )
				return true;
			if ( !incHuge && enemyToAdd.miniSize == FigureSize.Huge2x3 )
				return true;
			//traits
			List<string> traits = new List<string>();
			if ( incBrawler )
				traits.Add( "Brawler" );
			if ( incCreature )
				traits.Add( "Creature" );
			if ( incDroid )
				traits.Add( "Droid" );
			if ( incForceUser )
				traits.Add( "ForceUser" );
			if ( incGuardian )
				traits.Add( "Guardian" );
			if ( incHeavyWeapon )
				traits.Add( "HeavyWeapon" );
			if ( incHunter )
				traits.Add( "Hunter" );
			if ( incLeader )
				traits.Add( "Leader" );
			if ( incSmuggler )
				traits.Add( "Smuggler" );
			if ( incSpy )
				traits.Add( "Spy" );
			if ( incTrooper )
				traits.Add( "Trooper" );
			if ( incWookiee )
				traits.Add( "Wookiee" );
			if ( incVehicle )
				traits.Add( "Vehicle" );

			if ( !traits.Any( x => enemyToAdd.traits.Contains( x ) ) )
				return true;

			return false;
		}

		//public void CopyFrom( DeploymentPointProps dp )
		//{
		//	incImperial = dp.incImperial;
		//	incMercenary = dp.incMercenary;

		//	incSmall = dp.incSmall;
		//	incMedium = dp.incMedium;
		//	incLarge = dp.incLarge;
		//	incHuge = dp.incHuge;

		//	incBrawler = dp.incBrawler;
		//	incCreature = dp.incCreature;
		//	incDroid = dp.incDroid;
		//	incForceUser = dp.incForceUser;
		//	incGuardian = dp.incGuardian;
		//	incHeavyWeapon = dp.incHeavyWeapon;
		//	incHunter = dp.incHunter;
		//	incLeader = dp.incLeader;
		//	incSmuggler = dp.incSmuggler;
		//	incSpy = dp.incSpy;
		//	incTrooper = dp.incTrooper;
		//	incWookiee = dp.incWookiee;
		//}
	}
}
