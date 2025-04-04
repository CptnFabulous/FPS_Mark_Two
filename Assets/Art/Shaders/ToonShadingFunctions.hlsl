//UNITY_SHADER_NO_UPGRADE
#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED




//CGPROGRAM
//#include "UnityCG.cginc"
//TEXTURE2D(_CameraDepthTexture);
//SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D(_CameraDepthNormalsTexture);
SAMPLER(sampler_CameraDepthNormalsTexture);



//#pragma enable_d3d11_debug_symbols
/*
#if !defined(SHADERGRAPH_PREVIEW)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif
*/
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

//#ifdef UNIVERSAL_LIGHTING_INCLUDED



// This link should be super useful for learning how to use URP's lighting stuff:
// https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/use-built-in-shader-methods-lighting.html

//float InverseLerp

float StepValue(float input, float range)
{
    //if (input == 0) return input;
    float output = input;
    output /= range;
    output = round(output);
    //output = ceil(output);
    output *= range;
    return output;
}

#if !defined(SHADERGRAPH_PREVIEW)
float3 ApplyToonLight(Light light, float3 normal, float3 viewDirection, float glossiness)
{
    // Determine light intensity based on direction
    float lightDot = dot(light.direction, normal);
    float intensity = saturate(lightDot);
    
    // This bit here is responsible for the cel-shading, by stepping the lights.
    //intensity = smoothstep(0, 0.01, intensity);
    //intensity = step(0.01, intensity);
    //intensity = StepValue(intensity, 1);
    intensity = StepValue(intensity, 0.5);
    
    // This value would normally be good for distance, but it results in gradients.
    // Maybe I'll add it back in later once I figure out code for multiple steps
    //intensity *= light.distanceAttenuation;
    //intensity *= light.shadowAttenuation;
    //intensity = StepValue(intensity, 0.5);
    float3 lightColourValue = light.color * intensity;






    //Apply gloss
    float glossSize = 32;
    float3 glossColour = float3(1, 1, 1) * glossiness;

    // Calculate specular intensity
    viewDirection = normalize(viewDirection);
    float3 halfVector = normalize(light.direction + viewDirection);
    float specularDot = dot(normal, halfVector);
    float specularIntensity = pow(specularDot * intensity, glossSize * glossSize);

    // Step value for cel-shading
    //specularIntensity = smoothstep(0.005, 0.01, specularIntensity);
    specularIntensity = step(0.005, specularIntensity);
    
    // Combine with reflection colour (currently defaults to white)
    float3 specular = specularIntensity * glossColour;
    // Add to given value
    lightColourValue += specular;





    // Apply rim lighting
    float rimThreshold = 0.7;

    float rimDot = 1 - dot(viewDirection, normal);
    float rimIntensity = rimDot;
    // Determine how much of the edge the rim will cover
    rimIntensity *= pow(lightDot, rimThreshold);
    // Apply stepping for toon effect
    //rimIntensity = smoothstep(rimThreshold - 0.01, rimThreshold + 0.01, rimIntensity);
    rimIntensity = step(rimThreshold, rimIntensity);

    lightColourValue += light.color * rimIntensity;


    return lightColourValue;
}
#endif

void ProcessAllLightData_float(float3 worldPosition, float3 normal, float3 viewDirection, /*float4 ambientLight, */float glossiness, out float3 colour)
{
    #if !defined(SHADERGRAPH_PREVIEW)
    colour = float3(0, 0, 0);
    // Start off with main light
    colour += ApplyToonLight(GetMainLight(), normal, viewDirection, glossiness);

    // Apply all additional lights
    int count = GetAdditionalLightsCount();
    for (int i = 0; i < count; i++)
    {
        Light l = GetAdditionalLight(i, worldPosition);
        colour += ApplyToonLight(l, normal, viewDirection, glossiness);
    }
    
    #else
    colour = float3(1, 1, 1);
    #endif
    
    /*
    colour = float3(1, 1, 1);
    
    #if !defined(SHADERGRAPH_PREVIEW)
    // Start off with main light
    colour *= ApplyToonLight(GetMainLight(), normal, viewDirection, glossiness);

    // Apply all additional lights
    int count = GetAdditionalLightsCount();
    for (int i = 0; i < count; i++)
    {
        Light l = GetAdditionalLight(i, worldPosition);
        colour *= ApplyToonLight(l, normal, viewDirection, glossiness);
    }
    #endif
    */
}



