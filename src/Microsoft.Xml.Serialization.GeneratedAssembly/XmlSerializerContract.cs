using System;
using System.Collections;
using System.Xml.Serialization;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using TinyTween;

namespace Microsoft.Xml.Serialization.GeneratedAssembly
{
	public class XmlSerializerContract : XmlSerializerImplementation
	{
		private Hashtable readMethods;

		private Hashtable writeMethods;

		private Hashtable typedSerializers;

		public override XmlSerializationReader Reader => new XmlSerializationReader1();

		public override XmlSerializationWriter Writer => new XmlSerializationWriter1();

		public override Hashtable ReadMethods
		{
			get
			{
				if (readMethods == null)
				{
					Hashtable hashtable = new Hashtable();
					hashtable["WalkDirection::"] = "Read395_WalkDirection";
					hashtable["ObjectID::"] = "Read396_ObjectID";
					hashtable["BigCraftableID::"] = "Read397_BigCraftableID";
					hashtable["FurnitureID::"] = "Read398_FurnitureID";
					hashtable["MouseCursor::"] = "Read399_MouseCursor";
					hashtable["TapState::"] = "Read400_TapState";
					hashtable["DistanceToTarget::"] = "Read401_DistanceToTarget";
					hashtable["WeaponControl::"] = "Read402_WeaponControl";
					hashtable["tutorialType::"] = "Read403_tutorialType";
					hashtable["TutorialShopLocation::"] = "Read404_TutorialShopLocation";
					hashtable["LocationWeather::"] = "Read405_LocationWeather";
					hashtable["DontLoadDefaultSetting::"] = "Read406_DontLoadDefaultSetting";
					hashtable["WaterTiles+WaterTileData::"] = "Read407_WaterTileData";
					hashtable["TinyTween.TweenState::"] = "Read408_TweenState";
					hashtable["TinyTween.StopBehavior::"] = "Read409_StopBehavior";
					hashtable["TinyTween.FloatTween::"] = "Read410_FloatTween";
					hashtable["TinyTween.Vector2Tween::"] = "Read411_Vector2Tween";
					hashtable["TinyTween.Vector3Tween::"] = "Read412_Vector3Tween";
					hashtable["TinyTween.Vector4Tween::"] = "Read413_Vector4Tween";
					hashtable["TinyTween.ColorTween::"] = "Read414_ColorTween";
					hashtable["TinyTween.QuaternionTween::"] = "Read415_QuaternionTween";
					hashtable["StardewValley.BuildingPainter::"] = "Read416_BuildingPainter";
					hashtable["StardewValley.BuildingPaintColor::"] = "Read417_BuildingPaintColor";
					hashtable["StardewValley.HouseRenovation+AnimationType::"] = "Read418_AnimationType";
					hashtable["StardewValley.IslandGemBird::"] = "Read419_IslandGemBird";
					hashtable["StardewValley.IslandGemBird+GemBirdType::"] = "Read420_GemBirdType";
					hashtable["StardewValley.InstanceStatics::"] = "Read421_InstanceStatics";
					hashtable["StardewValley.InstancedStatic::"] = "Read422_InstancedStatic";
					hashtable["StardewValley.NonInstancedStatic::"] = "Read423_NonInstancedStatic";
					hashtable["StardewValley.LocalMultiplayer::"] = "Read424_LocalMultiplayer";
					hashtable["StardewValley.LocationName::"] = "Read425_LocationName";
					hashtable["StardewValley.Options::"] = "Read426_Options";
					hashtable["StardewValley.Options+ItemStowingModes::"] = "Read427_ItemStowingModes";
					hashtable["StardewValley.Options+GamepadModes::"] = "Read428_GamepadModes";
					hashtable["StardewValley.MapSeat::"] = "Read429_MapSeat";
					hashtable["StardewValley.SpecialOrder::"] = "Read430_SpecialOrder";
					hashtable["StardewValley.SpecialOrder+QuestState::"] = "Read431_QuestState";
					hashtable["StardewValley.SpecialOrder+QuestDuration::"] = "Read432_QuestDuration";
					hashtable["StardewValley.OrderObjective::"] = "Read433_OrderObjective";
					hashtable["StardewValley.CollectObjective::"] = "Read434_CollectObjective";
					hashtable["StardewValley.DonateObjective::"] = "Read435_DonateObjective";
					hashtable["StardewValley.ShipObjective::"] = "Read436_ShipObjective";
					hashtable["StardewValley.SlayObjective::"] = "Read437_SlayObjective";
					hashtable["StardewValley.FishObjective::"] = "Read438_FishObjective";
					hashtable["StardewValley.DeliverObjective::"] = "Read439_DeliverObjective";
					hashtable["StardewValley.GiftObjective::"] = "Read440_GiftObjective";
					hashtable["StardewValley.GiftObjective+LikeLevels::"] = "Read441_LikeLevels";
					hashtable["StardewValley.ReachMineFloorObjective::"] = "Read442_ReachMineFloorObjective";
					hashtable["StardewValley.JKScoreObjective::"] = "Read443_JKScoreObjective";
					hashtable["StardewValley.OrderReward::"] = "Read444_OrderReward";
					hashtable["StardewValley.MailReward::"] = "Read445_MailReward";
					hashtable["StardewValley.ResetEventReward::"] = "Read446_ResetEventReward";
					hashtable["StardewValley.GemsReward::"] = "Read447_GemsReward";
					hashtable["StardewValley.MoneyReward::"] = "Read448_MoneyReward";
					hashtable["StardewValley.FriendshipReward::"] = "Read449_FriendshipReward";
					hashtable["StardewValley.BaseEnchantment::"] = "Read450_BaseEnchantment";
					hashtable["StardewValley.BaseWeaponEnchantment::"] = "Read451_BaseWeaponEnchantment";
					hashtable["StardewValley.MagicEnchantment::"] = "Read452_MagicEnchantment";
					hashtable["StardewValley.AmethystEnchantment::"] = "Read453_AmethystEnchantment";
					hashtable["StardewValley.TopazEnchantment::"] = "Read454_TopazEnchantment";
					hashtable["StardewValley.AquamarineEnchantment::"] = "Read455_AquamarineEnchantment";
					hashtable["StardewValley.JadeEnchantment::"] = "Read456_JadeEnchantment";
					hashtable["StardewValley.DiamondEnchantment::"] = "Read457_DiamondEnchantment";
					hashtable["StardewValley.GalaxySoulEnchantment::"] = "Read458_GalaxySoulEnchantment";
					hashtable["StardewValley.RubyEnchantment::"] = "Read459_RubyEnchantment";
					hashtable["StardewValley.EmeraldEnchantment::"] = "Read460_EmeraldEnchantment";
					hashtable["StardewValley.ArtfulEnchantment::"] = "Read461_ArtfulEnchantment";
					hashtable["StardewValley.HaymakerEnchantment::"] = "Read462_HaymakerEnchantment";
					hashtable["StardewValley.BugKillerEnchantment::"] = "Read463_BugKillerEnchantment";
					hashtable["StardewValley.VampiricEnchantment::"] = "Read464_VampiricEnchantment";
					hashtable["StardewValley.CrusaderEnchantment::"] = "Read465_CrusaderEnchantment";
					hashtable["StardewValley.PickaxeEnchantment::"] = "Read466_PickaxeEnchantment";
					hashtable["StardewValley.HoeEnchantment::"] = "Read467_HoeEnchantment";
					hashtable["StardewValley.AxeEnchantment::"] = "Read468_AxeEnchantment";
					hashtable["StardewValley.FishingRodEnchantment::"] = "Read469_FishingRodEnchantment";
					hashtable["StardewValley.WateringCanEnchantment::"] = "Read470_WateringCanEnchantment";
					hashtable["StardewValley.PanEnchantment::"] = "Read471_PanEnchantment";
					hashtable["StardewValley.MilkPailEnchantment::"] = "Read472_MilkPailEnchantment";
					hashtable["StardewValley.ShearsEnchantment::"] = "Read473_ShearsEnchantment";
					hashtable["StardewValley.PowerfulEnchantment::"] = "Read474_PowerfulEnchantment";
					hashtable["StardewValley.EfficientToolEnchantment::"] = "Read475_EfficientToolEnchantment";
					hashtable["StardewValley.SwiftToolEnchantment::"] = "Read476_SwiftToolEnchantment";
					hashtable["StardewValley.ReachingToolEnchantment::"] = "Read477_ReachingToolEnchantment";
					hashtable["StardewValley.BottomlessEnchantment::"] = "Read478_BottomlessEnchantment";
					hashtable["StardewValley.ShavingEnchantment::"] = "Read479_ShavingEnchantment";
					hashtable["StardewValley.ArchaeologistEnchantment::"] = "Read480_ArchaeologistEnchantment";
					hashtable["StardewValley.GenerousEnchantment::"] = "Read481_GenerousEnchantment";
					hashtable["StardewValley.MasterEnchantment::"] = "Read482_MasterEnchantment";
					hashtable["StardewValley.AutoHookEnchantment::"] = "Read483_AutoHookEnchantment";
					hashtable["StardewValley.PreservingEnchantment::"] = "Read484_PreservingEnchantment";
					hashtable["StardewValley.StackDrawType::"] = "Read485_StackDrawType";
					hashtable["StardewValley.MovieInvitation::"] = "Read486_MovieInvitation";
					hashtable["StardewValley.StartMovieEvent::"] = "Read487_StartMovieEvent";
					hashtable["StardewValley.MovieViewerLockEvent::"] = "Read488_MovieViewerLockEvent";
					hashtable["StardewValley.WorldDate::"] = "Read489_WorldDate";
					hashtable["StardewValley.FriendshipStatus::"] = "Read490_FriendshipStatus";
					hashtable["StardewValley.Friendship::"] = "Read491_Friendship";
					hashtable["StardewValley.LocalizedContentManager+LanguageCode::"] = "Read492_LanguageCode";
					hashtable["StardewValley.AnimalHouse::"] = "Read493_AnimalHouse";
					hashtable["StardewValley.BuildingUpgrade::"] = "Read494_BuildingUpgrade";
					hashtable["StardewValley.Character::"] = "Read495_Character";
					hashtable["StardewValley.Chunk::"] = "Read496_Chunk";
					hashtable["StardewValley.FarmerRenderer::"] = "Read497_FarmerRenderer";
					hashtable["StardewValley.Shed::"] = "Read498_Shed";
					hashtable["StardewValley.SlimeHutch::"] = "Read499_SlimeHutch";
					hashtable["StardewValley.Farm::"] = "Read500_Farm";
					hashtable["StardewValley.Farm+LightningStrikeEvent::"] = "Read501_LightningStrikeEvent";
					hashtable["StardewValley.FarmAnimal::"] = "Read502_FarmAnimal";
					hashtable["StardewValley.NetLogger::"] = "Read503_NetLogger";
					hashtable["StardewValley.Noise::"] = "Read504_Noise";
					hashtable["StardewValley.NumberSprite::"] = "Read505_NumberSprite";
					hashtable["StardewValley.Fence::"] = "Read506_Fence";
					hashtable["StardewValley.Item::"] = "Read507_Item";
					hashtable["StardewValley.ModDataDictionary::"] = "Read508_Item";
					hashtable["StardewValley.LightSource::"] = "Read509_LightSource";
					hashtable["StardewValley.LightSource+LightContext::"] = "Read510_LightContext";
					hashtable["StardewValley.Torch::"] = "Read511_Torch";
					hashtable["StardewValley.InputButton::"] = "Read512_InputButton";
					hashtable["StardewValley.ServerPrivacy::"] = "Read513_ServerPrivacy";
					hashtable["StardewValley.NameSelect::"] = "Read514_NameSelect";
					hashtable["StardewValley.PriorityQueue::"] = "Read515_PriorityQueue";
					hashtable["StardewValley.Crop::"] = "Read516_Crop";
					hashtable["StardewValley.Farmer::"] = "Read517_Farmer";
					hashtable["StardewValley.NetIntIntArrayDictionary::"] = "Read518_Item";
					hashtable["StardewValley.HairStyleMetadata::"] = "Read519_HairStyleMetadata";
					hashtable["StardewValley.FarmerPair::"] = "Read520_FarmerPair";
					hashtable["StardewValley.NutDropRequest::"] = "Read521_NutDropRequest";
					hashtable["StardewValley.FarmerTeam+RemoteBuildingPermissions::"] = "Read522_RemoteBuildingPermissions";
					hashtable["StardewValley.FarmerTeam+SleepAnnounceModes::"] = "Read523_SleepAnnounceModes";
					hashtable["StardewValley.InteriorDoor::"] = "Read524_ArrayOfBoolean";
					hashtable["StardewValley.InteriorDoorDictionary::"] = "Read525_Item";
					hashtable["StardewValley.GameLocation::"] = "Read526_GameLocation";
					hashtable["StardewValley.GameLocation+LocationContext::"] = "Read527_LocationContext";
					hashtable["StardewValley.NPC::"] = "Read528_NPC";
					hashtable["StardewValley.MarriageDialogueReference::"] = "Read529_MarriageDialogueReference";
					hashtable["StardewValley.FarmActivity::"] = "Read530_FarmActivity";
					hashtable["StardewValley.ArtifactSpotWatchActivity::"] = "Read531_ArtifactSpotWatchActivity";
					hashtable["StardewValley.CropWatchActivity::"] = "Read532_CropWatchActivity";
					hashtable["StardewValley.FlowerWatchActivity::"] = "Read533_FlowerWatchActivity";
					hashtable["StardewValley.ShrineActivity::"] = "Read534_ShrineActivity";
					hashtable["StardewValley.MailActivity::"] = "Read535_MailActivity";
					hashtable["StardewValley.TreeActivity::"] = "Read536_TreeActivity";
					hashtable["StardewValley.ClearingActivity::"] = "Read537_ClearingActivity";
					hashtable["StardewValley.Object::"] = "Read538_Object";
					hashtable["StardewValley.Object+PreserveType::"] = "Read539_PreserveType";
					hashtable["StardewValley.Object+HoneyType::"] = "Read540_HoneyType";
					hashtable["StardewValley.RainDrop::"] = "Read541_RainDrop";
					hashtable["StardewValley.MineChestType::"] = "Read542_MineChestType";
					hashtable["StardewValley.Vector2Reader::"] = "Read543_Vector2Reader";
					hashtable["StardewValley.Vector2Writer::"] = "Read544_Vector2Writer";
					hashtable["StardewValley.Vector2Serializer::"] = "Read545_Vector2Serializer";
					hashtable["StardewValley.SaveGame::"] = "Read546_SaveGame";
					hashtable["StardewValley.SaveGame+SaveFixes::"] = "Read547_SaveFixes";
					hashtable["StardewValley.ChangeType::"] = "Read548_ChangeType";
					hashtable["StardewValley.StartupPreferences::"] = "Read549_StartupPreferences";
					hashtable["StardewValley.Stats::"] = "Read550_Stats";
					hashtable["StardewValley.Tool::"] = "Read551_Tool";
					hashtable["StardewValley.Warp::"] = "Read552_Warp";
					hashtable["StardewValley.WeatherDebris::"] = "Read553_WeatherDebris";
					hashtable["StardewValley.TerrainFeatures.Bush::"] = "Read554_Bush";
					hashtable["StardewValley.TerrainFeatures.CosmeticPlant::"] = "Read555_CosmeticPlant";
					hashtable["StardewValley.TerrainFeatures.Flooring::"] = "Read556_Flooring";
					hashtable["StardewValley.TerrainFeatures.FruitTree::"] = "Read557_FruitTree";
					hashtable["StardewValley.TerrainFeatures.GiantCrop::"] = "Read558_GiantCrop";
					hashtable["StardewValley.TerrainFeatures.HoeDirt::"] = "Read559_HoeDirt";
					hashtable["StardewValley.TerrainFeatures.LargeTerrainFeature::"] = "Read560_LargeTerrainFeature";
					hashtable["StardewValley.TerrainFeatures.ResourceClump::"] = "Read561_ResourceClump";
					hashtable["StardewValley.TerrainFeatures.Grass::"] = "Read562_Grass";
					hashtable["StardewValley.TerrainFeatures.Quartz::"] = "Read563_Quartz";
					hashtable["StardewValley.TerrainFeatures.TerrainFeature::"] = "Read564_TerrainFeature";
					hashtable["StardewValley.TerrainFeatures.Tree::"] = "Read565_Tree";
					hashtable["StardewValley.Projectiles.BasicProjectile::"] = "Read566_BasicProjectile";
					hashtable["StardewValley.Projectiles.DebuffingProjectile::"] = "Read567_DebuffingProjectile";
					hashtable["StardewValley.Projectiles.Projectile::"] = "Read568_Projectile";
					hashtable["StardewValley.Tools.GenericTool::"] = "Read569_GenericTool";
					hashtable["StardewValley.Tools.Axe::"] = "Read570_Axe";
					hashtable["StardewValley.Tools.Blueprints::"] = "Read571_Blueprints";
					hashtable["StardewValley.Tools.Pan::"] = "Read572_Pan";
					hashtable["StardewValley.Tools.FishingRod::"] = "Read573_FishingRod";
					hashtable["StardewValley.Tools.Lantern::"] = "Read574_Lantern";
					hashtable["StardewValley.Tools.MagnifyingGlass::"] = "Read575_MagnifyingGlass";
					hashtable["StardewValley.Tools.MeleeWeapon::"] = "Read576_MeleeWeapon";
					hashtable["StardewValley.Tools.MilkPail::"] = "Read577_MilkPail";
					hashtable["StardewValley.Tools.Pickaxe::"] = "Read578_Pickaxe";
					hashtable["StardewValley.Tools.Hoe::"] = "Read579_Hoe";
					hashtable["StardewValley.Tools.Raft::"] = "Read580_Raft";
					hashtable["StardewValley.Tools.Seeds::"] = "Read581_Seeds";
					hashtable["StardewValley.Tools.Shears::"] = "Read582_Shears";
					hashtable["StardewValley.Tools.Slingshot::"] = "Read583_Slingshot";
					hashtable["StardewValley.Tools.Stackable::"] = "Read584_Stackable";
					hashtable["StardewValley.Tools.Sword::"] = "Read585_Sword";
					hashtable["StardewValley.Tools.ToolDescription::"] = "Read586_ToolDescription";
					hashtable["StardewValley.Tools.ToolFactory::"] = "Read587_ToolFactory";
					hashtable["StardewValley.Tools.Wand::"] = "Read588_Wand";
					hashtable["StardewValley.Tools.WateringCan::"] = "Read589_WateringCan";
					hashtable["StardewValley.Util.BoundingBoxGroup::"] = "Read590_BoundingBoxGroup";
					hashtable["StardewValley.Util.ToolSpamInputSimulator::"] = "Read591_ToolSpamInputSimulator";
					hashtable["StardewValley.Util.LeftRightClickSpamInputSimulator::"] = "Read592_Item";
					hashtable["StardewValley.Util.SynchronizedShopStock::"] = "Read593_SynchronizedShopStock";
					hashtable["StardewValley.Util.SynchronizedShopStock+SynchedShop::"] = "Read594_SynchedShop";
					hashtable["StardewValley.Quests.SecretLostItemQuest::"] = "Read595_SecretLostItemQuest";
					hashtable["StardewValley.Quests.NetDescriptionElementRef::"] = "Read596_ArrayOfDescriptionElement";
					hashtable["StardewValley.Quests.NetDescriptionElementList::"] = "Read597_ArrayOfDescriptionElement";
					hashtable["StardewValley.Quests.DescriptionElement::"] = "Read598_DescriptionElement";
					hashtable["StardewValley.Quests.LostItemQuest::"] = "Read599_LostItemQuest";
					hashtable["StardewValley.Quests.ItemHarvestQuest::"] = "Read600_ItemHarvestQuest";
					hashtable["StardewValley.Quests.GoSomewhereQuest::"] = "Read601_GoSomewhereQuest";
					hashtable["StardewValley.Quests.CraftingQuest::"] = "Read602_CraftingQuest";
					hashtable["StardewValley.Quests.SocializeQuest::"] = "Read603_SocializeQuest";
					hashtable["StardewValley.Quests.FishingQuest::"] = "Read604_FishingQuest";
					hashtable["StardewValley.Quests.SlayMonsterQuest::"] = "Read605_SlayMonsterQuest";
					hashtable["StardewValley.Quests.ResourceCollectionQuest::"] = "Read606_ResourceCollectionQuest";
					hashtable["StardewValley.Quests.ItemDeliveryQuest::"] = "Read607_ItemDeliveryQuest";
					hashtable["StardewValley.Quests.Quest::"] = "Read608_Quest";
					hashtable["StardewValley.Objects.BedFurniture::"] = "Read609_BedFurniture";
					hashtable["StardewValley.Objects.BedFurniture+BedType::"] = "Read610_BedType";
					hashtable["StardewValley.Objects.FishTankFurniture::"] = "Read611_FishTankFurniture";
					hashtable["StardewValley.Objects.FishTankFurniture+FishTankCategories::"] = "Read612_FishTankCategories";
					hashtable["StardewValley.Objects.TankFish+FishType::"] = "Read613_FishType";
					hashtable["StardewValley.Objects.ItemPedestal::"] = "Read614_ItemPedestal";
					hashtable["StardewValley.Objects.Phone::"] = "Read615_Phone";
					hashtable["StardewValley.Objects.Phone+PhoneCalls::"] = "Read616_PhoneCalls";
					hashtable["StardewValley.Objects.StorageFurniture::"] = "Read617_StorageFurniture";
					hashtable["StardewValley.Objects.WoodChipper::"] = "Read618_WoodChipper";
					hashtable["StardewValley.Objects.Clothing::"] = "Read619_Clothing";
					hashtable["StardewValley.Objects.Clothing+ClothesType::"] = "Read620_ClothesType";
					hashtable["StardewValley.Objects.IndoorPot::"] = "Read621_IndoorPot";
					hashtable["StardewValley.Objects.MiniJukebox::"] = "Read622_MiniJukebox";
					hashtable["StardewValley.Objects.Sign::"] = "Read623_Sign";
					hashtable["StardewValley.Objects.Workbench::"] = "Read624_Workbench";
					hashtable["StardewValley.Objects.Boots::"] = "Read625_Boots";
					hashtable["StardewValley.Objects.BreakableContainer::"] = "Read626_BreakableContainer";
					hashtable["StardewValley.Objects.Cask::"] = "Read627_Cask";
					hashtable["StardewValley.Objects.Chest::"] = "Read628_Chest";
					hashtable["StardewValley.Objects.Chest+SpecialChestTypes::"] = "Read629_SpecialChestTypes";
					hashtable["StardewValley.Objects.ColoredObject::"] = "Read630_ColoredObject";
					hashtable["StardewValley.Objects.SpecialItem::"] = "Read631_SpecialItem";
					hashtable["StardewValley.Objects.TV::"] = "Read632_TV";
					hashtable["StardewValley.Objects.Wallpaper::"] = "Read633_Wallpaper";
					hashtable["StardewValley.Objects.Furniture::"] = "Read634_Furniture";
					hashtable["StardewValley.Objects.CrabPot::"] = "Read635_CrabPot";
					hashtable["StardewValley.Objects.Hat::"] = "Read636_Hat";
					hashtable["StardewValley.Objects.Hat+HairDrawType::"] = "Read637_HairDrawType";
					hashtable["StardewValley.Objects.ItemDescription::"] = "Read638_ItemDescription";
					hashtable["StardewValley.Objects.ObjectFactory::"] = "Read639_ObjectFactory";
					hashtable["StardewValley.Objects.Ring::"] = "Read640_Ring";
					hashtable["StardewValley.Objects.CombinedRing::"] = "Read641_CombinedRing";
					hashtable["StardewValley.Objects.SwitchFloor::"] = "Read642_SwitchFloor";
					hashtable["StardewValley.Network.IncomingMessage::"] = "Read643_IncomingMessage";
					hashtable["StardewValley.Network.NetAudio+SoundContext::"] = "Read644_SoundContext";
					hashtable["StardewValley.Network.NetDirection::"] = "Read645_ArrayOfInt";
					hashtable["StardewValley.Network.NetLocationRef::"] = "Read646_NetLocationRef";
					hashtable["StardewValley.Network.NetPosition::"] = "Read647_NetPosition";
					hashtable["StardewValley.Network.NetBundles::"] = "Read648_Item";
					hashtable["StardewValley.Network.NetFarmerCollection::"] = "Read649_ArrayOfFarmer";
					hashtable["StardewValley.Network.NetMutex::"] = "Read650_NetMutex";
					hashtable["StardewValley.Network.NetWitnessedLock::"] = "Read651_NetWitnessedLock";
					hashtable["StardewValley.Network.NetFarmerRoot::"] = "Read652_ArrayOfFarmer";
					hashtable["StardewValley.Network.NetNPCRef::"] = "Read653_NetNPCRef";
					hashtable["StardewValley.Network.NetCharacterRef::"] = "Read654_NetCharacterRef";
					hashtable["StardewValley.Network.NetDancePartner::"] = "Read655_NetDancePartner";
					hashtable["StardewValley.Network.OverlaidDictionary::"] = "Read656_Item";
					hashtable["StardewValley.Monsters.AngryRoger::"] = "Read657_AngryRoger";
					hashtable["StardewValley.Monsters.HotHead::"] = "Read658_HotHead";
					hashtable["StardewValley.Monsters.LavaLurk::"] = "Read659_LavaLurk";
					hashtable["StardewValley.Monsters.LavaLurk+State::"] = "Read660_LavaLurkState";
					hashtable["StardewValley.Monsters.Leaper::"] = "Read661_Leaper";
					hashtable["StardewValley.Monsters.Shooter::"] = "Read662_Shooter";
					hashtable["StardewValley.Monsters.Spiker::"] = "Read663_Spiker";
					hashtable["StardewValley.Monsters.DwarvishSentry::"] = "Read664_DwarvishSentry";
					hashtable["StardewValley.Monsters.BlueSquid::"] = "Read665_BlueSquid";
					hashtable["StardewValley.Monsters.DinoMonster::"] = "Read666_DinoMonster";
					hashtable["StardewValley.Monsters.DinoMonster+AttackState::"] = "Read667_AttackState";
					hashtable["StardewValley.Monsters.Bug::"] = "Read668_Bug";
					hashtable["StardewValley.Monsters.BigSlime::"] = "Read669_BigSlime";
					hashtable["StardewValley.Monsters.Mummy::"] = "Read670_Mummy";
					hashtable["StardewValley.Monsters.Serpent::"] = "Read671_Serpent";
					hashtable["StardewValley.Monsters.MetalHead::"] = "Read672_MetalHead";
					hashtable["StardewValley.Monsters.ShadowBrute::"] = "Read673_ShadowBrute";
					hashtable["StardewValley.Monsters.ShadowShaman::"] = "Read674_ShadowShaman";
					hashtable["StardewValley.Monsters.Skeleton::"] = "Read675_Skeleton";
					hashtable["StardewValley.Monsters.DustSpirit::"] = "Read676_DustSpirit";
					hashtable["StardewValley.Monsters.Bat::"] = "Read677_Bat";
					hashtable["StardewValley.Monsters.Fly::"] = "Read678_Fly";
					hashtable["StardewValley.Monsters.Grub::"] = "Read679_Grub";
					hashtable["StardewValley.Monsters.RockGolem::"] = "Read680_RockGolem";
					hashtable["StardewValley.Monsters.ShadowGirl::"] = "Read681_ShadowGirl";
					hashtable["StardewValley.Monsters.SquidKid::"] = "Read682_SquidKid";
					hashtable["StardewValley.Monsters.LavaCrab::"] = "Read683_LavaCrab";
					hashtable["StardewValley.Monsters.RockCrab::"] = "Read684_RockCrab";
					hashtable["StardewValley.Monsters.Duggy::"] = "Read685_Duggy";
					hashtable["StardewValley.Monsters.Ghost::"] = "Read686_Ghost";
					hashtable["StardewValley.Monsters.Ghost+GhostVariant::"] = "Read687_GhostVariant";
					hashtable["StardewValley.Monsters.ShadowGuy::"] = "Read688_ShadowGuy";
					hashtable["StardewValley.Monsters.GreenSlime::"] = "Read689_GreenSlime";
					hashtable["StardewValley.Monsters.Monster::"] = "Read690_Monster";
					hashtable["StardewValley.Minigames.NetLeaderboards::"] = "Read691_NetLeaderboards";
					hashtable["StardewValley.Minigames.NetLeaderboardsEntry::"] = "Read692_NetLeaderboardsEntry";
					hashtable["StardewValley.Minigames.AbigailGame+GameKeys::"] = "Read693_GameKeys";
					hashtable["StardewValley.Minigames.AbigailGame+JOTPKProgress::"] = "Read694_JOTPKProgress";
					hashtable["StardewValley.Minigames.AbigailGame+Dracula::"] = "Read695_Dracula";
					hashtable["StardewValley.Locations.AbandonedJojaMart::"] = "Read696_AbandonedJojaMart";
					hashtable["StardewValley.Locations.BoatTunnel::"] = "Read697_BoatTunnel";
					hashtable["StardewValley.Locations.BoatTunnel+TunnelAnimationState::"] = "Read698_TunnelAnimationState";
					hashtable["StardewValley.Locations.BugLand::"] = "Read699_BugLand";
					hashtable["StardewValley.Locations.Caldera::"] = "Read700_Caldera";
					hashtable["StardewValley.Locations.FishShop::"] = "Read701_FishShop";
					hashtable["StardewValley.Locations.IslandEast::"] = "Read702_IslandEast";
					hashtable["StardewValley.Locations.IslandFarmCave::"] = "Read703_IslandFarmCave";
					hashtable["StardewValley.Locations.IslandFarmHouse::"] = "Read704_IslandFarmHouse";
					hashtable["StardewValley.Locations.IslandFieldOffice::"] = "Read705_IslandFieldOffice";
					hashtable["StardewValley.Locations.IslandForestLocation::"] = "Read706_IslandForestLocation";
					hashtable["StardewValley.Locations.IslandHut::"] = "Read707_IslandHut";
					hashtable["StardewValley.Locations.IslandLocation::"] = "Read708_IslandLocation";
					hashtable["StardewValley.Locations.IslandNorth::"] = "Read709_IslandNorth";
					hashtable["StardewValley.Locations.IslandSecret::"] = "Read710_IslandSecret";
					hashtable["StardewValley.Locations.IslandShrine::"] = "Read711_IslandShrine";
					hashtable["StardewValley.Locations.IslandSouth::"] = "Read712_IslandSouth";
					hashtable["StardewValley.Locations.IslandSouthEast::"] = "Read713_IslandSouthEast";
					hashtable["StardewValley.Locations.IslandSouthEastCave::"] = "Read714_IslandSouthEastCave";
					hashtable["StardewValley.Locations.IslandWest::"] = "Read715_IslandWest";
					hashtable["StardewValley.Locations.IslandWestCave1::"] = "Read716_IslandWestCave1";
					hashtable["StardewValley.Locations.IslandWestCave1+CaveCrystal::"] = "Read717_CaveCrystal";
					hashtable["StardewValley.Locations.Mine::"] = "Read718_Mine";
					hashtable["StardewValley.Locations.ShopLocation::"] = "Read719_ShopLocation";
					hashtable["StardewValley.Locations.VolcanoDungeon+TileNeighbors::"] = "Read720_TileNeighbors";
					hashtable["StardewValley.Locations.BeachNightMarket::"] = "Read721_BeachNightMarket";
					hashtable["StardewValley.Locations.ManorHouse::"] = "Read722_ManorHouse";
					hashtable["StardewValley.Locations.MermaidHouse::"] = "Read723_MermaidHouse";
					hashtable["StardewValley.Locations.MovieTheater::"] = "Read724_MovieTheater";
					hashtable["StardewValley.Locations.MovieTheater+MovieStates::"] = "Read725_MovieStates";
					hashtable["StardewValley.Locations.Submarine::"] = "Read726_Submarine";
					hashtable["StardewValley.Locations.AdventureGuild::"] = "Read727_AdventureGuild";
					hashtable["StardewValley.Locations.Bus::"] = "Read728_Bus";
					hashtable["StardewValley.Locations.BuildableGameLocation::"] = "Read729_BuildableGameLocation";
					hashtable["StardewValley.Locations.BathHousePool::"] = "Read730_BathHousePool";
					hashtable["StardewValley.Locations.Club::"] = "Read731_Club";
					hashtable["StardewValley.Locations.Cellar::"] = "Read732_Cellar";
					hashtable["StardewValley.Locations.DecorationFacade::"] = "Read733_ArrayOfInt";
					hashtable["StardewValley.Locations.DecoratableLocation::"] = "Read734_DecoratableLocation";
					hashtable["StardewValley.Locations.Cabin::"] = "Read735_Cabin";
					hashtable["StardewValley.Locations.FarmCave::"] = "Read736_FarmCave";
					hashtable["StardewValley.Locations.WizardHouse::"] = "Read737_WizardHouse";
					hashtable["StardewValley.Locations.Sewer::"] = "Read738_Sewer";
					hashtable["StardewValley.Locations.CommunityCenter::"] = "Read739_CommunityCenter";
					hashtable["StardewValley.Locations.BusStop::"] = "Read740_BusStop";
					hashtable["StardewValley.Locations.MineShaft::"] = "Read741_MineShaft";
					hashtable["StardewValley.Locations.MineInfo::"] = "Read742_MineInfo";
					hashtable["StardewValley.Locations.FarmHouse::"] = "Read743_FarmHouse";
					hashtable["StardewValley.Locations.JojaMart::"] = "Read744_JojaMart";
					hashtable["StardewValley.Locations.Desert::"] = "Read745_Desert";
					hashtable["StardewValley.Locations.Town::"] = "Read746_Town";
					hashtable["StardewValley.Locations.Mountain::"] = "Read747_Mountain";
					hashtable["StardewValley.Locations.Forest::"] = "Read748_Forest";
					hashtable["StardewValley.Locations.LibraryMuseum::"] = "Read749_LibraryMuseum";
					hashtable["StardewValley.Locations.Railroad::"] = "Read750_Railroad";
					hashtable["StardewValley.Locations.SeedShop::"] = "Read751_SeedShop";
					hashtable["StardewValley.Locations.Summit::"] = "Read752_Summit";
					hashtable["StardewValley.Locations.Woods::"] = "Read753_Woods";
					hashtable["StardewValley.Locations.Beach::"] = "Read754_Beach";
					hashtable["StardewValley.Characters.TrashBear::"] = "Read755_TrashBear";
					hashtable["StardewValley.Characters.Cat::"] = "Read756_Cat";
					hashtable["StardewValley.Characters.Child::"] = "Read757_Child";
					hashtable["StardewValley.Characters.BotchedNetInt::"] = "Read758_BotchedNetInt";
					hashtable["StardewValley.Characters.BotchedNetBool::"] = "Read759_BotchedNetBool";
					hashtable["StardewValley.Characters.BotchedNetLong::"] = "Read760_BotchedNetLong";
					hashtable["StardewValley.Characters.Dog::"] = "Read761_Dog";
					hashtable["StardewValley.Characters.Horse::"] = "Read762_Horse";
					hashtable["StardewValley.Characters.Junimo::"] = "Read763_Junimo";
					hashtable["StardewValley.Characters.JunimoHarvester::"] = "Read764_JunimoHarvester";
					hashtable["StardewValley.Characters.Pet::"] = "Read765_Pet";
					hashtable["StardewValley.Buildings.FishPond::"] = "Read766_FishPond";
					hashtable["StardewValley.Buildings.GreenhouseBuilding::"] = "Read767_GreenhouseBuilding";
					hashtable["StardewValley.Buildings.JunimoHut::"] = "Read768_JunimoHut";
					hashtable["StardewValley.Buildings.Mill::"] = "Read769_Mill";
					hashtable["StardewValley.Buildings.ShippingBin::"] = "Read770_ShippingBin";
					hashtable["StardewValley.Buildings.Stable::"] = "Read771_Stable";
					hashtable["StardewValley.Buildings.Building::"] = "Read772_Building";
					hashtable["StardewValley.Buildings.Coop::"] = "Read773_Coop";
					hashtable["StardewValley.Buildings.Barn::"] = "Read774_Barn";
					hashtable["StardewValley.BellsAndWhistles.SandDuggy::"] = "Read775_SandDuggy";
					hashtable["StardewValley.BellsAndWhistles.SandDuggy+State::"] = "Read776_SandDuggyState";
					if (readMethods == null)
					{
						readMethods = hashtable;
					}
				}
				return readMethods;
			}
		}

