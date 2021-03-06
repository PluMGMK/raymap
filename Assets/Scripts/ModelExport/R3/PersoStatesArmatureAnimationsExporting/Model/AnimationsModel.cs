﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelExport.R3.PersoStatesArmatureAnimationsExporting.Model
{
    public class AnimationsModel
    {
        public Dictionary<string, Dictionary<int, AnimationFrameModel>> animationClips = new Dictionary<string, Dictionary<int, AnimationFrameModel>>();

        public void addAnimationFrameModelToAnimationClip(string animationClipName, AnimationFrameModel animationFrameModel, int frameNumber)
        {
            if (!animationClips.ContainsKey(animationClipName))
            {
                animationClips[animationClipName] = new Dictionary<int, AnimationFrameModel>();
            }

            animationClips[animationClipName][frameNumber] = animationFrameModel;
        }
    }
}
