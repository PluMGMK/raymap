﻿using UnityEngine;
using System.Collections;
using System;

namespace OpenSpace.Input {
    public class InputFunctions {

        public enum FunctionType {
            Unknown,
            And,
            Or,
            Not,
            KeyJustPressed,
            KeyJustReleased,
            KeyPressed,
            KeyReleased,
            ActionJustValidated,
            ActionJustInvalidated,
            ActionValidated,
            ActionInvalidated,
            PadJustPressed,
            PadJustReleased,
            PadPressed,
            PadReleased,
            JoystickAxeValue,
            JoystickAngularValue,
            JoystickTrueNormValue,
            JoystickCorrectedNormValue,
            JoystickJustPressed,
            JoystickJustReleased,
            JoystickPressed,
            JoystickReleased,
            JoystickOrPadJustPressed,
            JoystickOrPadJustReleased,
            JoystickOrPadPressed,
            JoystickOrPadReleased,
            MouseAxeValue,
            MouseAxePosition,
            MouseJustPressed,
            MouseJustReleased,
            MousePressed,
            Sequence,
            SequenceKey,
            SequenceKeyEnd,
            SequencePad,
            SequencePadEnd
        }
		
        public static FunctionType[] functionTypesSE = new FunctionType[] {
            FunctionType.Unknown,
            FunctionType.And,
            FunctionType.Or,
            FunctionType.Not,
            FunctionType.KeyJustPressed,
            FunctionType.KeyJustReleased,
            FunctionType.KeyPressed,
            FunctionType.KeyReleased,
            FunctionType.ActionJustValidated,
            FunctionType.ActionJustInvalidated,
            FunctionType.ActionValidated,
            FunctionType.ActionInvalidated,
            FunctionType.PadJustPressed,
            FunctionType.PadJustReleased,
            FunctionType.PadPressed,
            FunctionType.PadReleased,
            FunctionType.JoystickAxeValue,

            FunctionType.JoystickJustPressed,
            FunctionType.JoystickJustReleased,
            FunctionType.JoystickPressed,
            FunctionType.JoystickReleased,
            FunctionType.JoystickOrPadJustPressed,
            FunctionType.JoystickOrPadJustReleased,
            FunctionType.JoystickOrPadPressed,
            FunctionType.JoystickOrPadReleased,

            FunctionType.Sequence,
            FunctionType.SequenceKey,
            FunctionType.SequenceKeyEnd,
            FunctionType.SequencePad,
            FunctionType.SequencePadEnd
        };


        public static FunctionType[] functionTypesTT = new FunctionType[] {
            FunctionType.Unknown,
            FunctionType.And,
            FunctionType.Or,
            FunctionType.Not,
            FunctionType.KeyJustPressed,
            FunctionType.KeyJustReleased,
            FunctionType.KeyPressed,
            FunctionType.KeyReleased,
            FunctionType.ActionJustValidated,
            FunctionType.ActionJustInvalidated,
            FunctionType.ActionValidated,
            FunctionType.ActionInvalidated,

            FunctionType.PadJustPressed,
            FunctionType.PadJustReleased,
            FunctionType.PadPressed,
            FunctionType.PadReleased,

            FunctionType.JoystickAxeValue,
            FunctionType.JoystickAngularValue,
            FunctionType.JoystickTrueNormValue,
            FunctionType.JoystickCorrectedNormValue,

            FunctionType.JoystickJustPressed,
            FunctionType.JoystickJustReleased,
            FunctionType.JoystickPressed,
            FunctionType.JoystickReleased,
            FunctionType.JoystickOrPadJustPressed,
            FunctionType.JoystickOrPadJustReleased,
            FunctionType.JoystickOrPadPressed,
            FunctionType.JoystickOrPadReleased,

            FunctionType.MouseAxeValue,
            FunctionType.MouseAxePosition,
            FunctionType.MouseJustPressed,
            FunctionType.MouseJustReleased,
            FunctionType.MousePressed,

            FunctionType.Sequence,
            FunctionType.SequenceKey,
            FunctionType.SequenceKeyEnd,
            FunctionType.SequencePad,
            FunctionType.SequencePadEnd
        };


