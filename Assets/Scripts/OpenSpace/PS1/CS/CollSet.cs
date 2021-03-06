﻿using System;
using System.Collections.Generic;
using System.Linq;
using CollideType = OpenSpace.Collide.CollideType;

namespace OpenSpace.PS1 {
	public class CollSet : OpenSpaceStruct {
		public Dictionary<CollideType, Pointer> off_zdxList = new Dictionary<CollideType, Pointer>();
		public Pointer off_activationList;
		public byte byte_14;
		public byte byte_15;
		public byte byte_16;
		public byte byte_17;

		// Parsed
		public ActivationList activationList;
		public Dictionary<CollideType, ZdxList> zdxList = new Dictionary<CollideType, ZdxList>();

		bool zdxParsed = false;

		protected override void ReadInternal(Reader reader) {
			//Load.print("CollSet @ " + Offset);
			if (Settings.s.game == Settings.Game.R2) {
				reader.ReadUInt32();
				reader.ReadUInt32();
				reader.ReadUInt32();
				reader.ReadUInt32();
			} else {
				off_zdxList[CollideType.ZDD] = Pointer.Read(reader);
				off_zdxList[CollideType.ZDE] = Pointer.Read(reader);
				off_zdxList[CollideType.ZDM] = Pointer.Read(reader);
				off_zdxList[CollideType.ZDR] = Pointer.Read(reader);
			}
			off_activationList = Pointer.Read(reader);
			byte_14 = reader.ReadByte();
			byte_15 = reader.ReadByte();
			byte_16 = reader.ReadByte();
			byte_17 = reader.ReadByte();


			foreach (CollideType key in off_zdxList.Keys) {
				//Load.print(key + " - " + off_zdxList[key]);
				zdxList[key] = Load.FromOffsetOrRead<ZdxList>(reader, off_zdxList[key]);
			}
			activationList = Load.FromOffsetOrRead<ActivationList>(reader, off_activationList);
		}

		public void ReadZdxListDependingOnStates(Reader reader, State[] states) {
			if (Settings.s.game == Settings.Game.R2) {
				Loader.R2PS1Loader l = Load as Loader.R2PS1Loader;
				if (zdxParsed) return;
				zdxParsed = true;
				//Load.print("CollSet @ " + Offset + " - " + states.Length + " - " + (states.Length > 0 ? states[0].Offset.ToString() : ""));
				if (states?.Length > 0) {
					bool HasZdxList(CollideType type) {
						return states.Any(s => s.zoneZdx[type] != -1
						&& activationList?.activationZones.Length > s.zoneZdx[type]
						&& activationList?.activationZones[s.zoneZdx[type]].num_activations != 0);
					}
					off_zdxList[CollideType.ZDD] = HasZdxList(CollideType.ZDD) ? Pointer.GetPointerAtOffset(Offset) : null; // 0
					off_zdxList[CollideType.ZDE] = HasZdxList(CollideType.ZDE) ? Pointer.GetPointerAtOffset(Offset + 4) : null; // 2
					off_zdxList[CollideType.ZDM] = HasZdxList(CollideType.ZDM) ? Pointer.GetPointerAtOffset(Offset + 8) : null; // 1
					off_zdxList[CollideType.ZDR] = HasZdxList(CollideType.ZDR) ? Pointer.GetPointerAtOffset(Offset + 0xC) : null; // 3
				} else {
					off_zdxList[CollideType.ZDD] = Pointer.GetPointerAtOffset(Offset); // 0
					off_zdxList[CollideType.ZDE] = Pointer.GetPointerAtOffset(Offset + 4); // 2
					off_zdxList[CollideType.ZDM] = Pointer.GetPointerAtOffset(Offset + 8); // 1
					off_zdxList[CollideType.ZDR] = Pointer.GetPointerAtOffset(Offset + 0xC); // 3
				}
				foreach (CollideType key in off_zdxList.Keys) {
					//Load.print(key + " - " + off_zdxList[key]);
					zdxList[key] = Load.FromOffsetOrRead<ZdxList>(reader, off_zdxList[key]);
				}
			}
		}
	}
}
