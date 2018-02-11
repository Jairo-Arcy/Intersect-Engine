﻿using System.Collections.Generic;
using System.Linq;
using Intersect.Migration.UpgradeInstructions.Upgrade_10.Intersect_Convert_Lib;
using Intersect.Migration.UpgradeInstructions.Upgrade_7.Intersect_Convert_Lib.GameObjects.Conditions;
using Intersect.Migration.UpgradeInstructions.Upgrade_7.Intersect_Convert_Lib.GameObjects.Events;

namespace Intersect.Migration.UpgradeInstructions.Upgrade_7.Intersect_Convert_Lib.GameObjects
{
    public class SpellBase : DatabaseObject
    {
        //Core Info
        public new const string DATABASE_TABLE = "spells";

        public new const GameObject OBJECT_TYPE = GameObject.Spell;
        protected static Dictionary<int, DatabaseObject> sObjects = new Dictionary<int, DatabaseObject>();

        //Animations
        public int CastAnimation = -1;

        //Spell Times
        public int CastDuration;

        //Requirements
        public ConditionLists CastingReqs = new ConditionLists();

        public int CastRange;
        public int CooldownDuration;
        public int Cost;

        //Damage
        public int CritChance;

        public int DamageType = 1;
        public int Data1;
        public int Data2;
        public int Data3;
        public int Data4;
        public string Data5 = "";
        public string Desc = "";
        public int Friendly;
        public int HitAnimation = -1;
        public int HitRadius;

        //OldRequirements
        public int LevelReq;

        public string Name = "New Spell";
        public string Pic = "";

        //Extra Data, Teleport Coords, Custom Spells, Etc
        public int Projectile;

        public int Scaling;
        public int ScalingStat;
        public byte SpellType;

        //Buff/Debuff Data
        public int[] StatDiff = new int[(int) Stats.StatCount];

        public int[] StatReq = new int[(int) Stats.StatCount];

        //Targetting Stuff
        public int TargetType;

        //Costs
        public int[] VitalCost = new int[(int) Vitals.VitalCount];

        //Heal/Damage
        public int[] VitalDiff = new int[(int) Vitals.VitalCount];

        public SpellBase(int id) : base(id)
        {
        }

        public override void Load(byte[] packet)
        {
            var myBuffer = new ByteBuffer();
            myBuffer.WriteBytes(packet);
            Name = myBuffer.ReadString();
            Desc = myBuffer.ReadString();
            SpellType = myBuffer.ReadByte();
            Cost = myBuffer.ReadInteger();
            Pic = myBuffer.ReadString();

            CastDuration = myBuffer.ReadInteger();
            CooldownDuration = myBuffer.ReadInteger();

            CastAnimation = myBuffer.ReadInteger();
            HitAnimation = myBuffer.ReadInteger();

            TargetType = myBuffer.ReadInteger();
            CastRange = myBuffer.ReadInteger();
            HitRadius = myBuffer.ReadInteger();

            for (int i = 0; i < (int) Vitals.VitalCount; i++)
            {
                VitalCost[i] = myBuffer.ReadInteger();
            }

            LevelReq = myBuffer.ReadInteger();
            for (int i = 0; i < (int) Stats.StatCount; i++)
            {
                StatReq[i] = myBuffer.ReadInteger();
            }

            for (int i = 0; i < (int) Vitals.VitalCount; i++)
            {
                VitalDiff[i] = myBuffer.ReadInteger();
            }

            for (int i = 0; i < (int) Stats.StatCount; i++)
            {
                StatDiff[i] = myBuffer.ReadInteger();
            }

            CritChance = myBuffer.ReadInteger();
            DamageType = myBuffer.ReadInteger();
            ScalingStat = myBuffer.ReadInteger();
            Scaling = myBuffer.ReadInteger();
            Friendly = myBuffer.ReadInteger();

            Projectile = myBuffer.ReadInteger();
            Data1 = myBuffer.ReadInteger();
            Data2 = myBuffer.ReadInteger();
            Data3 = myBuffer.ReadInteger();
            Data4 = myBuffer.ReadInteger();
            Data5 = myBuffer.ReadString();

            myBuffer.Dispose();

            var cndList = new ConditionList()
            {
                Name = "Migrated Requirements"
            };
            if (LevelReq > 0)
            {
                var req = new EventCommand
                {
                    Type = EventCommandType.ConditionalBranch,
                    Ints =
                    {
                        [0] = 7,
                        [1] = 1,
                        [2] = LevelReq,
                        [3] = 0
                    }
                };
                //Level or Stat is
                //Greater than or equal to
                //Level To Compare
                //Level not stat
                cndList.Conditions.Add(req);
            }
            for (var i = 0; i < Options.MaxStats; i++)
            {
                if (StatReq[i] > 0)
                {
                    var req = new EventCommand
                    {
                        Type = EventCommandType.ConditionalBranch,
                        Ints =
                        {
                            [0] = 7,
                            [1] = 1,
                            [2] = StatReq[i],
                            [3] = i + 1
                        }
                    };
                    //Level or Stat is
                    //Greater than or equal to
                    //Value To Compare
                    //Stat index
                    cndList.Conditions.Add(req);
                }
            }
            if (cndList.Conditions.Count > 0) CastingReqs.Lists.Add(cndList);
        }

