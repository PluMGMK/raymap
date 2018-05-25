﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LibR3 {
    public class R3Family {
        public R3Pointer offset;
        public R3Pointer off_family_next;
        public R3Pointer off_family_prev;
        public R3Pointer off_family_unk; // at this offset, start and end pointers appear again
        public uint family_index;
        public R3Pointer off_states_first;
        public R3Pointer off_states_last;
        public uint num_states;
        public R3Pointer off_preloadAnim_first; // (0x10 blocks: next, prev, list end, a3d pointer)
        public R3Pointer off_preloadAnim_last;
        public uint num_preloadAnim;
        public R3Pointer off_physical_list;
        public R3Pointer off_bounding_volume;
        public R3Pointer off_vector4s;
        public uint num_vector4s;
        public byte animBank;
        public byte properties;

        public R3State[] states = null;
        public R3PhysicalObject[] physical_objects = null;

        public string name;
        private GameObject gao;
        public GameObject Gao {
            get {
                if (gao == null) {
                    gao = new GameObject("[Family] " + name);
                }
                return gao;
            }
        }

        public R3Family(R3Pointer offset) {
            this.offset = offset;
        }

        public static R3Family Read(EndianBinaryReader reader, R3Pointer offset) {
            R3Loader l = R3Loader.Loader;
            R3Family f = new R3Family(offset);
            f.off_family_next = R3Pointer.Read(reader);
            f.off_family_prev = R3Pointer.Read(reader);
            f.off_family_unk = R3Pointer.Read(reader); // at this offset, start and end pointers appear again
            f.family_index = reader.ReadUInt32();
            f.name = l.objectTypes[0][f.family_index].name;
            f.off_states_first = R3Pointer.Read(reader);
            if (l.mode != R3Loader.Mode.RaymanArenaGC) f.off_states_last = R3Pointer.Read(reader);
            f.num_states = reader.ReadUInt32();
            f.off_preloadAnim_first = R3Pointer.Read(reader); // (0x10 blocks: next, prev, list end, a3d pointer)
            if (l.mode != R3Loader.Mode.RaymanArenaGC) f.off_preloadAnim_last = R3Pointer.Read(reader);
            f.num_preloadAnim = reader.ReadUInt32();
            f.off_physical_list = R3Pointer.Read(reader); // Default objects table
            R3Pointer.Read(reader);                       // Current objects table
            if(l.mode != R3Loader.Mode.RaymanArenaGC) R3Pointer.Read(reader); // same address always?
            reader.ReadUInt32(); // always 1?
            f.off_bounding_volume = R3Pointer.Read(reader);
            if (l.mode == R3Loader.Mode.Rayman3GC || l.mode == R3Loader.Mode.Rayman3PC) {
                f.off_vector4s = R3Pointer.Read(reader);
                f.num_vector4s = reader.ReadUInt32();
                reader.ReadUInt32();
            }

            reader.ReadUInt32();
            reader.ReadByte();
            reader.ReadByte();
            f.animBank = reader.ReadByte();
            f.properties = reader.ReadByte();

            f.states = new R3State[f.num_states];
            if (f.num_states > 0) {
                R3Pointer off_states_current = f.off_states_first;
                for (int i = 0; i < f.num_states; i++) {
                    R3Pointer.Goto(ref reader, off_states_current);
                    f.states[i] = R3State.Read(reader, off_states_current, f);
                    if (l.mode == R3Loader.Mode.RaymanArenaGC) {
                        off_states_current = f.states[i].offset + 0x28;
                    } else {
                        off_states_current = f.states[i].off_state_next;
                    }
                }
            }

            if (f.off_physical_list != null) {
                R3Pointer.Goto(ref reader, f.off_physical_list);
                if (l.mode == R3Loader.Mode.Rayman3GC) {
                    reader.ReadUInt32(); // 0
                    reader.ReadUInt32(); // 0
                    R3Pointer off_list_hdr_ptr = R3Pointer.Read(reader);
                } else if (l.mode == R3Loader.Mode.Rayman3PC || l.mode == R3Loader.Mode.RaymanArenaPC) {
                    reader.ReadUInt32(); // 0
                }
                R3Pointer off_list_start = R3Pointer.Read(reader);
                R3Pointer off_list_2 = R3Pointer.Read(reader); // is this a copy of the list or something?
                ushort num_entries = reader.ReadUInt16();
                reader.ReadUInt16();
                if (l.mode == R3Loader.Mode.Rayman3PC || l.mode == R3Loader.Mode.Rayman3GC) {
                    R3Pointer off_list_hdr_1 = R3Pointer.Read(reader); // copy of off_subblocklist?
                    R3Pointer off_list_hdr_2 = R3Pointer.Read(reader); // same?
                    reader.ReadUInt32(); // 1?
                }
                R3Pointer.Goto(ref reader, off_list_start);
                f.physical_objects = new R3PhysicalObject[num_entries];
                for (uint i = 0; i < num_entries; i++) {
                    // each entry is 0x14
                    R3Pointer off1 = R3Pointer.Read(reader);
                    R3Pointer off_subblock = R3Pointer.Read(reader);
                    reader.ReadUInt32();
                    reader.ReadUInt32();
                    uint lastvalue = reader.ReadUInt32();
                    if (lastvalue != 0 && off_subblock != null) {

                        R3Pointer curPos = R3Pointer.Goto(ref reader, off_subblock);
                        R3PhysicalObject subobj = R3PhysicalObject.Read(reader, off_subblock);
                        if (subobj != null) {
                            f.physical_objects[i] = subobj;
                            subobj.Gao.transform.parent = f.Gao.transform;
                            /*if (subobj.visualSet.Count > 0 && subobj.visualSet[0].obj is R3Mesh) {
                                GameObject meshGAO = ((R3Mesh)subobj.visualSet[0].obj).gao;
                                meshGAO.transform.parent = f.Gao.transform;
                            }*/
                        }
                        R3Pointer.Goto(ref reader, curPos);
                    }
                }

                /*for (uint i = 0; i < num_entries; i++) {
                    R3PhysicalObject o = subblocks[i];
                    if (o != null && o.visualSet.Count > 0 && o.visualSet[0].obj is R3Unknown) {
                        R3Unknown a = (R3Unknown)o.visualSet[0].obj;
                        if (a.off_model != null) {
                            R3PhysicalObject po = subblocks.Where(
                                s => s != null
                                && s.visualSet.Count > 0
                                && s.visualSet[0].obj is R3Mesh
                                && ((R3Mesh)s.visualSet[0].obj).off_modelstart == a.off_model
                            ).FirstOrDefault();
                            if (po != null) {
                                ((R3Mesh)po.visualSet[0].obj).listUnknown.Add(a);
                                //((R3MeshObject)model).gao.name += "!";
                            }
                        }
                    }
                }*/
            }
            return f;
        }

        public static R3Family FromOffset(R3Pointer offset) {
            if (offset == null) return null;
            R3Loader l = R3Loader.Loader;
            return l.families.FirstOrDefault(f => f.offset == offset);
        }
    }
}