		public override Hashtable WriteMethods
		{
			get
			{
				if (writeMethods == null)
				{
					Hashtable hashtable = new Hashtable();
					hashtable["WalkDirection::"] = "Write393_WalkDirection";
					hashtable["ObjectID::"] = "Write394_ObjectID";
					hashtable["BigCraftableID::"] = "Write395_BigCraftableID";
					hashtable["FurnitureID::"] = "Write396_FurnitureID";
					hashtable["MouseCursor::"] = "Write397_MouseCursor";
					hashtable["TapState::"] = "Write398_TapState";
					hashtable["DistanceToTarget::"] = "Write399_DistanceToTarget";
					hashtable["WeaponControl::"] = "Write400_WeaponControl";
					hashtable["tutorialType::"] = "Write401_tutorialType";
					hashtable["TutorialShopLocation::"] = "Write402_TutorialShopLocation";
					hashtable["LocationWeather::"] = "Write403_LocationWeather";
					hashtable["DontLoadDefaultSetting::"] = "Write404_DontLoadDefaultSetting";
					hashtable["WaterTiles+WaterTileData::"] = "Write405_WaterTileData";
					hashtable["TinyTween.TweenState::"] = "Write406_TweenState";
					hashtable["TinyTween.StopBehavior::"] = "Write407_StopBehavior";
					hashtable["TinyTween.FloatTween::"] = "Write408_FloatTween";
					hashtable["TinyTween.Vector2Tween::"] = "Write409_Vector2Tween";
					hashtable["TinyTween.Vector3Tween::"] = "Write410_Vector3Tween";
					hashtable["TinyTween.Vector4Tween::"] = "Write411_Vector4Tween";
					hashtable["TinyTween.ColorTween::"] = "Write412_ColorTween";
					hashtable["TinyTween.QuaternionTween::"] = "Write413_QuaternionTween";
					hashtable["StardewValley.BuildingPainter::"] = "Write414_BuildingPainter";
					hashtable["StardewValley.BuildingPaintColor::"] = "Write415_BuildingPaintColor";
					hashtable["StardewValley.HouseRenovation+AnimationType::"] = "Write416_AnimationType";
					hashtable["StardewValley.IslandGemBird::"] = "Write417_IslandGemBird";
					hashtable["StardewValley.IslandGemBird+GemBirdType::"] = "Write418_GemBirdType";
					hashtable["StardewValley.InstanceStatics::"] = "Write419_InstanceStatics";
					hashtable["StardewValley.InstancedStatic::"] = "Write420_InstancedStatic";
					hashtable["StardewValley.NonInstancedStatic::"] = "Write421_NonInstancedStatic";
					hashtable["StardewValley.LocalMultiplayer::"] = "Write422_LocalMultiplayer";
					hashtable["StardewValley.LocationName::"] = "Write423_LocationName";
					hashtable["StardewValley.Options::"] = "Write424_Options";
					hashtable["StardewValley.Options+ItemStowingModes::"] = "Write425_ItemStowingModes";
					hashtable["StardewValley.Options+GamepadModes::"] = "Write426_GamepadModes";
					hashtable["StardewValley.MapSeat::"] = "Write427_MapSeat";
					hashtable["StardewValley.SpecialOrder::"] = "Write428_SpecialOrder";
					hashtable["StardewValley.SpecialOrder+QuestState::"] = "Write429_QuestState";
					hashtable["StardewValley.SpecialOrder+QuestDuration::"] = "Write430_QuestDuration";
					hashtable["StardewValley.OrderObjective::"] = "Write431_OrderObjective";
					hashtable["StardewValley.CollectObjective::"] = "Write432_CollectObjective";
					hashtable["StardewValley.DonateObjective::"] = "Write433_DonateObjective";
					hashtable["StardewValley.ShipObjective::"] = "Write434_ShipObjective";
					hashtable["StardewValley.SlayObjective::"] = "Write435_SlayObjective";
					hashtable["StardewValley.FishObjective::"] = "Write436_FishObjective";
					hashtable["StardewValley.DeliverObjective::"] = "Write437_DeliverObjective";
					hashtable["StardewValley.GiftObjective::"] = "Write438_GiftObjective";
					hashtable["StardewValley.GiftObjective+LikeLevels::"] = "Write439_LikeLevels";
					hashtable["StardewValley.ReachMineFloorObjective::"] = "Write440_ReachMineFloorObjective";
					hashtable["StardewValley.JKScoreObjective::"] = "Write441_JKScoreObjective";
					hashtable["StardewValley.OrderReward::"] = "Write442_OrderReward";
					hashtable["StardewValley.MailReward::"] = "Write443_MailReward";
					hashtable["StardewValley.ResetEventReward::"] = "Write444_ResetEventReward";
					hashtable["StardewValley.GemsReward::"] = "Write445_GemsReward";
					hashtable["StardewValley.MoneyReward::"] = "Write446_MoneyReward";
					hashtable["StardewValley.FriendshipReward::"] = "Write447_FriendshipReward";
					hashtable["StardewValley.BaseEnchantment::"] = "Write448_BaseEnchantment";
					hashtable["StardewValley.BaseWeaponEnchantment::"] = "Write449_BaseWeaponEnchantment";
					hashtable["StardewValley.MagicEnchantment::"] = "Write450_MagicEnchantment";
					hashtable["StardewValley.AmethystEnchantment::"] = "Write451_AmethystEnchantment";
					hashtable["StardewValley.TopazEnchantment::"] = "Write452_TopazEnchantment";
					hashtable["StardewValley.AquamarineEnchantment::"] = "Write453_AquamarineEnchantment";
					hashtable["StardewValley.JadeEnchantment::"] = "Write454_JadeEnchantment";
					hashtable["StardewValley.DiamondEnchantment::"] = "Write455_DiamondEnchantment";
					hashtable["StardewValley.GalaxySoulEnchantment::"] = "Write456_GalaxySoulEnchantment";
					hashtable["StardewValley.RubyEnchantment::"] = "Write457_RubyEnchantment";
					hashtable["StardewValley.EmeraldEnchantment::"] = "Write458_EmeraldEnchantment";
					hashtable["StardewValley.ArtfulEnchantment::"] = "Write459_ArtfulEnchantment";
					hashtable["StardewValley.HaymakerEnchantment::"] = "Write460_HaymakerEnchantment";
					hashtable["StardewValley.BugKillerEnchantment::"] = "Write461_BugKillerEnchantment";
					hashtable["StardewValley.VampiricEnchantment::"] = "Write462_VampiricEnchantment";
					hashtable["StardewValley.CrusaderEnchantment::"] = "Write463_CrusaderEnchantment";
					hashtable["StardewValley.PickaxeEnchantment::"] = "Write464_PickaxeEnchantment";
					hashtable["StardewValley.HoeEnchantment::"] = "Write465_HoeEnchantment";
					hashtable["StardewValley.AxeEnchantment::"] = "Write466_AxeEnchantment";
					hashtable["StardewValley.FishingRodEnchantment::"] = "Write467_FishingRodEnchantment";
					hashtable["StardewValley.WateringCanEnchantment::"] = "Write468_WateringCanEnchantment";
					hashtable["StardewValley.PanEnchantment::"] = "Write469_PanEnchantment";
					hashtable["StardewValley.MilkPailEnchantment::"] = "Write470_MilkPailEnchantment";
					hashtable["StardewValley.ShearsEnchantment::"] = "Write471_ShearsEnchantment";
					hashtable["StardewValley.PowerfulEnchantment::"] = "Write472_PowerfulEnchantment";
					hashtable["StardewValley.EfficientToolEnchantment::"] = "Write473_EfficientToolEnchantment";
					hashtable["StardewValley.SwiftToolEnchantment::"] = "Write474_SwiftToolEnchantment";
					hashtable["StardewValley.ReachingToolEnchantment::"] = "Write475_ReachingToolEnchantment";
					hashtable["StardewValley.BottomlessEnchantment::"] = "Write476_BottomlessEnchantment";
					hashtable["StardewValley.ShavingEnchantment::"] = "Write477_ShavingEnchantment";
					hashtable["StardewValley.ArchaeologistEnchantment::"] = "Write478_ArchaeologistEnchantment";
					hashtable["StardewValley.GenerousEnchantment::"] = "Write479_GenerousEnchantment";
					hashtable["StardewValley.MasterEnchantment::"] = "Write480_MasterEnchantment";
					hashtable["StardewValley.AutoHookEnchantment::"] = "Write481_AutoHookEnchantment";
					hashtable["StardewValley.PreservingEnchantment::"] = "Write482_PreservingEnchantment";
					hashtable["StardewValley.StackDrawType::"] = "Write483_StackDrawType";
					hashtable["StardewValley.MovieInvitation::"] = "Write484_MovieInvitation";
					hashtable["StardewValley.StartMovieEvent::"] = "Write485_StartMovieEvent";
					hashtable["StardewValley.MovieViewerLockEvent::"] = "Write486_MovieViewerLockEvent";
					hashtable["StardewValley.WorldDate::"] = "Write487_WorldDate";
					hashtable["StardewValley.FriendshipStatus::"] = "Write488_FriendshipStatus";
					hashtable["StardewValley.Friendship::"] = "Write489_Friendship";
					hashtable["StardewValley.LocalizedContentManager+LanguageCode::"] = "Write490_LanguageCode";
					hashtable["StardewValley.AnimalHouse::"] = "Write491_AnimalHouse";
					hashtable["StardewValley.BuildingUpgrade::"] = "Write492_BuildingUpgrade";
					hashtable["StardewValley.Character::"] = "Write493_Character";
					hashtable["StardewValley.Chunk::"] = "Write494_Chunk";
					hashtable["StardewValley.FarmerRenderer::"] = "Write495_FarmerRenderer";
					hashtable["StardewValley.Shed::"] = "Write496_Shed";
					hashtable["StardewValley.SlimeHutch::"] = "Write497_SlimeHutch";
					hashtable["StardewValley.Farm::"] = "Write498_Farm";
					hashtable["StardewValley.Farm+LightningStrikeEvent::"] = "Write499_LightningStrikeEvent";
					hashtable["StardewValley.FarmAnimal::"] = "Write500_FarmAnimal";
					hashtable["StardewValley.NetLogger::"] = "Write501_NetLogger";
					hashtable["StardewValley.Noise::"] = "Write502_Noise";
					hashtable["StardewValley.NumberSprite::"] = "Write503_NumberSprite";
					hashtable["StardewValley.Fence::"] = "Write504_Fence";
					hashtable["StardewValley.Item::"] = "Write505_Item";
					hashtable["StardewValley.ModDataDictionary::"] = "Write506_Item";
					hashtable["StardewValley.LightSource::"] = "Write507_LightSource";
					hashtable["StardewValley.LightSource+LightContext::"] = "Write508_LightContext";
					hashtable["StardewValley.Torch::"] = "Write509_Torch";
					hashtable["StardewValley.InputButton::"] = "Write510_InputButton";
					hashtable["StardewValley.ServerPrivacy::"] = "Write511_ServerPrivacy";
					hashtable["StardewValley.NameSelect::"] = "Write512_NameSelect";
					hashtable["StardewValley.PriorityQueue::"] = "Write513_PriorityQueue";
					hashtable["StardewValley.Crop::"] = "Write514_Crop";
					hashtable["StardewValley.Farmer::"] = "Write515_Farmer";
					hashtable["StardewValley.NetIntIntArrayDictionary::"] = "Write516_Item";
					hashtable["StardewValley.HairStyleMetadata::"] = "Write517_HairStyleMetadata";
					hashtable["StardewValley.FarmerPair::"] = "Write518_FarmerPair";
					hashtable["StardewValley.NutDropRequest::"] = "Write519_NutDropRequest";
					hashtable["StardewValley.FarmerTeam+RemoteBuildingPermissions::"] = "Write520_RemoteBuildingPermissions";
					hashtable["StardewValley.FarmerTeam+SleepAnnounceModes::"] = "Write521_SleepAnnounceModes";
					hashtable["StardewValley.InteriorDoor::"] = "Write522_ArrayOfBoolean";
					hashtable["StardewValley.InteriorDoorDictionary::"] = "Write523_Item";
					hashtable["StardewValley.GameLocation::"] = "Write524_GameLocation";
					hashtable["StardewValley.GameLocation+LocationContext::"] = "Write525_LocationContext";
					hashtable["StardewValley.NPC::"] = "Write526_NPC";
					hashtable["StardewValley.MarriageDialogueReference::"] = "Write527_MarriageDialogueReference";
					hashtable["StardewValley.FarmActivity::"] = "Write528_FarmActivity";
					hashtable["StardewValley.ArtifactSpotWatchActivity::"] = "Write529_ArtifactSpotWatchActivity";
					hashtable["StardewValley.CropWatchActivity::"] = "Write530_CropWatchActivity";
					hashtable["StardewValley.FlowerWatchActivity::"] = "Write531_FlowerWatchActivity";
					hashtable["StardewValley.ShrineActivity::"] = "Write532_ShrineActivity";
					hashtable["StardewValley.MailActivity::"] = "Write533_MailActivity";
					hashtable["StardewValley.TreeActivity::"] = "Write534_TreeActivity";
					hashtable["StardewValley.ClearingActivity::"] = "Write535_ClearingActivity";
					hashtable["StardewValley.Object::"] = "Write536_Object";
					hashtable["StardewValley.Object+PreserveType::"] = "Write537_PreserveType";
					hashtable["StardewValley.Object+HoneyType::"] = "Write538_HoneyType";
					hashtable["StardewValley.RainDrop::"] = "Write539_RainDrop";
					hashtable["StardewValley.MineChestType::"] = "Write540_MineChestType";
					hashtable["StardewValley.Vector2Reader::"] = "Write541_Vector2Reader";
					hashtable["StardewValley.Vector2Writer::"] = "Write542_Vector2Writer";
					hashtable["StardewValley.Vector2Serializer::"] = "Write543_Vector2Serializer";
					hashtable["StardewValley.SaveGame::"] = "Write544_SaveGame";
					hashtable["StardewValley.SaveGame+SaveFixes::"] = "Write545_SaveFixes";
					hashtable["StardewValley.ChangeType::"] = "Write546_ChangeType";
					hashtable["StardewValley.StartupPreferences::"] = "Write547_StartupPreferences";
					hashtable["StardewValley.Stats::"] = "Write548_Stats";
					hashtable["StardewValley.Tool::"] = "Write549_Tool";
					hashtable["StardewValley.Warp::"] = "Write550_Warp";
					hashtable["StardewValley.WeatherDebris::"] = "Write551_WeatherDebris";
					hashtable["StardewValley.TerrainFeatures.Bush::"] = "Write552_Bush";
					hashtable["StardewValley.TerrainFeatures.CosmeticPlant::"] = "Write553_CosmeticPlant";
					hashtable["StardewValley.TerrainFeatures.Flooring::"] = "Write554_Flooring";
					hashtable["StardewValley.TerrainFeatures.FruitTree::"] = "Write555_FruitTree";
					hashtable["StardewValley.TerrainFeatures.GiantCrop::"] = "Write556_GiantCrop";
					hashtable["StardewValley.TerrainFeatures.HoeDirt::"] = "Write557_HoeDirt";
					hashtable["StardewValley.TerrainFeatures.LargeTerrainFeature::"] = "Write558_LargeTerrainFeature";
					hashtable["StardewValley.TerrainFeatures.ResourceClump::"] = "Write559_ResourceClump";
					hashtable["StardewValley.TerrainFeatures.Grass::"] = "Write560_Grass";
					hashtable["StardewValley.TerrainFeatures.Quartz::"] = "Write561_Quartz";
					hashtable["StardewValley.TerrainFeatures.TerrainFeature::"] = "Write562_TerrainFeature";
					hashtable["StardewValley.TerrainFeatures.Tree::"] = "Write563_Tree";
					hashtable["StardewValley.Projectiles.BasicProjectile::"] = "Write564_BasicProjectile";
					hashtable["StardewValley.Projectiles.DebuffingProjectile::"] = "Write565_DebuffingProjectile";
					hashtable["StardewValley.Projectiles.Projectile::"] = "Write566_Projectile";
					hashtable["StardewValley.Tools.GenericTool::"] = "Write567_GenericTool";
					hashtable["StardewValley.Tools.Axe::"] = "Write568_Axe";
					hashtable["StardewValley.Tools.Blueprints::"] = "Write569_Blueprints";
					hashtable["StardewValley.Tools.Pan::"] = "Write570_Pan";
					hashtable["StardewValley.Tools.FishingRod::"] = "Write571_FishingRod";
					hashtable["StardewValley.Tools.Lantern::"] = "Write572_Lantern";
					hashtable["StardewValley.Tools.MagnifyingGlass::"] = "Write573_MagnifyingGlass";
					hashtable["StardewValley.Tools.MeleeWeapon::"] = "Write574_MeleeWeapon";
					hashtable["StardewValley.Tools.MilkPail::"] = "Write575_MilkPail";
					hashtable["StardewValley.Tools.Pickaxe::"] = "Write576_Pickaxe";
					hashtable["StardewValley.Tools.Hoe::"] = "Write577_Hoe";
					hashtable["StardewValley.Tools.Raft::"] = "Write578_Raft";
					hashtable["StardewValley.Tools.Seeds::"] = "Write579_Seeds";
					hashtable["StardewValley.Tools.Shears::"] = "Write580_Shears";
					hashtable["StardewValley.Tools.Slingshot::"] = "Write581_Slingshot";
					hashtable["StardewValley.Tools.Stackable::"] = "Write582_Stackable";
					hashtable["StardewValley.Tools.Sword::"] = "Write583_Sword";
					hashtable["StardewValley.Tools.ToolDescription::"] = "Write584_ToolDescription";
					hashtable["StardewValley.Tools.ToolFactory::"] = "Write585_ToolFactory";
					hashtable["StardewValley.Tools.Wand::"] = "Write586_Wand";
					hashtable["StardewValley.Tools.WateringCan::"] = "Write587_WateringCan";
					hashtable["StardewValley.Util.BoundingBoxGroup::"] = "Write588_BoundingBoxGroup";
					hashtable["StardewValley.Util.ToolSpamInputSimulator::"] = "Write589_ToolSpamInputSimulator";
					hashtable["StardewValley.Util.LeftRightClickSpamInputSimulator::"] = "Write590_Item";
					hashtable["StardewValley.Util.SynchronizedShopStock::"] = "Write591_SynchronizedShopStock";
					hashtable["StardewValley.Util.SynchronizedShopStock+SynchedShop::"] = "Write592_SynchedShop";
					hashtable["StardewValley.Quests.SecretLostItemQuest::"] = "Write593_SecretLostItemQuest";
					hashtable["StardewValley.Quests.NetDescriptionElementRef::"] = "Write594_ArrayOfDescriptionElement";
					hashtable["StardewValley.Quests.NetDescriptionElementList::"] = "Write595_ArrayOfDescriptionElement";
					hashtable["StardewValley.Quests.DescriptionElement::"] = "Write596_DescriptionElement";
					hashtable["StardewValley.Quests.LostItemQuest::"] = "Write597_LostItemQuest";
					hashtable["StardewValley.Quests.ItemHarvestQuest::"] = "Write598_ItemHarvestQuest";
					hashtable["StardewValley.Quests.GoSomewhereQuest::"] = "Write599_GoSomewhereQuest";
					hashtable["StardewValley.Quests.CraftingQuest::"] = "Write600_CraftingQuest";
					hashtable["StardewValley.Quests.SocializeQuest::"] = "Write601_SocializeQuest";
					hashtable["StardewValley.Quests.FishingQuest::"] = "Write602_FishingQuest";
					hashtable["StardewValley.Quests.SlayMonsterQuest::"] = "Write603_SlayMonsterQuest";
					hashtable["StardewValley.Quests.ResourceCollectionQuest::"] = "Write604_ResourceCollectionQuest";
					hashtable["StardewValley.Quests.ItemDeliveryQuest::"] = "Write605_ItemDeliveryQuest";
					hashtable["StardewValley.Quests.Quest::"] = "Write606_Quest";
					hashtable["StardewValley.Objects.BedFurniture::"] = "Write607_BedFurniture";
					hashtable["StardewValley.Objects.BedFurniture+BedType::"] = "Write608_BedType";
					hashtable["StardewValley.Objects.FishTankFurniture::"] = "Write609_FishTankFurniture";
					hashtable["StardewValley.Objects.FishTankFurniture+FishTankCategories::"] = "Write610_FishTankCategories";
					hashtable["StardewValley.Objects.TankFish+FishType::"] = "Write611_FishType";
					hashtable["StardewValley.Objects.ItemPedestal::"] = "Write612_ItemPedestal";
					hashtable["StardewValley.Objects.Phone::"] = "Write613_Phone";
					hashtable["StardewValley.Objects.Phone+PhoneCalls::"] = "Write614_PhoneCalls";
					hashtable["StardewValley.Objects.StorageFurniture::"] = "Write615_StorageFurniture";
					hashtable["StardewValley.Objects.WoodChipper::"] = "Write616_WoodChipper";
					hashtable["StardewValley.Objects.Clothing::"] = "Write617_Clothing";
					hashtable["StardewValley.Objects.Clothing+ClothesType::"] = "Write618_ClothesType";
					hashtable["StardewValley.Objects.IndoorPot::"] = "Write619_IndoorPot";
					hashtable["StardewValley.Objects.MiniJukebox::"] = "Write620_MiniJukebox";
					hashtable["StardewValley.Objects.Sign::"] = "Write621_Sign";
					hashtable["StardewValley.Objects.Workbench::"] = "Write622_Workbench";
					hashtable["StardewValley.Objects.Boots::"] = "Write623_Boots";
					hashtable["StardewValley.Objects.BreakableContainer::"] = "Write624_BreakableContainer";
					hashtable["StardewValley.Objects.Cask::"] = "Write625_Cask";
					hashtable["StardewValley.Objects.Chest::"] = "Write626_Chest";
					hashtable["StardewValley.Objects.Chest+SpecialChestTypes::"] = "Write627_SpecialChestTypes";
					hashtable["StardewValley.Objects.ColoredObject::"] = "Write628_ColoredObject";
					hashtable["StardewValley.Objects.SpecialItem::"] = "Write629_SpecialItem";
					hashtable["StardewValley.Objects.TV::"] = "Write630_TV";
					hashtable["StardewValley.Objects.Wallpaper::"] = "Write631_Wallpaper";
					hashtable["StardewValley.Objects.Furniture::"] = "Write632_Furniture";
					hashtable["StardewValley.Objects.CrabPot::"] = "Write633_CrabPot";
					hashtable["StardewValley.Objects.Hat::"] = "Write634_Hat";
					hashtable["StardewValley.Objects.Hat+HairDrawType::"] = "Write635_HairDrawType";
					hashtable["StardewValley.Objects.ItemDescription::"] = "Write636_ItemDescription";
					hashtable["StardewValley.Objects.ObjectFactory::"] = "Write637_ObjectFactory";
					hashtable["StardewValley.Objects.Ring::"] = "Write638_Ring";
					hashtable["StardewValley.Objects.CombinedRing::"] = "Write639_CombinedRing";
					hashtable["StardewValley.Objects.SwitchFloor::"] = "Write640_SwitchFloor";
					hashtable["StardewValley.Network.IncomingMessage::"] = "Write641_IncomingMessage";
					hashtable["StardewValley.Network.NetAudio+SoundContext::"] = "Write642_SoundContext";
					hashtable["StardewValley.Network.NetDirection::"] = "Write643_ArrayOfInt";
					hashtable["StardewValley.Network.NetLocationRef::"] = "Write644_NetLocationRef";
					hashtable["StardewValley.Network.NetPosition::"] = "Write645_NetPosition";
					hashtable["StardewValley.Network.NetBundles::"] = "Write646_Item";
					hashtable["StardewValley.Network.NetFarmerCollection::"] = "Write647_ArrayOfFarmer";
					hashtable["StardewValley.Network.NetMutex::"] = "Write648_NetMutex";
					hashtable["StardewValley.Network.NetWitnessedLock::"] = "Write649_NetWitnessedLock";
					hashtable["StardewValley.Network.NetFarmerRoot::"] = "Write650_ArrayOfFarmer";
					hashtable["StardewValley.Network.NetNPCRef::"] = "Write651_NetNPCRef";
					hashtable["StardewValley.Network.NetCharacterRef::"] = "Write652_NetCharacterRef";
					hashtable["StardewValley.Network.NetDancePartner::"] = "Write653_NetDancePartner";
					hashtable["StardewValley.Network.OverlaidDictionary::"] = "Write654_Item";
					hashtable["StardewValley.Monsters.AngryRoger::"] = "Write655_AngryRoger";
					hashtable["StardewValley.Monsters.HotHead::"] = "Write656_HotHead";
					hashtable["StardewValley.Monsters.LavaLurk::"] = "Write657_LavaLurk";
					hashtable["StardewValley.Monsters.LavaLurk+State::"] = "Write658_LavaLurkState";
					hashtable["StardewValley.Monsters.Leaper::"] = "Write659_Leaper";
					hashtable["StardewValley.Monsters.Shooter::"] = "Write660_Shooter";
					hashtable["StardewValley.Monsters.Spiker::"] = "Write661_Spiker";
					hashtable["StardewValley.Monsters.DwarvishSentry::"] = "Write662_DwarvishSentry";
					hashtable["StardewValley.Monsters.BlueSquid::"] = "Write663_BlueSquid";
					hashtable["StardewValley.Monsters.DinoMonster::"] = "Write664_DinoMonster";
					hashtable["StardewValley.Monsters.DinoMonster+AttackState::"] = "Write665_AttackState";
					hashtable["StardewValley.Monsters.Bug::"] = "Write666_Bug";
					hashtable["StardewValley.Monsters.BigSlime::"] = "Write667_BigSlime";
					hashtable["StardewValley.Monsters.Mummy::"] = "Write668_Mummy";
					hashtable["StardewValley.Monsters.Serpent::"] = "Write669_Serpent";
					hashtable["StardewValley.Monsters.MetalHead::"] = "Write670_MetalHead";
					hashtable["StardewValley.Monsters.ShadowBrute::"] = "Write671_ShadowBrute";
					hashtable["StardewValley.Monsters.ShadowShaman::"] = "Write672_ShadowShaman";
					hashtable["StardewValley.Monsters.Skeleton::"] = "Write673_Skeleton";
					hashtable["StardewValley.Monsters.DustSpirit::"] = "Write674_DustSpirit";
					hashtable["StardewValley.Monsters.Bat::"] = "Write675_Bat";
					hashtable["StardewValley.Monsters.Fly::"] = "Write676_Fly";
					hashtable["StardewValley.Monsters.Grub::"] = "Write677_Grub";
					hashtable["StardewValley.Monsters.RockGolem::"] = "Write678_RockGolem";
					hashtable["StardewValley.Monsters.ShadowGirl::"] = "Write679_ShadowGirl";
					hashtable["StardewValley.Monsters.SquidKid::"] = "Write680_SquidKid";
					hashtable["StardewValley.Monsters.LavaCrab::"] = "Write681_LavaCrab";
					hashtable["StardewValley.Monsters.RockCrab::"] = "Write682_RockCrab";
					hashtable["StardewValley.Monsters.Duggy::"] = "Write683_Duggy";
					hashtable["StardewValley.Monsters.Ghost::"] = "Write684_Ghost";
					hashtable["StardewValley.Monsters.Ghost+GhostVariant::"] = "Write685_GhostVariant";
					hashtable["StardewValley.Monsters.ShadowGuy::"] = "Write686_ShadowGuy";
					hashtable["StardewValley.Monsters.GreenSlime::"] = "Write687_GreenSlime";
					hashtable["StardewValley.Monsters.Monster::"] = "Write688_Monster";
					hashtable["StardewValley.Minigames.NetLeaderboards::"] = "Write689_NetLeaderboards";
					hashtable["StardewValley.Minigames.NetLeaderboardsEntry::"] = "Write690_NetLeaderboardsEntry";
					hashtable["StardewValley.Minigames.AbigailGame+GameKeys::"] = "Write691_GameKeys";
					hashtable["StardewValley.Minigames.AbigailGame+JOTPKProgress::"] = "Write692_JOTPKProgress";
					hashtable["StardewValley.Minigames.AbigailGame+Dracula::"] = "Write693_Dracula";
					hashtable["StardewValley.Locations.AbandonedJojaMart::"] = "Write694_AbandonedJojaMart";
					hashtable["StardewValley.Locations.BoatTunnel::"] = "Write695_BoatTunnel";
					hashtable["StardewValley.Locations.BoatTunnel+TunnelAnimationState::"] = "Write696_TunnelAnimationState";
					hashtable["StardewValley.Locations.BugLand::"] = "Write697_BugLand";
					hashtable["StardewValley.Locations.Caldera::"] = "Write698_Caldera";
					hashtable["StardewValley.Locations.FishShop::"] = "Write699_FishShop";
					hashtable["StardewValley.Locations.IslandEast::"] = "Write700_IslandEast";
					hashtable["StardewValley.Locations.IslandFarmCave::"] = "Write701_IslandFarmCave";
					hashtable["StardewValley.Locations.IslandFarmHouse::"] = "Write702_IslandFarmHouse";
					hashtable["StardewValley.Locations.IslandFieldOffice::"] = "Write703_IslandFieldOffice";
					hashtable["StardewValley.Locations.IslandForestLocation::"] = "Write704_IslandForestLocation";
					hashtable["StardewValley.Locations.IslandHut::"] = "Write705_IslandHut";
					hashtable["StardewValley.Locations.IslandLocation::"] = "Write706_IslandLocation";
					hashtable["StardewValley.Locations.IslandNorth::"] = "Write707_IslandNorth";
					hashtable["StardewValley.Locations.IslandSecret::"] = "Write708_IslandSecret";
					hashtable["StardewValley.Locations.IslandShrine::"] = "Write709_IslandShrine";
					hashtable["StardewValley.Locations.IslandSouth::"] = "Write710_IslandSouth";
					hashtable["StardewValley.Locations.IslandSouthEast::"] = "Write711_IslandSouthEast";
					hashtable["StardewValley.Locations.IslandSouthEastCave::"] = "Write712_IslandSouthEastCave";
					hashtable["StardewValley.Locations.IslandWest::"] = "Write713_IslandWest";
					hashtable["StardewValley.Locations.IslandWestCave1::"] = "Write714_IslandWestCave1";
					hashtable["StardewValley.Locations.IslandWestCave1+CaveCrystal::"] = "Write715_CaveCrystal";
					hashtable["StardewValley.Locations.Mine::"] = "Write716_Mine";
					hashtable["StardewValley.Locations.ShopLocation::"] = "Write717_ShopLocation";
					hashtable["StardewValley.Locations.VolcanoDungeon+TileNeighbors::"] = "Write718_TileNeighbors";
					hashtable["StardewValley.Locations.BeachNightMarket::"] = "Write719_BeachNightMarket";
					hashtable["StardewValley.Locations.ManorHouse::"] = "Write720_ManorHouse";
					hashtable["StardewValley.Locations.MermaidHouse::"] = "Write721_MermaidHouse";
					hashtable["StardewValley.Locations.MovieTheater::"] = "Write722_MovieTheater";
					hashtable["StardewValley.Locations.MovieTheater+MovieStates::"] = "Write723_MovieStates";
					hashtable["StardewValley.Locations.Submarine::"] = "Write724_Submarine";
					hashtable["StardewValley.Locations.AdventureGuild::"] = "Write725_AdventureGuild";
					hashtable["StardewValley.Locations.Bus::"] = "Write726_Bus";
					hashtable["StardewValley.Locations.BuildableGameLocation::"] = "Write727_BuildableGameLocation";
					hashtable["StardewValley.Locations.BathHousePool::"] = "Write728_BathHousePool";
					hashtable["StardewValley.Locations.Club::"] = "Write729_Club";
					hashtable["StardewValley.Locations.Cellar::"] = "Write730_Cellar";
					hashtable["StardewValley.Locations.DecorationFacade::"] = "Write731_ArrayOfInt";
					hashtable["StardewValley.Locations.DecoratableLocation::"] = "Write732_DecoratableLocation";
					hashtable["StardewValley.Locations.Cabin::"] = "Write733_Cabin";
					hashtable["StardewValley.Locations.FarmCave::"] = "Write734_FarmCave";
					hashtable["StardewValley.Locations.WizardHouse::"] = "Write735_WizardHouse";
					hashtable["StardewValley.Locations.Sewer::"] = "Write736_Sewer";
					hashtable["StardewValley.Locations.CommunityCenter::"] = "Write737_CommunityCenter";
					hashtable["StardewValley.Locations.BusStop::"] = "Write738_BusStop";
					hashtable["StardewValley.Locations.MineShaft::"] = "Write739_MineShaft";
					hashtable["StardewValley.Locations.MineInfo::"] = "Write740_MineInfo";
					hashtable["StardewValley.Locations.FarmHouse::"] = "Write741_FarmHouse";
					hashtable["StardewValley.Locations.JojaMart::"] = "Write742_JojaMart";
					hashtable["StardewValley.Locations.Desert::"] = "Write743_Desert";
					hashtable["StardewValley.Locations.Town::"] = "Write744_Town";
					hashtable["StardewValley.Locations.Mountain::"] = "Write745_Mountain";
					hashtable["StardewValley.Locations.Forest::"] = "Write746_Forest";
					hashtable["StardewValley.Locations.LibraryMuseum::"] = "Write747_LibraryMuseum";
					hashtable["StardewValley.Locations.Railroad::"] = "Write748_Railroad";
					hashtable["StardewValley.Locations.SeedShop::"] = "Write749_SeedShop";
					hashtable["StardewValley.Locations.Summit::"] = "Write750_Summit";
					hashtable["StardewValley.Locations.Woods::"] = "Write751_Woods";
					hashtable["StardewValley.Locations.Beach::"] = "Write752_Beach";
					hashtable["StardewValley.Characters.TrashBear::"] = "Write753_TrashBear";
					hashtable["StardewValley.Characters.Cat::"] = "Write754_Cat";
					hashtable["StardewValley.Characters.Child::"] = "Write755_Child";
					hashtable["StardewValley.Characters.BotchedNetInt::"] = "Write756_BotchedNetInt";
					hashtable["StardewValley.Characters.BotchedNetBool::"] = "Write757_BotchedNetBool";
					hashtable["StardewValley.Characters.BotchedNetLong::"] = "Write758_BotchedNetLong";
					hashtable["StardewValley.Characters.Dog::"] = "Write759_Dog";
					hashtable["StardewValley.Characters.Horse::"] = "Write760_Horse";
					hashtable["StardewValley.Characters.Junimo::"] = "Write761_Junimo";
					hashtable["StardewValley.Characters.JunimoHarvester::"] = "Write762_JunimoHarvester";
					hashtable["StardewValley.Characters.Pet::"] = "Write763_Pet";
					hashtable["StardewValley.Buildings.FishPond::"] = "Write764_FishPond";
					hashtable["StardewValley.Buildings.GreenhouseBuilding::"] = "Write765_GreenhouseBuilding";
					hashtable["StardewValley.Buildings.JunimoHut::"] = "Write766_JunimoHut";
					hashtable["StardewValley.Buildings.Mill::"] = "Write767_Mill";
					hashtable["StardewValley.Buildings.ShippingBin::"] = "Write768_ShippingBin";
					hashtable["StardewValley.Buildings.Stable::"] = "Write769_Stable";
					hashtable["StardewValley.Buildings.Building::"] = "Write770_Building";
					hashtable["StardewValley.Buildings.Coop::"] = "Write771_Coop";
					hashtable["StardewValley.Buildings.Barn::"] = "Write772_Barn";
					hashtable["StardewValley.BellsAndWhistles.SandDuggy::"] = "Write773_SandDuggy";
					hashtable["StardewValley.BellsAndWhistles.SandDuggy+State::"] = "Write774_SandDuggyState";
					if (writeMethods == null)
					{
						writeMethods = hashtable;
					}
				}
				return writeMethods;
			}
		}