        public byte[] SpellData()
        {
            var myBuffer = new ByteBuffer();
            myBuffer.WriteString(Name);
            myBuffer.WriteString(Desc);
            myBuffer.WriteByte(SpellType);
            myBuffer.WriteInteger(Cost);
            myBuffer.WriteString(Pic);

            myBuffer.WriteInteger(CastDuration);
            myBuffer.WriteInteger(CooldownDuration);

            myBuffer.WriteInteger(CastAnimation);
            myBuffer.WriteInteger(HitAnimation);

            myBuffer.WriteInteger(TargetType);
            myBuffer.WriteInteger(CastRange);
            myBuffer.WriteInteger(HitRadius);

            for (int i = 0; i < (int) Vitals.VitalCount; i++)
            {
                myBuffer.WriteInteger(VitalCost[i]);
            }

            CastingReqs.Save(myBuffer);

            for (int i = 0; i < (int) Vitals.VitalCount; i++)
            {
                myBuffer.WriteInteger(VitalDiff[i]);
            }

            for (int i = 0; i < (int) Stats.StatCount; i++)
            {
                myBuffer.WriteInteger(StatDiff[i]);
            }

            myBuffer.WriteInteger(CritChance);
            myBuffer.WriteInteger(DamageType);
            myBuffer.WriteInteger(ScalingStat);
            myBuffer.WriteInteger(Scaling);
            myBuffer.WriteInteger(Friendly);

            myBuffer.WriteInteger(Projectile);
            myBuffer.WriteInteger(Data1);
            myBuffer.WriteInteger(Data2);
            myBuffer.WriteInteger(Data3);
            myBuffer.WriteInteger(Data4);
            myBuffer.WriteString(Data5);
            return myBuffer.ToArray();
        }

        public static SpellBase GetSpell(int index)
        {
            if (sObjects.ContainsKey(index))
            {
                return (SpellBase) sObjects[index];
            }
            return null;
        }

        public static string GetName(int index)
        {
            if (sObjects.ContainsKey(index))
            {
                return ((SpellBase) sObjects[index]).Name;
            }
            return "Deleted";
        }

        public override byte[] GetData()
        {
            return SpellData();
        }

        public override string GetTable()
        {
            return DATABASE_TABLE;
        }

        public override GameObject GetGameObjectType()
        {
            return OBJECT_TYPE;
        }

        public static DatabaseObject Get(int index)
        {
            if (sObjects.ContainsKey(index))
            {
                return sObjects[index];
            }
            return null;
        }

        public override void Delete()
        {
            sObjects.Remove(GetId());
        }

        public static void ClearObjects()
        {
            sObjects.Clear();
        }

        public static void AddObject(int index, DatabaseObject obj)
        {
            sObjects.Remove(index);
            sObjects.Add(index, obj);
        }

        public static int ObjectCount()
        {
            return sObjects.Count;
        }

        public static Dictionary<int, SpellBase> GetObjects()
        {
            Dictionary<int, SpellBase> objects = sObjects.ToDictionary(k => k.Key, v => (SpellBase) v.Value);
            return objects;
        }
    }
}