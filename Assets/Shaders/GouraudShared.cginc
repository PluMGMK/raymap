#ifndef SHARED_GOURAUD
#define SHARED_GOURAUD

#include "UnityCG.cginc"
uniform float4 _LightColor0; // color of light source (from "Lighting.cginc")

// User-specified properties
uniform float4 _Color;
uniform float4 _AmbientCoef;
uniform float4 _DiffuseCoef;
uniform float4 _SpecularCoef;
uniform float _SpecularFactor;
uniform float4 _EmissionColor;
float _NumTextures;
sampler2D _MainTex;
sampler2D _MainTex2;
sampler2D _MainTex3;
sampler2D _MainTex4;
uniform float4 _MainTex_ST;
uniform float4 _MainTex2_ST;
uniform float4 _MainTex3_ST;
uniform float4 _MainTex4_ST;
float _UVSec;
float _Blend;
float _ShadingMode;

// Lighting
uniform float4 _SectorAmbient;
uniform float4 _SectorFog;
uniform float4 _SectorFogParams;
float4 _StaticLightPos[512];
float4 _StaticLightDir[512];
float4 _StaticLightCol[512];
float4 _StaticLightParams[512];
float _StaticLightCount = 0;
float _Luminosity = 0.5;
float _Saturate = 1.0;
float _DisableLighting = 0;

struct v2f {
	float4 pos : SV_POSITION;
	float3 uv1 : TEXCOORD0; // The first UV coordinate.
	float3 uv2 : TEXCOORD1; // The second UV coordinate.
	float3 uv3 : TEXCOORD2; // The second UV coordinate.
	float3 uv4 : TEXCOORD3; // The second UV coordinate.
	float4 diffuseColor : TEXCOORD4;
	float3 normal : TEXCOORD5;
	float3 multipliedPosition : TEXCOORD6;
	float fog : TEXCOORD7;
	//UNITY_FOG_COORDS(3)
};

float CalcSphereAttenuation(float distance, float near, float far) {
	if (distance <= near) {
		return 1.0;
	} else {
		return 1.0 - (distance - near) / (far - near); // TODO: Get correct attenuation
	}
}

float4 ApplyStaticLights(float3 colRgb, float3 normalDirection, float3 multipliedPosition) {
	if(_DisableLighting == 1.0) return float4(1.0, 1.0, 1.0, 1.0);

	/* Alpha light flags:
	    0 = Affect color and alpha
	    1 = Only affect alpha
	    2 = Only affect color
	*/

	float3 lightDirection;
	float3 vertexToLightSource;
	float attenuation;
	float near;
	float far;
	float distance;
	float normalFactor;
	float alpha = 0.0;
	float3 diffuseReflection = float3(0.0, 0.0, 0.0);
	float3 luminosity = float3(_Luminosity - 0.5, _Luminosity - 0.5, _Luminosity - 0.5);
	float4 ambient = _AmbientCoef;
	for (int i = 0; i < _StaticLightCount; i++) {
		if (_StaticLightPos[i].w == 1) {
			attenuation = 1.0; // no attenuation
			lightDirection = normalize(_StaticLightDir[i].xyz);
			/*if (_StaticLightParams[i].z == 0) {
				normalFactor = max(0.0, dot(normalDirection, lightDirection));
			} else normalFactor = 1.0;*/
			normalFactor = max(0.0, dot(normalDirection, lightDirection));
			if (_StaticLightParams[i].w != 1) diffuseReflection = diffuseReflection + attenuation * _StaticLightCol[i].xyz * normalFactor;
				//* colRgb * _DiffuseCoef.xyz //* _DiffuseCoef.w
			if (_StaticLightParams[i].w != 2) alpha = alpha + _StaticLightCol[i].w * normalFactor;// * _DiffuseCoef.w;
		} else if (_StaticLightPos[i].w == 2) {
			vertexToLightSource = _StaticLightPos[i].xyz - multipliedPosition;
			distance = length(vertexToLightSource);
			far = _StaticLightParams[i].y;
			if (distance < far) {
				near = _StaticLightParams[i].x;
				attenuation = CalcSphereAttenuation(distance, near, far);
				lightDirection = normalize(vertexToLightSource);
				if (_StaticLightParams[i].z == 0) {
					normalFactor = max(0.0, dot(normalDirection, lightDirection));
				} else normalFactor = 1.0;
				if (_StaticLightParams[i].w != 1) diffuseReflection = diffuseReflection + attenuation * _StaticLightCol[i].xyz * normalFactor;
					//* colRgb * _DiffuseCoef.xyz //* _DiffuseCoef.w
				if (_StaticLightParams[i].w != 2) alpha = alpha + attenuation * _StaticLightCol[i].w * normalFactor;// * _DiffuseCoef.w;
			}
		} else if (_StaticLightPos[i].w == 4) {
			if (_StaticLightParams[i].w != 1) ambient.xyz = ambient.xyz + _StaticLightCol[i].xyz * _DiffuseCoef.xyz;
			//* colRgb * _DiffuseCoef.xyz //* _DiffuseCoef.w
			if (_StaticLightParams[i].w != 2) ambient.w = ambient.w + _StaticLightCol[i].w* _DiffuseCoef.w;// * _DiffuseCoef.w;
		} else if (_StaticLightPos[i].w == 7) {
			vertexToLightSource = _StaticLightPos[i].xyz - multipliedPosition;
			distance = length(vertexToLightSource);
			far = _StaticLightParams[i].y;
			if (distance < far) {
				near = _StaticLightParams[i].x;
				attenuation = CalcSphereAttenuation(distance, near, far);
				lightDirection = normalize(_StaticLightDir[i].xyz);
				if (_StaticLightParams[i].z == 0) {
					normalFactor = max(0.0, dot(normalDirection, lightDirection));
				} else normalFactor = 1.0;
				if (_StaticLightParams[i].w != 1) diffuseReflection = diffuseReflection + attenuation * _StaticLightCol[i].xyz * normalFactor;
					//* colRgb * _DiffuseCoef.xyz //* _DiffuseCoef.w
				if (_StaticLightParams[i].w != 2) alpha = alpha + attenuation * _StaticLightCol[i].w * normalFactor;// *_DiffuseCoef.w;
			}
		}
	}
	//float3 ambientLighting = ambient.xyz * _DiffuseCoef.xyz;
	diffuseReflection = luminosity + ambient.xyz + diffuseReflection * (luminosity + _DiffuseCoef.xyz);
	alpha = ambient.w + alpha * _DiffuseCoef.w;
	if (_Saturate == 1.0) {
		diffuseReflection.x = saturate(diffuseReflection.x);
		diffuseReflection.y = saturate(diffuseReflection.y);
		diffuseReflection.z = saturate(diffuseReflection.z);
		alpha = saturate(alpha);
	}
	return float4(diffuseReflection, alpha);
}

