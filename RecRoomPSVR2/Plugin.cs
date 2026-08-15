using System;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using RecRoom.Core.Combat;
using RecRoom.Tools.Weapons;
using RecRoomPSVR2.PSVR2;

namespace RecRoomPSVR2
{
    // lapis docs
    // player bodypart refs: Player.GAHEJKDCLLE
    // local player: Player.MDMMDPEKICF
    // player hand holding tool: tool.LLMFLFPBJCD
    
    [BepInPlugin("lapis.recroompsvr2", "Rec Room PSVR2", "1.0.0")]
    public class Plugin : BasePlugin
    {
        private static CancellationTokenSource? _hmdRumbleCancellation;
        private static Il2CppSystem.Action<Tool>? _toolPickupCallback;
        private static Il2CppSystem.Action<Tool>? _toolDropCallback;
        
        public static ConfigEntry<bool> adaptiveTriggers;
        public static ConfigEntry<bool> hmdRumble;
        
        // prefab names, usually will have (Clone) at the end, but will be checked in the isToolWeapon check
        private static readonly System.Collections.Generic.HashSet<string> _weaponNames =
            new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

        public override void Load()
        {
            // init harmony
            var harmony = new Harmony("RecRoomPSVR2.Patches");
            harmony.PatchAll();
            
            // just use prefab names since thats what tool.name returns
            // there may be some other way to do this, but this doesnt feel too bad to do,
            // so whatever! if it works, it works!
            _weaponNames.Add("[Arena_Pistol]");
            _weaponNames.Add("[Arena_Shotgun]");
            _weaponNames.Add("[Arena_RailGun]");
            _weaponNames.Add("[Arena_AutomaticGun]");
            _weaponNames.Add("[Arena_RocketLauncher]");
            _weaponNames.Add("[Arena_PowerWeapon_BeamGun]");
            _weaponNames.Add("[PaintballAssaultRifle]");
            _weaponNames.Add("[PaintballGrenadeLauncher]");
            _weaponNames.Add("[PaintballRifleScoped]");
            _weaponNames.Add("[PaintballGun]");
            _weaponNames.Add("[PaintballShotgun]");
            _weaponNames.Add("[Paintball_PaintThrower]");
            
            // tool event setup, i hate this why do you have to do it like this
            Action<Tool> callbackPickup = OnToolPickup;
            Action<Tool> callbackDrop = OnToolDrop;
            
            _toolPickupCallback = DelegateSupport.ConvertDelegate<Il2CppSystem.Action<Tool>>(callbackPickup);
            _toolDropCallback = DelegateSupport.ConvertDelegate<Il2CppSystem.Action<Tool>>(callbackDrop);
            
            MFBGLIHGCGO<Tool>.BPJDFPACLMK(Tool.StaticLockedToolPickedUpEvent, _toolPickupCallback);
            MFBGLIHGCGO<Tool>.BPJDFPACLMK(Tool.StaticToolPostReleaseEvent, _toolDropCallback);
            
            hmdRumble = Config.Bind
            (
                "Features",
                "HMD Rumble",
                true,
                "If true, when you get headshot by a bullet, the HMD will rumble for a short time. (REQUIRES JAILBREAK TO BE ACTIVE)"
            );
            
            adaptiveTriggers = Config.Bind
            (
                "Features",
                "Adaptive Triggers",
                true,
                "If true, adaptive triggers for weapons will be enabled."
            );
            
            // init psvr2toolkit, fails if the psvr2toolkit capi api dll isnt next to the normal plugin dll
            PSVR2ToolkitCAPI.Init();
        }

        private void OnToolPickup(Tool tool)
        {
            if (!adaptiveTriggers.Value)
            {
                return;
            }
            
            // tool ref here is the player holding the tool, player ref is to get local player
            if (tool.JCGBLBCLAPI == Player.MDMMDPEKICF)
            {
                VRControllerType hand;
                
                // tool ref here is getting the hand which the tool is being held by (can be "LeftHand" or "RightHand")
                if (tool.LLMFLFPBJCD.name == "RightHand")
                {
                    hand = VRControllerType.Right;
                }
                else
                {
                    hand = VRControllerType.Left;
                }
                
                if (isToolWeapon(tool))
                {
                    Log.LogInfo("tool is a weapon");
                    string cleanToolName = tool.name.Replace("(Clone)", "");
                    switch (cleanToolName)
                    {
                        case "[Arena_Pistol]":
                            SetWeaponTrigger(hand, 5, 2, 7);
                            break;

                        case "[Arena_Railgun]":
                            SetWeaponTrigger(hand, 8, 2, 8);
                            break;
                        
                        case "[Arena_Shotgun]":
                            SetWeaponTrigger(hand, 8, 2, 8);
                            break;

                        case "[PaintballGun]":
                            SetWeaponTrigger(hand, 4, 2, 7);
                            break;

                        case "[PaintballShotgun]":
                            SetWeaponTrigger(hand, 7, 2, 8);
                            break;

                        case "[PaintballRifleScoped]":
                            SetWeaponTrigger(hand, 6, 2, 8);
                            break;

                        case "[PaintballGrenadeLauncher]":
                            SetWeaponTrigger(hand, 7, 2, 8);
                            break;
                        
                        case "[PaintballAssaultRifle]":
                            SetWeaponVibration(hand, 5, 10, 2);
                            break;
                        
                        case "[Arena_AutomaticGun]":
                            SetWeaponVibration(hand, 4, 17, 4);
                            break;
                        
                        case "[Paintball_PaintThrower]":
                            SetWeaponVibration(hand, 8, 29, 3);
                            break;
                        
                        case "[Arena_RocketLauncher]":
                            SetWeaponTrigger(hand, 8, 2, 9);
                            break;
                        
                        case "[Arena_PowerWeapon_BeamGun]":
                            SetWeaponVibration(hand, 10, 30, 5);
                            break;
                    }
                }
            }
        }