		public override Hashtable TypedSerializers
		{
			get
			{
				if (typedSerializers == null)
				{
					Hashtable hashtable = new Hashtable();
					hashtable.Add("StardewValley.Minigames.AbigailGame+Dracula::", new DraculaSerializer());
					hashtable.Add("StardewValley.FarmActivity::", new FarmActivitySerializer());
					hashtable.Add("StardewValley.SpecialOrder+QuestState::", new QuestStateSerializer());
					hashtable.Add("StardewValley.Stats::", new StatsSerializer());
					hashtable.Add("StardewValley.Quests.DescriptionElement::", new DescriptionElementSerializer());
					hashtable.Add("StardewValley.FlowerWatchActivity::", new FlowerWatchActivitySerializer());
					hashtable.Add("StardewValley.Minigames.AbigailGame+JOTPKProgress::", new JOTPKProgressSerializer());
					hashtable.Add("StardewValley.Locations.Beach::", new BeachSerializer());
					hashtable.Add("StardewValley.Monsters.SquidKid::", new SquidKidSerializer());
					hashtable.Add("StardewValley.Locations.FishShop::", new FishShopSerializer());
					hashtable.Add("TinyTween.Vector4Tween::", new Vector4TweenSerializer());
					hashtable.Add("StardewValley.OrderObjective::", new OrderObjectiveSerializer());
					hashtable.Add("StardewValley.Locations.Summit::", new SummitSerializer());
					hashtable.Add("StardewValley.Network.IncomingMessage::", new IncomingMessageSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.FruitTree::", new FruitTreeSerializer());
					hashtable.Add("StardewValley.Quests.LostItemQuest::", new LostItemQuestSerializer());
					hashtable.Add("StardewValley.Tool::", new ToolSerializer());
					hashtable.Add("StardewValley.Characters.JunimoHarvester::", new JunimoHarvesterSerializer());
					hashtable.Add("TinyTween.Vector3Tween::", new Vector3TweenSerializer());
					hashtable.Add("StardewValley.Projectiles.Projectile::", new ProjectileSerializer());
					hashtable.Add("StardewValley.Buildings.GreenhouseBuilding::", new GreenhouseBuildingSerializer());
					hashtable.Add("StardewValley.Objects.Clothing+ClothesType::", new ClothesTypeSerializer());
					hashtable.Add("StardewValley.Network.NetMutex::", new NetMutexSerializer());
					hashtable.Add("StardewValley.Monsters.BlueSquid::", new BlueSquidSerializer());
					hashtable.Add("StardewValley.Monsters.ShadowGirl::", new ShadowGirlSerializer());
					hashtable.Add("StardewValley.ModDataDictionary::", new ModDataDictionarySerializer());
					hashtable.Add("StardewValley.FishObjective::", new FishObjectiveSerializer());
					hashtable.Add("TinyTween.Vector2Tween::", new Vector2TweenSerializer());
					hashtable.Add("StardewValley.Objects.FishTankFurniture+FishTankCategories::", new FishTankCategoriesSerializer());
					hashtable.Add("StardewValley.Monsters.GreenSlime::", new GreenSlimeSerializer());
					hashtable.Add("StardewValley.Locations.FarmHouse::", new FarmHouseSerializer());
					hashtable.Add("StardewValley.Locations.BoatTunnel::", new BoatTunnelSerializer());
					hashtable.Add("StardewValley.Tools.Shears::", new ShearsSerializer());
					hashtable.Add("StardewValley.SlayObjective::", new SlayObjectiveSerializer());
					hashtable.Add("StardewValley.Objects.ColoredObject::", new ColoredObjectSerializer());
					hashtable.Add("StardewValley.Locations.ShopLocation::", new ShopLocationSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.CosmeticPlant::", new CosmeticPlantSerializer());
					hashtable.Add("StardewValley.HairStyleMetadata::", new HairStyleMetadataSerializer());
					hashtable.Add("StardewValley.ReachingToolEnchantment::", new ReachingToolEnchantmentSerializer());
					hashtable.Add("StardewValley.Locations.IslandFarmHouse::", new IslandFarmHouseSerializer());
					hashtable.Add("StardewValley.BottomlessEnchantment::", new BottomlessEnchantmentSerializer());
					hashtable.Add("StardewValley.Objects.Boots::", new BootsSerializer());
					hashtable.Add("StardewValley.HouseRenovation+AnimationType::", new AnimationTypeSerializer());
					hashtable.Add("StardewValley.CropWatchActivity::", new CropWatchActivitySerializer());
					hashtable.Add("StardewValley.Locations.BeachNightMarket::", new BeachNightMarketSerializer());
					hashtable.Add("StardewValley.Crop::", new CropSerializer());
					hashtable.Add("StardewValley.Tools.ToolDescription::", new ToolDescriptionSerializer());
					hashtable.Add("StardewValley.Monsters.Ghost::", new GhostSerializer());
					hashtable.Add("TinyTween.QuaternionTween::", new QuaternionTweenSerializer());
					hashtable.Add("StardewValley.PowerfulEnchantment::", new PowerfulEnchantmentSerializer());
					hashtable.Add("StardewValley.SlimeHutch::", new SlimeHutchSerializer());
					hashtable.Add("StardewValley.Farmer::", new FarmerSerializer());
					hashtable.Add("StardewValley.Locations.IslandForestLocation::", new IslandForestLocationSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.Grass::", new GrassSerializer());
					hashtable.Add("StardewValley.FishingRodEnchantment::", new FishingRodEnchantmentSerializer());
					hashtable.Add("StardewValley.Buildings.FishPond::", new FishPondSerializer());
					hashtable.Add("StardewValley.Buildings.Stable::", new StableSerializer());
					hashtable.Add("StardewValley.Objects.Wallpaper::", new WallpaperSerializer());
					hashtable.Add("StardewValley.BuildingPainter::", new BuildingPainterSerializer());
					hashtable.Add("StardewValley.AxeEnchantment::", new AxeEnchantmentSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.Bush::", new BushSerializer());
					hashtable.Add("StardewValley.Quests.NetDescriptionElementList::", new NetDescriptionElementRefSerializer1());
					hashtable.Add("StardewValley.DonateObjective::", new DonateObjectiveSerializer());
					hashtable.Add("StardewValley.Objects.Workbench::", new WorkbenchSerializer());
					hashtable.Add("StardewValley.SpecialOrder::", new SpecialOrderSerializer());
					hashtable.Add("StardewValley.ShrineActivity::", new ShrineActivitySerializer());
					hashtable.Add("StardewValley.Util.BoundingBoxGroup::", new BoundingBoxGroupSerializer());
					hashtable.Add("StardewValley.Objects.CrabPot::", new CrabPotSerializer());
					hashtable.Add("StardewValley.Objects.Chest::", new ChestSerializer());
					hashtable.Add("StardewValley.Locations.Sewer::", new SewerSerializer());
					hashtable.Add("StardewValley.Locations.Mine::", new MineSerializer());
					hashtable.Add("StardewValley.Monsters.Monster::", new MonsterSerializer());
					hashtable.Add("StardewValley.Locations.BugLand::", new BugLandSerializer());
					hashtable.Add("StardewValley.GiftObjective::", new GiftObjectiveSerializer());
					hashtable.Add("StardewValley.Friendship::", new FriendshipSerializer());
					hashtable.Add("StardewValley.ServerPrivacy::", new ServerPrivacySerializer());
					hashtable.Add("StardewValley.Quests.GoSomewhereQuest::", new GoSomewhereQuestSerializer());
					hashtable.Add("StardewValley.Objects.StorageFurniture::", new StorageFurnitureSerializer());
					hashtable.Add("StardewValley.Objects.BedFurniture+BedType::", new BedTypeSerializer());
					hashtable.Add("StardewValley.LightSource::", new LightSourceSerializer());
					hashtable.Add("StardewValley.Monsters.RockGolem::", new RockGolemSerializer());
					hashtable.Add("StardewValley.MovieViewerLockEvent::", new MovieViewerLockEventSerializer());
					hashtable.Add("StardewValley.LocationName::", new LocationNameSerializer());
					hashtable.Add("DistanceToTarget::", new DistanceToTargetSerializer());
					hashtable.Add("StardewValley.Locations.IslandSouth::", new IslandSouthSerializer());
					hashtable.Add("StardewValley.Tools.Slingshot::", new SlingshotSerializer());
					hashtable.Add("StardewValley.Locations.IslandShrine::", new IslandShrineSerializer());
					hashtable.Add("StardewValley.LocalizedContentManager+LanguageCode::", new LanguageCodeSerializer());
					hashtable.Add("StardewValley.Network.NetAudio+SoundContext::", new SoundContextSerializer());
					hashtable.Add("StardewValley.Monsters.Bat::", new BatSerializer());
					hashtable.Add("StardewValley.PanEnchantment::", new PanEnchantmentSerializer());
					hashtable.Add("StardewValley.FarmerTeam+SleepAnnounceModes::", new SleepAnnounceModesSerializer());
					hashtable.Add("StardewValley.Tools.MilkPail::", new MilkPailSerializer());
					hashtable.Add("StardewValley.Quests.SocializeQuest::", new SocializeQuestSerializer());
					hashtable.Add("StardewValley.VampiricEnchantment::", new VampiricEnchantmentSerializer());
					hashtable.Add("StardewValley.Locations.IslandSecret::", new IslandSecretSerializer());
					hashtable.Add("StardewValley.IslandGemBird::", new IslandGemBirdSerializer());
					hashtable.Add("StardewValley.Objects.Sign::", new SignSerializer());
					hashtable.Add("StardewValley.CrusaderEnchantment::", new CrusaderEnchantmentSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.HoeDirt::", new HoeDirtSerializer());
					hashtable.Add("StardewValley.Monsters.DinoMonster::", new DinoMonsterSerializer());
					hashtable.Add("StardewValley.Locations.Railroad::", new RailroadSerializer());
					hashtable.Add("StardewValley.Characters.TrashBear::", new TrashBearSerializer());
					hashtable.Add("StardewValley.Locations.BusStop::", new BusStopSerializer());
					hashtable.Add("StardewValley.Minigames.NetLeaderboards::", new NetLeaderboardsSerializer());
					hashtable.Add("StardewValley.Monsters.DustSpirit::", new DustSpiritSerializer());
					hashtable.Add("StardewValley.PickaxeEnchantment::", new PickaxeEnchantmentSerializer());
					hashtable.Add("StardewValley.Characters.BotchedNetBool::", new BotchedNetBoolSerializer());
					hashtable.Add("StardewValley.BuildingPaintColor::", new BuildingPaintColorSerializer());
					hashtable.Add("StardewValley.Network.NetPosition::", new NetPositionSerializer());
					hashtable.Add("StardewValley.Locations.Caldera::", new CalderaSerializer());
					hashtable.Add("StardewValley.Objects.ItemPedestal::", new ItemPedestalSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.Quartz::", new QuartzSerializer());
					hashtable.Add("StardewValley.SaveGame::", new SaveGameSerializer());
					hashtable.Add("StardewValley.NumberSprite::", new NumberSpriteSerializer());
					hashtable.Add("StardewValley.Locations.IslandHut::", new IslandHutSerializer());
					hashtable.Add("StardewValley.Objects.Phone+PhoneCalls::", new PhoneCallsSerializer());
					hashtable.Add("StardewValley.Locations.MovieTheater+MovieStates::", new MovieStatesSerializer());
					hashtable.Add("StardewValley.BaseWeaponEnchantment::", new BaseWeaponEnchantmentSerializer());
					hashtable.Add("StardewValley.Monsters.Bug::", new BugSerializer());
					hashtable.Add("StardewValley.Locations.Bus::", new BusSerializer());
					hashtable.Add("StardewValley.SpecialOrder+QuestDuration::", new QuestDurationSerializer());
					hashtable.Add("StardewValley.Tools.Lantern::", new LanternSerializer());
					hashtable.Add("StardewValley.Characters.BotchedNetLong::", new BotchedNetLongSerializer());
					hashtable.Add("MouseCursor::", new MouseCursorSerializer());
					hashtable.Add("StardewValley.Locations.SeedShop::", new SeedShopSerializer());
					hashtable.Add("StardewValley.Quests.FishingQuest::", new FishingQuestSerializer());
					hashtable.Add("StardewValley.Locations.LibraryMuseum::", new LibraryMuseumSerializer());
					hashtable.Add("StardewValley.ArtifactSpotWatchActivity::", new ArtifactSpotWatchActivitySerializer());
					hashtable.Add("StardewValley.Objects.Hat+HairDrawType::", new HairDrawTypeSerializer());
					hashtable.Add("StardewValley.Locations.IslandNorth::", new IslandNorthSerializer());
					hashtable.Add("StardewValley.AnimalHouse::", new AnimalHouseSerializer());
					hashtable.Add("StardewValley.JKScoreObjective::", new JKScoreObjectiveSerializer());
					hashtable.Add("StardewValley.InstanceStatics::", new InstanceStaticsSerializer());
					hashtable.Add("StardewValley.ArchaeologistEnchantment::", new ArchaeologistEnchantmentSerializer());
					hashtable.Add("StardewValley.Objects.ItemDescription::", new ItemDescriptionSerializer());
					hashtable.Add("StardewValley.Util.LeftRightClickSpamInputSimulator::", new LeftRightClickSpamInputSimulatorSerializer());
					hashtable.Add("StardewValley.ShavingEnchantment::", new ShavingEnchantmentSerializer());
					hashtable.Add("StardewValley.Tools.Pan::", new PanSerializer());
					hashtable.Add("StardewValley.Objects.Chest+SpecialChestTypes::", new SpecialChestTypesSerializer());
					hashtable.Add("StardewValley.Tools.Raft::", new RaftSerializer());
					hashtable.Add("StardewValley.PriorityQueue::", new PriorityQueueSerializer());
					hashtable.Add("StardewValley.Monsters.Spiker::", new SpikerSerializer());
					hashtable.Add("LocationWeather::", new LocationWeatherSerializer());
					hashtable.Add("StardewValley.MailReward::", new MailRewardSerializer());
					hashtable.Add("StardewValley.Fence::", new FenceSerializer());
					hashtable.Add("StardewValley.Farm::", new FarmSerializer());
					hashtable.Add("StardewValley.Warp::", new WarpSerializer());
					hashtable.Add("StardewValley.Buildings.ShippingBin::", new ShippingBinSerializer());
					hashtable.Add("StardewValley.Monsters.LavaLurk+State::", new StateSerializer());
					hashtable.Add("StardewValley.Buildings.Barn::", new BarnSerializer());
					hashtable.Add("StardewValley.GameLocation::", new GameLocationSerializer());
					hashtable.Add("StardewValley.Chunk::", new ChunkSerializer());
					hashtable.Add("StardewValley.IslandGemBird+GemBirdType::", new GemBirdTypeSerializer());
					hashtable.Add("StardewValley.Objects.MiniJukebox::", new MiniJukeboxSerializer());
					hashtable.Add("StardewValley.Characters.BotchedNetInt::", new BotchedNetIntSerializer());
					hashtable.Add("StardewValley.MarriageDialogueReference::", new MarriageDialogueReferenceSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.Tree::", new TreeSerializer());
					hashtable.Add("StardewValley.MagicEnchantment::", new MagicEnchantmentSerializer());
					hashtable.Add("StardewValley.Objects.BreakableContainer::", new BreakableContainerSerializer());
					hashtable.Add("StardewValley.Network.NetFarmerRoot::", new NetFarmerCollectionSerializer1());
					hashtable.Add("StardewValley.Options+ItemStowingModes::", new ItemStowingModesSerializer());
					hashtable.Add("StardewValley.Monsters.Serpent::", new SerpentSerializer());
					hashtable.Add("StardewValley.ResetEventReward::", new ResetEventRewardSerializer());
					hashtable.Add("StardewValley.Locations.IslandFarmCave::", new IslandFarmCaveSerializer());
					hashtable.Add("StardewValley.Quests.SecretLostItemQuest::", new SecretLostItemQuestSerializer());
					hashtable.Add("FurnitureID::", new FurnitureIDSerializer());
					hashtable.Add("StardewValley.ClearingActivity::", new ClearingActivitySerializer());
					hashtable.Add("StardewValley.Quests.Quest::", new QuestSerializer());
					hashtable.Add("StardewValley.AutoHookEnchantment::", new AutoHookEnchantmentSerializer());
					hashtable.Add("StardewValley.WorldDate::", new WorldDateSerializer());
					hashtable.Add("StardewValley.SaveGame+SaveFixes::", new SaveFixesSerializer());
					hashtable.Add("StardewValley.InteriorDoorDictionary::", new InteriorDoorDictionarySerializer());
					hashtable.Add("StardewValley.Objects.Clothing::", new ClothingSerializer());
					hashtable.Add("StardewValley.Tools.Pickaxe::", new PickaxeSerializer());
					hashtable.Add("StardewValley.BuildingUpgrade::", new BuildingUpgradeSerializer());
					hashtable.Add("StardewValley.Monsters.RockCrab::", new RockCrabSerializer());
					hashtable.Add("StardewValley.Buildings.Coop::", new CoopSerializer());
					hashtable.Add("StardewValley.TopazEnchantment::", new TopazEnchantmentSerializer());
					hashtable.Add("StardewValley.Object+HoneyType::", new HoneyTypeSerializer());
					hashtable.Add("StardewValley.Quests.ItemHarvestQuest::", new ItemHarvestQuestSerializer());
					hashtable.Add("StardewValley.Objects.TV::", new TVSerializer());
					hashtable.Add("WalkDirection::", new WalkDirectionSerializer());
					hashtable.Add("StardewValley.Minigames.AbigailGame+GameKeys::", new GameKeysSerializer());
					hashtable.Add("StardewValley.BellsAndWhistles.SandDuggy::", new SandDuggySerializer());
					hashtable.Add("StardewValley.FriendshipReward::", new FriendshipRewardSerializer());
					hashtable.Add("StardewValley.CollectObjective::", new CollectObjectiveSerializer());
					hashtable.Add("StardewValley.EfficientToolEnchantment::", new EfficientToolEnchantmentSerializer());
					hashtable.Add("StardewValley.Vector2Serializer::", new Vector2SerializerSerializer());
					hashtable.Add("StardewValley.Network.NetDirection::", new NetDirectionSerializer());
					hashtable.Add("StardewValley.Characters.Child::", new ChildSerializer());
					hashtable.Add("StardewValley.ReachMineFloorObjective::", new ReachMineFloorObjectiveSerializer());
					hashtable.Add("StardewValley.FarmerRenderer::", new FarmerRendererSerializer());
					hashtable.Add("StardewValley.Locations.WizardHouse::", new WizardHouseSerializer());
					hashtable.Add("StardewValley.Locations.FarmCave::", new FarmCaveSerializer());
					hashtable.Add("StardewValley.MailActivity::", new MailActivitySerializer());
					hashtable.Add("StardewValley.MapSeat::", new MapSeatSerializer());
					hashtable.Add("StardewValley.Tools.Wand::", new WandSerializer());
					hashtable.Add("StardewValley.Monsters.ShadowShaman::", new ShadowShamanSerializer());
					hashtable.Add("StardewValley.NonInstancedStatic::", new NonInstancedStaticSerializer());
					hashtable.Add("StardewValley.Tools.Seeds::", new SeedsSerializer());
					hashtable.Add("StardewValley.JadeEnchantment::", new JadeEnchantmentSerializer());
					hashtable.Add("StardewValley.Monsters.ShadowBrute::", new ShadowBruteSerializer());
					hashtable.Add("StardewValley.PreservingEnchantment::", new PreservingEnchantmentSerializer());
					hashtable.Add("StardewValley.Objects.ObjectFactory::", new ObjectFactorySerializer());
					hashtable.Add("StardewValley.NPC::", new NPCSerializer());
					hashtable.Add("StardewValley.Monsters.HotHead::", new HotHeadSerializer());
					hashtable.Add("StardewValley.Noise::", new NoiseSerializer());
					hashtable.Add("StardewValley.Monsters.Duggy::", new DuggySerializer());
					hashtable.Add("StardewValley.Characters.Cat::", new CatSerializer());
					hashtable.Add("StardewValley.FarmerTeam+RemoteBuildingPermissions::", new RemoteBuildingPermissionsSerializer());
					hashtable.Add("StardewValley.Util.SynchronizedShopStock::", new SynchronizedShopStockSerializer());
					hashtable.Add("StardewValley.Quests.SlayMonsterQuest::", new SlayMonsterQuestSerializer());
					hashtable.Add("StardewValley.InputButton::", new InputButtonSerializer());
					hashtable.Add("StardewValley.Vector2Reader::", new Vector2ReaderSerializer());
					hashtable.Add("StardewValley.NetIntIntArrayDictionary::", new NetIntIntArrayDictionarySerializer());
					hashtable.Add("StardewValley.Objects.IndoorPot::", new IndoorPotSerializer());
					hashtable.Add("TapState::", new TapStateSerializer());
					hashtable.Add("StardewValley.Tools.Hoe::", new HoeSerializer());
					hashtable.Add("StardewValley.Monsters.Mummy::", new MummySerializer());
					hashtable.Add("StardewValley.FriendshipStatus::", new FriendshipStatusSerializer());
					hashtable.Add("StardewValley.Monsters.AngryRoger::", new AngryRogerSerializer());
					hashtable.Add("StardewValley.Util.ToolSpamInputSimulator::", new ToolSpamInputSimulatorSerializer());
					hashtable.Add("StardewValley.BaseEnchantment::", new BaseEnchantmentSerializer());
					hashtable.Add("StardewValley.TreeActivity::", new TreeActivitySerializer());
					hashtable.Add("StardewValley.Objects.Cask::", new CaskSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.ResourceClump::", new ResourceClumpSerializer());
					hashtable.Add("StardewValley.Character::", new CharacterSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.GiantCrop::", new GiantCropSerializer());
					hashtable.Add("StardewValley.Locations.BathHousePool::", new BathHousePoolSerializer());
					hashtable.Add("StardewValley.MovieInvitation::", new MovieInvitationSerializer());
					hashtable.Add("DontLoadDefaultSetting::", new DontLoadDefaultSettingSerializer());
					hashtable.Add("StardewValley.Locations.IslandEast::", new IslandEastSerializer());
					hashtable.Add("StardewValley.HaymakerEnchantment::", new HaymakerEnchantmentSerializer());
					hashtable.Add("StardewValley.Buildings.Building::", new BuildingSerializer());
					hashtable.Add("StardewValley.Objects.FishTankFurniture::", new FishTankFurnitureSerializer());
					hashtable.Add("StardewValley.Object::", new ObjectSerializer());
					hashtable.Add("StardewValley.Util.SynchronizedShopStock+SynchedShop::", new SynchedShopSerializer());
					hashtable.Add("StardewValley.RainDrop::", new RainDropSerializer());
					hashtable.Add("TinyTween.TweenState::", new TweenStateSerializer());
					hashtable.Add("StardewValley.AquamarineEnchantment::", new AquamarineEnchantmentSerializer());
					hashtable.Add("StardewValley.GalaxySoulEnchantment::", new GalaxySoulEnchantmentSerializer());
					hashtable.Add("StardewValley.Locations.Forest::", new ForestSerializer());
					hashtable.Add("StardewValley.StartupPreferences::", new StartupPreferencesSerializer());
					hashtable.Add("StardewValley.Quests.ItemDeliveryQuest::", new ItemDeliveryQuestSerializer());
					hashtable.Add("StardewValley.GiftObjective+LikeLevels::", new LikeLevelsSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.LargeTerrainFeature::", new LargeTerrainFeatureSerializer());
					hashtable.Add("StardewValley.BellsAndWhistles.SandDuggy+State::", new StateSerializer1());
					hashtable.Add("StardewValley.Locations.ManorHouse::", new ManorHouseSerializer());
					hashtable.Add("StardewValley.Network.NetLocationRef::", new NetLocationRefSerializer());
					hashtable.Add("StardewValley.Tools.MeleeWeapon::", new MeleeWeaponSerializer());
					hashtable.Add("StardewValley.MasterEnchantment::", new MasterEnchantmentSerializer());
					hashtable.Add("TinyTween.StopBehavior::", new StopBehaviorSerializer());
					hashtable.Add("StardewValley.Characters.Dog::", new DogSerializer());
					hashtable.Add("StardewValley.Torch::", new TorchSerializer());
					hashtable.Add("StardewValley.Tools.WateringCan::", new WateringCanSerializer());
					hashtable.Add("StardewValley.Locations.DecorationFacade::", new NetDirectionSerializer1());
					hashtable.Add("StardewValley.Farm+LightningStrikeEvent::", new LightningStrikeEventSerializer());
					hashtable.Add("StardewValley.Projectiles.DebuffingProjectile::", new DebuffingProjectileSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.TerrainFeature::", new TerrainFeatureSerializer());
					hashtable.Add("StardewValley.MilkPailEnchantment::", new MilkPailEnchantmentSerializer());
					hashtable.Add("StardewValley.AmethystEnchantment::", new AmethystEnchantmentSerializer());
					hashtable.Add("StardewValley.Options+GamepadModes::", new GamepadModesSerializer());
					hashtable.Add("StardewValley.Locations.CommunityCenter::", new CommunityCenterSerializer());
					hashtable.Add("StardewValley.Objects.SpecialItem::", new SpecialItemSerializer());
					hashtable.Add("StardewValley.Network.OverlaidDictionary::", new OverlaidDictionarySerializer());
					hashtable.Add("StardewValley.MoneyReward::", new MoneyRewardSerializer());
					hashtable.Add("StardewValley.MineChestType::", new MineChestTypeSerializer());
					hashtable.Add("StardewValley.Minigames.NetLeaderboardsEntry::", new NetLeaderboardsEntrySerializer());
					hashtable.Add("StardewValley.Objects.SwitchFloor::", new SwitchFloorSerializer());
					hashtable.Add("StardewValley.Projectiles.BasicProjectile::", new BasicProjectileSerializer());
					hashtable.Add("StardewValley.LocalMultiplayer::", new LocalMultiplayerSerializer());
					hashtable.Add("StardewValley.Objects.Phone::", new PhoneSerializer());
					hashtable.Add("StardewValley.ShearsEnchantment::", new ShearsEnchantmentSerializer());
					hashtable.Add("StardewValley.RubyEnchantment::", new RubyEnchantmentSerializer());
					hashtable.Add("StardewValley.Monsters.ShadowGuy::", new ShadowGuySerializer());
					hashtable.Add("StardewValley.Tools.MagnifyingGlass::", new MagnifyingGlassSerializer());
					hashtable.Add("StardewValley.ArtfulEnchantment::", new ArtfulEnchantmentSerializer());
					hashtable.Add("StardewValley.Locations.DecoratableLocation::", new DecoratableLocationSerializer());
					hashtable.Add("StardewValley.SwiftToolEnchantment::", new SwiftToolEnchantmentSerializer());
					hashtable.Add("StardewValley.Network.NetCharacterRef::", new NetCharacterRefSerializer());
					hashtable.Add("StardewValley.WateringCanEnchantment::", new WateringCanEnchantmentSerializer());
					hashtable.Add("StardewValley.GenerousEnchantment::", new GenerousEnchantmentSerializer());
					hashtable.Add("StardewValley.Vector2Writer::", new Vector2WriterSerializer());
					hashtable.Add("StardewValley.Locations.IslandFieldOffice::", new IslandFieldOfficeSerializer());
					hashtable.Add("StardewValley.Quests.NetDescriptionElementRef::", new NetDescriptionElementRefSerializer());
					hashtable.Add("StardewValley.EmeraldEnchantment::", new EmeraldEnchantmentSerializer());
					hashtable.Add("StardewValley.NutDropRequest::", new NutDropRequestSerializer());
					hashtable.Add("StardewValley.Locations.IslandWest::", new IslandWestSerializer());
					hashtable.Add("StardewValley.InteriorDoor::", new InteriorDoorSerializer());
					hashtable.Add("StardewValley.Objects.Furniture::", new FurnitureSerializer());
					hashtable.Add("StardewValley.NetLogger::", new NetLoggerSerializer());
					hashtable.Add("StardewValley.StackDrawType::", new StackDrawTypeSerializer());
					hashtable.Add("WaterTiles+WaterTileData::", new WaterTileDataSerializer());
					hashtable.Add("StardewValley.Locations.MovieTheater::", new MovieTheaterSerializer());
					hashtable.Add("StardewValley.Locations.IslandLocation::", new IslandLocationSerializer());
					hashtable.Add("StardewValley.Monsters.LavaCrab::", new LavaCrabSerializer());
					hashtable.Add("StardewValley.DeliverObjective::", new DeliverObjectiveSerializer());
					hashtable.Add("StardewValley.Monsters.Skeleton::", new SkeletonSerializer());
					hashtable.Add("StardewValley.ChangeType::", new ChangeTypeSerializer());
					hashtable.Add("StardewValley.Tools.ToolFactory::", new ToolFactorySerializer());
					hashtable.Add("StardewValley.StartMovieEvent::", new StartMovieEventSerializer());
					hashtable.Add("StardewValley.Locations.BuildableGameLocation::", new BuildableGameLocationSerializer());
					hashtable.Add("StardewValley.Monsters.BigSlime::", new BigSlimeSerializer());
					hashtable.Add("StardewValley.LightSource+LightContext::", new LightContextSerializer());
					hashtable.Add("StardewValley.Monsters.DwarvishSentry::", new DwarvishSentrySerializer());
					hashtable.Add("StardewValley.Network.NetFarmerCollection::", new NetFarmerCollectionSerializer());
					hashtable.Add("StardewValley.Locations.MineShaft::", new MineShaftSerializer());
					hashtable.Add("StardewValley.GameLocation+LocationContext::", new LocationContextSerializer());
					hashtable.Add("StardewValley.Locations.MermaidHouse::", new MermaidHouseSerializer());
					hashtable.Add("StardewValley.Object+PreserveType::", new PreserveTypeSerializer());
					hashtable.Add("StardewValley.Locations.Club::", new ClubSerializer());
					hashtable.Add("WeaponControl::", new WeaponControlSerializer());
					hashtable.Add("StardewValley.Objects.BedFurniture::", new BedFurnitureSerializer());
					hashtable.Add("StardewValley.Locations.AdventureGuild::", new AdventureGuildSerializer());
					hashtable.Add("StardewValley.Tools.Stackable::", new StackableSerializer());
					hashtable.Add("StardewValley.Locations.IslandWestCave1::", new IslandWestCave1Serializer());
					hashtable.Add("StardewValley.Monsters.Fly::", new FlySerializer());
					hashtable.Add("TinyTween.FloatTween::", new FloatTweenSerializer());
					hashtable.Add("StardewValley.Locations.IslandWestCave1+CaveCrystal::", new CaveCrystalSerializer());
					hashtable.Add("StardewValley.Characters.Junimo::", new JunimoSerializer());
					hashtable.Add("StardewValley.Monsters.Leaper::", new LeaperSerializer());
					hashtable.Add("StardewValley.Shed::", new ShedSerializer());
					hashtable.Add("StardewValley.Buildings.JunimoHut::", new JunimoHutSerializer());
					hashtable.Add("StardewValley.Locations.VolcanoDungeon+TileNeighbors::", new TileNeighborsSerializer());
					hashtable.Add("StardewValley.Monsters.Shooter::", new ShooterSerializer());
					hashtable.Add("StardewValley.Locations.Cabin::", new CabinSerializer());
					hashtable.Add("StardewValley.Locations.Mountain::", new MountainSerializer());
					hashtable.Add("StardewValley.Monsters.LavaLurk::", new LavaLurkSerializer());
					hashtable.Add("StardewValley.HoeEnchantment::", new HoeEnchantmentSerializer());
					hashtable.Add("StardewValley.Objects.WoodChipper::", new WoodChipperSerializer());
					hashtable.Add("StardewValley.Tools.GenericTool::", new GenericToolSerializer());
					hashtable.Add("StardewValley.WeatherDebris::", new WeatherDebrisSerializer());
					hashtable.Add("StardewValley.Buildings.Mill::", new MillSerializer());
					hashtable.Add("StardewValley.Locations.BoatTunnel+TunnelAnimationState::", new TunnelAnimationStateSerializer());
					hashtable.Add("StardewValley.Locations.MineInfo::", new MineInfoSerializer());
					hashtable.Add("StardewValley.Locations.Desert::", new DesertSerializer());
					hashtable.Add("StardewValley.Tools.FishingRod::", new FishingRodSerializer());
					hashtable.Add("StardewValley.Objects.Ring::", new RingSerializer());
					hashtable.Add("StardewValley.BugKillerEnchantment::", new BugKillerEnchantmentSerializer());
					hashtable.Add("StardewValley.FarmerPair::", new FarmerPairSerializer());
					hashtable.Add("StardewValley.Characters.Horse::", new HorseSerializer());
					hashtable.Add("StardewValley.Network.NetDancePartner::", new NetDancePartnerSerializer());
					hashtable.Add("StardewValley.Objects.Hat::", new HatSerializer());
					hashtable.Add("StardewValley.Locations.Town::", new TownSerializer());
					hashtable.Add("StardewValley.DiamondEnchantment::", new DiamondEnchantmentSerializer());
					hashtable.Add("ObjectID::", new ObjectIDSerializer());
					hashtable.Add("StardewValley.Tools.Axe::", new AxeSerializer());
					hashtable.Add("StardewValley.TerrainFeatures.Flooring::", new FlooringSerializer());
					hashtable.Add("BigCraftableID::", new BigCraftableIDSerializer());
					hashtable.Add("StardewValley.ShipObjective::", new ShipObjectiveSerializer());
					hashtable.Add("StardewValley.Locations.IslandSouthEast::", new IslandSouthEastSerializer());
					hashtable.Add("TutorialShopLocation::", new TutorialShopLocationSerializer());
					hashtable.Add("StardewValley.Monsters.DinoMonster+AttackState::", new AttackStateSerializer());
					hashtable.Add("StardewValley.Locations.JojaMart::", new JojaMartSerializer());
					hashtable.Add("StardewValley.Item::", new ItemSerializer());
					hashtable.Add("StardewValley.Monsters.MetalHead::", new MetalHeadSerializer());
					hashtable.Add("StardewValley.Characters.Pet::", new PetSerializer());
					hashtable.Add("StardewValley.Locations.Cellar::", new CellarSerializer());
					hashtable.Add("StardewValley.Options::", new OptionsSerializer());
					hashtable.Add("StardewValley.Quests.ResourceCollectionQuest::", new ResourceCollectionQuestSerializer());
					hashtable.Add("StardewValley.Network.NetBundles::", new NetBundlesSerializer());
					hashtable.Add("StardewValley.Network.NetWitnessedLock::", new NetWitnessedLockSerializer());
					hashtable.Add("StardewValley.GemsReward::", new GemsRewardSerializer());
					hashtable.Add("StardewValley.Locations.Submarine::", new SubmarineSerializer());
					hashtable.Add("StardewValley.Locations.Woods::", new WoodsSerializer());
					hashtable.Add("StardewValley.Objects.CombinedRing::", new CombinedRingSerializer());
					hashtable.Add("StardewValley.Monsters.Ghost+GhostVariant::", new GhostVariantSerializer());
					hashtable.Add("StardewValley.Network.NetNPCRef::", new NetNPCRefSerializer());
					hashtable.Add("StardewValley.Monsters.Grub::", new GrubSerializer());
					hashtable.Add("StardewValley.Objects.TankFish+FishType::", new FishTypeSerializer());
					hashtable.Add("StardewValley.Locations.AbandonedJojaMart::", new AbandonedJojaMartSerializer());
					hashtable.Add("StardewValley.Tools.Sword::", new SwordSerializer());
					hashtable.Add("StardewValley.InstancedStatic::", new InstancedStaticSerializer());
					hashtable.Add("StardewValley.NameSelect::", new NameSelectSerializer());
					hashtable.Add("tutorialType::", new tutorialTypeSerializer());
					hashtable.Add("TinyTween.ColorTween::", new ColorTweenSerializer());
					hashtable.Add("StardewValley.FarmAnimal::", new FarmAnimalSerializer());
					hashtable.Add("StardewValley.OrderReward::", new OrderRewardSerializer());
					hashtable.Add("StardewValley.Tools.Blueprints::", new BlueprintsSerializer());
					hashtable.Add("StardewValley.Quests.CraftingQuest::", new CraftingQuestSerializer());
					hashtable.Add("StardewValley.Locations.IslandSouthEastCave::", new IslandSouthEastCaveSerializer());
					if (typedSerializers == null)
					{
						typedSerializers = hashtable;
					}
				}
				return typedSerializers;
			}
		}

		public override bool CanSerialize(Type type)
		{
			if (type == typeof(WalkDirection))
			{
				return true;
			}
			if (type == typeof(ObjectID))
			{
				return true;
			}
			if (type == typeof(BigCraftableID))
			{
				return true;
			}
			if (type == typeof(FurnitureID))
			{
				return true;
			}
			if (type == typeof(MouseCursor))
			{
				return true;
			}
			if (type == typeof(TapState))
			{
				return true;
			}
			if (type == typeof(DistanceToTarget))
			{
				return true;
			}
			if (type == typeof(WeaponControl))
			{
				return true;
			}
			if (type == typeof(tutorialType))
			{
				return true;
			}
			if (type == typeof(TutorialShopLocation))
			{
				return true;
			}
			if (type == typeof(LocationWeather))
			{
				return true;
			}
			if (type == typeof(DontLoadDefaultSetting))
			{
				return true;
			}
			if (type == typeof(WaterTiles.WaterTileData))
			{
				return true;
			}
			if (type == typeof(TweenState))
			{
				return true;
			}
			if (type == typeof(StopBehavior))
			{
				return true;
			}
			if (type == typeof(FloatTween))
			{
				return true;
			}
			if (type == typeof(Vector2Tween))
			{
				return true;
			}
			if (type == typeof(Vector3Tween))
			{
				return true;
			}
			if (type == typeof(Vector4Tween))
			{
				return true;
			}
			if (type == typeof(ColorTween))
			{
				return true;
			}
			if (type == typeof(QuaternionTween))
			{
				return true;
			}
			if (type == typeof(BuildingPainter))
			{
				return true;
			}
			if (type == typeof(BuildingPaintColor))
			{
				return true;
			}
			if (type == typeof(HouseRenovation.AnimationType))
			{
				return true;
			}
			if (type == typeof(IslandGemBird))
			{
				return true;
			}
			if (type == typeof(IslandGemBird.GemBirdType))
			{
				return true;
			}
			if (type == typeof(InstanceStatics))
			{
				return true;
			}
			if (type == typeof(InstancedStatic))
			{
				return true;
			}
			if (type == typeof(NonInstancedStatic))
			{
				return true;
			}
			if (type == typeof(LocalMultiplayer))
			{
				return true;
			}
			if (type == typeof(LocationName))
			{
				return true;
			}
			if (type == typeof(Options))
			{
				return true;
			}
			if (type == typeof(Options.ItemStowingModes))
			{
				return true;
			}
			if (type == typeof(Options.GamepadModes))
			{
				return true;
			}
			if (type == typeof(MapSeat))
			{
				return true;
			}
			if (type == typeof(SpecialOrder))
			{
				return true;
			}
			if (type == typeof(SpecialOrder.QuestState))
			{
				return true;
			}
			if (type == typeof(SpecialOrder.QuestDuration))
			{
				return true;
			}
			if (type == typeof(OrderObjective))
			{
				return true;
			}
			if (type == typeof(CollectObjective))
			{
				return true;
			}
			if (type == typeof(DonateObjective))
			{
				return true;
			}
			if (type == typeof(ShipObjective))
			{
				return true;
			}
			if (type == typeof(SlayObjective))
			{
				return true;
			}
			if (type == typeof(FishObjective))
			{
				return true;
			}
			if (type == typeof(DeliverObjective))
			{
				return true;
			}
			if (type == typeof(GiftObjective))
			{
				return true;
			}
			if (type == typeof(GiftObjective.LikeLevels))
			{
				return true;
			}
			if (type == typeof(ReachMineFloorObjective))
			{
				return true;
			}
			if (type == typeof(JKScoreObjective))
			{
				return true;
			}
			if (type == typeof(OrderReward))
			{
				return true;
			}
			if (type == typeof(MailReward))
			{
				return true;
			}
			if (type == typeof(ResetEventReward))
			{
				return true;
			}
			if (type == typeof(GemsReward))
			{
				return true;
			}
			if (type == typeof(MoneyReward))
			{
				return true;
			}
			if (type == typeof(FriendshipReward))
			{
				return true;
			}
			if (type == typeof(BaseEnchantment))
			{
				return true;
			}
			if (type == typeof(BaseWeaponEnchantment))
			{
				return true;
			}
			if (type == typeof(MagicEnchantment))
			{
				return true;
			}
			if (type == typeof(AmethystEnchantment))
			{
				return true;
			}
			if (type == typeof(TopazEnchantment))
			{
				return true;
			}
			if (type == typeof(AquamarineEnchantment))
			{
				return true;
			}
			if (type == typeof(JadeEnchantment))
			{
				return true;
			}
			if (type == typeof(DiamondEnchantment))
			{
				return true;
			}
			if (type == typeof(GalaxySoulEnchantment))
			{
				return true;
			}
			if (type == typeof(RubyEnchantment))
			{
				return true;
			}
			if (type == typeof(EmeraldEnchantment))
			{
				return true;
			}
			if (type == typeof(ArtfulEnchantment))
			{
				return true;
			}
			if (type == typeof(HaymakerEnchantment))
			{
				return true;
			}
			if (type == typeof(BugKillerEnchantment))
			{
				return true;
			}
			if (type == typeof(VampiricEnchantment))
			{
				return true;
			}
			if (type == typeof(CrusaderEnchantment))
			{
				return true;
			}
			if (type == typeof(PickaxeEnchantment))
			{
				return true;
			}
			if (type == typeof(HoeEnchantment))
			{
				return true;
			}
			if (type == typeof(AxeEnchantment))
			{
				return true;
			}
			if (type == typeof(FishingRodEnchantment))
			{
				return true;
			}
			if (type == typeof(WateringCanEnchantment))
			{
				return true;
			}
			if (type == typeof(PanEnchantment))
			{
				return true;
			}
			if (type == typeof(MilkPailEnchantment))
			{
				return true;
			}
			if (type == typeof(ShearsEnchantment))
			{
				return true;
			}
			if (type == typeof(PowerfulEnchantment))
			{
				return true;
			}
			if (type == typeof(EfficientToolEnchantment))
			{
				return true;
			}
			if (type == typeof(SwiftToolEnchantment))
			{
				return true;
			}
			if (type == typeof(ReachingToolEnchantment))
			{
				return true;
			}
			if (type == typeof(BottomlessEnchantment))
			{
				return true;
			}
			if (type == typeof(ShavingEnchantment))
			{
				return true;
			}
			if (type == typeof(ArchaeologistEnchantment))
			{
				return true;
			}
			if (type == typeof(GenerousEnchantment))
			{
				return true;
			}
			if (type == typeof(MasterEnchantment))
			{
				return true;
			}
			if (type == typeof(AutoHookEnchantment))
			{
				return true;
			}
			if (type == typeof(PreservingEnchantment))
			{
				return true;
			}
			if (type == typeof(StackDrawType))
			{
				return true;
			}
			if (type == typeof(MovieInvitation))
			{
				return true;
			}
			if (type == typeof(StartMovieEvent))
			{
				return true;
			}
			if (type == typeof(MovieViewerLockEvent))
			{
				return true;
			}
			if (type == typeof(WorldDate))
			{
				return true;
			}
			if (type == typeof(FriendshipStatus))
			{
				return true;
			}
			if (type == typeof(Friendship))
			{
				return true;
			}
			if (type == typeof(LocalizedContentManager.LanguageCode))
			{
				return true;
			}
			if (type == typeof(AnimalHouse))
			{
				return true;
			}
			if (type == typeof(BuildingUpgrade))
			{
				return true;
			}
			if (type == typeof(Character))
			{
				return true;
			}
			if (type == typeof(Chunk))
			{
				return true;
			}
			if (type == typeof(FarmerRenderer))
			{
				return true;
			}
			if (type == typeof(Shed))
			{
				return true;
			}
			if (type == typeof(SlimeHutch))
			{
				return true;
			}
			if (type == typeof(Farm))
			{
				return true;
			}
			if (type == typeof(Farm.LightningStrikeEvent))
			{
				return true;
			}
			if (type == typeof(FarmAnimal))
			{
				return true;
			}
			if (type == typeof(NetLogger))
			{
				return true;
			}
			if (type == typeof(Noise))
			{
				return true;
			}
			if (type == typeof(NumberSprite))
			{
				return true;
			}
			if (type == typeof(Fence))
			{
				return true;
			}
			if (type == typeof(Item))
			{
				return true;
			}
			if (type == typeof(ModDataDictionary))
			{
				return true;
			}
			if (type == typeof(LightSource))
			{
				return true;
			}
			if (type == typeof(LightSource.LightContext))
			{
				return true;
			}
			if (type == typeof(Torch))
			{
				return true;
			}
			if (type == typeof(InputButton))
			{
				return true;
			}
			if (type == typeof(ServerPrivacy))
			{
				return true;
			}
			if (type == typeof(NameSelect))
			{
				return true;
			}
			if (type == typeof(PriorityQueue))
			{
				return true;
			}
			if (type == typeof(Crop))
			{
				return true;
			}
			if (type == typeof(Farmer))
			{
				return true;
			}
			if (type == typeof(NetIntIntArrayDictionary))
			{
				return true;
			}
			if (type == typeof(HairStyleMetadata))
			{
				return true;
			}
			if (type == typeof(FarmerPair))
			{
				return true;
			}
			if (type == typeof(NutDropRequest))
			{
				return true;
			}
			if (type == typeof(FarmerTeam.RemoteBuildingPermissions))
			{
				return true;
			}
			if (type == typeof(FarmerTeam.SleepAnnounceModes))
			{
				return true;
			}
			if (type == typeof(InteriorDoor))
			{
				return true;
			}
			if (type == typeof(InteriorDoorDictionary))
			{
				return true;
			}
			if (type == typeof(GameLocation))
			{
				return true;
			}
			if (type == typeof(GameLocation.LocationContext))
			{
				return true;
			}
			if (type == typeof(NPC))
			{
				return true;
			}
			if (type == typeof(MarriageDialogueReference))
			{
				return true;
			}
			if (type == typeof(FarmActivity))
			{
				return true;
			}
			if (type == typeof(ArtifactSpotWatchActivity))
			{
				return true;
			}
			if (type == typeof(CropWatchActivity))
			{
				return true;
			}
			if (type == typeof(FlowerWatchActivity))
			{
				return true;
			}
			if (type == typeof(ShrineActivity))
			{
				return true;
			}
			if (type == typeof(MailActivity))
			{
				return true;
			}
			if (type == typeof(TreeActivity))
			{
				return true;
			}
			if (type == typeof(ClearingActivity))
			{
				return true;
			}
			if (type == typeof(StardewValley.Object))
			{
				return true;
			}
			if (type == typeof(StardewValley.Object.PreserveType))
			{
				return true;
			}
			if (type == typeof(StardewValley.Object.HoneyType))
			{
				return true;
			}
			if (type == typeof(RainDrop))
			{
				return true;
			}
			if (type == typeof(MineChestType))
			{
				return true;
			}
			if (type == typeof(Vector2Reader))
			{
				return true;
			}
			if (type == typeof(Vector2Writer))
			{
				return true;
			}
			if (type == typeof(Vector2Serializer))
			{
				return true;
			}
			if (type == typeof(SaveGame))
			{
				return true;
			}
			if (type == typeof(SaveGame.SaveFixes))
			{
				return true;
			}
			if (type == typeof(ChangeType))
			{
				return true;
			}
			if (type == typeof(StartupPreferences))
			{
				return true;
			}
			if (type == typeof(Stats))
			{
				return true;
			}
			if (type == typeof(Tool))
			{
				return true;
			}
			if (type == typeof(Warp))
			{
				return true;
			}
			if (type == typeof(WeatherDebris))
			{
				return true;
			}
			if (type == typeof(Bush))
			{
				return true;
			}
			if (type == typeof(CosmeticPlant))
			{
				return true;
			}
			if (type == typeof(Flooring))
			{
				return true;
			}
			if (type == typeof(FruitTree))
			{
				return true;
			}
			if (type == typeof(GiantCrop))
			{
				return true;
			}
			if (type == typeof(HoeDirt))
			{
				return true;
			}
			if (type == typeof(LargeTerrainFeature))
			{
				return true;
			}
			if (type == typeof(ResourceClump))
			{
				return true;
			}
			if (type == typeof(Grass))
			{
				return true;
			}
			if (type == typeof(Quartz))
			{
				return true;
			}
			if (type == typeof(TerrainFeature))
			{
				return true;
			}
			if (type == typeof(Tree))
			{
				return true;
			}
			if (type == typeof(BasicProjectile))
			{
				return true;
			}
			if (type == typeof(DebuffingProjectile))
			{
				return true;
			}
			if (type == typeof(Projectile))
			{
				return true;
			}
			if (type == typeof(GenericTool))
			{
				return true;
			}
			if (type == typeof(Axe))
			{
				return true;
			}
			if (type == typeof(Blueprints))
			{
				return true;
			}
			if (type == typeof(Pan))
			{
				return true;
			}
			if (type == typeof(FishingRod))
			{
				return true;
			}
			if (type == typeof(Lantern))
			{
				return true;
			}
			if (type == typeof(MagnifyingGlass))
			{
				return true;
			}
			if (type == typeof(MeleeWeapon))
			{
				return true;
			}
			if (type == typeof(MilkPail))
			{
				return true;
			}
			if (type == typeof(Pickaxe))
			{
				return true;
			}
			if (type == typeof(Hoe))
			{
				return true;
			}
			if (type == typeof(Raft))
			{
				return true;
			}
			if (type == typeof(Seeds))
			{
				return true;
			}
			if (type == typeof(Shears))
			{
				return true;
			}
			if (type == typeof(Slingshot))
			{
				return true;
			}
			if (type == typeof(Stackable))
			{
				return true;
			}
			if (type == typeof(Sword))
			{
				return true;
			}
			if (type == typeof(ToolDescription))
			{
				return true;
			}
			if (type == typeof(ToolFactory))
			{
				return true;
			}
			if (type == typeof(Wand))
			{
				return true;
			}
			if (type == typeof(WateringCan))
			{
				return true;
			}
			if (type == typeof(BoundingBoxGroup))
			{
				return true;
			}
			if (type == typeof(ToolSpamInputSimulator))
			{
				return true;
			}
			if (type == typeof(LeftRightClickSpamInputSimulator))
			{
				return true;
			}
			if (type == typeof(SynchronizedShopStock))
			{
				return true;
			}
			if (type == typeof(SynchronizedShopStock.SynchedShop))
			{
				return true;
			}
			if (type == typeof(SecretLostItemQuest))
			{
				return true;
			}
			if (type == typeof(NetDescriptionElementRef))
			{
				return true;
			}
			if (type == typeof(NetDescriptionElementList))
			{
				return true;
			}
			if (type == typeof(DescriptionElement))
			{
				return true;
			}
			if (type == typeof(LostItemQuest))
			{
				return true;
			}
			if (type == typeof(ItemHarvestQuest))
			{
				return true;
			}
			if (type == typeof(GoSomewhereQuest))
			{
				return true;
			}
			if (type == typeof(CraftingQuest))
			{
				return true;
			}
			if (type == typeof(SocializeQuest))
			{
				return true;
			}
			if (type == typeof(FishingQuest))
			{
				return true;
			}
			if (type == typeof(SlayMonsterQuest))
			{
				return true;
			}
			if (type == typeof(ResourceCollectionQuest))
			{
				return true;
			}
			if (type == typeof(ItemDeliveryQuest))
			{
				return true;
			}
			if (type == typeof(Quest))
			{
				return true;
			}
			if (type == typeof(BedFurniture))
			{
				return true;
			}
			if (type == typeof(BedFurniture.BedType))
			{
				return true;
			}
			if (type == typeof(FishTankFurniture))
			{
				return true;
			}
			if (type == typeof(FishTankFurniture.FishTankCategories))
			{
				return true;
			}
			if (type == typeof(TankFish.FishType))
			{
				return true;
			}
			if (type == typeof(ItemPedestal))
			{
				return true;
			}
			if (type == typeof(Phone))
			{
				return true;
			}
			if (type == typeof(Phone.PhoneCalls))
			{
				return true;
			}
			if (type == typeof(StorageFurniture))
			{
				return true;
			}
			if (type == typeof(WoodChipper))
			{
				return true;
			}
			if (type == typeof(Clothing))
			{
				return true;
			}
			if (type == typeof(Clothing.ClothesType))
			{
				return true;
			}
			if (type == typeof(IndoorPot))
			{
				return true;
			}
			if (type == typeof(MiniJukebox))
			{
				return true;
			}
			if (type == typeof(Sign))
			{
				return true;
			}
			if (type == typeof(Workbench))
			{
				return true;
			}
			if (type == typeof(Boots))
			{
				return true;
			}
			if (type == typeof(BreakableContainer))
			{
				return true;
			}
			if (type == typeof(Cask))
			{
				return true;
			}
			if (type == typeof(Chest))
			{
				return true;
			}
			if (type == typeof(Chest.SpecialChestTypes))
			{
				return true;
			}
			if (type == typeof(ColoredObject))
			{
				return true;
			}
			if (type == typeof(SpecialItem))
			{
				return true;
			}
			if (type == typeof(TV))
			{
				return true;
			}
			if (type == typeof(Wallpaper))
			{
				return true;
			}
			if (type == typeof(Furniture))
			{
				return true;
			}
			if (type == typeof(CrabPot))
			{
				return true;
			}
			if (type == typeof(Hat))
			{
				return true;
			}
			if (type == typeof(Hat.HairDrawType))
			{
				return true;
			}
			if (type == typeof(ItemDescription))
			{
				return true;
			}
			if (type == typeof(ObjectFactory))
			{
				return true;
			}
			if (type == typeof(Ring))
			{
				return true;
			}
			if (type == typeof(CombinedRing))
			{
				return true;
			}
			if (type == typeof(SwitchFloor))
			{
				return true;
			}
			if (type == typeof(IncomingMessage))
			{
				return true;
			}
			if (type == typeof(NetAudio.SoundContext))
			{
				return true;
			}
			if (type == typeof(NetDirection))
			{
				return true;
			}
			if (type == typeof(NetLocationRef))
			{
				return true;
			}
			if (type == typeof(NetPosition))
			{
				return true;
			}
			if (type == typeof(NetBundles))
			{
				return true;
			}
			if (type == typeof(NetFarmerCollection))
			{
				return true;
			}
			if (type == typeof(NetMutex))
			{
				return true;
			}
			if (type == typeof(NetWitnessedLock))
			{
				return true;
			}
			if (type == typeof(NetFarmerRoot))
			{
				return true;
			}
			if (type == typeof(NetNPCRef))
			{
				return true;
			}
			if (type == typeof(NetCharacterRef))
			{
				return true;
			}
			if (type == typeof(NetDancePartner))
			{
				return true;
			}
			if (type == typeof(OverlaidDictionary))
			{
				return true;
			}
			if (type == typeof(AngryRoger))
			{
				return true;
			}
			if (type == typeof(HotHead))
			{
				return true;
			}
			if (type == typeof(LavaLurk))
			{
				return true;
			}
			if (type == typeof(LavaLurk.State))
			{
				return true;
			}
			if (type == typeof(Leaper))
			{
				return true;
			}
			if (type == typeof(Shooter))
			{
				return true;
			}
			if (type == typeof(Spiker))
			{
				return true;
			}
			if (type == typeof(DwarvishSentry))
			{
				return true;
			}
			if (type == typeof(BlueSquid))
			{
				return true;
			}
			if (type == typeof(DinoMonster))
			{
				return true;
			}
			if (type == typeof(DinoMonster.AttackState))
			{
				return true;
			}
			if (type == typeof(Bug))
			{
				return true;
			}
			if (type == typeof(BigSlime))
			{
				return true;
			}
			if (type == typeof(Mummy))
			{
				return true;
			}
			if (type == typeof(Serpent))
			{
				return true;
			}
			if (type == typeof(MetalHead))
			{
				return true;
			}
			if (type == typeof(ShadowBrute))
			{
				return true;
			}
			if (type == typeof(ShadowShaman))
			{
				return true;
			}
			if (type == typeof(Skeleton))
			{
				return true;
			}
			if (type == typeof(DustSpirit))
			{
				return true;
			}
			if (type == typeof(Bat))
			{
				return true;
			}
			if (type == typeof(Fly))
			{
				return true;
			}
			if (type == typeof(Grub))
			{
				return true;
			}
			if (type == typeof(RockGolem))
			{
				return true;
			}
			if (type == typeof(ShadowGirl))
			{
				return true;
			}
			if (type == typeof(SquidKid))
			{
				return true;
			}
			if (type == typeof(LavaCrab))
			{
				return true;
			}
			if (type == typeof(RockCrab))
			{
				return true;
			}
			if (type == typeof(Duggy))
			{
				return true;
			}
			if (type == typeof(Ghost))
			{
				return true;
			}
			if (type == typeof(Ghost.GhostVariant))
			{
				return true;
			}
			if (type == typeof(ShadowGuy))
			{
				return true;
			}
			if (type == typeof(GreenSlime))
			{
				return true;
			}
			if (type == typeof(Monster))
			{
				return true;
			}
			if (type == typeof(NetLeaderboards))
			{
				return true;
			}
			if (type == typeof(NetLeaderboardsEntry))
			{
				return true;
			}
			if (type == typeof(AbigailGame.GameKeys))
			{
				return true;
			}
			if (type == typeof(AbigailGame.JOTPKProgress))
			{
				return true;
			}
			if (type == typeof(AbigailGame.Dracula))
			{
				return true;
			}
			if (type == typeof(AbandonedJojaMart))
			{
				return true;
			}
			if (type == typeof(BoatTunnel))
			{
				return true;
			}
			if (type == typeof(BoatTunnel.TunnelAnimationState))
			{
				return true;
			}
			if (type == typeof(BugLand))
			{
				return true;
			}
			if (type == typeof(Caldera))
			{
				return true;
			}
			if (type == typeof(FishShop))
			{
				return true;
			}
			if (type == typeof(IslandEast))
			{
				return true;
			}
			if (type == typeof(IslandFarmCave))
			{
				return true;
			}
			if (type == typeof(IslandFarmHouse))
			{
				return true;
			}
			if (type == typeof(IslandFieldOffice))
			{
				return true;
			}
			if (type == typeof(IslandForestLocation))
			{
				return true;
			}
			if (type == typeof(IslandHut))
			{
				return true;
			}
			if (type == typeof(IslandLocation))
			{
				return true;
			}
			if (type == typeof(IslandNorth))
			{
				return true;
			}
			if (type == typeof(IslandSecret))
			{
				return true;
			}
			if (type == typeof(IslandShrine))
			{
				return true;
			}
			if (type == typeof(IslandSouth))
			{
				return true;
			}
			if (type == typeof(IslandSouthEast))
			{
				return true;
			}
			if (type == typeof(IslandSouthEastCave))
			{
				return true;
			}
			if (type == typeof(IslandWest))
			{
				return true;
			}
			if (type == typeof(IslandWestCave1))
			{
				return true;
			}
			if (type == typeof(IslandWestCave1.CaveCrystal))
			{
				return true;
			}
			if (type == typeof(Mine))
			{
				return true;
			}
			if (type == typeof(ShopLocation))
			{
				return true;
			}
			if (type == typeof(VolcanoDungeon.TileNeighbors))
			{
				return true;
			}
			if (type == typeof(BeachNightMarket))
			{
				return true;
			}
			if (type == typeof(ManorHouse))
			{
				return true;
			}
			if (type == typeof(MermaidHouse))
			{
				return true;
			}
			if (type == typeof(MovieTheater))
			{
				return true;
			}
			if (type == typeof(MovieTheater.MovieStates))
			{
				return true;
			}
			if (type == typeof(Submarine))
			{
				return true;
			}
			if (type == typeof(AdventureGuild))
			{
				return true;
			}
			if (type == typeof(Bus))
			{
				return true;
			}
			if (type == typeof(BuildableGameLocation))
			{
				return true;
			}
			if (type == typeof(BathHousePool))
			{
				return true;
			}
			if (type == typeof(Club))
			{
				return true;
			}
			if (type == typeof(Cellar))
			{
				return true;
			}
			if (type == typeof(DecorationFacade))
			{
				return true;
			}
			if (type == typeof(DecoratableLocation))
			{
				return true;
			}
			if (type == typeof(Cabin))
			{
				return true;
			}
			if (type == typeof(FarmCave))
			{
				return true;
			}
			if (type == typeof(WizardHouse))
			{
				return true;
			}
			if (type == typeof(Sewer))
			{
				return true;
			}
			if (type == typeof(CommunityCenter))
			{
				return true;
			}
			if (type == typeof(BusStop))
			{
				return true;
			}
			if (type == typeof(MineShaft))
			{
				return true;
			}
			if (type == typeof(MineInfo))
			{
				return true;
			}
			if (type == typeof(FarmHouse))
			{
				return true;
			}
			if (type == typeof(JojaMart))
			{
				return true;
			}
			if (type == typeof(Desert))
			{
				return true;
			}
			if (type == typeof(Town))
			{
				return true;
			}
			if (type == typeof(Mountain))
			{
				return true;
			}
			if (type == typeof(Forest))
			{
				return true;
			}
			if (type == typeof(LibraryMuseum))
			{
				return true;
			}
			if (type == typeof(Railroad))
			{
				return true;
			}
			if (type == typeof(SeedShop))
			{
				return true;
			}
			if (type == typeof(Summit))
			{
				return true;
			}
			if (type == typeof(Woods))
			{
				return true;
			}
			if (type == typeof(Beach))
			{
				return true;
			}
			if (type == typeof(TrashBear))
			{
				return true;
			}
			if (type == typeof(Cat))
			{
				return true;
			}
			if (type == typeof(Child))
			{
				return true;
			}
			if (type == typeof(BotchedNetInt))
			{
				return true;
			}
			if (type == typeof(BotchedNetBool))
			{
				return true;
			}
			if (type == typeof(BotchedNetLong))
			{
				return true;
			}
			if (type == typeof(Dog))
			{
				return true;
			}
			if (type == typeof(Horse))
			{
				return true;
			}
			if (type == typeof(Junimo))
			{
				return true;
			}
			if (type == typeof(JunimoHarvester))
			{
				return true;
			}
			if (type == typeof(Pet))
			{
				return true;
			}
			if (type == typeof(FishPond))
			{
				return true;
			}
			if (type == typeof(GreenhouseBuilding))
			{
				return true;
			}
			if (type == typeof(JunimoHut))
			{
				return true;
			}
			if (type == typeof(Mill))
			{
				return true;
			}
			if (type == typeof(ShippingBin))
			{
				return true;
			}
			if (type == typeof(Stable))
			{
				return true;
			}
			if (type == typeof(Building))
			{
				return true;
			}
			if (type == typeof(Coop))
			{
				return true;
			}
			if (type == typeof(Barn))
			{
				return true;
			}
			if (type == typeof(SandDuggy))
			{
				return true;
			}
			if (type == typeof(SandDuggy.State))
			{
				return true;
			}
			return false;
		}

		public override XmlSerializer GetSerializer(Type type)
		{
			if (type == typeof(WalkDirection))
			{
				return new WalkDirectionSerializer();
			}
			if (type == typeof(ObjectID))
			{
				return new ObjectIDSerializer();
			}
			if (type == typeof(BigCraftableID))
			{
				return new BigCraftableIDSerializer();
			}
			if (type == typeof(FurnitureID))
			{
				return new FurnitureIDSerializer();
			}
			if (type == typeof(MouseCursor))
			{
				return new MouseCursorSerializer();
			}
			if (type == typeof(TapState))
			{
				return new TapStateSerializer();
			}
			if (type == typeof(DistanceToTarget))
			{
				return new DistanceToTargetSerializer();
			}
			if (type == typeof(WeaponControl))
			{
				return new WeaponControlSerializer();
			}
			if (type == typeof(tutorialType))
			{
				return new tutorialTypeSerializer();
			}
			if (type == typeof(TutorialShopLocation))
			{
				return new TutorialShopLocationSerializer();
			}
			if (type == typeof(LocationWeather))
			{
				return new LocationWeatherSerializer();
			}
			if (type == typeof(DontLoadDefaultSetting))
			{
				return new DontLoadDefaultSettingSerializer();
			}
			if (type == typeof(WaterTiles.WaterTileData))
			{
				return new WaterTileDataSerializer();
			}
			if (type == typeof(TweenState))
			{
				return new TweenStateSerializer();
			}
			if (type == typeof(StopBehavior))
			{
				return new StopBehaviorSerializer();
			}
			if (type == typeof(FloatTween))
			{
				return new FloatTweenSerializer();
			}
			if (type == typeof(Vector2Tween))
			{
				return new Vector2TweenSerializer();
			}
			if (type == typeof(Vector3Tween))
			{
				return new Vector3TweenSerializer();
			}
			if (type == typeof(Vector4Tween))
			{
				return new Vector4TweenSerializer();
			}
			if (type == typeof(ColorTween))
			{
				return new ColorTweenSerializer();
			}
			if (type == typeof(QuaternionTween))
			{
				return new QuaternionTweenSerializer();
			}
			if (type == typeof(BuildingPainter))
			{
				return new BuildingPainterSerializer();
			}
			if (type == typeof(BuildingPaintColor))
			{
				return new BuildingPaintColorSerializer();
			}
			if (type == typeof(HouseRenovation.AnimationType))
			{
				return new AnimationTypeSerializer();
			}
			if (type == typeof(IslandGemBird))
			{
				return new IslandGemBirdSerializer();
			}
			if (type == typeof(IslandGemBird.GemBirdType))
			{
				return new GemBirdTypeSerializer();
			}
			if (type == typeof(InstanceStatics))
			{
				return new InstanceStaticsSerializer();
			}
			if (type == typeof(InstancedStatic))
			{
				return new InstancedStaticSerializer();
			}
			if (type == typeof(NonInstancedStatic))
			{
				return new NonInstancedStaticSerializer();
			}
			if (type == typeof(LocalMultiplayer))
			{
				return new LocalMultiplayerSerializer();
			}
			if (type == typeof(LocationName))
			{
				return new LocationNameSerializer();
			}
			if (type == typeof(Options))
			{
				return new OptionsSerializer();
			}
			if (type == typeof(Options.ItemStowingModes))
			{
				return new ItemStowingModesSerializer();
			}
			if (type == typeof(Options.GamepadModes))
			{
				return new GamepadModesSerializer();
			}
			if (type == typeof(MapSeat))
			{
				return new MapSeatSerializer();
			}
			if (type == typeof(SpecialOrder))
			{
				return new SpecialOrderSerializer();
			}
			if (type == typeof(SpecialOrder.QuestState))
			{
				return new QuestStateSerializer();
			}
			if (type == typeof(SpecialOrder.QuestDuration))
			{
				return new QuestDurationSerializer();
			}
			if (type == typeof(OrderObjective))
			{
				return new OrderObjectiveSerializer();
			}
			if (type == typeof(CollectObjective))
			{
				return new CollectObjectiveSerializer();
			}
			if (type == typeof(DonateObjective))
			{
				return new DonateObjectiveSerializer();
			}
			if (type == typeof(ShipObjective))
			{
				return new ShipObjectiveSerializer();
			}
			if (type == typeof(SlayObjective))
			{
				return new SlayObjectiveSerializer();
			}
			if (type == typeof(FishObjective))
			{
				return new FishObjectiveSerializer();
			}
			if (type == typeof(DeliverObjective))
			{
				return new DeliverObjectiveSerializer();
			}
			if (type == typeof(GiftObjective))
			{
				return new GiftObjectiveSerializer();
			}
			if (type == typeof(GiftObjective.LikeLevels))
			{
				return new LikeLevelsSerializer();
			}
			if (type == typeof(ReachMineFloorObjective))
			{
				return new ReachMineFloorObjectiveSerializer();
			}
			if (type == typeof(JKScoreObjective))
			{
				return new JKScoreObjectiveSerializer();
			}
			if (type == typeof(OrderReward))
			{
				return new OrderRewardSerializer();
			}
			if (type == typeof(MailReward))
			{
				return new MailRewardSerializer();
			}
			if (type == typeof(ResetEventReward))
			{
				return new ResetEventRewardSerializer();
			}
			if (type == typeof(GemsReward))
			{
				return new GemsRewardSerializer();
			}
			if (type == typeof(MoneyReward))
			{
				return new MoneyRewardSerializer();
			}
			if (type == typeof(FriendshipReward))
			{
				return new FriendshipRewardSerializer();
			}
			if (type == typeof(BaseEnchantment))
			{
				return new BaseEnchantmentSerializer();
			}
			if (type == typeof(BaseWeaponEnchantment))
			{
				return new BaseWeaponEnchantmentSerializer();
			}
			if (type == typeof(MagicEnchantment))
			{
				return new MagicEnchantmentSerializer();
			}
			if (type == typeof(AmethystEnchantment))
			{
				return new AmethystEnchantmentSerializer();
			}
			if (type == typeof(TopazEnchantment))
			{
				return new TopazEnchantmentSerializer();
			}
			if (type == typeof(AquamarineEnchantment))
			{
				return new AquamarineEnchantmentSerializer();
			}
			if (type == typeof(JadeEnchantment))
			{
				return new JadeEnchantmentSerializer();
			}
			if (type == typeof(DiamondEnchantment))
			{
				return new DiamondEnchantmentSerializer();
			}
			if (type == typeof(GalaxySoulEnchantment))
			{
				return new GalaxySoulEnchantmentSerializer();
			}
			if (type == typeof(RubyEnchantment))
			{
				return new RubyEnchantmentSerializer();
			}
			if (type == typeof(EmeraldEnchantment))
			{
				return new EmeraldEnchantmentSerializer();
			}
			if (type == typeof(ArtfulEnchantment))
			{
				return new ArtfulEnchantmentSerializer();
			}
			if (type == typeof(HaymakerEnchantment))
			{
				return new HaymakerEnchantmentSerializer();
			}
			if (type == typeof(BugKillerEnchantment))
			{
				return new BugKillerEnchantmentSerializer();
			}
			if (type == typeof(VampiricEnchantment))
			{
				return new VampiricEnchantmentSerializer();
			}
			if (type == typeof(CrusaderEnchantment))
			{
				return new CrusaderEnchantmentSerializer();
			}
			if (type == typeof(PickaxeEnchantment))
			{
				return new PickaxeEnchantmentSerializer();
			}
			if (type == typeof(HoeEnchantment))
			{
				return new HoeEnchantmentSerializer();
			}
			if (type == typeof(AxeEnchantment))
			{
				return new AxeEnchantmentSerializer();
			}
			if (type == typeof(FishingRodEnchantment))
			{
				return new FishingRodEnchantmentSerializer();
			}
			if (type == typeof(WateringCanEnchantment))
			{
				return new WateringCanEnchantmentSerializer();
			}
			if (type == typeof(PanEnchantment))
			{
				return new PanEnchantmentSerializer();
			}
			if (type == typeof(MilkPailEnchantment))
			{
				return new MilkPailEnchantmentSerializer();
			}
			if (type == typeof(ShearsEnchantment))
			{
				return new ShearsEnchantmentSerializer();
			}
			if (type == typeof(PowerfulEnchantment))
			{
				return new PowerfulEnchantmentSerializer();
			}
			if (type == typeof(EfficientToolEnchantment))
			{
				return new EfficientToolEnchantmentSerializer();
			}
			if (type == typeof(SwiftToolEnchantment))
			{
				return new SwiftToolEnchantmentSerializer();
			}
			if (type == typeof(ReachingToolEnchantment))
			{
				return new ReachingToolEnchantmentSerializer();
			}
			if (type == typeof(BottomlessEnchantment))
			{
				return new BottomlessEnchantmentSerializer();
			}
			if (type == typeof(ShavingEnchantment))
			{
				return new ShavingEnchantmentSerializer();
			}
			if (type == typeof(ArchaeologistEnchantment))
			{
				return new ArchaeologistEnchantmentSerializer();
			}
			if (type == typeof(GenerousEnchantment))
			{
				return new GenerousEnchantmentSerializer();
			}
			if (type == typeof(MasterEnchantment))
			{
				return new MasterEnchantmentSerializer();
			}
			if (type == typeof(AutoHookEnchantment))
			{
				return new AutoHookEnchantmentSerializer();
			}
			if (type == typeof(PreservingEnchantment))
			{
				return new PreservingEnchantmentSerializer();
			}
			if (type == typeof(StackDrawType))
			{
				return new StackDrawTypeSerializer();
			}
			if (type == typeof(MovieInvitation))
			{
				return new MovieInvitationSerializer();
			}
			if (type == typeof(StartMovieEvent))
			{
				return new StartMovieEventSerializer();
			}
			if (type == typeof(MovieViewerLockEvent))
			{
				return new MovieViewerLockEventSerializer();
			}
			if (type == typeof(WorldDate))
			{
				return new WorldDateSerializer();
			}
			if (type == typeof(FriendshipStatus))
			{
				return new FriendshipStatusSerializer();
			}
			if (type == typeof(Friendship))
			{
				return new FriendshipSerializer();
			}
			if (type == typeof(LocalizedContentManager.LanguageCode))
			{
				return new LanguageCodeSerializer();
			}
			if (type == typeof(AnimalHouse))
			{
				return new AnimalHouseSerializer();
			}
			if (type == typeof(BuildingUpgrade))
			{
				return new BuildingUpgradeSerializer();
			}
			if (type == typeof(Character))
			{
				return new CharacterSerializer();
			}
			if (type == typeof(Chunk))
			{
				return new ChunkSerializer();
			}
			if (type == typeof(FarmerRenderer))
			{
				return new FarmerRendererSerializer();
			}
			if (type == typeof(Shed))
			{
				return new ShedSerializer();
			}
			if (type == typeof(SlimeHutch))
			{
				return new SlimeHutchSerializer();
			}
			if (type == typeof(Farm))
			{
				return new FarmSerializer();
			}
			if (type == typeof(Farm.LightningStrikeEvent))
			{
				return new LightningStrikeEventSerializer();
			}
			if (type == typeof(FarmAnimal))
			{
				return new FarmAnimalSerializer();
			}
			if (type == typeof(NetLogger))
			{
				return new NetLoggerSerializer();
			}
			if (type == typeof(Noise))
			{
				return new NoiseSerializer();
			}
			if (type == typeof(NumberSprite))
			{
				return new NumberSpriteSerializer();
			}
			if (type == typeof(Fence))
			{
				return new FenceSerializer();
			}
			if (type == typeof(Item))
			{
				return new ItemSerializer();
			}
			if (type == typeof(ModDataDictionary))
			{
				return new ModDataDictionarySerializer();
			}
			if (type == typeof(LightSource))
			{
				return new LightSourceSerializer();
			}
			if (type == typeof(LightSource.LightContext))
			{
				return new LightContextSerializer();
			}
			if (type == typeof(Torch))
			{
				return new TorchSerializer();
			}
			if (type == typeof(InputButton))
			{
				return new InputButtonSerializer();
			}
			if (type == typeof(ServerPrivacy))
			{
				return new ServerPrivacySerializer();
			}
			if (type == typeof(NameSelect))
			{
				return new NameSelectSerializer();
			}
			if (type == typeof(PriorityQueue))
			{
				return new PriorityQueueSerializer();
			}
			if (type == typeof(Crop))
			{
				return new CropSerializer();
			}
			if (type == typeof(Farmer))
			{
				return new FarmerSerializer();
			}
			if (type == typeof(NetIntIntArrayDictionary))
			{
				return new NetIntIntArrayDictionarySerializer();
			}
			if (type == typeof(HairStyleMetadata))
			{
				return new HairStyleMetadataSerializer();
			}
			if (type == typeof(FarmerPair))
			{
				return new FarmerPairSerializer();
			}
			if (type == typeof(NutDropRequest))
			{
				return new NutDropRequestSerializer();
			}
			if (type == typeof(FarmerTeam.RemoteBuildingPermissions))
			{
				return new RemoteBuildingPermissionsSerializer();
			}
			if (type == typeof(FarmerTeam.SleepAnnounceModes))
			{
				return new SleepAnnounceModesSerializer();
			}
			if (type == typeof(InteriorDoor))
			{
				return new InteriorDoorSerializer();
			}
			if (type == typeof(InteriorDoorDictionary))
			{
				return new InteriorDoorDictionarySerializer();
			}
			if (type == typeof(GameLocation))
			{
				return new GameLocationSerializer();
			}
			if (type == typeof(GameLocation.LocationContext))
			{
				return new LocationContextSerializer();
			}
			if (type == typeof(NPC))
			{
				return new NPCSerializer();
			}
			if (type == typeof(MarriageDialogueReference))
			{
				return new MarriageDialogueReferenceSerializer();
			}
			if (type == typeof(FarmActivity))
			{
				return new FarmActivitySerializer();
			}
			if (type == typeof(ArtifactSpotWatchActivity))
			{
				return new ArtifactSpotWatchActivitySerializer();
			}
			if (type == typeof(CropWatchActivity))
			{
				return new CropWatchActivitySerializer();
			}
			if (type == typeof(FlowerWatchActivity))
			{
				return new FlowerWatchActivitySerializer();
			}
			if (type == typeof(ShrineActivity))
			{
				return new ShrineActivitySerializer();
			}
			if (type == typeof(MailActivity))
			{
				return new MailActivitySerializer();
			}
			if (type == typeof(TreeActivity))
			{
				return new TreeActivitySerializer();
			}
			if (type == typeof(ClearingActivity))
			{
				return new ClearingActivitySerializer();
			}
			if (type == typeof(StardewValley.Object))
			{
				return new ObjectSerializer();
			}
			if (type == typeof(StardewValley.Object.PreserveType))
			{
				return new PreserveTypeSerializer();
			}
			if (type == typeof(StardewValley.Object.HoneyType))
			{
				return new HoneyTypeSerializer();
			}
			if (type == typeof(RainDrop))
			{
				return new RainDropSerializer();
			}
			if (type == typeof(MineChestType))
			{
				return new MineChestTypeSerializer();
			}
			if (type == typeof(Vector2Reader))
			{
				return new Vector2ReaderSerializer();
			}
			if (type == typeof(Vector2Writer))
			{
				return new Vector2WriterSerializer();
			}
			if (type == typeof(Vector2Serializer))
			{
				return new Vector2SerializerSerializer();
			}
			if (type == typeof(SaveGame))
			{
				return new SaveGameSerializer();
			}
			if (type == typeof(SaveGame.SaveFixes))
			{
				return new SaveFixesSerializer();
			}
			if (type == typeof(ChangeType))
			{
				return new ChangeTypeSerializer();
			}
			if (type == typeof(StartupPreferences))
			{
				return new StartupPreferencesSerializer();
			}
			if (type == typeof(Stats))
			{
				return new StatsSerializer();
			}
			if (type == typeof(Tool))
			{
				return new ToolSerializer();
			}
			if (type == typeof(Warp))
			{
				return new WarpSerializer();
			}
			if (type == typeof(WeatherDebris))
			{
				return new WeatherDebrisSerializer();
			}
			if (type == typeof(Bush))
			{
				return new BushSerializer();
			}
			if (type == typeof(CosmeticPlant))
			{
				return new CosmeticPlantSerializer();
			}
			if (type == typeof(Flooring))
			{
				return new FlooringSerializer();
			}
			if (type == typeof(FruitTree))
			{
				return new FruitTreeSerializer();
			}
			if (type == typeof(GiantCrop))
			{
				return new GiantCropSerializer();
			}
			if (type == typeof(HoeDirt))
			{
				return new HoeDirtSerializer();
			}
			if (type == typeof(LargeTerrainFeature))
			{
				return new LargeTerrainFeatureSerializer();
			}
			if (type == typeof(ResourceClump))
			{
				return new ResourceClumpSerializer();
			}
			if (type == typeof(Grass))
			{
				return new GrassSerializer();
			}
			if (type == typeof(Quartz))
			{
				return new QuartzSerializer();
			}
			if (type == typeof(TerrainFeature))
			{
				return new TerrainFeatureSerializer();
			}
			if (type == typeof(Tree))
			{
				return new TreeSerializer();
			}
			if (type == typeof(BasicProjectile))
			{
				return new BasicProjectileSerializer();
			}
			if (type == typeof(DebuffingProjectile))
			{
				return new DebuffingProjectileSerializer();
			}
			if (type == typeof(Projectile))
			{
				return new ProjectileSerializer();
			}
			if (type == typeof(GenericTool))
			{
				return new GenericToolSerializer();
			}
			if (type == typeof(Axe))
			{
				return new AxeSerializer();
			}
			if (type == typeof(Blueprints))
			{
				return new BlueprintsSerializer();
			}
			if (type == typeof(Pan))
			{
				return new PanSerializer();
			}
			if (type == typeof(FishingRod))
			{
				return new FishingRodSerializer();
			}
			if (type == typeof(Lantern))
			{
				return new LanternSerializer();
			}
			if (type == typeof(MagnifyingGlass))
			{
				return new MagnifyingGlassSerializer();
			}
			if (type == typeof(MeleeWeapon))
			{
				return new MeleeWeaponSerializer();
			}
			if (type == typeof(MilkPail))
			{
				return new MilkPailSerializer();
			}
			if (type == typeof(Pickaxe))
			{
				return new PickaxeSerializer();
			}
			if (type == typeof(Hoe))
			{
				return new HoeSerializer();
			}
			if (type == typeof(Raft))
			{
				return new RaftSerializer();
			}
			if (type == typeof(Seeds))
			{
				return new SeedsSerializer();
			}
			if (type == typeof(Shears))
			{
				return new ShearsSerializer();
			}
			if (type == typeof(Slingshot))
			{
				return new SlingshotSerializer();
			}
			if (type == typeof(Stackable))
			{
				return new StackableSerializer();
			}
			if (type == typeof(Sword))
			{
				return new SwordSerializer();
			}
			if (type == typeof(ToolDescription))
			{
				return new ToolDescriptionSerializer();
			}
			if (type == typeof(ToolFactory))
			{
				return new ToolFactorySerializer();
			}
			if (type == typeof(Wand))
			{
				return new WandSerializer();
			}
			if (type == typeof(WateringCan))
			{
				return new WateringCanSerializer();
			}
			if (type == typeof(BoundingBoxGroup))
			{
				return new BoundingBoxGroupSerializer();
			}
			if (type == typeof(ToolSpamInputSimulator))
			{
				return new ToolSpamInputSimulatorSerializer();
			}
			if (type == typeof(LeftRightClickSpamInputSimulator))
			{
				return new LeftRightClickSpamInputSimulatorSerializer();
			}
			if (type == typeof(SynchronizedShopStock))
			{
				return new SynchronizedShopStockSerializer();
			}
			if (type == typeof(SynchronizedShopStock.SynchedShop))
			{
				return new SynchedShopSerializer();
			}
			if (type == typeof(SecretLostItemQuest))
			{
				return new SecretLostItemQuestSerializer();
			}
			if (type == typeof(NetDescriptionElementRef))
			{
				return new NetDescriptionElementRefSerializer();
			}
			if (type == typeof(NetDescriptionElementRef))
			{
				return new NetDescriptionElementRefSerializer1();
			}
			if (type == typeof(DescriptionElement))
			{
				return new DescriptionElementSerializer();
			}
			if (type == typeof(LostItemQuest))
			{
				return new LostItemQuestSerializer();
			}
			if (type == typeof(ItemHarvestQuest))
			{
				return new ItemHarvestQuestSerializer();
			}
			if (type == typeof(GoSomewhereQuest))
			{
				return new GoSomewhereQuestSerializer();
			}
			if (type == typeof(CraftingQuest))
			{
				return new CraftingQuestSerializer();
			}
			if (type == typeof(SocializeQuest))
			{
				return new SocializeQuestSerializer();
			}
			if (type == typeof(FishingQuest))
			{
				return new FishingQuestSerializer();
			}
			if (type == typeof(SlayMonsterQuest))
			{
				return new SlayMonsterQuestSerializer();
			}
			if (type == typeof(ResourceCollectionQuest))
			{
				return new ResourceCollectionQuestSerializer();
			}
			if (type == typeof(ItemDeliveryQuest))
			{
				return new ItemDeliveryQuestSerializer();
			}
			if (type == typeof(Quest))
			{
				return new QuestSerializer();
			}
			if (type == typeof(BedFurniture))
			{
				return new BedFurnitureSerializer();
			}
			if (type == typeof(BedFurniture.BedType))
			{
				return new BedTypeSerializer();
			}
			if (type == typeof(FishTankFurniture))
			{
				return new FishTankFurnitureSerializer();
			}
			if (type == typeof(FishTankFurniture.FishTankCategories))
			{
				return new FishTankCategoriesSerializer();
			}
			if (type == typeof(TankFish.FishType))
			{
				return new FishTypeSerializer();
			}
			if (type == typeof(ItemPedestal))
			{
				return new ItemPedestalSerializer();
			}
			if (type == typeof(Phone))
			{
				return new PhoneSerializer();
			}
			if (type == typeof(Phone.PhoneCalls))
			{
				return new PhoneCallsSerializer();
			}
			if (type == typeof(StorageFurniture))
			{
				return new StorageFurnitureSerializer();
			}
			if (type == typeof(WoodChipper))
			{
				return new WoodChipperSerializer();
			}
			if (type == typeof(Clothing))
			{
				return new ClothingSerializer();
			}
			if (type == typeof(Clothing.ClothesType))
			{
				return new ClothesTypeSerializer();
			}
			if (type == typeof(IndoorPot))
			{
				return new IndoorPotSerializer();
			}
			if (type == typeof(MiniJukebox))
			{
				return new MiniJukeboxSerializer();
			}
			if (type == typeof(Sign))
			{
				return new SignSerializer();
			}
			if (type == typeof(Workbench))
			{
				return new WorkbenchSerializer();
			}
			if (type == typeof(Boots))
			{
				return new BootsSerializer();
			}
			if (type == typeof(BreakableContainer))
			{
				return new BreakableContainerSerializer();
			}
			if (type == typeof(Cask))
			{
				return new CaskSerializer();
			}
			if (type == typeof(Chest))
			{
				return new ChestSerializer();
			}
			if (type == typeof(Chest.SpecialChestTypes))
			{
				return new SpecialChestTypesSerializer();
			}
			if (type == typeof(ColoredObject))
			{
				return new ColoredObjectSerializer();
			}
			if (type == typeof(SpecialItem))
			{
				return new SpecialItemSerializer();
			}
			if (type == typeof(TV))
			{
				return new TVSerializer();
			}
			if (type == typeof(Wallpaper))
			{
				return new WallpaperSerializer();
			}
			if (type == typeof(Furniture))
			{
				return new FurnitureSerializer();
			}
			if (type == typeof(CrabPot))
			{
				return new CrabPotSerializer();
			}
			if (type == typeof(Hat))
			{
				return new HatSerializer();
			}
			if (type == typeof(Hat.HairDrawType))
			{
				return new HairDrawTypeSerializer();
			}
			if (type == typeof(ItemDescription))
			{
				return new ItemDescriptionSerializer();
			}
			if (type == typeof(ObjectFactory))
			{
				return new ObjectFactorySerializer();
			}
			if (type == typeof(Ring))
			{
				return new RingSerializer();
			}
			if (type == typeof(CombinedRing))
			{
				return new CombinedRingSerializer();
			}
			if (type == typeof(SwitchFloor))
			{
				return new SwitchFloorSerializer();
			}
			if (type == typeof(IncomingMessage))
			{
				return new IncomingMessageSerializer();
			}
			if (type == typeof(NetAudio.SoundContext))
			{
				return new SoundContextSerializer();
			}
			if (type == typeof(NetDirection))
			{
				return new NetDirectionSerializer();
			}
			if (type == typeof(NetLocationRef))
			{
				return new NetLocationRefSerializer();
			}
			if (type == typeof(NetPosition))
			{
				return new NetPositionSerializer();
			}
			if (type == typeof(NetBundles))
			{
				return new NetBundlesSerializer();
			}
			if (type == typeof(NetFarmerCollection))
			{
				return new NetFarmerCollectionSerializer();
			}
			if (type == typeof(NetMutex))
			{
				return new NetMutexSerializer();
			}
			if (type == typeof(NetWitnessedLock))
			{
				return new NetWitnessedLockSerializer();
			}
			if (type == typeof(NetFarmerCollection))
			{
				return new NetFarmerCollectionSerializer1();
			}
			if (type == typeof(NetNPCRef))
			{
				return new NetNPCRefSerializer();
			}
			if (type == typeof(NetCharacterRef))
			{
				return new NetCharacterRefSerializer();
			}
			if (type == typeof(NetDancePartner))
			{
				return new NetDancePartnerSerializer();
			}
			if (type == typeof(OverlaidDictionary))
			{
				return new OverlaidDictionarySerializer();
			}
			if (type == typeof(AngryRoger))
			{
				return new AngryRogerSerializer();
			}
			if (type == typeof(HotHead))
			{
				return new HotHeadSerializer();
			}
			if (type == typeof(LavaLurk))
			{
				return new LavaLurkSerializer();
			}
			if (type == typeof(LavaLurk.State))
			{
				return new StateSerializer();
			}
			if (type == typeof(Leaper))
			{
				return new LeaperSerializer();
			}
			if (type == typeof(Shooter))
			{
				return new ShooterSerializer();
			}
			if (type == typeof(Spiker))
			{
				return new SpikerSerializer();
			}
			if (type == typeof(DwarvishSentry))
			{
				return new DwarvishSentrySerializer();
			}
			if (type == typeof(BlueSquid))
			{
				return new BlueSquidSerializer();
			}
			if (type == typeof(DinoMonster))
			{
				return new DinoMonsterSerializer();
			}
			if (type == typeof(DinoMonster.AttackState))
			{
				return new AttackStateSerializer();
			}
			if (type == typeof(Bug))
			{
				return new BugSerializer();
			}
			if (type == typeof(BigSlime))
			{
				return new BigSlimeSerializer();
			}
			if (type == typeof(Mummy))
			{
				return new MummySerializer();
			}
			if (type == typeof(Serpent))
			{
				return new SerpentSerializer();
			}
			if (type == typeof(MetalHead))
			{
				return new MetalHeadSerializer();
			}
			if (type == typeof(ShadowBrute))
			{
				return new ShadowBruteSerializer();
			}
			if (type == typeof(ShadowShaman))
			{
				return new ShadowShamanSerializer();
			}
			if (type == typeof(Skeleton))
			{
				return new SkeletonSerializer();
			}
			if (type == typeof(DustSpirit))
			{
				return new DustSpiritSerializer();
			}
			if (type == typeof(Bat))
			{
				return new BatSerializer();
			}
			if (type == typeof(Fly))
			{
				return new FlySerializer();
			}
			if (type == typeof(Grub))
			{
				return new GrubSerializer();
			}
			if (type == typeof(RockGolem))
			{
				return new RockGolemSerializer();
			}
			if (type == typeof(ShadowGirl))
			{
				return new ShadowGirlSerializer();
			}
			if (type == typeof(SquidKid))
			{
				return new SquidKidSerializer();
			}
			if (type == typeof(LavaCrab))
			{
				return new LavaCrabSerializer();
			}
			if (type == typeof(RockCrab))
			{
				return new RockCrabSerializer();
			}
			if (type == typeof(Duggy))
			{
				return new DuggySerializer();
			}
			if (type == typeof(Ghost))
			{
				return new GhostSerializer();
			}
			if (type == typeof(Ghost.GhostVariant))
			{
				return new GhostVariantSerializer();
			}
			if (type == typeof(ShadowGuy))
			{
				return new ShadowGuySerializer();
			}
			if (type == typeof(GreenSlime))
			{
				return new GreenSlimeSerializer();
			}
			if (type == typeof(Monster))
			{
				return new MonsterSerializer();
			}
			if (type == typeof(NetLeaderboards))
			{
				return new NetLeaderboardsSerializer();
			}
			if (type == typeof(NetLeaderboardsEntry))
			{
				return new NetLeaderboardsEntrySerializer();
			}
			if (type == typeof(AbigailGame.GameKeys))
			{
				return new GameKeysSerializer();
			}
			if (type == typeof(AbigailGame.JOTPKProgress))
			{
				return new JOTPKProgressSerializer();
			}
			if (type == typeof(AbigailGame.Dracula))
			{
				return new DraculaSerializer();
			}
			if (type == typeof(AbandonedJojaMart))
			{
				return new AbandonedJojaMartSerializer();
			}
			if (type == typeof(BoatTunnel))
			{
				return new BoatTunnelSerializer();
			}
			if (type == typeof(BoatTunnel.TunnelAnimationState))
			{
				return new TunnelAnimationStateSerializer();
			}
			if (type == typeof(BugLand))
			{
				return new BugLandSerializer();
			}
			if (type == typeof(Caldera))
			{
				return new CalderaSerializer();
			}
			if (type == typeof(FishShop))
			{
				return new FishShopSerializer();
			}
			if (type == typeof(IslandEast))
			{
				return new IslandEastSerializer();
			}
			if (type == typeof(IslandFarmCave))
			{
				return new IslandFarmCaveSerializer();
			}
			if (type == typeof(IslandFarmHouse))
			{
				return new IslandFarmHouseSerializer();
			}
			if (type == typeof(IslandFieldOffice))
			{
				return new IslandFieldOfficeSerializer();
			}
			if (type == typeof(IslandForestLocation))
			{
				return new IslandForestLocationSerializer();
			}
			if (type == typeof(IslandHut))
			{
				return new IslandHutSerializer();
			}
			if (type == typeof(IslandLocation))
			{
				return new IslandLocationSerializer();
			}
			if (type == typeof(IslandNorth))
			{
				return new IslandNorthSerializer();
			}
			if (type == typeof(IslandSecret))
			{
				return new IslandSecretSerializer();
			}
			if (type == typeof(IslandShrine))
			{
				return new IslandShrineSerializer();
			}
			if (type == typeof(IslandSouth))
			{
				return new IslandSouthSerializer();
			}
			if (type == typeof(IslandSouthEast))
			{
				return new IslandSouthEastSerializer();
			}
			if (type == typeof(IslandSouthEastCave))
			{
				return new IslandSouthEastCaveSerializer();
			}
			if (type == typeof(IslandWest))
			{
				return new IslandWestSerializer();
			}
			if (type == typeof(IslandWestCave1))
			{
				return new IslandWestCave1Serializer();
			}
			if (type == typeof(IslandWestCave1.CaveCrystal))
			{
				return new CaveCrystalSerializer();
			}
			if (type == typeof(Mine))
			{
				return new MineSerializer();
			}
			if (type == typeof(ShopLocation))
			{
				return new ShopLocationSerializer();
			}
			if (type == typeof(VolcanoDungeon.TileNeighbors))
			{
				return new TileNeighborsSerializer();
			}
			if (type == typeof(BeachNightMarket))
			{
				return new BeachNightMarketSerializer();
			}
			if (type == typeof(ManorHouse))
			{
				return new ManorHouseSerializer();
			}
			if (type == typeof(MermaidHouse))
			{
				return new MermaidHouseSerializer();
			}
			if (type == typeof(MovieTheater))
			{
				return new MovieTheaterSerializer();
			}
			if (type == typeof(MovieTheater.MovieStates))
			{
				return new MovieStatesSerializer();
			}
			if (type == typeof(Submarine))
			{
				return new SubmarineSerializer();
			}
			if (type == typeof(AdventureGuild))
			{
				return new AdventureGuildSerializer();
			}
			if (type == typeof(Bus))
			{
				return new BusSerializer();
			}
			if (type == typeof(BuildableGameLocation))
			{
				return new BuildableGameLocationSerializer();
			}
			if (type == typeof(BathHousePool))
			{
				return new BathHousePoolSerializer();
			}
			if (type == typeof(Club))
			{
				return new ClubSerializer();
			}
			if (type == typeof(Cellar))
			{
				return new CellarSerializer();
			}
			if (type == typeof(NetDirection))
			{
				return new NetDirectionSerializer1();
			}
			if (type == typeof(DecoratableLocation))
			{
				return new DecoratableLocationSerializer();
			}
			if (type == typeof(Cabin))
			{
				return new CabinSerializer();
			}
			if (type == typeof(FarmCave))
			{
				return new FarmCaveSerializer();
			}
			if (type == typeof(WizardHouse))
			{
				return new WizardHouseSerializer();
			}
			if (type == typeof(Sewer))
			{
				return new SewerSerializer();
			}
			if (type == typeof(CommunityCenter))
			{
				return new CommunityCenterSerializer();
			}
			if (type == typeof(BusStop))
			{
				return new BusStopSerializer();
			}
			if (type == typeof(MineShaft))
			{
				return new MineShaftSerializer();
			}
			if (type == typeof(MineInfo))
			{
				return new MineInfoSerializer();
			}
			if (type == typeof(FarmHouse))
			{
				return new FarmHouseSerializer();
			}
			if (type == typeof(JojaMart))
			{
				return new JojaMartSerializer();
			}
			if (type == typeof(Desert))
			{
				return new DesertSerializer();
			}
			if (type == typeof(Town))
			{
				return new TownSerializer();
			}
			if (type == typeof(Mountain))
			{
				return new MountainSerializer();
			}
			if (type == typeof(Forest))
			{
				return new ForestSerializer();
			}
			if (type == typeof(LibraryMuseum))
			{
				return new LibraryMuseumSerializer();
			}
			if (type == typeof(Railroad))
			{
				return new RailroadSerializer();
			}
			if (type == typeof(SeedShop))
			{
				return new SeedShopSerializer();
			}
			if (type == typeof(Summit))
			{
				return new SummitSerializer();
			}
			if (type == typeof(Woods))
			{
				return new WoodsSerializer();
			}
			if (type == typeof(Beach))
			{
				return new BeachSerializer();
			}
			if (type == typeof(TrashBear))
			{
				return new TrashBearSerializer();
			}
			if (type == typeof(Cat))
			{
				return new CatSerializer();
			}
			if (type == typeof(Child))
			{
				return new ChildSerializer();
			}
			if (type == typeof(BotchedNetInt))
			{
				return new BotchedNetIntSerializer();
			}
			if (type == typeof(BotchedNetBool))
			{
				return new BotchedNetBoolSerializer();
			}
			if (type == typeof(BotchedNetLong))
			{
				return new BotchedNetLongSerializer();
			}
			if (type == typeof(Dog))
			{
				return new DogSerializer();
			}
			if (type == typeof(Horse))
			{
				return new HorseSerializer();
			}
			if (type == typeof(Junimo))
			{
				return new JunimoSerializer();
			}
			if (type == typeof(JunimoHarvester))
			{
				return new JunimoHarvesterSerializer();
			}
			if (type == typeof(Pet))
			{
				return new PetSerializer();
			}
			if (type == typeof(FishPond))
			{
				return new FishPondSerializer();
			}
			if (type == typeof(GreenhouseBuilding))
			{
				return new GreenhouseBuildingSerializer();
			}
			if (type == typeof(JunimoHut))
			{
				return new JunimoHutSerializer();
			}
			if (type == typeof(Mill))
			{
				return new MillSerializer();
			}
			if (type == typeof(ShippingBin))
			{
				return new ShippingBinSerializer();
			}
			if (type == typeof(Stable))
			{
				return new StableSerializer();
			}
			if (type == typeof(Building))
			{
				return new BuildingSerializer();
			}
			if (type == typeof(Coop))
			{
				return new CoopSerializer();
			}
			if (type == typeof(Barn))
			{
				return new BarnSerializer();
			}
			if (type == typeof(SandDuggy))
			{
				return new SandDuggySerializer();
			}
			if (type == typeof(SandDuggy.State))
			{
				return new StateSerializer1();
			}
			return null;
		}
	}
}
