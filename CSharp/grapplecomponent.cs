using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;

using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Barotrauma;

using HarmonyLib;

namespace GrappleMod//todo make a structural namespace DieHard.Item.Components. namespace can't be used in elsewhere
{
    class Grapple : Rope
    {
		[Serialize(100.0f, IsPropertySaveable.No, description: "The maximum strength of the rope before it snaps.")]
        public float Strength
        {
            get;
            set;
        }
		
		//private Rope ropeComponent;//declare a component here 
        public override void OnItemLoaded()
        {
            base.OnItemLoaded();
        }
		public Grapple(Item item, ContentXElement element)
            : base (item, element)
        {
            
			
        }
       
       public override void Update(float deltaTime, Camera cam)
        {
            var user = item.GetComponent<Projectile>()?.User;
            if (source == null || target == null || target.Removed ||
                (source is Entity sourceEntity && sourceEntity.Removed) ||
                (source is Limb limb && limb.Removed) ||
                (user != null && user.Removed))
            {
                ResetSource();
                target = null;
                IsActive = false;
                return;
            }

            if (Snapped)
            {
                snapTimer += deltaTime;
                if (snapTimer >= SnapAnimDuration)
                {
                    IsActive = false;
                }
                return;
            }

            Vector2 diff = target.WorldPosition - GetSourcePos(useDrawPosition: false);
            float lengthSqr = diff.LengthSquared();
            

            if (MaxAngle < 180 && lengthSqr > 2500)
            {
                if (launchDir == null)
                {
                    launchDir = diff;
                }
                float angle = MathHelper.ToDegrees(VectorExtensions.Angle(launchDir.Value, diff));
                if (angle > MaxAngle)
                {
                    Snap();
                    return;
                }
            }

#if CLIENT
            item.ResetCachedVisibleSize();
#endif
            var projectile = target.GetComponent<Projectile>();
            if (projectile == null) { return; }

            if (SnapOnCollision)
            {
                raycastTimer += deltaTime;
                if (raycastTimer > RayCastInterval)
                {
                    if (Submarine.PickBody(ConvertUnits.ToSimUnits(source.WorldPosition), ConvertUnits.ToSimUnits(target.WorldPosition),
                        collisionCategory: Physics.CollisionLevel | Physics.CollisionWall,
                        customPredicate: (Fixture f) =>
                        {
                            foreach (Body body in projectile.Hits)
                            {
                                Submarine alreadyHitSub = null;
                                if (body.UserData is Structure hitStructure)
                                {
                                    alreadyHitSub = hitStructure.Submarine;
                                }
                                else if (body.UserData is Submarine hitSub)
                                {
                                    alreadyHitSub = hitSub;
                                }
                                if (alreadyHitSub != null)
                                {
                                    if (f.Body?.UserData is MapEntity me && me.Submarine == alreadyHitSub) { return false; }
                                    if (f.Body?.UserData as Submarine == alreadyHitSub) { return false; }
                                }
                            }
                            Submarine targetSub = projectile.StickTarget?.UserData as Submarine ?? target.Submarine;

                            if (f.Body?.UserData is MapEntity mapEntity && mapEntity.Submarine != null)
                            {
                                if (mapEntity.Submarine == targetSub || mapEntity.Submarine == source.Submarine)
                                {
                                    return false;
                                }
                            }
                            else if (f.Body?.UserData is Submarine sub)
                            {
                                if (sub == targetSub || sub == source.Submarine)
                                {
                                    return false;
                                }
                            }
                            return true;
                        }) != null)
                    {
                        Snap();
                        return;
                    }
                    raycastTimer = 0.0f;
                }
            }

            Vector2 forceDir = diff;
            float distance = diff.Length();
            if (distance > 0.001f)
            {
                forceDir = Vector2.Normalize(forceDir);
            }

            if (Math.Abs(ProjectilePullForce) > 0.001f)
            {
                projectile.Item?.body?.ApplyForce(-forceDir * ProjectilePullForce);                
            }

            if (projectile.StickTarget != null)
            {
                float targetMass = float.MaxValue;
                Character targetCharacter = null;
                if (projectile.StickTarget.UserData is Limb targetLimb)
                {
                    targetCharacter = targetLimb.character;
                    targetMass = targetLimb.ragdoll.Mass;
                }
                else if (projectile.StickTarget.UserData is Character character)
                {
                    targetCharacter = character;
                    targetMass = character.Mass;
                }
                else if (projectile.StickTarget.UserData is Item item)
                {
                    targetMass = projectile.StickTarget.Mass;
                }
                if (projectile.StickTarget.BodyType != BodyType.Dynamic)
                {
                    targetMass = float.MaxValue;
                }
                if (targetMass > TargetMinMass)
                {
                    if (Math.Abs(SourcePullForce) > 0.001f)
                    {
                        var sourceBody = GetBodyToPull(source);
                        if (sourceBody != null)
                        {
                            if (user != null )
                            {
                                
                                    // Reel in towards the target.
                                    user.AnimController.Hang();
                                    float force = LerpForces ? MathHelper.Lerp(0, SourcePullForce, MathUtils.InverseLerp(0, MaxLength / 2, distance)) : SourcePullForce;
                                    sourceBody.ApplyForce(forceDir * force);
                                    user.AnimController.Collider.FarseerBody.IgnoreGravity = true;
                                
                                // Take the target velocity into account.
                                if (targetCharacter != null)
                                {
                                    var myCollider = user.AnimController.Collider;
                                    var targetCollider = targetCharacter.AnimController.Collider;
                                    if (myCollider.LinearVelocity != Vector2.Zero && targetCollider.LinearVelocity != Vector2.Zero)
                                    {
                                        if (Vector2.Dot(Vector2.Normalize(myCollider.LinearVelocity), Vector2.Normalize(targetCollider.LinearVelocity)) < 0)
                                        {
                                            myCollider.ApplyForce(targetCollider.LinearVelocity * targetCollider.Mass);
                                        }
                                    }
                                }
                                else
                                {
                                    var targetBody = GetBodyToPull(target);
                                    if (targetBody != null)
                                    {
                                        sourceBody.ApplyForce(targetBody.LinearVelocity * sourceBody.Mass);
                                    }
                                }
                            }
                        }
                    }
                }
                if (lengthSqr >= MaxLength * MaxLength)
                {
                    var targetBody = GetBodyToPull(target);
                    bool lerpForces = LerpForces;
                    if (!lerpForces && user != null && targetCharacter != null && !user.AnimController.InWater)
                    {
                        if ((forceDir.X < 0) != (user.AnimController.Dir < 0))
                        {
                            // Prevents rubberbanding horizontally when dragging a corpse.
                            lerpForces = true;
                        }
                    }
                    float force = lerpForces ? MathHelper.Lerp(0, TargetPullForce, MathUtils.InverseLerp(0, MaxLength / 3, distance - 50)) : TargetPullForce;
                    targetBody?.ApplyForce(-forceDir * force);
                    var targetRagdoll = targetCharacter?.AnimController;
                    if (targetRagdoll?.Collider != null && (targetRagdoll.InWater || targetRagdoll.OnGround))
                    {
                        targetRagdoll.Collider.ApplyForce(-forceDir * force * 3);
                    }
                    if (lengthSqr > MaxLength * MaxLength)
                    {
                        if (Math.Abs(force) > Strength)
                        {
                            Snap();
                            return;
                        }
                    }
                }
            }
        }
		
       

        
    }
}
