﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OpenSpace.Animation.Component {
    public class AnimOnlyFrame {
        public ushort quaternion;
        public ushort vector;
        public ushort num_hierarchies_for_frame;
        public ushort start_hierarchies_for_frame;
        public ushort unk8;
        public ushort deformation;
        public ushort numOfNTTO;

        public AnimOnlyFrame() {}

        public static AnimOnlyFrame Read(Reader reader) {
            MapLoader l = MapLoader.Loader;
            AnimOnlyFrame of = new AnimOnlyFrame();
            of.quaternion = reader.ReadUInt16();
            of.vector = reader.ReadUInt16();
            of.num_hierarchies_for_frame = reader.ReadUInt16();
            of.start_hierarchies_for_frame = reader.ReadUInt16();
            if (Settings.s.engineVersion == Settings.EngineVersion.R3) {
                of.unk8 = reader.ReadUInt16();
                of.deformation = reader.ReadUInt16();
            }
            of.numOfNTTO = reader.ReadUInt16();
            if (Settings.s.engineVersion == Settings.EngineVersion.TT) {
                of.unk8 = reader.ReadUInt16();
            }
            return of;
        }

        public static int Size {
            get {
                switch (Settings.s.engineVersion) {
                    case Settings.EngineVersion.R3: return 14;
                    case Settings.EngineVersion.TT: return 12;
                    default: return 10;
                }
            }
        }

        public static bool Aligned {
            get { return false; }
        }
    }
}
