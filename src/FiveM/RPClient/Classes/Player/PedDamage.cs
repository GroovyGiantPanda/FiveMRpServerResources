using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Common;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Player
{
    enum KillerCategory
    {
        None = 0,
        Vehicle = 1,
        Player = 2,
        MountainLion = 3,
        TigerShark = 4,
        HammerShark = 5,
        KillerWhale = 6
    }

    // TODO: Add GET_PED_SOURCE_OF_DEATH for extra redundancy
    // TODO: Add extra flag for damage done by mountain lion as I don't believe they have a special weapon
    // TODO: Add check for if they got damaged from behind/front
    static class PedDamage
    {
        static PedList PedList = new PedList();
        static List<Ped> NearbyPeds = null;
        static List<WeaponHash> SharpMeleeWeapons = new List<WeaponHash>()
        {
            WeaponHash.BattleAxe,
            WeaponHash.Dagger,
            WeaponHash.Hatchet,
            WeaponHash.SwitchBlade,
            WeaponHash.Knife
        };

        static public void Init()
        {
            PeriodicDamageCheck();
            PeriodicNearbyRefresh();
            Function.Call(Hash.DECOR_REGISTER, $"Damage.Petrol", 3);
            Function.Call(Hash.DECOR_REGISTER, $"Damage.Projectile", 3);
            Function.Call(Hash.DECOR_REGISTER, $"Damage.Melee.Sharp", 3);
            Function.Call(Hash.DECOR_REGISTER, $"Damage.Melee.Blunt", 3);
            Function.Call(Hash.DECOR_REGISTER, $"Damage.Vehicle", 3);
            Function.Call(Hash.DECOR_REGISTER, $"Damage.KillerCategory", 3);

            var pedList = new PedList();

            // Gate after testing
            var weaponSourceMenuItem = new MenuItemStandard { Title = "Check Closest for Weapon Source", OnActivate = (item) => { CheckWeaponSources(new PedList().Select(p => new Ped(p)).Where(p => p != Game.PlayerPed).OrderBy(p => p.Position.DistanceToSquared(Game.PlayerPed.Position)).First()); } };
            InteractionListMenu.RegisterInteractionMenuItem(weaponSourceMenuItem, () => pedList.Select(p => new Ped(p)).Where(p => p != Game.PlayerPed).Where(p => p.Position.DistanceToSquared(Game.PlayerPed.Position) < 9f).Count() > 0, 1029);

            var damageSourceMenuItem = new MenuItemStandard { Title = "Check Closest for Damage Source Type", OnActivate = (item) => { Enum.GetValues(typeof(CountedDamageType)).OfType<CountedDamageType>().ToList().ForEach(dt => { CheckDamageSourceType(new PedList().Select(p => new Ped(p)).Where(p => p != Game.PlayerPed).OrderBy(p => p.Position.DistanceToSquared(Game.PlayerPed.Position)).First(), dt); }); } };
            InteractionListMenu.RegisterInteractionMenuItem(damageSourceMenuItem, () => pedList.Select(p => new Ped(p)).Where(p => p != Game.PlayerPed).Where(p => p.Position.DistanceToSquared(Game.PlayerPed.Position) < 9f).Count() > 0, 1030);

            var damageLocationMenuItem = new MenuItemStandard { Title = "Check Closest for Damage Location", OnActivate = (item) => { CheckDamageLocation(new PedList().Select(p => new Ped(p)).Where(p => p != Game.PlayerPed).OrderBy(p => p.Position.DistanceToSquared(Game.PlayerPed.Position)).First()); } };
            InteractionListMenu.RegisterInteractionMenuItem(damageLocationMenuItem, () => pedList.Select(p => new Ped(p)).Where(p => p != Game.PlayerPed).Where(p => p.Position.DistanceToSquared(Game.PlayerPed.Position) < 9f).Count() > 0, 1031);
        }

        static async Task PeriodicDamageCheck()
        {
            while (true)
            {
                try
                {
                    //if (Function.Call<bool>(Hash.HAS_PLAYER_DAMAGED_AT_LEAST_ONE_PED, Game.Player.Handle))
                    //{
                    //    //Log.ToChat($"{Function.Call<bool>(Hash.HAS_PLAYER_DAMAGED_AT_LEAST_ONE_PED, Game.Player.Handle)} {(NearbyPeds != null ? NearbyPeds.Count : 0)}");
                    //    Function.Call(Hash.CLEAR_PLAYER_HAS_DAMAGED_AT_LEAST_ONE_PED, Game.Player.Handle);
                    //}

                    if (NearbyPeds != null)
                    {
                        NearbyPeds.ForEach(p =>
                        {
                            if (p.Health < p.MaxHealth)
                            {
                                if (!Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, p.Handle) && !Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_VEHICLE, p.Handle)/* && !(Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed && Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, p.Handle, Game.PlayerPed.CurrentVehicle.Handle)))*/)
                                {
                                    //Log.ToChat("Not by me?");
                                    return;
                                }
                                else
                                {
                                    //Log.ToChat("Damaged ped!");
                                    if (Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_VEHICLE, p.Handle))
                                    {
                                        //Log.ToChat("VEHICLE");
                                        IncrementPedFlag(p, "Damage.Vehicle");
                                        if(p.IsDead)
                                        {
                                            Log.ToChat("VEHICLE KILLER");
                                            Function.Call(Hash.DECOR_SET_INT, p.Handle, "Damage.KillerCategory", KillerCategory.Vehicle);
                                        }
                                    }
                                    else
                                    {
                                        //Log.ToChat(":( NO VEHICLE");
                                    }

                                    if (p.IsDead)
                                    {
                                        Entity killer = Entity.FromHandle(Function.Call<int>(Hash.GET_PED_SOURCE_OF_DEATH, p.Handle));
                                        if(killer.Exists())
                                        {
                                            if ((PedHash)killer.Model.Hash == PedHash.MountainLion)
                                            {
                                                Log.ToChat("MOUNTAIN LION KILLER");
                                                Function.Call(Hash.DECOR_SET_INT, p.Handle, "Damage.KillerCategory", KillerCategory.MountainLion);
                                            }
                                        }
                                    }

                                    PedBone LastDamagedBone = p.Bones.LastDamaged;
                                    OutputArgument outBone = new OutputArgument();
                                    Function.Call<bool>(Hash.GET_PED_LAST_DAMAGE_BONE, p.Handle, outBone);
                                    //Log.ToChat($"{(Bone)outBone.GetResult<int>()}");
                                    DamageBitFlags.SetPedBoneFlag(p, (Bone)outBone.GetResult<int>());
                                    Enum.GetValues(typeof(WeaponHash)).OfType<WeaponHash>().ToList().ForEach(w =>
                                    {
                                        if (Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON, p.Handle, (int)w, 0))
                                        {
                                            //Log.ToChat($"{w}");
                                            switch (Function.Call<int>(Hash.GET_WEAPON_DAMAGE_TYPE, (int)w))
                                            {
                                                case 2: // Melee
                                                if (SharpMeleeWeapons.Contains(w))
                                                    {
                                                        IncrementPedFlag(p, "Damage.Melee.Sharp");
                                                    }
                                                    else
                                                    {
                                                        IncrementPedFlag(p, "Damage.Melee.Blunt");
                                                    }
                                                    break;
                                                case 3: // Projectile Weapon
                                                IncrementPedFlag(p, "Damage.Projectile");
                                                    break;
                                                case 13: // Gasoline?
                                                IncrementPedFlag(p, "Damage.Gas");
                                                    break;
                                                default:
                                                    break;
                                            }
                                            DamageBitFlags.SetPedWeaponFlag(p, w);
                                        //Log.ToChat($"{w} Type: {Function.Call<int>(Hash.GET_WEAPON_DAMAGE_TYPE, (int)w)}");
                                    }
                                    });
                                    p.Bones.ClearLastDamaged();
                                    Function.Call(Hash.CLEAR_ENTITY_LAST_DAMAGE_ENTITY, p.Handle);
                                    p.ClearLastWeaponDamage();
                                    //var builder = new StringBuilder();
                                    //builder.AppendLine($"Damaged by weapons: {String.Join(", ", DamageBitFlags.PedGetAllWeaponDamageTypes(p))}");
                                    //builder.AppendLine($"Damaged in bones: {String.Join(", ", DamageBitFlags.PedGetAllDamagedBones(p))}");
                                    //builder.AppendLine($"Hit by vehicle: {Function.Call<int>(Hash.DECOR_GET_INT, p.Handle, "Damage.Vehicle")}");
                                    //builder.AppendLine($"Shot: {Function.Call<int>(Hash.DECOR_GET_INT, p.Handle, "Damage.Projectile")}");
                                    //builder.AppendLine($"Beaten: {Function.Call<int>(Hash.DECOR_GET_INT, p.Handle, "Damage.Melee.Blunt")}");
                                    //builder.AppendLine($"Cut: {Function.Call<int>(Hash.DECOR_GET_INT, p.Handle, "Damage.Melee.Sharp")}");
                                    //builder.AppendLine($"Gas damage: {Function.Call<int>(Hash.DECOR_GET_INT, p.Handle, "Damage.Gas")}");
                                    //Log.ToChat(builder.ToString());
                                }
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"PedDamage PeriodicDamageCheck Error: {ex.Message}");
                }
                await BaseScript.Delay(100);
            }
        }

        static private void IncrementPedFlag(Ped p, string v)
        {
            try
            {
                int value = 0;
                //Log.ToChat($"Incrementing flag '{v}'");
                if (Function.Call<bool>(Hash.DECOR_EXIST_ON, p.Handle, v))
                {
                    //Log.ToChat($"Decor exists!");
                    value = Function.Call<int>(Hash.DECOR_GET_INT, p.Handle, v);
                }
                value++;
                //Log.ToChat($"New value {value}");
                Function.Call(Hash.DECOR_SET_INT, p.Handle, v, value);
            }
            catch (Exception ex)
            {
                Log.Error($"PedDamage IncrementPedFlag Error: {ex.Message}");
            }
        }

        static Dictionary<Bone, string> boneToNameMap = new Dictionary<Bone, string>()
        {
            [Bone.SKEL_ROOT] = "mid-section",
            [Bone.FB_R_Brow_Out_000] = "eyebrows",
            [Bone.SKEL_L_Toe0] = "left foot toes",
            [Bone.MH_R_Elbow] = "elbow",
            // TODO: Make below figures accurate
            [Bone.SKEL_L_Finger01] = "left ring finger",
            [Bone.SKEL_L_Finger02] = "left pinky",
            [Bone.SKEL_L_Finger31] = "left index finger",
            [Bone.SKEL_L_Finger32] = "left middle finger",
            [Bone.SKEL_L_Finger41] = "left thumb",
            [Bone.SKEL_L_Finger42] = "left ring finger",
            [Bone.SKEL_L_Finger11] = "left pinky",
            [Bone.SKEL_L_Finger12] = "left index finger",
            [Bone.SKEL_L_Finger21] = "left middle finger",
            [Bone.SKEL_L_Finger22] = "left thumb",
            [Bone.RB_L_ArmRoll] = "left arm",
            [Bone.IK_R_Hand] = "right hand",
            [Bone.RB_R_ThighRoll] = "right thigh",
            [Bone.SKEL_R_Clavicle] = "right clavicle",
            [Bone.FB_R_Lip_Corner_000] = "lips",
            [Bone.SKEL_Pelvis] = "pelvis",
            [Bone.IK_Head] = "head",
            [Bone.SKEL_L_Foot] = "left foot",
            [Bone.MH_R_Knee] = "right knee",
            [Bone.FB_LowerLipRoot_000] = "lips",
            [Bone.FB_R_Lip_Top_000] = "lips",
            [Bone.SKEL_L_Hand] = "left hand",
            [Bone.FB_R_CheekBone_000] = "right cheek",
            [Bone.FB_UpperLipRoot_000] = "lips",
            [Bone.FB_L_Lip_Top_000] = "lips",
            [Bone.FB_LowerLip_000] = "lips",
            [Bone.SKEL_R_Toe0] = "right foot toes",
            [Bone.FB_L_CheekBone_000] = "left cheek",
            [Bone.MH_L_Elbow] = "elbow",
            [Bone.SKEL_Spine0] = "mid-section",
            [Bone.RB_L_ThighRoll] = "left thigh",
            [Bone.PH_R_Foot] = "right foot",
            [Bone.SKEL_Spine1] = "mid-section",
            [Bone.SKEL_Spine2] = "mid-section",
            [Bone.SKEL_Spine3] = "mid-section",
            [Bone.FB_L_Eye_000] = "left eye",
            [Bone.SKEL_L_Finger00] = "left pinky",
            [Bone.SKEL_L_Finger10] = "left ring finger",
            [Bone.SKEL_L_Finger20] = "left middle finger",
            [Bone.SKEL_L_Finger30] = "left index finger",
            [Bone.SKEL_L_Finger40] = "left thumb",
            [Bone.FB_R_Eye_000] = "right eye",
            [Bone.SKEL_R_Forearm] = "right forearm",
            [Bone.PH_R_Hand] = "right hand",
            [Bone.FB_L_Lip_Corner_000] = "lips",
            [Bone.SKEL_Head] = "head",
            [Bone.IK_R_Foot] = "right foot",
            [Bone.RB_Neck_1] = "neck",
            [Bone.IK_L_Hand] = "left hand",
            [Bone.SKEL_R_Calf] = "right calf",
            [Bone.RB_R_ArmRoll] = "right arm",
            [Bone.FB_Brow_Centre_000] = "eyebrows",
            [Bone.SKEL_Neck_1] = "neck",
            [Bone.SKEL_R_UpperArm] = "upper arm",
            [Bone.FB_R_Lid_Upper_000] = "lips", // Lid == ?
            [Bone.RB_R_ForeArmRoll] = "forearm",
            [Bone.SKEL_L_UpperArm] = "upper arm",
            [Bone.FB_L_Lid_Upper_000] = "lips",
            [Bone.MH_L_Knee] = "left knee",
            [Bone.FB_Jaw_000] = "jaw",
            [Bone.FB_L_Lip_Bot_000] = "lips",
            [Bone.FB_Tongue_000] = "tongue",
            [Bone.FB_R_Lip_Bot_000] = "lips",
            [Bone.SKEL_R_Thigh] = "right thigh",
            [Bone.SKEL_R_Foot] = "right foot",
            [Bone.IK_Root] = "mid-region",
            [Bone.SKEL_R_Hand] = "right hand",
            [Bone.SKEL_Spine_Root] = "mid-region",
            [Bone.PH_L_Foot] = "left foot",
            [Bone.SKEL_L_Thigh] = "left thigh",
            [Bone.FB_L_Brow_Out_000] = "eyebrows",
            [Bone.SKEL_R_Finger00] = "right pinky",
            [Bone.SKEL_R_Finger10] = "right ring finger",
            [Bone.SKEL_R_Finger20] = "right middle finger",
            [Bone.SKEL_R_Finger30] = "right index finger",
            [Bone.SKEL_R_Finger40] = "right thumb",
            [Bone.PH_L_Hand] = "left hand",
            [Bone.RB_L_ForeArmRoll] = "left forearm",
            [Bone.SKEL_L_Forearm] = "left forearm",
            [Bone.FB_UpperLip_000] = "lips",
            [Bone.SKEL_L_Calf] = "left calf",
            [Bone.SKEL_R_Finger01] = "right pinky",
            [Bone.SKEL_R_Finger02] = "right ring finger",
            [Bone.SKEL_R_Finger31] = "right middle finger",
            [Bone.SKEL_R_Finger32] = "right index finger",
            [Bone.SKEL_R_Finger41] = "right thumb",
            [Bone.SKEL_R_Finger42] = "right pinky",
            [Bone.SKEL_R_Finger11] = "right ring finger",
            [Bone.SKEL_R_Finger12] = "right middle finger",
            [Bone.SKEL_R_Finger21] = "right index finger",
            [Bone.SKEL_R_Finger22] = "right thumb",
            [Bone.SKEL_L_Clavicle] = "left clavicle",
            [Bone.FACIAL_facialRoot] = "face",
            [Bone.IK_L_Foot] = "foot"
        };

        static public void CheckDamageLocation(Ped p)
        {
            try
            {
                List<Bone> damagedBones = DamageBitFlags.PedGetAllDamagedBones(p);
                List<string> damagedBonesInHuman = damagedBones.Select(b => boneToNameMap[b]).ToList();
                Dictionary<string, int> aggregateDamagedBones = new Dictionary<string, int>();
                damagedBonesInHuman.ForEach(d =>
                {
                    if (aggregateDamagedBones.ContainsKey(d)) aggregateDamagedBones[d]++;
                    else aggregateDamagedBones[d] = 1;
                });
                List<string> result = new List<string>();
                damagedBonesInHuman.ForEach(d =>
                {
                    if (aggregateDamagedBones[d] > 1)
                    {
                        result.Add($"multiple instances of damage to the {d}");
                    }
                    else
                    {
                        result.Add($"damage to the {d}");
                    }
                });

                if (aggregateDamagedBones.Count() > 0)
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AABB88", $"You see {String.Join(",", result)}.");
                }
                else
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AABB88", $"There is no damage that is clearly visible to you at the moment.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PedDamage CheckDamageLocation Error: {ex.Message}");
            }
        }

        public enum CountedDamageType
        {
            Vehicle,
            Projectile,
            BluntMelee,
            SharpMelee,
            Gas
        }

        static Dictionary<CountedDamageType, string> countedDamageMapToHuman = new Dictionary<CountedDamageType, string>()
        {
            [CountedDamageType.Vehicle] = "getting hit by a vehicle",
            [CountedDamageType.Projectile] = "getting hit by a projectile weapon",
            [CountedDamageType.BluntMelee] = "getting hit by fists or a blunt melee weapon",
            [CountedDamageType.SharpMelee] = "getting hit by a sharp melee weapon",
            [CountedDamageType.Gas] = "being exposed to some chemical, either some gas or gasoline"
        };

        static Dictionary<CountedDamageType, string> countedDamageMapToDecorNames = new Dictionary<CountedDamageType, string>()
        {
            [CountedDamageType.Vehicle] = "Damage.Vehicle",
            [CountedDamageType.Projectile] = "Damage.Projectile",
            [CountedDamageType.BluntMelee] = "Damage.Melee.Blunt",
            [CountedDamageType.SharpMelee] = "Damage.Melee.Sharp",
            [CountedDamageType.Gas] = "Dummy"
        };

        static public string CheckDamageSourceType(Ped p, CountedDamageType damageType)
        {
            try
            {
                int damageInstances = Function.Call<int>(Hash.DECOR_GET_INT, p.Handle, countedDamageMapToDecorNames[damageType]);

                if (damageInstances > 1)
                {
                    return $"You see multiple damage instances that were probably caused by {countedDamageMapToHuman[damageType]}.";
                }
                else if (damageInstances > 0)
                {
                    return $"You see one damage instance that was probably caused by {countedDamageMapToHuman[damageType]}.";
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PedDamage CheckDamageSourceType Error: {ex.Message}");
            }
            return null;
        }

        static public void CheckWeaponSources(Ped p)
        {
            try
            {
                List<WeaponHash> weaponDamageTypes = DamageBitFlags.PedGetAllWeaponDamageTypes(p);
                if (weaponDamageTypes.Count > 0)
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AABB88", $"You see damage that was probably caused by weapons such as: {String.Join(",", weaponDamageTypes.Select(w => w.ToString().AddSpacesToCamelCase()))}.");
                }
                else
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AABB88", $"There is no damage that is clearly visible to you at the moment.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PedDamage CheckWeaponSources Error: {ex.Message}");
            }
        }

        static async Task PeriodicNearbyRefresh()
        {
            while (true)
            {
                try
                {
                    NearbyPeds = PedList.Select(p => new Ped(p)).ToList();
                }
                catch (Exception ex)
                {
                    Log.Error($"PedDamage PeriodicNearbyRefresh Error: {ex.Message}");
                }
                await BaseScript.Delay(4000);
            }
        }
    }

    class DamageBitFlags
    {
        static public Dictionary<WeaponHash, int> WeaponHashToFlag;
        static public Dictionary<Bone, int> BoneIdToFlag;

        static DamageBitFlags()
        {
            List<WeaponHash> weapons = Enum.GetValues(typeof(WeaponHash)).OfType<WeaponHash>().ToList();
            // We need to renumber the weapons and bones to be able to use them in 32-bit Decor int flags
            WeaponHashToFlag = new Dictionary<WeaponHash, int>();
            int i = 0;
            weapons.ForEach(w =>
            {
                WeaponHashToFlag.Add(w, i);
                i++;
            });
            Log.Verbose($"{String.Join(", ", WeaponHashToFlag.Select(w => $"[{w.Value}] {w.Key}"))}");

            int FlagsToRegister = (int)Math.Ceiling(WeaponHashToFlag.Count / 32f) - 1;
            while (FlagsToRegister >= 0)
            {
                Function.Call(Hash.DECOR_REGISTER, $"DamagedByWeaponFlags{FlagsToRegister}", 3);
                //Log.ToChat($"DamagedByWeaponFlags{FlagsToRegister}");
                FlagsToRegister--;
            }

            List<Bone> bones = Enum.GetValues(typeof(Bone)).OfType<Bone>().ToList();
            // We need to renumber the weapons and bones to be able to use them in 32-bit Decor int flags
            BoneIdToFlag = new Dictionary<Bone, int>();
            i = 0;
            bones.ForEach(b =>
            {
                BoneIdToFlag.Add(b, i);
                i++;
            });

            FlagsToRegister = (int)Math.Ceiling(BoneIdToFlag.Count / 32f) - 1;
            while (FlagsToRegister >= 0)
            {
                Function.Call(Hash.DECOR_REGISTER, $"DamagedBoneFlags{FlagsToRegister}", 3);
                //Log.ToChat($"DamagedBoneFlags{FlagsToRegister}");
                FlagsToRegister--;
            }
        }

        public static bool PedHasBoneFlag(Ped ped, Bone bone)
        {
            if (BoneIdToFlag.ContainsKey(bone))
            {
                int WhichFlagInt = (int)Math.Ceiling(BoneIdToFlag[bone] / 32f) - 1;
                if (!Function.Call<bool>(Hash.DECOR_EXIST_ON, ped.Handle, $"DamagedBoneFlags{WhichFlagInt}"))
                {
                    return false;
                }
                else
                {
                    if (((uint)Function.Call<int>(Hash.DECOR_GET_INT, ped.Handle, $"DamagedBoneFlags{WhichFlagInt}") & ((uint)1 << BoneIdToFlag[bone])) != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public static bool PedHasWeaponFlag(Ped ped, WeaponHash weapon)
        {
            if (WeaponHashToFlag.ContainsKey(weapon))
            {
                int WhichFlagInt = (int)Math.Ceiling(WeaponHashToFlag[weapon] / 32f) - 1;
                if (!Function.Call<bool>(Hash.DECOR_EXIST_ON, ped.Handle, $"DamagedByWeaponFlags{WhichFlagInt}"))
                {
                    return false;
                }
                else
                {
                    if (((uint)Function.Call<int>(Hash.DECOR_GET_INT, ped.Handle, $"DamagedByWeaponFlags{WhichFlagInt}") & ((uint)1 << WeaponHashToFlag[weapon])) != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public static void SetPedBoneFlag(Ped ped, Bone bone)
        {
            if (BoneIdToFlag.ContainsKey(bone))
            {
                int WhichFlagInt = (int)Math.Ceiling(BoneIdToFlag[bone] / 32f) - 1;
                uint currentFlag = 0;
                if (!Function.Call<bool>(Hash.DECOR_EXIST_ON, ped.Handle, $"DamagedBoneFlags{WhichFlagInt}"))
                {
                    currentFlag = (uint)Function.Call<int>(Hash.DECOR_GET_INT, ped.Handle, $"DamagedBoneFlags{WhichFlagInt}");
                }

                Function.Call(Hash.DECOR_SET_INT, ped.Handle, $"DamagedBoneFlags{WhichFlagInt}", currentFlag | ((uint)1 << BoneIdToFlag[bone]));
            }
        }

        public static void SetPedWeaponFlag(Ped ped, WeaponHash weapon)
        {
            if (WeaponHashToFlag.ContainsKey(weapon))
            {
                int WhichFlagInt = (int)Math.Ceiling(WeaponHashToFlag[weapon] / 32f) - 1;
                uint currentFlag = 0;
                if (Function.Call<bool>(Hash.DECOR_EXIST_ON, ped.Handle, $"DamagedByWeaponFlags{WhichFlagInt}"))
                {
                    currentFlag = (uint)Function.Call<int>(Hash.DECOR_GET_INT, ped.Handle, $"DamagedByWeaponFlags{WhichFlagInt}");
                    Log.ToChat($"Fetched current weapon flag {currentFlag}");
                }

                Log.ToChat($"Setting flag value {(uint)1 << WeaponHashToFlag[weapon]}");
                Function.Call(Hash.DECOR_SET_INT, ped.Handle, $"DamagedByWeaponFlags{WhichFlagInt}", (int)(currentFlag | ((uint)1 << WeaponHashToFlag[weapon])));
            }
        }

        public static List<Bone> PedGetAllDamagedBones(Ped ped)
        {
            return BoneIdToFlag.Keys.ToList().Where(b => PedHasBoneFlag(ped, b)).ToList();
        }

        public static List<WeaponHash> PedGetAllWeaponDamageTypes(Ped ped)
        {
            return WeaponHashToFlag.Keys.ToList().Where(w => PedHasWeaponFlag(ped, w)).ToList();
        }

        /// <summary>
        /// Upon revive etcetera
        /// </summary>
        /// <param name="ped"></param>
        public static void ClearAllDamageFlags(Ped ped)
        {
            int FlagsToClear = (int)Math.Ceiling(WeaponHashToFlag.Count / 32f) - 1;
            while (FlagsToClear >= 0)
            {
                Function.Call(Hash.DECOR_SET_INT, $"DamagedByWeaponFlags{FlagsToClear}", 0);
                FlagsToClear--;
            }

            FlagsToClear = (int)Math.Ceiling(BoneIdToFlag.Count / 32f) - 1;
            while (FlagsToClear >= 0)
            {
                Function.Call(Hash.DECOR_SET_INT, $"DamagedBoneFlags{FlagsToClear}", 0);
                FlagsToClear--;
            }
            Function.Call(Hash.DECOR_SET_INT, $"Damage.Melee", 0);
            Function.Call(Hash.DECOR_SET_INT, $"Damage.Projectile", 0);
            Function.Call(Hash.DECOR_SET_INT, $"Damage.Gas", 0);
            Function.Call(Hash.DECOR_SET_INT, $"Damage.Vehicle", 0);
            Function.Call(Hash.DECOR_SET_INT, $"Damage.KillerCategory", 0);
        }
    }
}
