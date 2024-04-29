using System.Collections.Generic;

namespace Saga
{
	public class GroupPriorityTraits
	{
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
		public bool useDefaultPriority;

		public GroupPriorityTraits()
		{
			incBrawler = incCreature = incDroid = incForceUser = true;
			incGuardian = incHeavyWeapon = incHunter = incLeader = true;
			incSmuggler = incSpy = incTrooper = incWookiee = incVehicle = true;
			useDefaultPriority = true;
		}

		public GroupTraits[] GetTraitArray()
		{
			List<GroupTraits> list = new List<GroupTraits>();
			if ( incBrawler )
				list.Add( GroupTraits.Brawler );
			if ( incCreature )
				list.Add( GroupTraits.Creature );
			if ( incDroid )
				list.Add( GroupTraits.Droid );
			if ( incForceUser )
				list.Add( GroupTraits.ForceUser );
			if ( incGuardian )
				list.Add( GroupTraits.Guardian );
			if ( incHeavyWeapon )
				list.Add( GroupTraits.HeavyWeapon );
			if ( incHunter )
				list.Add( GroupTraits.Hunter );
			if ( incLeader )
				list.Add( GroupTraits.Leader );
			if ( incSmuggler )
				list.Add( GroupTraits.Smuggler );
			if ( incSpy )
				list.Add( GroupTraits.Spy );
			if ( incTrooper )
				list.Add( GroupTraits.Trooper );
			if ( incWookiee )
				list.Add( GroupTraits.Wookiee );
			if ( incVehicle )
				list.Add( GroupTraits.Vehicle );

			return list.ToArray();
		}

		//public GroupPriorityTraits FromArray( GroupTraits[] traits )
		//{
		//	GroupPriorityTraits result = new GroupPriorityTraits()
		//	{
		//		incBrawler = incCreature = incDroid = incForceUser = false,
		//		incGuardian = incHeavyWeapon = incHunter = incLeader = false,
		//		incSmuggler = incSpy = incTrooper = incWookiee = false,
		//	};

		//	foreach ( GroupTraits t in traits )
		//	{
		//		if ( t == GroupTraits.Brawler )
		//			incBrawler = true;
		//	}
		//	return result;
		//}
	}
}
