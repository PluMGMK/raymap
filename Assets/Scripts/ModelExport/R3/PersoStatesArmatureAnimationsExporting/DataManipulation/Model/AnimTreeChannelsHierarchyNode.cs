﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModelExport.R3.PersoStatesArmatureAnimationsExporting.DataManipulation.Model
{
    public class AnimTreeChannelsHierarchyNode
    {
        public string Name;
        public bool IsKeyframe;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public AnimTreeChannelsHierarchyNode(
            string Name,
            bool IsKeyframe,
            Vector3 LocalPosition,
            Quaternion LocalRotation,
            Vector3 LocalScale,
            Vector3 Position,
            Quaternion Rotation,
            Vector3 Scale)
        {
            this.Name = Name;
            this.IsKeyframe = IsKeyframe;
            this.LocalPosition = LocalPosition;
            this.LocalRotation = LocalRotation;
            this.LocalScale = LocalScale;
            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;
        }
    }
}