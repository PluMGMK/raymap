﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelExport.MathDescription;
using ModelExport.R3.SkinnedAnimatedMeshesExporting.Model.AnimatedExportObjectModelDescription.MaterialsDescription;
using UnityEngine;

namespace ModelExport.R3.SkinnedAnimatedMeshesExporting.DataManipulation.ModelConstructing.ModelConversion
{
    public class UnityMaterialToMaterialDescriptionConverter
    {
        private static int TEXTURE_MIPMAP_LEVEL = 0;

        public Model.AnimatedExportObjectModelDescription.MaterialsDescription.Material 
            MaterialDescriptionFromUnityMaterial(UnityEngine.Material unityMaterial)
        {
            var result = new Model.AnimatedExportObjectModelDescription.MaterialsDescription.Material();
            result.name = unityMaterial.name;
            result.mainTexture = ConvertUnityTextureToTextureDescription("_Tex0", unityMaterial.GetTexture("_Tex0"));
            result.mainTextureOffset = new Vector2d(unityMaterial.mainTextureOffset.x, unityMaterial.mainTextureOffset.y);
            result.mainTextureScale = new Vector2d(unityMaterial.mainTextureScale.x, unityMaterial.mainTextureScale.y);
            return result;
        }

        private Model.AnimatedExportObjectModelDescription.MaterialsDescription.Texture 
            ConvertUnityTextureToTextureDescription(string textureName, UnityEngine.Texture unityTexture)
        {
            var result = new Model.AnimatedExportObjectModelDescription.MaterialsDescription.Texture();
            result.name = textureName;
            result.width = unityTexture.width;
            result.height = unityTexture.height;
            if (unityTexture.GetType() == typeof(Texture2D))
            {
                result.pixels = ((Texture2D)unityTexture).GetPixels(TEXTURE_MIPMAP_LEVEL)
                .Select(x => new Model.AnimatedExportObjectModelDescription.MaterialsDescription.Color(x.r, x.g, x.b, x.a)).ToList();
            } else
            {
                throw new InvalidOperationException("Mesh texture is not Texture2D!");
            }            
            return result;
        }
    }
}
