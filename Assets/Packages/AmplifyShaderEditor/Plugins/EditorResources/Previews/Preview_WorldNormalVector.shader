﻿Shader "Hidden/WorldNormalVector"
{
	Properties
	{
		_A ("_WorldNormal", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag

			sampler2D _A;
			float _Connected;

			float4 frag(v2f_img i) : SV_Target
			{
				float2 p = 2 * i.uv - 1;
				float r = sqrt( dot(p,p) );
				r = saturate( r );
				/*if ( r < 1 )
				{*/
					float2 uvs;
					float f = ( 1 - sqrt( 1 - r ) ) / r;
					uvs.x = p.x;
					uvs.y = p.y;
					float3 vertexPos = float3( uvs, ( f - 1 ) * 2 );
					float3 normal = normalize(vertexPos);
					float3 worldNormal = UnityObjectToWorldNormal(normal);
					
					if(_Connected == 0)
						return float4(worldNormal, 1);

					float3 tangent = normalize(float3( 1-f, p.y*0.01, p.x ));
					float3 worldPos = mul(unity_ObjectToWorld, vertexPos).xyz;
					float3 worldTangent = UnityObjectToWorldDir(tangent);
					float tangentSign = -1;
					float3 worldBinormal = normalize( cross(worldNormal, worldTangent) * tangentSign);
					float4 tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
					float4 tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
					float4 tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

					float2 sphereUVs = i.uv;

					sphereUVs.x = (atan2(vertexPos.x, -vertexPos.z) / (UNITY_PI) + 0.5);
					float3 tangentNormal = tex2Dlod(_A, float4(sphereUVs,0,0)).xyz;

					worldNormal = fixed3( dot( tSpace0.xyz, tangentNormal ), dot( tSpace1.xyz, tangentNormal ), dot( tSpace2.xyz, tangentNormal ) );

					return float4(worldNormal, 1);
				/*}
				else {
					return 0;
				}*/
			}
			ENDCG
		}
	}
}
