﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OpenSpace.AI {
    public class DsgVarInfoEntry {
        public Pointer offset;

        public uint offsetInBuffer; // offset in DsgMemBuffer
        public uint typeNumber;
        public uint saveType;
        public uint initType;

        // Derived values
        public DsgVarType type;
        public object value;

        public DsgVarInfoEntry(Pointer offset) {
            this.offset = offset;
        }

        public static DsgVarInfoEntry Read(EndianBinaryReader reader, Pointer offset) {
            MapLoader l = MapLoader.Loader;
            DsgVarInfoEntry d = new DsgVarInfoEntry(offset);
            d.offsetInBuffer = reader.ReadUInt32();
            d.typeNumber = reader.ReadUInt32();
            d.saveType = reader.ReadUInt32();
            d.initType = reader.ReadUInt32();

            d.type = DsgVarType.None;
            if (Settings.s.engineMode == Settings.EngineMode.R2) {
                if (d.typeNumber >= 0 && d.typeNumber < R2AITypes.dsgVarTypeTable.Count) d.type = R2AITypes.dsgVarTypeTable[(int)d.typeNumber];
            } else {
                if (d.typeNumber >= 0 && d.typeNumber < R3AITypes.dsgVarTypeTable.Count) d.type = R3AITypes.dsgVarTypeTable[(int)d.typeNumber];
            }

            return d;
        }

        public enum DsgVarType {
            None,
            Boolean,
            Byte,
            UByte, // Unsigned
            Short,
            UShort, // Unsigned
            Int,
            UInt, // Unsigned
            Float,
            Vector,
            List,
            Comport,
            Action,
            Caps, // Capabilities
            Input,
            SoundEvent,
            Light,
            GameMaterial,
            VisualMaterial,
            Perso,
            Waypoint,
            Graph,
            Text,
            SuperObject,
            SOLinks,
            Array1,
            Array2,
            Array3,
            Array4,
            Array5,
            Array6,
            Array7,
            Array8,
            Array9,
            Array10,
            Array11
        }
    }
}