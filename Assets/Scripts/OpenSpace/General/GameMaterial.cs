﻿using OpenSpace.Animation;
using OpenSpace.Collide;
using OpenSpace.Visual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OpenSpace {
    public class GameMaterial {
        public Pointer offset;

        public Pointer off_visualMaterial;
        public Pointer off_mechanicsMaterial;
        public Pointer off_soundMaterial;
        public Pointer off_collideMaterial;
        public int hasSoundMaterial;
        public int hasCollideMaterial;

        public VisualMaterial visualMaterial;
        public CollideMaterial collideMaterial;

        public GameMaterial(Pointer offset) {
            this.offset = offset;
        }

        public static GameMaterial Read(EndianBinaryReader reader, Pointer offset) {
            MapLoader l = MapLoader.Loader;
            GameMaterial gm = new GameMaterial(offset);

            if (Settings.s.engineMode == Settings.EngineMode.R2) {
                gm.off_visualMaterial = Pointer.Read(reader);
                gm.off_mechanicsMaterial = Pointer.Read(reader);
            }
            // Very ugly code incoming
            Pointer off_sndPtr = Pointer.Current(reader);
            gm.hasSoundMaterial = reader.ReadInt32();
            Pointer off_collidePtr = Pointer.Current(reader);
            gm.hasCollideMaterial = reader.ReadInt32();

            if(gm.hasSoundMaterial != -1) gm.off_soundMaterial = Pointer.GetPointerAtOffset(off_sndPtr);
            if (gm.hasCollideMaterial != -1) gm.off_collideMaterial = Pointer.GetPointerAtOffset(off_collidePtr);

            if (gm.off_visualMaterial != null) {
                gm.visualMaterial = VisualMaterial.FromOffsetOrRead(gm.off_visualMaterial, reader);
            }
            if (gm.off_collideMaterial != null) {
                gm.collideMaterial = CollideMaterial.FromOffsetOrRead(gm.off_collideMaterial, reader);
            }
            return gm;
        }

        public static GameMaterial FromOffsetOrRead(Pointer offset, EndianBinaryReader reader) {
            GameMaterial gm = FromOffset(offset);
            if (gm == null) {
                Pointer off_current = Pointer.Goto(ref reader, offset);
                gm = GameMaterial.Read(reader, offset);
                Pointer.Goto(ref reader, off_current);
                MapLoader.Loader.gameMaterials.Add(gm);
            }
            return gm;
        }

        public static GameMaterial FromOffset(Pointer offset) {
            MapLoader l = MapLoader.Loader;
            for (int i = 0; i < l.gameMaterials.Count; i++) {
                if (offset == l.gameMaterials[i].offset) return l.gameMaterials[i];
            }
            return null;
        }
    }
}