        public static FunctionType[] functionTypesLargo = new FunctionType[] {
            FunctionType.Unknown,
            FunctionType.And,
            FunctionType.Or,
            FunctionType.Not,
            FunctionType.KeyJustPressed,
            FunctionType.KeyJustReleased,
            FunctionType.KeyPressed,
            FunctionType.KeyReleased,
            FunctionType.ActionJustValidated,
            FunctionType.ActionJustInvalidated,
            FunctionType.ActionValidated,
            FunctionType.ActionInvalidated,

            FunctionType.PadJustPressed,
            FunctionType.PadJustReleased,
            FunctionType.PadPressed,
            FunctionType.PadReleased,

            FunctionType.JoystickAxeValue,

            FunctionType.JoystickJustPressed,
            FunctionType.JoystickJustReleased,
            FunctionType.JoystickPressed,
            FunctionType.JoystickReleased,
            FunctionType.JoystickOrPadJustPressed,
            FunctionType.JoystickOrPadJustReleased,
            FunctionType.JoystickOrPadPressed,
            FunctionType.JoystickOrPadReleased,

            FunctionType.Sequence,
            FunctionType.SequenceKey,
            FunctionType.SequenceKeyEnd,
            FunctionType.SequencePad,
            FunctionType.SequencePadEnd
        };

        public static FunctionType[] functionTypesHype = new FunctionType[] {
            FunctionType.Unknown,
            FunctionType.And,
            FunctionType.Or,
            FunctionType.Not,
            FunctionType.KeyJustPressed,
            FunctionType.KeyJustReleased,
            FunctionType.KeyPressed,
            FunctionType.KeyReleased,
            FunctionType.ActionJustValidated,
            FunctionType.ActionJustInvalidated,
            FunctionType.ActionValidated,
            FunctionType.ActionInvalidated,
            FunctionType.PadJustPressed,
            FunctionType.PadJustReleased,
            FunctionType.PadPressed,
            FunctionType.PadReleased,
            FunctionType.JoystickAxeValue,
            FunctionType.JoystickAngularValue,
            FunctionType.JoystickTrueNormValue,
            FunctionType.JoystickCorrectedNormValue,
            FunctionType.JoystickJustPressed,
            FunctionType.JoystickJustReleased,
            FunctionType.JoystickPressed,
            FunctionType.JoystickReleased,
            FunctionType.JoystickOrPadJustPressed,
            FunctionType.JoystickOrPadJustReleased,
            FunctionType.JoystickOrPadPressed,
            FunctionType.JoystickOrPadReleased,
            FunctionType.MouseAxeValue,
            FunctionType.MouseAxePosition,
            FunctionType.MouseJustPressed,
            FunctionType.MouseJustReleased,
            FunctionType.MousePressed,
            FunctionType.Sequence,
            FunctionType.SequenceKey,
            FunctionType.SequenceKeyEnd,
            FunctionType.SequencePad,
            FunctionType.SequencePadEnd
        };

		public static FunctionType[] functionTypesDC = new FunctionType[] {
			FunctionType.Unknown,
			FunctionType.And,
			FunctionType.Or,
			FunctionType.Not,
			FunctionType.KeyJustPressed,
			FunctionType.KeyJustReleased,
			FunctionType.KeyPressed,
			FunctionType.KeyReleased,
			FunctionType.ActionJustValidated,
			FunctionType.ActionJustInvalidated,
			FunctionType.ActionValidated,
			FunctionType.ActionInvalidated,
			FunctionType.PadJustPressed,
			FunctionType.PadJustReleased,
			FunctionType.PadPressed,
			FunctionType.PadReleased,
			FunctionType.JoystickAxeValue,
			FunctionType.JoystickJustPressed,
			FunctionType.JoystickJustReleased,
			FunctionType.JoystickPressed,
			FunctionType.JoystickReleased,
			FunctionType.JoystickOrPadJustPressed,
			FunctionType.JoystickOrPadJustReleased,
			FunctionType.JoystickOrPadPressed,
			FunctionType.JoystickOrPadReleased,
			FunctionType.Sequence,
			FunctionType.SequenceKey,
			FunctionType.SequenceKeyEnd,
			FunctionType.SequencePad,
			FunctionType.SequencePadEnd,
		};

