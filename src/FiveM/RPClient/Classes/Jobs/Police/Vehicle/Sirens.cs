using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Enums.Controls;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Police.Vehicle
{
    // TODO: add net sync with triggers (not too much work)
    static class Sirens
    {
        static List<string> SirenModes = new List<string>()
        {
            "", // No siren - lights only
            "VEHICLES_HORNS_SIREN_1",
            "VEHICLES_HORNS_SIREN_2"
        };

        // Initial siren preset
        static string CurrentSirenPreset = "VEHICLES_HORNS_SIREN_1";

        // One per player
        // More is not needed
        static Dictionary<int, int> SirenSoundIds = new Dictionary<int, int>();

        static bool SirenActive = false;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            Client.GetInstance().RegisterEventHandler("Jobs.Sirens.SoundEvent", new Action<string>(ReceiveSoundEvent));
            API.DecorRegister("Vehicle.SirensInstalled", 2); // 2 == bool
        }

        static private async Task OnTick()
        {
            if (!SirenActive && Game.PlayerPed.IsInVehicle()
                && Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed
                && (((Game.PlayerPed.CurrentVehicle.Model.IsCar
                    || Game.PlayerPed.CurrentVehicle.Model.IsBike
                    || Game.PlayerPed.CurrentVehicle.Model.IsBoat)
                    && API.DecorGetBool(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled"))
                    || Game.PlayerPed.IsInPoliceVehicle))
            {
                if (ControlHelper.IsControlPressed(Control.Sprint, true, ControlModifier.Shift))
                {
                    SirenActive = true;
                    SendSoundEvent("SIRENS_AIRHORN");
                    while (ControlHelper.IsControlPressed(Control.Sprint, true, ControlModifier.Shift) && Game.PlayerPed.IsInVehicle())
                    {
                        await BaseScript.Delay(0);
                    }
                    StopSound();
                    SirenActive = false;
                }
                else if (ControlHelper.IsControlPressed(Control.Duck, true, ControlModifier.Ctrl))
                {
                    SirenActive = true;
                    SendSoundEvent("VEHICLES_HORNS_POLICE_WARNING");
                    while (ControlHelper.IsControlPressed(Control.Duck, true, ControlModifier.Ctrl) && Game.PlayerPed.IsInVehicle())
                    {
                        await BaseScript.Delay(0);
                    }
                    StopSound();
                    SirenActive = false;
                }
                else if (ControlHelper.IsControlJustPressed(Control.SpecialAbilitySecondary, true, ControlModifier.Any))
                {
                    SirenActive = true;
                    Function.Call(Hash.BLIP_SIREN, Game.PlayerPed.CurrentVehicle.Handle);
                    await BaseScript.Delay(700);
                    SirenActive = false;
                }
                else if (ControlHelper.IsControlJustPressed(Control.ThrowGrenade)) // Preset on/off
                {
                    CruiseControl.CruiseControlDisabled = true;
                    Function.Call(Hash.DISABLE_VEHICLE_IMPACT_EXPLOSION_ACTIVATION, Game.PlayerPed.CurrentVehicle.Handle, true);
                    Function.Call(Hash.SET_VEHICLE_SIREN, Game.PlayerPed.CurrentVehicle.Handle, true);
                    Function.Call(Hash.SET_SIREN_WITH_NO_DRIVER, Game.PlayerPed.CurrentVehicle.Handle, true);

                    SirenActive = true;
                    PlayCurrentPresetSound();

                    while (Game.PlayerPed.IsInVehicle()) 
                    {
                        await BaseScript.Delay(0);
                        if(ControlHelper.IsControlJustPressed(Control.ThrowGrenade))
                        {
                            break;
                        }
                        else if(ControlHelper.IsControlJustPressed(Control.MpTextChatTeam)) // Cycle presets
                        {
                            StopSound();
                            CurrentSirenPreset = SirenModes[(SirenModes.IndexOf(CurrentSirenPreset) + 1) % SirenModes.Count];
                            PlayCurrentPresetSound();
                        }
                        else if (ControlHelper.IsControlPressed(Control.Sprint, true, ControlModifier.Shift))
                        {
                            StopSound();
                            SendSoundEvent("SIRENS_AIRHORN");
                            while (ControlHelper.IsControlPressed(Control.Sprint, true, ControlModifier.Shift) && Game.PlayerPed.IsInVehicle())
                            {
                                await BaseScript.Delay(0);
                            }
                            StopSound();
                            PlayCurrentPresetSound();
                        }
                        else if (ControlHelper.IsControlPressed(Control.Duck, true, ControlModifier.Ctrl))
                        {
                            StopSound();
                            SendSoundEvent("VEHICLES_HORNS_POLICE_WARNING");
                            while (ControlHelper.IsControlPressed(Control.Duck, true, ControlModifier.Ctrl) && Game.PlayerPed.IsInVehicle())
                            {
                                await BaseScript.Delay(0);
                            }
                            StopSound();
                            PlayCurrentPresetSound();
                        }
                        else if (ControlHelper.IsControlPressed(Control.SpecialAbilitySecondary, true, ControlModifier.Any))
                        {
                            StopSound();
                            Function.Call(Hash.BLIP_SIREN, Game.PlayerPed.CurrentVehicle.Handle);
                            await BaseScript.Delay(700);
                            PlayCurrentPresetSound();
                        }
                    }
                    StopSound();
                    // TODO: Figure out if this is needed
                    //Function.Call(Hash.SET_VEHICLE_SIREN, Game.PlayerPed.CurrentVehicle.Handle, false);
                    SirenActive = false;
                    CruiseControl.CruiseControlDisabled = false;
                }
            }
            else if(!Game.PlayerPed.IsInVehicle())
            {
                SirenActive = false;
                CruiseControl.CruiseControlDisabled = false;
            }
        }

        static void PlaySound(int sourceServerId, string sound)
        {
            SirenSoundIds[Game.Player.ServerId] = Function.Call<int>(Hash.GET_SOUND_ID);
            Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, SirenSoundIds[Game.Player.ServerId], sound, Game.PlayerPed.CurrentVehicle.Handle, 0, 0, 0);
        }

        class SoundEventModel
        {
            public string SoundName { get; set; } = ""; // Empty sound name will be our stop event
            public int SourceServerId { get; set; }
            public SoundEventModel() { }
        }

        static void SendSoundEvent(string sound)
        {
            string serializedSoundEvent = Helpers.MsgPack.Serialize(new SoundEventModel { SoundName = sound, SourceServerId = Game.Player.ServerId });
            string serializedEvent = Helpers.MsgPack.Serialize(new TriggerEventForAllModel("Jobs.Sirens.SoundEvent", serializedSoundEvent));
            BaseScript.TriggerServerEvent("TriggerEventForAll", serializedEvent);
        }

        static void ReceiveSoundEvent(string serializedSoundEvent)
        {
            try
            {
                SoundEventModel SoundEvent = Helpers.MsgPack.Deserialize<SoundEventModel>(serializedSoundEvent);
                if (SoundEvent.SoundName == "STOP" && SirenSoundIds.ContainsKey(SoundEvent.SourceServerId) && SirenSoundIds[SoundEvent.SourceServerId] != -1)
                {
                    Function.Call(Hash.STOP_SOUND, SirenSoundIds[SoundEvent.SourceServerId]);
                    Function.Call(Hash.RELEASE_SOUND_ID, SirenSoundIds[SoundEvent.SourceServerId]);
                    SirenSoundIds[SoundEvent.SourceServerId] = -1;
                }
                else
                {
                    if (SirenSoundIds.ContainsKey(SoundEvent.SourceServerId) && SirenSoundIds[SoundEvent.SourceServerId] != -1)
                    {
                        Function.Call(Hash.STOP_SOUND, SirenSoundIds[SoundEvent.SourceServerId]);
                        Function.Call(Hash.RELEASE_SOUND_ID, SirenSoundIds[SoundEvent.SourceServerId]);
                    }
                    SirenSoundIds[SoundEvent.SourceServerId] = Function.Call<int>(Hash.GET_SOUND_ID);
                    Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, SirenSoundIds[SoundEvent.SourceServerId], SoundEvent.SoundName, new PlayerList()[SoundEvent.SourceServerId].Character.CurrentVehicle.Handle, 0, 0, 0);
                }
            }
            catch(Exception ex)
            {
                Log.Error($"Siren event error: {ex.Message}");
            }
        }

        static void PlayCurrentPresetSound()
        {
            SendSoundEvent(CurrentSirenPreset);
        }

        static void StopSound()
        {
            SendSoundEvent("STOP");
        }
    }
}