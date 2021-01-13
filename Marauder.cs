using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Marauder.Weapon1;
using EntityStates.Marauder.Weapon2;
//using EntityStates.Marauder.Weapon3;
using EntityStates.Marauder.Weapon4;
using RoR2.Projectile;
using EntityStates.Marauder.Weapon22;

namespace Marauder
{
    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin(MODUID, "Marauder", "0.0.1")]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(LanguageAPI), nameof(BuffAPI), nameof(EffectAPI))]

    public class MarauderMod : BaseUnityPlugin
    {
        public const string MODUID = "com.Ruxbieno.Marauder";

        internal static MarauderMod instance;

        public static GameObject myCharacter;
        public static GameObject characterDisplay;
        public static GameObject doppelganger;

        public static GameObject marauderCrosshair;

        private static readonly Color CHAR_COLOR = new Color(0f, 0f, 0f);

        private static ConfigEntry<float> baseHealth;
        private static ConfigEntry<float> healthGrowth;
        private static ConfigEntry<float> baseArmor;
        private static ConfigEntry<float> baseDamage;
        private static ConfigEntry<float> damageGrowth;
        private static ConfigEntry<float> baseRegen;
        private static ConfigEntry<float> regenGrowth;
        private static ConfigEntry<float> baseSpeed;

        public static GameObject Bomb;

        public static BuffIndex comboBuff;

        public static BuffIndex explodeDebuff;

        public void Awake()
        {
            instance = this;

            ReadConfig();
            RegisterBuffs();
            RegisterStates();
            RegisterCharacter();
            Skins.RegisterSkins();
            CreateMaster();
            RegisterProjectiles();
        }

        private void ReadConfig()
        {
            baseHealth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Health"), 170f, new ConfigDescription("Base health", null, Array.Empty<object>()));
            healthGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Health growth"), 37f, new ConfigDescription("Health per level", null, Array.Empty<object>()));
            baseArmor = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Armor"), 25f, new ConfigDescription("Base armor", null, Array.Empty<object>()));
            baseDamage = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Damage"), 12f, new ConfigDescription("Base damage", null, Array.Empty<object>()));
            damageGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Damage growth"), 2.4f, new ConfigDescription("Damage per level", null, Array.Empty<object>()));
            baseRegen = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Regen"), 2f, new ConfigDescription("Base HP regen", null, Array.Empty<object>()));
            regenGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Regen growth"), 1.2f, new ConfigDescription("HP regen per level", null, Array.Empty<object>()));
            baseSpeed = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Speed"), 7f, new ConfigDescription("Base speed", null, Array.Empty<object>()));
        }


        private void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += new On.RoR2.HealthComponent.hook_TakeDamage(this.HealthComponent_TakeDamage);
            On.RoR2.CharacterBody.RecalculateStats += new On.RoR2.CharacterBody.hook_RecalculateStats(this.CharacterBody_RecalculateStats);
        }


        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo di)
        {
            orig.Invoke(self, di);
            bool flag = di.inflictor != null;
            bool flag2 = flag;
            if (flag2)
            {
                bool flag3 = self != null;
                bool flag4 = flag3;
                if (flag4)
                {
                    bool flag5 = self.GetComponent<CharacterBody>() != null;
                    bool flag6 = flag5;
                    if (flag6)
                    {
                        bool flag7 = di.attacker != null;
                        bool flag8 = flag7;
                        if (flag8)
                        {
                            bool flag9 = di.attacker.GetComponent<CharacterBody>() != null;
                            bool flag10 = flag9;
                            if (flag10)
                            {
                                bool flag11 = di.attacker.GetComponent<CharacterBody>().baseNameToken == "MARAUDER_NAME";
                                bool flag12 = flag11;
                                if (flag12)
                                {
                                    bool flag13 = di.damageType.HasFlag(DamageType.Silent);
                                    bool flag14 = flag13;
                                    if (flag14)
                                    {
                                        di.damageType = DamageType.AOE;
                                        bool flag15 = self.GetComponent<CharacterBody>().HasBuff(BuffIndex.ClayGoo) && !self.GetComponent<CharacterBody>().HasBuff(MarauderMod.instance.igniteDebuff);
                                        bool flag16 = flag15;
                                        if (flag16)
                                        {
                                            self.GetComponent<CharacterBody>().AddTimedBuff(MarauderMod.instance.igniteDebuff, 16f);
                                            bool flag17 = self.GetComponent<CharacterBody>().modelLocator;
                                            if (flag17)
                                            {
                                                Transform modelTransform = self.GetComponent<CharacterBody>().modelLocator.modelTransform;
                                                bool flag18 = modelTransform.GetComponent<CharacterModel>();
                                                if (flag18)
                                                {
                                                    TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                                                    temporaryOverlay.duration = 16f;
                                                    temporaryOverlay.animateShaderAlpha = true;
                                                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                                                    temporaryOverlay.destroyComponentOnEnd = true;
                                                    temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matDoppelganger");
                                                    temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
                                                }
                                            }
                                            BlastAttack blastAttack = new BlastAttack
                                            {
                                                attacker = di.inflictor,
                                                inflictor = di.inflictor,
                                                teamIndex = TeamIndex.Player,
                                                baseForce = 0f,
                                                position = self.transform.position,
                                                radius = 12f,
                                                falloffModel = BlastAttack.FalloffModel.None,
                                                crit = di.crit,
                                                baseDamage = di.damage * 0.2f,
                                                procCoefficient = di.procCoefficient
                                            };
                                            blastAttack.damageType |= DamageType.Stun1s;
                                            blastAttack.Fire();
                                            BlastAttack blastAttack2 = new BlastAttack
                                            {
                                                attacker = di.inflictor,
                                                inflictor = di.inflictor,
                                                teamIndex = TeamIndex.Player,
                                                baseForce = 0f,
                                                position = self.transform.position,
                                                radius = 16f,
                                                falloffModel = BlastAttack.FalloffModel.None,
                                                crit = false,
                                                baseDamage = 0f,
                                                procCoefficient = 0f,
                                                damageType = DamageType.BypassOneShotProtection
                                            };
                                            blastAttack2.Fire();
                                            EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/MagmaOrbExplosion"), new EffectData
                                            {
                                                origin = self.transform.position,
                                                scale = 16f
                                            }, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x06000010 RID: 16 RVA: 0x000050C8 File Offset: 0x000032C8
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig.Invoke(self);
            bool flag = self;
            if (flag)
            {
                bool flag2 = self.HasBuff(MarauderMod.comboBuff);
                if (flag2)
                {
                    Reflection.SetPropertyValue<float>(self, "armor", self.armor + 25f);
                    Reflection.SetPropertyValue<float>(self, "moveSpeed", self.moveSpeed * 0.5f);
                }
                bool flag3 = self.HasBuff(MarauderMod.explodeDebuff);
                if (flag3)
                {
                    Reflection.SetPropertyValue<float>(self, "armor", self.armor + 25f);
                    Reflection.SetPropertyValue<float>(self, "moveSpeed", self.moveSpeed * 0.5f);
                }
            }
        }

        private void RegisterBuffs()
        {
            BuffDef buffDef = new BuffDef
            {
                name = "ComboBlock",
                iconPath = "Textures/BuffIcons/texBuffGenericShield",
                buffColor = MarauderMod.CHAR_COLOR,
                canStack = false,
                isDebuff = false,
                eliteIndex = EliteIndex.None
            };
            CustomBuff customBuff = new CustomBuff(buffDef);
            MarauderMod.comboBuff = BuffAPI.Add(customBuff);
        }

        private void RegisterCharacter()
        {
            //create a clone of the grovetender prefab
            myCharacter = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/characterbodies/handBody"), "Prefabs/CharacterBodies/MarauderBody", true);
            //create a display prefab
            characterDisplay = PrefabAPI.InstantiateClone(myCharacter.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "MarauderDisplay", true);

            var component1 = myCharacter.AddComponent<SetStateOnHurt>();
            component1.canBeHitStunned = false;
            component1.canBeStunned = true;
            component1.canBeFrozen = true;

            //add custom menu animation script
            characterDisplay.AddComponent<MenuAnim>();


            CharacterBody charBody = myCharacter.GetComponent<CharacterBody>();
            charBody.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;

            //swap to generic mainstate to fix clunky controls
            myCharacter.GetComponent<EntityStateMachine>().mainStateType = new SerializableEntityStateType(typeof(GenericCharacterMain));

            myCharacter.GetComponentInChildren<Interactor>().maxInteractionDistance = 4f;

            //crosshair stuff
            charBody.SetSpreadBloom(0, false);
            charBody.spreadBloomCurve = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CharacterBody>().spreadBloomCurve;
            charBody.spreadBloomDecayTime = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CharacterBody>().spreadBloomDecayTime;

            charBody.hullClassification = HullClassification.Human;



            characterDisplay.transform.localScale = Vector3.one * 1f;
            characterDisplay.AddComponent<NetworkIdentity>();

            //create the custom crosshair
            marauderCrosshair = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Crosshair/BanditCrosshair"), "MarauderCrosshair", true);
            marauderCrosshair.AddComponent<NetworkIdentity>();

            //networking

            if (myCharacter) PrefabAPI.RegisterNetworkPrefab(myCharacter);
            if (characterDisplay) PrefabAPI.RegisterNetworkPrefab(characterDisplay);
            if (doppelganger) PrefabAPI.RegisterNetworkPrefab(doppelganger);
            if (marauderCrosshair) PrefabAPI.RegisterNetworkPrefab(marauderCrosshair);



            string desc = "The Marauder is a heavyweight pure melee fighter with control over tar and an insatiable drive to fight.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Cleaving Strikes " + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Bone Crusher " + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Tar Tendril " + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Enragement </color>" + Environment.NewLine;

            LanguageAPI.Add("MARAUDER_NAME", "Marauder");
            LanguageAPI.Add("MARAUDER_DESCRIPTION", desc);
            LanguageAPI.Add("MARAUDER_SUBTITLE", "Warmongering Aphelian");
            LanguageAPI.Add("MARAUDER_OUTRO_FLAVOR", "...and so he left, consumed by the influence of power.");


            charBody.name = "MarauderBody";
            charBody.baseNameToken = "MARAUDER_NAME";
            charBody.subtitleNameToken = "MARAUDER_SUBTITLE";
            charBody.crosshairPrefab = marauderCrosshair;

            charBody.baseMaxHealth = baseHealth.Value;
            charBody.levelMaxHealth = healthGrowth.Value;
            charBody.baseRegen = baseRegen.Value;
            charBody.levelRegen = regenGrowth.Value;
            charBody.baseDamage = baseDamage.Value;
            charBody.levelDamage = damageGrowth.Value;
            charBody.baseArmor = baseArmor.Value;
            charBody.baseMoveSpeed = baseSpeed.Value;
            charBody.levelArmor = 0;
            charBody.baseCrit = 1;

            charBody.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CharacterBody>().preferredPodPrefab;


            //create a survivordef for our grovetender
            SurvivorDef survivorDef = new SurvivorDef
            {
                name = "MARAUDER_NAME",
                unlockableName = "",
                descriptionToken = "MARAUDER_DESCRIPTION",
                primaryColor = CHAR_COLOR,
                bodyPrefab = myCharacter,
                displayPrefab = characterDisplay,
                outroFlavorToken = "MARAUDER_OUTRO_FLAVOR"
            };


            SurvivorAPI.AddSurvivor(survivorDef);


            SkillSetup();


            //add it to the body catalog
            BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(myCharacter);
            };
        }

        private void RegisterStates()
        {
            LoadoutAPI.AddSkill(typeof(CleavingStrikes));
            LoadoutAPI.AddSkill(typeof(BoneCrusher1));
            LoadoutAPI.AddSkill(typeof(BoneCrusher2));
            //LoadoutAPI.AddSkill(typeof(TarTendril));
            LoadoutAPI.AddSkill(typeof(Enragement));
        }

        private void SkillSetup()
        {
            foreach (GenericSkill obj in myCharacter.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }
            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }


        private void PassiveSetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();
            LanguageAPI.Add("MARAUDER_PASSIVE_NAME", "Gladiator's Will");
            LanguageAPI.Add("MARAUDER_PASSIVE_DESCRIPTION", "Gain <style=cIsDamage>damage</style> as you lose health.");
            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = "MARAUDER_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "MARAUDER_PASSIVE_DESCRIPTION";
        }

        private void PrimarySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Slash with all your might, dealing <style=cIsDamage>240% damage</style>. The third strike will deal <style=cIsDamage>400% damage</style> and cause an earthquake of tar.";

            LanguageAPI.Add("MARAUDER_PRIMARY_COMBO_NAME", "Cleaving Strikes");
            LanguageAPI.Add("MARAUDER_PRIMARY_COMBO_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(CleavingStrikes));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.skillDescriptionToken = "MARAUDER_PRIMARY_COMBO_DESCRIPTION";
            mySkillDef.skillName = "MARAUDER_PRIMARY_COMBO_NAME";
            mySkillDef.skillNameToken = "MARAUDER_PRIMARY_COMBO_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);
            component.primary = myCharacter.AddComponent<GenericSkill>();
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            skillFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(skillFamily);
            Reflection.SetFieldValue<SkillFamily>(component.primary, "_skillFamily", skillFamily);
            SkillFamily skillFamily2 = component.primary.skillFamily;
            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }


        private void SecondarySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Charge up and release a devastating smash, dealing <style=cIsDamage>350-700% damage</style> and <style=cIsUtility>covering enemies in tar</style>.";

            LanguageAPI.Add("MARAUDER_SECONDARY_CRUSH_NAME", "Bone Crusher");
            LanguageAPI.Add("MARAUDER_SECONDARY_CRUSH_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(BoneCrusher1));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 4f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.skillDescriptionToken = "MARAUDER_SECONDARY_CRUSH_DESCRIPTION";
            mySkillDef.skillName = "MARAUDER_SECONDARY_CRUSH_NAME";
            mySkillDef.skillNameToken = "MARAUDER_SECONDARY_CRUSH_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.secondary = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily2 = component.secondary.skillFamily;

            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }


        private void UtilitySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Reach out and touch all enemies covered in tar, attaching a<style=cIsHealing> leeching</style>tendril to them for 3 seconds.";

            LanguageAPI.Add("MARAUDER_UTILITY_SUCK_NAME", "Tar Tendril");
            LanguageAPI.Add("MARAUDER_UTILITY_SUCK_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Huntress.BlinkState));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseRechargeInterval = 7;
            mySkillDef.baseMaxStock = 1;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.skillDescriptionToken = "MARAUDER_UTILITY_SUCK_DESCRIPTION";
            mySkillDef.skillName = "MARAUDER_UTILITY_SUCK_NAME";
            mySkillDef.skillNameToken = "MARAUDER_UTILITY_SUCK_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.utility = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void SpecialSetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Enter a blood-fueled rage for 4 seconds, increasing <style=cIsUtility>attack speed and passively pulling enemies towards you</style>. Increase the duration of the buff by <style=cIsDamage>attacking enemies</style>.";

            LanguageAPI.Add("MARAUDER_SPECIAL_RAGE_NAME", "Enragement");
            LanguageAPI.Add("MARAUDER_SPECIAL_RAGE_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Enragement));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 10;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.skillDescriptionToken = "MARAUDER_SPECIAL_RAGE_DESCRIPTION";
            mySkillDef.skillName = "MARAUDER_SPECIAL_RAGE_NAME";
            mySkillDef.skillNameToken = "MARAUDER_SPECIAL_RAGE_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.special = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily2 = component.special.skillFamily;

            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void CreateMaster()
        {
            //create the doppelganger, uses commando ai bc i can't be bothered writing my own
            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/LoaderMonsterMaster"), "MarauderMonsterMaster", true);

            MasterCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(doppelganger);
            };

            CharacterMaster component = doppelganger.GetComponent<CharacterMaster>();
            component.bodyPrefab = myCharacter;
        }


        public void RegisterProjectiles()
        {
            MarauderMod.Bomb = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/CommandoGrenadeProjectile"), "prefabs/projectiles/AetxelBomb", true, "C:Aetxel.cs", "RegisterProjectiles", 422);
            MarauderMod.Bomb.GetComponent<ProjectileImpactExplosion>().impactEffect = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFXDroneDeath");
            MarauderMod.Bomb.GetComponent<ProjectileImpactExplosion>().blastProcCoefficient = 1f;
            MarauderMod.Bomb.GetComponent<ProjectileImpactExplosion>().blastDamageCoefficient = 1f;
            MarauderMod.Bomb.GetComponent<ProjectileImpactExplosion>().blastRadius = 25f;
            MarauderMod.Bomb.GetComponent<ProjectileImpactExplosion>().falloffModel = BlastAttack.FalloffModel.Linear;
            MarauderMod.Bomb.GetComponent<SphereCollider>().transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
            MarauderMod.Bomb.AddComponent<NetworkIdentity>();
            PrefabAPI.RegisterNetworkPrefab(MarauderMod.Bomb, "C:Aetxel.cs", "Prefabs/Projectiles/AetxelBomb", 43);
            ProjectileCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(MarauderMod.Bomb);
            };
        }
        public class MenuAnim : MonoBehaviour
        {
            //animates him in character select
            internal void OnEnable()
            {
                bool flag = base.gameObject.transform.parent.gameObject.name == "CharacterPad";
                if (flag)
                {
                    base.StartCoroutine(this.SpawnAnim());
                }
            }

            private IEnumerator SpawnAnim()
            {
                Animator animator = base.GetComponentInChildren<Animator>();
                Transform effectTransform = base.gameObject.transform;

                ChildLocator component = base.gameObject.GetComponentInChildren<ChildLocator>();

                if (component) effectTransform = component.FindChild("Root");

                GameObject.Instantiate<GameObject>(EntityStates.HermitCrab.SpawnState.burrowPrefab, effectTransform.position, Quaternion.identity);


                PlayAnimation("Body", "Spawn", "Spawn.playbackRate", 3, animator);

                yield break;
            }


            private void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration, Animator animator)
            {
                int layerIndex = animator.GetLayerIndex(layerName);
                animator.SetFloat(playbackRateParam, 1f);
                animator.PlayInFixedTime(animationStateName, layerIndex, 0f);
                animator.Update(0f);
                float length = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
                animator.SetFloat(playbackRateParam, length / duration);
            }
        }
    }


}