        private void OnToolDrop(Tool tool)
        {
            if (!adaptiveTriggers.Value)
            {
                return;
            }
            
            // tool ref here is the player holding the tool, player ref is to get local player
            if (tool.JCGBLBCLAPI == Player.MDMMDPEKICF)
            {
                VRControllerType hand;
                
                // tool ref here is getting the hand which the tool is being held by (can be "LeftHand" or "RightHand")
                if (tool.LLMFLFPBJCD.name == "RightHand")
                {
                    hand = VRControllerType.Right;
                }
                else
                {
                    hand = VRControllerType.Left;
                }
                
                if (isToolWeapon(tool))
                {
                    Log.LogInfo("tool is a weapon");
                    ClearTriggerEffect(hand);
                }
            }
        }

        // just a helper function to check if the tool is a weapon, so the same stuff isnt spammed a million times
        private bool isToolWeapon(Tool tool)
        {
            if (_weaponNames.Contains(tool.name) || _weaponNames.Contains(tool.name.Replace("(Clone)", "")))
                return true;
            return false;
        }
        
        // weapon trigger effect for a given hand
        public static void SetWeaponTrigger(VRControllerType hand, int strength, int startPosition, int endPosition)
        {
            if (!adaptiveTriggers.Value)
            {
                return;
            }
            
            strength = Math.Clamp(strength, 0, 8);
            startPosition = Math.Clamp(startPosition, 0, 255);
            endPosition = Math.Clamp(endPosition, 0, 255);

            var command = new ScePadTriggerEffectCommand
            {
                mode = ScePadTriggerEffectMode.SCE_PAD_TRIGGER_EFFECT_MODE_WEAPON,
                padding = 0,
                commandData = new ScePadTriggerEffectCommandData
                {
                    weaponStartPosition = (byte)startPosition,
                    weaponEndPosition = (byte)endPosition,
                    weaponStrength = (byte)strength
                }
            };

            PSVR2ToolkitCAPI.SetTriggerEffect(hand, ref command);
        }
        
        // get rid of all effects for a given hand
        public static void ClearTriggerEffect(VRControllerType hand)
        {
            if (!adaptiveTriggers.Value)
            {
                return;
            }
            
            var command = new ScePadTriggerEffectCommand
            {
                mode = ScePadTriggerEffectMode.SCE_PAD_TRIGGER_EFFECT_MODE_OFF,
                padding = 0,
                commandData = new ScePadTriggerEffectCommandData { }
            };

            PSVR2ToolkitCAPI.SetTriggerEffect(hand, ref command);
        }

        // vibration trigger effect for a given hand (used for paint thrower/assult rifles)
        public static void SetWeaponVibration(VRControllerType hand, int amplitude, int freq, int pos)
        {
            if (!adaptiveTriggers.Value)
            {
                return;
            }
            
            amplitude = Math.Clamp(amplitude, 0, 8);
            freq = Math.Clamp(freq, 0, 255);
            pos = Math.Clamp(pos, 0, 255);

            var command = new ScePadTriggerEffectCommand
            {
                mode = ScePadTriggerEffectMode.SCE_PAD_TRIGGER_EFFECT_MODE_VIBRATION,
                padding = 0,
                commandData = new ScePadTriggerEffectCommandData
                {
                    vibrationAmplitude = (byte)amplitude,
                    vibrationFrequency = (byte)freq,
                    vibrationPosition = (byte)pos
                }
            };

            PSVR2ToolkitCAPI.SetTriggerEffect(hand, ref command);
        }

        public static void TickFeedback(Tool tool)
        {
            if (!adaptiveTriggers.Value)
            {
                return;
            }
            
            VRControllerType hand;
            
            if (tool.LLMFLFPBJCD.name == "RightHand")
            {
                hand = VRControllerType.Right;
            }
            else
            {
                hand = VRControllerType.Left;
            }
            
            SetWeaponVibration(hand, 8, 1, 3);
            ClearTriggerEffect(hand);
        }

        public static async void HeadshotHMDFeedback()
        {
            if (!hmdRumble.Value)
            {
                return;
            }
            
            _hmdRumbleCancellation?.Cancel();
            _hmdRumbleCancellation?.Dispose();

            _hmdRumbleCancellation = new CancellationTokenSource();
            var token = _hmdRumbleCancellation.Token;

            PSVR2ToolkitCAPI.SetHmdRumble(25);

            try
            {
                await Task.Delay(500, token);

                if (!token.IsCancellationRequested)
                    PSVR2ToolkitCAPI.SetHmdRumble(0);
            }
            catch (TaskCanceledException)
            {
                
            }
        }
    }
}