		public static FunctionType[] functionTypesGC = new FunctionType[] {
			FunctionType.Unknown,
			FunctionType.And,
			FunctionType.Or,
			FunctionType.Not,
			FunctionType.ActionJustValidated,
			FunctionType.ActionJustInvalidated,
			FunctionType.ActionValidated,
			FunctionType.ActionInvalidated,
			FunctionType.PadJustPressed,
			FunctionType.PadJustReleased,
			FunctionType.PadPressed,
			FunctionType.PadReleased,
			FunctionType.JoystickAxeValue,
			FunctionType.JoystickAngularValue,
			FunctionType.JoystickTrueNormValue,
			FunctionType.JoystickCorrectedNormValue,
			FunctionType.JoystickJustPressed,
			FunctionType.JoystickJustReleased,
			FunctionType.JoystickPressed,
			FunctionType.JoystickReleased,
			FunctionType.JoystickOrPadJustPressed,
			FunctionType.JoystickOrPadJustReleased,
			FunctionType.JoystickOrPadPressed,
			FunctionType.JoystickOrPadReleased,
			FunctionType.Sequence,
			FunctionType.SequenceKey,
			FunctionType.SequenceKeyEnd,
			FunctionType.SequencePad,
			FunctionType.SequencePadEnd,
		};

        public static FunctionType[] functionTypesPS2 = new FunctionType[] {
            FunctionType.Unknown,
            FunctionType.And,
            FunctionType.Or,
            FunctionType.Not,
            FunctionType.ActionJustValidated,
            FunctionType.ActionJustInvalidated,
            FunctionType.ActionValidated,
            FunctionType.ActionInvalidated,
            FunctionType.PadJustPressed,
            FunctionType.PadJustReleased,
            FunctionType.PadPressed,
            FunctionType.PadReleased,
            FunctionType.JoystickAxeValue,
            FunctionType.JoystickAngularValue,
            FunctionType.JoystickTrueNormValue,
            FunctionType.JoystickCorrectedNormValue,
            FunctionType.JoystickJustPressed,
            FunctionType.JoystickJustReleased,
            FunctionType.JoystickPressed,
            FunctionType.JoystickReleased,
            FunctionType.JoystickOrPadJustPressed,
            FunctionType.JoystickOrPadJustReleased,
            FunctionType.JoystickOrPadPressed,
            FunctionType.JoystickOrPadReleased,
            FunctionType.MousePressed,
            FunctionType.Sequence,
            FunctionType.SequenceKey,
            FunctionType.SequenceKeyEnd,
            FunctionType.SequencePad,
            FunctionType.SequencePadEnd,
        };

        public static FunctionType GetFunctionType(uint index) {
            try {
				if (Settings.s.game == Settings.Game.TTSE) return functionTypesSE[index];
                if (Settings.s.game == Settings.Game.TT) return functionTypesTT[index];
                if (Settings.s.game == Settings.Game.LargoWinch) return functionTypesLargo[index];
                if (Settings.s.engineVersion == Settings.EngineVersion.Montreal) return functionTypesHype[index];
				if (Settings.s.platform == Settings.Platform.GC) return functionTypesGC[index];
				if (Settings.s.platform == Settings.Platform.DC || Settings.s.platform == Settings.Platform.iOS) return functionTypesDC[index];
                if (Settings.s.engineVersion > Settings.EngineVersion.R2 && 
                    Settings.s.platform == Settings.Platform.PS2) return functionTypesPS2[index];
                return (FunctionType)(index);
            } catch (Exception) {
                return FunctionType.Unknown;
            }
        }
    }
}