v2f process_vert(appdata_full v, float isAdd) {
	v2f o;
	o.uv1 = float3(TRANSFORM_TEX(v.texcoord.xy, _MainTex), v.texcoord.z);
	o.uv2 = float3(TRANSFORM_TEX(v.texcoord1.xy, _MainTex2), v.texcoord1.z);
	o.uv3 = float3(TRANSFORM_TEX(v.texcoord2.xy, _MainTex3), v.texcoord2.z);
	o.uv4 = float3(TRANSFORM_TEX(v.texcoord3.xy, _MainTex4), v.texcoord3.z);
	o.pos = UnityObjectToClipPos(v.vertex);
	float4x4 modelMatrix = unity_ObjectToWorld;
	float4x4 modelMatrixInverse = unity_WorldToObject;

	float3 normalDirection = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz); // Normal in object space
	float3 multipliedPosition = mul(modelMatrix, v.vertex).xyz;
	//float3 viewDirection = normalize(_WorldSpaceCameraPos - mul(modelMatrix, v.vertex).xyz);
	float3 lightDirection;
	float attenuation;

	float alpha = 1.0;
	float3 colRgb = _Color.rgb * _Color.w + float3(1.0, 1.0, 1.0) * (1.0 - _Color.w);

	float3 ambientLighting = 0.0;
	/*if (isAdd == 0.0) {
		ambientLighting = _SectorAmbient.rgb * _AmbientCoef.w;
		ambientLighting = ambientLighting + _AmbientCoef.xyz * colRgb;

		//ambientLighting = _SectorAmbient.rgb * _AmbientCoef.w;
		//ambientLighting = ambientLighting + _AmbientCoef.xyz * colRgb;
		
		//ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * colRgb * _AmbientCoef.xyz * _AmbientCoef.w;
		//ambientLighting = ambientLighting + colRgb * (1.0 - _AmbientCoef.w);
		
		//UNITY_LIGHTMODEL_AMBIENT.rgb * colRgb * _AmbientCoef.xyz * (1.0-(_SpecularCoef.w/100.0));
		//ambientLighting = ambientLighting + colRgb * (_SpecularCoef.w/100.0);
	}*/
	float3 diffuseReflection = float3(0.0, 0.0, 0.0);
	//if (isAdd == 1.0) {
	if (0.0 == _WorldSpaceLightPos0.w) { // directional light?
		attenuation = 1.0; // no attenuation
		lightDirection = normalize(_WorldSpaceLightPos0.xyz);
	} else { // point or spot light
		float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - multipliedPosition;
		float distance = length(vertexToLightSource);
		attenuation = 1.0 / distance; // linear attenuation
		lightDirection = normalize(vertexToLightSource);
	}
	diffuseReflection = attenuation * _LightColor0.rgb
		* colRgb
		* _DiffuseCoef.xyz //* _DiffuseCoef.w
		* max(0.0, dot(normalDirection, lightDirection));
	if (/*_ShadingMode == 0.0 && */isAdd == 0.0) {
		float4 lightCol = ApplyStaticLights(colRgb, normalDirection, multipliedPosition);
		diffuseReflection = diffuseReflection + lightCol.xyz;
		alpha = lightCol.w;
	}
	/*} else {
		for (int i = 0; i < 4; i++) {
			float4 lightPosition = float4(unity_4LightPosX0[i], unity_4LightPosY0[i], unity_4LightPosZ0[i], 1.0);
			float3 difference = lightPosition.xyz - o.pos.xyz;
			float squaredDistance = dot(difference, difference);
			attenuation = 1.0 / (1.0 + unity_4LightAtten0[i] * squaredDistance);
			lightDirection = normalize(difference);

			diffuseReflection = diffuseReflection + attenuation * unity_LightColor[i].rgb
				* colRgb
				* _DiffuseCoef.xyz //* _DiffuseCoef.w
				* max(0.0, dot(normalDirection, lightDirection));
		}
	}*/
	o.normal = normalDirection;
	o.multipliedPosition = multipliedPosition;
	o.diffuseColor = float4(ambientLighting + diffuseReflection, alpha);
	if (_SectorFog.w != 0) {
		if (_SectorFogParams.x != _SectorFogParams.y) { // Blend near != Blend far
			float fogz = length(WorldSpaceViewDir(v.vertex));
			o.fog = _SectorFogParams.x + 
				saturate((fogz - _SectorFogParams.z) / (_SectorFogParams.w - _SectorFogParams.z))
				* (_SectorFogParams.y - _SectorFogParams.x);
		} else {
			o.fog = _SectorFogParams.y;
		}
	}
	//o.specularColor = specularReflection;
	//UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

