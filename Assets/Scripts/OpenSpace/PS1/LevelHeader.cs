﻿using OpenSpace.FileFormat.Texture;
using OpenSpace.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenSpace.PS1 {
	public class LevelHeader : OpenSpaceStruct {
		public Pointer off_dynamicWorld;
		public Pointer off_fatherSector;
		public Pointer off_inactiveDynamicWorld;
		public uint num_always;
		public Pointer off_always;
		public uint uint_104;
		public uint uint_108;
		public Pointer off_10C;
		public Pointer off_110;
		public ushort num_persos;
		public ushort ushort_116;
		public Pointer off_persos;
		public Pointer off_states;
		public ushort num_states;
		public ushort ushort_122;
		public uint uint_124;
		public uint uint_128;
		public Pointer off_12C;
		public Pointer off_130;
		public uint uint_134;
		public uint uint_138;
		public uint uint_13C;
		public uint uint_140;
		public uint uint_144;
		public int int_148;
		public Pointer off_vectors;
		public Pointer off_quaternions;
		public Pointer off_hierarchies;

		// UI Textures
		public byte num_ui_textures;
		public byte byte_159;
		public ushort ushort_15A;
		public Pointer off_ui_textures_names;
		public Pointer off_ui_textures_width;
		public Pointer off_ui_textures_height;
		public Pointer off_ui_textures_pageInfo;
		public Pointer off_ui_textures_palette;
		public Pointer off_ui_textures_xInPage;
		public Pointer off_ui_textures_yInPage;

		public Pointer off_178;
		public int int_17C;

		public Pointer off_geometricObjects_dynamic;
		public Pointer off_geometricObjects_static;
		public uint num_geometricObjects_dynamic;

		public ushort ushort_18C;
		public ushort ushort_18E;
		public short short_190;
		public ushort ushort_192;
		public uint num_ipos;
		public Pointer off_ipos;
		public uint uint_19C;
		public uint bad_off_1A0;
		public Pointer off_sectors;
		public ushort num_sectors;

		// Parsed
		public PointerList<State> states;
		public SuperObject fatherSector;
		public SuperObject dynamicWorld;
		public SuperObject inactiveDynamicWorld;
		public ObjectsTable geometricObjectsStatic;
		public ObjectsTable geometricObjectsDynamic;
		public Perso[] persos;
		public AlwaysList[] always;
		public Sector[] sectors;

		protected override void ReadInternal(Reader reader) {
			reader.ReadBytes(0xF0);
			off_dynamicWorld = Pointer.Read(reader);
			off_fatherSector = Pointer.Read(reader);
			off_inactiveDynamicWorld = Pointer.Read(reader);
			num_always = reader.ReadUInt32();
			off_always = Pointer.Read(reader); //
			uint_104 = reader.ReadUInt32(); // x
			uint_108 = reader.ReadUInt32();
			off_10C = Pointer.Read(reader); // x structs of 0x14
			off_110 = Pointer.Read(reader);
			num_persos = reader.ReadUInt16();
			ushort_116 = reader.ReadUInt16();
			off_persos = Pointer.Read(reader);
			Load.print(off_dynamicWorld + " - " + off_fatherSector + " - " + off_inactiveDynamicWorld + " - " + off_always + " - " + off_10C + " - " + off_110 + " - " + off_persos);
			off_states = Pointer.Read(reader);
			num_states = reader.ReadUInt16();
			ushort_122 = reader.ReadUInt16();
			uint_124 = reader.ReadUInt32();
			uint_128 = reader.ReadUInt32();
			off_12C = Pointer.Read(reader);
			off_130 = Pointer.Read(reader);
			uint_134 = reader.ReadUInt32();
			uint_138 = reader.ReadUInt32();
			uint_13C = reader.ReadUInt32(); // same as mainChar_states count
			uint_140 = reader.ReadUInt32();
			uint_144 = reader.ReadUInt32();
			int_148 = reader.ReadInt32(); // -1
			off_vectors = Pointer.Read(reader); // 0x6 size
			off_quaternions = Pointer.Read(reader); // big array of structs of 0x8 size. 4 ushorts per struct
			off_hierarchies = Pointer.Read(reader);
			Load.print(off_12C + " - " + off_130 + " - " + off_vectors + " - " + off_quaternions + " - " + off_hierarchies);

			// Vignette stuff, big textures
			num_ui_textures = reader.ReadByte();
			byte_159 = reader.ReadByte();
			ushort_15A = reader.ReadUInt16();
			off_ui_textures_names = Pointer.Read(reader);
			off_ui_textures_width = Pointer.Read(reader); // num_vignettes * ushort
			off_ui_textures_height = Pointer.Read(reader); // num_vignettes * ushort
			off_ui_textures_pageInfo = Pointer.Read(reader); // num_vignettes * ushort
			off_ui_textures_palette = Pointer.Read(reader);
			off_ui_textures_xInPage = Pointer.Read(reader);
			off_ui_textures_yInPage = Pointer.Read(reader); // still related to the vignette stuff

			ParseUITextures(reader);

			// Something else
			off_178 = Pointer.Read(reader);
			int_17C = reader.ReadInt32(); // -1

			off_geometricObjects_dynamic = Pointer.Read(reader);
			off_geometricObjects_static = Pointer.Read(reader);
			num_geometricObjects_dynamic = reader.ReadUInt32();

			ushort_18C = reader.ReadUInt16();
			ushort_18E = reader.ReadUInt16();
			short_190 = reader.ReadInt16();
			ushort_192 = reader.ReadUInt16();
			num_ipos = reader.ReadUInt32(); // y
			off_ipos = Pointer.Read(reader); // y structs of 0x3c
			uint_19C = reader.ReadUInt32();
			bad_off_1A0 = reader.ReadUInt32(); //Pointer.Read(reader);
			off_sectors = Pointer.Read(reader); // num_1A8 structs of 0x54
			num_sectors = reader.ReadUInt16(); // actual sectors
			//Load.print(off_sector_minus_one_things + " - " + bad_off_1A0 + " - " + off_sectors);

			// Parse
			states = Load.FromOffsetOrRead<PointerList<State>>(reader, off_states, s => s.length = num_states);
			persos = Load.ReadArray<Perso>(num_persos, reader, off_persos);
			ParseSectors(reader);
			geometricObjectsDynamic = Load.FromOffsetOrRead<ObjectsTable>(reader, off_geometricObjects_dynamic, onPreRead: t => t.length = num_geometricObjects_dynamic - 2);
			geometricObjectsStatic = Load.FromOffsetOrRead<ObjectsTable>(reader, off_geometricObjects_static, onPreRead: t => t.length = num_ipos);
			always = Load.ReadArray<AlwaysList>(num_always, reader, off_always);
			sectors = Load.ReadArray<Sector>(num_sectors, reader, off_sectors);
		}

		public void ParseUITextures(Reader reader) {
			Load.print("Num UI Textures: " + num_ui_textures);

			UITexture[] textures = new UITexture[num_ui_textures];
			for (int i = 0; i < textures.Length; i++) {
				textures[i] = new UITexture();
			}
			Pointer.DoAt(ref reader, off_ui_textures_names, () => {
				for (int i = 0; i < textures.Length; i++) {
					Pointer p = Pointer.Read(reader);
					Pointer.DoAt(ref reader, p, () => {
						textures[i].name = reader.ReadString(0x1C);
					});
				}
			});
			Pointer.DoAt(ref reader, off_ui_textures_width, () => {
				for (int i = 0; i < textures.Length; i++) {
					textures[i].width = reader.ReadUInt16();
				}
			});
			Pointer.DoAt(ref reader, off_ui_textures_height, () => {
				for (int i = 0; i < textures.Length; i++) {
					textures[i].height = reader.ReadUInt16();
				}
			});
			Pointer.DoAt(ref reader, off_ui_textures_pageInfo, () => {
				for (int i = 0; i < textures.Length; i++) {
					textures[i].pageInfo = reader.ReadUInt16();
				}
			});
			Pointer.DoAt(ref reader, off_ui_textures_palette, () => {
				for (int i = 0; i < textures.Length; i++) {
					textures[i].palette = reader.ReadUInt16();
				}
			});
			Pointer.DoAt(ref reader, off_ui_textures_xInPage, () => {
				for (int i = 0; i < textures.Length; i++) {
					textures[i].xInPage = reader.ReadByte();
				}
			});
			Pointer.DoAt(ref reader, off_ui_textures_yInPage, () => {
				for (int i = 0; i < textures.Length; i++) {
					textures[i].yInPage = reader.ReadByte();
				}
			});


			R2PS1Loader l = Load as R2PS1Loader;
			PS1VRAM vram = l.vram;
			for (int i = 0; i < textures.Length; i++) {
				UITexture t = textures[i];
				//Load.print(t.name + " - " + t.width + " - " + t.height + " - " + t.xInPage + " - " + t.yInPage);
				t.texture = vram.GetTexture(t.width, t.height, t.pageInfo, t.palette, t.xInPage, t.yInPage);
				Util.ByteArrayToFile(l.gameDataBinFolder + "ui_tex/" + t.name + ".png", t.texture.EncodeToPNG());
			}
		}

		public void ParseSectors(Reader reader) {
			/*Pointer.DoAt(ref reader, off_sectors, () => {
				for (int i = 0; i < count; i++) {
					reader.ReadBytes(0x1c);
					int x = reader.ReadInt32();
					int y = reader.ReadInt32();
					int z = reader.ReadInt32();
					reader.ReadUInt32();
					int x2 = reader.ReadInt32();
					int y2 = reader.ReadInt32();
					int z2 = reader.ReadInt32();
					reader.ReadBytes(0x1c);
					if (ipos[i] != null) {
						GameObject g = ipos[i].CreateGAO();
						g.transform.localPosition = new Vector3(x / 256f, z / 256f, y / 256f);
						//g.transform.localEulerAngles = new Vector3(0f, -90f, 0f);
					}

				}
			});*/
			/*Pointer.DoAt(ref reader, off_sector_minus_one_things, () => {
				for (int i = 0; i < count; i++) {
					ushort type = reader.ReadUInt16();
					reader.ReadUInt16();
					reader.ReadUInt16();
					reader.ReadUInt16();

					short x = reader.ReadInt16();
					short y4 = reader.ReadInt16();
					short x3 = reader.ReadInt16();
					reader.ReadUInt16();

					short y = reader.ReadInt16();
					reader.ReadUInt16();
					short y3 = reader.ReadInt16();
					reader.ReadUInt16();

					short z = reader.ReadInt16();
					reader.ReadUInt16();
					short z3 = reader.ReadInt16();
					reader.ReadUInt16();

					short x2 = reader.ReadInt16();
					short y2 = reader.ReadInt16();
					short z2 = reader.ReadInt16();
					reader.ReadUInt16();

					Pointer.Read(reader);
					Pointer.Read(reader);
					Pointer.Read(reader);
					Pointer.Read(reader);
					Pointer.Read(reader);

					GameObject g = ipos[i].CreateGAO();
					//g.transform.localPosition = new Vector3(x3 / 256f, 0f / 256f, y3 / 256f);
				}
			});*/
		}

		public class UITexture {
			public string name;
			public ushort width;
			public ushort height;
			public ushort pageInfo;
			public ushort palette;
			public byte xInPage;
			public byte yInPage;

			public Texture2D texture;
		}
	}
}
