using System;
using Newtonsoft.Json;

namespace Saga
{
	public interface IPropertyModel { };
	public interface IMapEntity
	{
		Guid GUID { get; set; }
		string name { get; set; }
		EntityType entityType { get; set; }
		Vector entityPosition { get; set; }
		float entityRotation { get; set; }
		[JsonIgnore]
		bool hasProperties { get; }
		[JsonIgnore]
		bool hasColor { get; }
		EntityProperties entityProperties { get; set; }
		Guid mapSectionOwner { get; set; }
		//void ModifyEntity( ModifyMapEntity mod );
		//IMapEntity Duplicate();
		//bool Validate();
	}
	public interface IEventActionDialog
	{
		IEventAction eventAction { get; set; }
	}
	public interface IEventAction
	{
		Guid GUID { get; set; }
		EventActionType eventActionType { get; set; }
		string displayName { get; set; }
	}
	public interface IEndTurnCleanup
	{
		void EndTurnCleanup();
	}

	public interface IEntityPrefab
	{
		IMapEntity mapEntity { get; set; }
		bool isAnimationBusy { get; set; }
		void ModifyEntity( EntityProperties mod );
		/// <summary>
		/// Makes an entity visible IF isActive=true
		/// </summary>
		void ShowEntity();
		/// <summary>
		/// Hides an entity, does NOT modify IsActive
		/// </summary>
		void HideEntity();
	}

	///enums
	public enum CustomInstructionType { Top, Bottom, Replace }
	public enum ThreatModifierType { None, Fixed, Multiple }
	public enum YesNoAll { Yes, No, All }
	public enum PriorityTargetType { Rebel, Hero, Ally, Other, Trait }
	public enum EntityType { Tile, Terminal, Crate, DeploymentPoint, Token, Highlight, Door }
	public enum TokenShape { Circle, Square, Rectangle }
	public enum EventActionType { G1, G2, G3, G4, G5, G6, D1, D2, D3, D4, D5, GM1, GM2, GM3, M1, M2, G7, GM4, GM5, G8, G9, D6 }
	public enum ThreatAction { Add, Remove }
	public enum SourceType { InitialReserved, Manual, Hand }
	public enum DeploymentSpot { Active, Specific, None }
	public enum GroupType { All, Specific }
	public enum MarkerType { Neutral, Rebel, Imperial }
	public enum MissionType { Story, Side, Forced }
	public enum MissionSubType { Agenda, Threat, Other, Finale, General, Personal, Villain, Ally }
	public enum PickerMode { BuiltIn, Custom }
	//public enum DiceColor { White, Black, Yellow, Red, Green, Blue, Grey }
	//public enum AttackType { Ranged, Melee, None }
	//public enum FigureSize { Small1x1, Medium1x2, Large2x2, Huge2x3 }
	//public enum GroupTraits { Trooper, Leader, HeavyWeapon, Guardian, Brawler, Droid, Vehicle, Hunter, Creature, Smuggler, Spy, ForceUser, Wookiee, Hero }
	public enum LifeSpan { Manual, EndTurn }
}