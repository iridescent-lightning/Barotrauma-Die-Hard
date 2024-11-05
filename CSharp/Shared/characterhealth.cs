using System;
using System.Reflection;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;


using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.Networking;


using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CharacterHealthMod
{
  class CharacterHealthMod : IAssemblyPlugin
  {
    public Harmony harmony;
	public AfflictionPrefab hypothermiaPrefab;
	public Item armor;
	
    public void Initialize()
    {
      harmony = new Harmony("CharacterHealth");

      harmony.Patch(
                original: typeof(CharacterHealth).GetMethod("ApplyDamage"),
                postfix: new HarmonyMethod(typeof(CharacterHealthMod).GetMethod("ApplyDamage"))
            );

	harmony.Patch(
		original: typeof(CharacterHealth).GetMethod("Update"),
		prefix: new HarmonyMethod(typeof(CharacterHealthMod).GetMethod("Update"))
	);
	
	hypothermiaPrefab = AfflictionPrefab.Prefabs["coldwater"];
    }
    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void Dispose()
    {
      harmony.UnpatchAll();
      harmony = null;
    }
	//leftHand = characterHealth.Character.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand);
	
	
	
    public static void ApplyDamage(Limb hitLimb, AttackResult attackResult, bool allowStacking, CharacterHealth __instance)
    {

		CharacterHealth _ = __instance;

		
		


		var leftHand = _.Character.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand);
		var RightHand = _.Character.Inventory.GetItemInLimbSlot(InvSlotType.RightHand);
		var armor = _.Character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes);
		var innerCloth = _.Character.Inventory.GetItemInLimbSlot(InvSlotType.InnerClothes);
		var headWear = _.Character.Inventory.GetItemInLimbSlot(InvSlotType.Head);

		foreach (Affliction newAffliction in attackResult.Afflictions)
		{

			if (!_.Character.IsHuman || hitLimb.type == null) {return;}
			
			
			if (newAffliction.Prefab.LimbSpecific)
			{
				_.AddLimbAffliction(hitLimb, newAffliction, allowStacking);
				
				if (newAffliction.Identifier == "blunttrauma" && hitLimb.type == LimbType.LeftArm)
				{
					if (leftHand != null)
					{
						leftHand.Drop(_.Character);
					}
				}
				else if (newAffliction.Identifier == "blunttrauma" && hitLimb.type == LimbType.RightArm)//type is lowercase
				{
					if (RightHand != null)
					{
						RightHand.Drop(_.Character);
					}
				}
				
				else if (newAffliction.Identifier == "Armormarker" && hitLimb.type == LimbType.Torso && armor != null)
				{
					if (armor.Prefab.Identifier == "bodyarmorI")
					{
						armor.Condition = armor.Condition - 10f;
						if (armor.Condition == 0f)
						{
							Entity.Spawner.AddEntityToRemoveQueue(armor);
						}
					}
					else if (armor.Prefab.Identifier == "bodyarmorII")
					{
						armor.Condition = armor.Condition - 15f;
						if (armor.Condition == 0f)
						{
							Entity.Spawner.AddEntityToRemoveQueue(armor);
						}
					}
					else if (armor.Prefab.Identifier == "bodyarmorIII")
					{
						armor.Condition = armor.Condition - 20f;
						if (armor.Condition == 0f)
						{
							Entity.Spawner.AddEntityToRemoveQueue(armor);
						}
					}
				}
				else if (newAffliction.Identifier == "Armormarker" && hitLimb.type == LimbType.Torso && innerCloth != null &&
				armor == null && innerCloth.HasTag("clothing"))
				{
					innerCloth.Condition = innerCloth.Condition - 10f;
					if (innerCloth.Condition == 0f)
					{
						Entity.Spawner.AddEntityToRemoveQueue(innerCloth);
					}
				}
				else if (newAffliction.Identifier == "Armormarker" && hitLimb.type == LimbType.Head && headWear != null)
				{
					headWear.Condition = headWear.Condition - 40f;
					if (headWear.Condition == 0f)
					{
						Entity.Spawner.AddEntityToRemoveQueue(headWear);
					}
				}
			}
			
			else
			{
			_.AddAffliction(newAffliction, allowStacking);
			}
		
      }


		// Sever legs or waist effect
	  if (_.Character.AnimController is HumanoidAnimController humanAnimController) // cast type
		{
			// Severed legs cause the player to crouch or fall down
			foreach (Limb limb in humanAnimController.Limbs)
			{
				if (limb.IsSevered && (limb.type == LimbType.LeftLeg || limb.type == LimbType.RightLeg || limb.type == LimbType.LeftThigh || limb.type == LimbType.RightThigh || limb.type == LimbType.Waist))
				{
					// Force the crouching state. This only works controlled character
					/*humanAnimController.ForceSelectAnimationType = AnimationType.Crouch; 
					humanAnimController.Crouching = true;
					_.Character.SetInput(InputType.Crouch, hit: false, held: true);*/

					// Load the crouch animation
					_.Character.SetStun(1);
					AnimationParams animParams;

					humanAnimController.TryLoadAnimation(AnimationType.Run, "HumanRunCrawl_LegSevered", out animParams, true);
					humanAnimController.TryLoadAnimation(AnimationType.Walk, "HumanWalkCrawl_LegSevered", out animParams, true);
					
					break; // Exit the loop once a severed limb is found
				}
			}
		}
	  
      
    }
	
	public static void ApplyFlowForces(float deltaTime, Character character, Hull hull)
        {
            
            foreach (var gap in hull.ConnectedGaps.Where(gap => gap.Open > 0 && !gap.IsRoomToRoom))
            {
                var distance = MathHelper.Max(Vector2.DistanceSquared(character.Position, gap.Position) / 1000, 1f);
                Vector2 force = (gap.LerpedFlowForce / (distance / 15)) * deltaTime;
                if (force.LengthSquared() > 0.01f)
                {
                    character.AnimController.Collider.FarseerBody.ApplyLinearImpulse(force * 100);
                }
            }
            
        }
	
	public static void Update(CharacterHealth __instance, float deltaTime)
        {
			CharacterHealth _ = __instance;


        
			// Defualt Character Status Effect Attributes
			if (_.Character.IsHuman && _.Character.InWater)
			{
				_.ApplyAffliction(_.Character.AnimController.MainLimb, AfflictionPrefab.Prefabs["coldwater"].Instantiate(0.3f * deltaTime));
			}
			else if (_.Character.IsHuman && _.Character.AnimController.IsMovingFast)//IsMovingFast doesn't detect water
			{
				_.ApplyAffliction(_.Character.AnimController.MainLimb, AfflictionPrefab.Prefabs["fatigue"].Instantiate(10f * deltaTime));
			}
			
			if (_.Character.IsHuman && !_.Character.IsDead && _.Character.CurrentHull != null)
			{
				_.Character.PressureProtection= 4500.0f;
				
			}

			/*if (_.Character.CurrentHull !=null && _.Character.InWater)
			{
				ApplyFlowForces(deltaTime, _.Character, _.Character.CurrentHull);
			}*/
		
		}

  	}
}