void GetShadowAttenuations_float(float3 worldPosition, float3 normal, float3 viewDirection, float glossiness, out float3 colour)
{
    colour = float3(1, 1, 1);
#if !defined(SHADERGRAPH_PREVIEW)
    
    float s = 0;
    
    s += GetMainLight().shadowAttenuation;

    // Apply all additional lights
    int count = GetAdditionalLightsCount();
    for (int i = 0; i < count; i++)
    {
        Light l = GetAdditionalLight(i, worldPosition);
        s += l.shadowAttenuation;
    }
    
    colour *= s;
    
#endif
}




void SobelDetectionCorners_float(float2 coordinates, float2 texelSize, int scale, out float2 topLeftUV, out float2 topRightUV, out float2 bottomLeftUV, out float2 bottomRightUV)
{
    float halfScaleFloor = floor(scale * 0.5);
    float halfScaleCeil = ceil(scale * 0.5);

    bottomLeftUV = coordinates - float2(texelSize.x, texelSize.y) * halfScaleFloor;
    topRightUV = coordinates + float2(texelSize.x, texelSize.y) * halfScaleCeil;  
    bottomRightUV = coordinates + float2(texelSize.x * halfScaleCeil, -texelSize.y * halfScaleFloor);
    topLeftUV = coordinates + float2(-texelSize.x * halfScaleFloor, texelSize.y * halfScaleCeil);
}
void SobelDetectionSides_float(float2 coordinates, float2 texelSize, int scale, out float2 top, out float2 bottom, out float2 left, out float2 right)
{
    float halfScaleFloor = floor(scale * 0.5);
    float halfScaleCeil = ceil(scale * 0.5);

    top = coordinates + float2(0, texelSize.y) * halfScaleCeil;
    bottom = coordinates + float2(0, -texelSize.y) * halfScaleFloor;  
    left = coordinates + float2(-texelSize.x, 0) * halfScaleFloor;
    right = coordinates + float2(texelSize.x, 0) * halfScaleCeil;
}




float3 DecodeNormal(float4 enc)
{
    // I copied this code from a tutorial and have no idea how it works
    float kScale = 1.7777;
    float3 nn = enc.xyz*float3(2*kScale,2*kScale,0) + float3(-kScale,-kScale,1);
    float g = 2.0 / dot(nn.xyz,nn.xyz);
    float3 n;
    n.xy = g*nn.xy;
    n.z = g-1;
    return n;
}


void GetNormalBuffer_float(out Texture2D buffer)
{
    //float3 normalSample = DecodeNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, screenPosition));
    buffer = _CameraDepthNormalsTexture;
}
void SampleNormalBuffer_float(float2 screenPosition, out float3 normal, out float depth)
{
    float4 encoded = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, screenPosition);
    normal = DecodeNormal(encoded);
    //normal = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, screenPosition).rgb;

    depth = encoded.w;
    //depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenPosition).r;
}

/*
void GetDepthDifference(float2 coordinates, float2 texelSize, int scale, out float depthDifference)
{
    SobelDetectionCorners(coordinates, texelSize, scale, float2 topLeftUV, float2 topRightUV, float2 bottomLeftUV, float2 bottomRightUV);
    //SAMPLE_DEPTH_TEXTURE()
}
*/

//#endif

#endif //CUSTOM_LIGHTING_INCLUDED