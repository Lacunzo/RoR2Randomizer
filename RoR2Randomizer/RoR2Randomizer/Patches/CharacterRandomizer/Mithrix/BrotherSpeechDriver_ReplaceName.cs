using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterSpeech;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.CharacterRandomizer.Mithrix
{
    public static class BrotherSpeechDriver_ReplaceName
    {
        class OverrideMithrixChatMessage : Chat.NpcChatMessage
        {
            CharacterMaster _master;

            public OverrideMithrixChatMessage(Chat.NpcChatMessage original, CharacterMaster master)
            {
                sender = original.sender;
                baseToken = original.baseToken;
                sound = original.sound;
                formatStringToken = original.formatStringToken;

                _master = master;
            }

            public OverrideMithrixChatMessage()
            {
            }

            public override string ConstructChatString()
            {
                string chatString = base.ConstructChatString();

                if (_master)
                {
                    CharacterBody body = _master.GetBody();
                    if (body)
                    {
                        chatString = chatString.Replace("Mithrix", body.GetDisplayName() ?? "???");
                    }
                }

                return chatString;
            }

            public override void Serialize(NetworkWriter writer)
            {
                base.Serialize(writer);

                writer.Write(_master.gameObject);
            }

            public override void Deserialize(NetworkReader reader)
            {
                base.Deserialize(reader);

                _master = reader.ReadGameObject().GetComponent<CharacterMaster>();
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
                    if (Main.MITHRIX_RANDOMIZER_ENABLED && instance.characterMaster.GetComponent<SpawnHook.MithrixReplacement>())
                    {
                        return new OverrideMithrixChatMessage(originalMessage, instance.characterMaster);
                    }

                    return originalMessage;
                });
            }
        }
    }
}
