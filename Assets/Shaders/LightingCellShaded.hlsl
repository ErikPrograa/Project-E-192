#ifndef LIGHTING_CELL_SHADED_INCLUDED
#define LIGHTING_CELL_SHADED_INCLUDED

#ifndef SHADERGRAPH_PREVIEW

struct SurfaceVariables
{
    float rimThreshold;
    float shininess;
    float smoothness;
    float3 view;
    float3 normal;
    float3 position;
};

float3 CalculateCellShading(Light l,SurfaceVariables s)
{
    float attenuation = l.shadowAttenuation;
    float diffuse = saturate(dot(s.normal, l.direction));
    diffuse *= attenuation;
    float3 h = SafeNormalize(l.direction+s.view);
    float specular = saturate(dot(s.normal, h));
    specular = pow(specular, s.shininess);
    specular *= diffuse * s.smoothness;
    float rim = 1 - dot(s.view, s.normal);
    rim *= pow(diffuse, s.rimThreshold);
    return l.color * (diffuse + max(specular, rim));
}
#endif
void LightingCellShaded_float(float Smoothness,float RimThreshold,float3 Position, float3 Normal,float3 View,out float3 Color)
{
#if defined(SHADERGRAPH_PREVIEW)
    Color = float3(0.0f,0.0f,0.0f);
#else
    SurfaceVariables s;
    
    s.normal = normalize(Normal);
    s.view = SafeNormalize(View);
    s.smoothness = Smoothness;
    s.shininess = exp2(10 * Smoothness + 1);
    s.rimThreshold = RimThreshold;
    
#if SHADOWS_SCREEN
    float4 clipPos = TransformWorldToHClip(Position);
    float4 shadowCoord = ComputeScreenPos(clipPos);
#else 
    float4 shadowCoord = TransformWorldToShadowCoord(Position);
#endif
    Light light = GetMainLight(shadowCoord);
    Color = CalculateCellShading(light,s);
#endif
}
#endif
