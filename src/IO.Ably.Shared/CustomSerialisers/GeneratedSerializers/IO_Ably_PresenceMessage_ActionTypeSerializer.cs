﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IO.Ably.CustomSerialisers {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("MsgPack.Serialization.CodeDomSerializers.CodeDomSerializerBuilder", "0.6.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public class IO_Ably_PresenceMessage_ActionTypeSerializer : MsgPack.Serialization.EnumMessagePackSerializer<IO.Ably.PresenceMessage.ActionType> {
        
        public IO_Ably_PresenceMessage_ActionTypeSerializer(MsgPack.Serialization.SerializationContext context) : 
                this(context, MsgPack.Serialization.EnumSerializationMethod.ByUnderlyingValue) {
        }
        
        public IO_Ably_PresenceMessage_ActionTypeSerializer(MsgPack.Serialization.SerializationContext context, MsgPack.Serialization.EnumSerializationMethod enumSerializationMethod) : 
                base(context, enumSerializationMethod) {
        }
        
        protected override void PackUnderlyingValueTo(MsgPack.Packer packer, IO.Ably.PresenceMessage.ActionType enumValue) {
            packer.Pack(((int)(enumValue)));
        }
        
        protected override IO.Ably.PresenceMessage.ActionType UnpackFromUnderlyingValue(MsgPack.MessagePackObject messagePackObject) {
            return ((IO.Ably.PresenceMessage.ActionType)(messagePackObject.AsInt32()));
        }
        
        private static T @__Conditional<T>(bool condition, T whenTrue, T whenFalse)
         {
            if (condition) {
                return whenTrue;
            }
            else {
                return whenFalse;
            }
        }
    }
}