float4 process_frag(v2f i, float clipAlpha, float isAdd) : SV_TARGET {
	float4 c = float4(0.0, 0.0, 0.0, 0.0);
	if (_NumTextures > 0) {
		c = lerp(c, tex2D(_MainTex, i.uv1.xy), i.uv1.z);
		if (_NumTextures > 1) {
			c = lerp(c, tex2D(_MainTex2, i.uv2.xy), i.uv2.z);
			if (_NumTextures > 2) {
				c = lerp(c, tex2D(_MainTex3, i.uv3.xy), i.uv3.z);
				if (_NumTextures > 3) {
					c = lerp(c, tex2D(_MainTex4, i.uv4.xy), i.uv4.z);
				}
			}
		}
	}

	/*float blendfactor = i.uv2.z;
	if (_Blend == 1) {
		c = lerp(tex2D(_MainTex, uv1), tex2D(_MainTex2, uv2), blendfactor);
	} else {
		c = tex2D(_MainTex, uv1.xy);
	}*/
	c.a = c.a * i.diffuseColor.w;
	clip(clipAlpha * (c.a - 1.0));
	c.rgb = c.rgb * (1 + (_EmissionColor.rgb * 2 * _EmissionColor.a));
	//if (_ShadingMode == 0.0 || isAdd == 1.0) {
		c = float4(i.diffuseColor.xyz * c, c.a);
	/*} else {
		float3 colRgb = _Color.rgb * _Color.w + float3(1.0, 1.0, 1.0) * (1.0 - _Color.w);
		float4 lightColor = ApplyStaticLights(colRgb, normalize(i.normal), i.multipliedPosition);
		c = float4((i.diffuseColor.xyz + lightColor) * c, c.a * lightColor.w);
		clip(clipAlpha * (c.a - 1.0));
	}*/
	// Add fog
	if (_SectorFog.w != 0) {
		float fog = i.fog;
		if (isAdd == 1.0) {
			c.rgb = lerp(c.rgb, float3(0, 0, 0), fog * _SectorFog.w);
		} else {
			c.rgb = lerp(c.rgb, _SectorFog.xyz, fog * _SectorFog.w);
		}
		//c.rgb = lerp(c.rgb, _SectorFog.xyz, fog);
	}
	//UNITY_APPLY_FOG(i.fogCoord, c);
	//return float4(i.specularColor + i.diffuseColor * c, c.a);
	return c;
}
#endif // SHARED_GOURAUD