﻿using ModelExport.R3.SkinnedAnimatedMeshesExporting.Model.AnimatedExportObjectModelDescription;
using ModelExport.R3.SkinnedAnimatedMeshesExporting.Model.AnimatedExportObjectModelDescription.MaterialsDescription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelExport.R3.SkinnedAnimatedMeshesExporting.Model
{
    public class AnimatedExportObjectModel
    {
        public string Name;
        public TransformModel transform;
        public MeshGeometry meshGeometry;
        public Dictionary<string, BoneBindPose> bindBonePoses;
        public List<Material> materials;
    }
}
