using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Networking;
using RoR2;
using RoR2.CharacterSpeech;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerController.Boss;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.BossRandomizer.Mithrix
{
    public static class BrotherSpeechDriver_ReplaceName
    {
        class OverrideMithrixChatMessage : Chat.NpcChatMessage
        {
            string _masterName;

            public OverrideMithrixChatMessage(Chat.NpcChatMessage original, CharacterMaster master)
            {
                sender = original.sender;
                baseToken = original.baseToken;
                sound = original.sound;
                formatStringToken = original.formatStringToken;

                if (master)
                {
                    CharacterBody body = master.GetBody();
                    if (body)
                    {
                        _masterName = body.GetDisplayName();
                    }
                }

                _masterName ??= "???";
            }

            public OverrideMithrixChatMessage()
            {
            }

            public override string ConstructChatString()
            {
                return base.ConstructChatString().Replace("Mithrix", _masterName);
            }

            public override void Serialize(NetworkWriter writer)
            {
                base.Serialize(writer);

                writer.Write(_masterName);
            }

            public override void Deserialize(NetworkReader reader)
            {
                base.Deserialize(reader);

                _masterName = reader.ReadString();
            }
        }

        public static void Apply()
        {
            ChatMessageBase.chatMessageTypeToIndex.Add(typeof(OverrideMithrixChatMessage), (byte)ChatMessageBase.chatMessageIndexToType.Count);
            ChatMessageBase.chatMessageIndexToType.Add(typeof(OverrideMithrixChatMessage));

            IL.RoR2.CharacterSpeech.CharacterSpeechController.SpeakNow += CharacterSpeechController_SpeakNow;
        }

        public static void Cleanup()
        {
            ChatMessageBase.chatMessageTypeToIndex.Remove(typeof(OverrideMithrixChatMessage));
            ChatMessageBase.chatMessageIndexToType.Remove(typeof(OverrideMithrixChatMessage));

            IL.RoR2.CharacterSpeech.CharacterSpeechController.SpeakNow -= CharacterSpeechController_SpeakNow;
        }

        static void CharacterSpeechController_SpeakNow(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCall(SymbolExtensions.GetMethodInfo(() => Chat.SendBroadcastChat(default)))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Chat.NpcChatMessage originalMessage, CharacterSpeechController instance) =>
                {
                    if (ConfigManager.BossRandomizer.AnyMithrixRandomizerEnabled && BossRandomizerController.Mithrix.IsReplacedPartOfMithrixFight(instance.characterMaster.gameObject))
                    {
                        return new OverrideMithrixChatMessage(originalMessage, instance.characterMaster);
                    }

                    return originalMessage;
                });
            }
        }
    }
}
