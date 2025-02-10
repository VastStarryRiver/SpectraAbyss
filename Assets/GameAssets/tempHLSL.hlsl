#pragma vertex analyzeInputData// 指定顶点着色器函数
#pragma fragment getOutputData// 指定片段着色器函数
#include "E:\UnityEditor\2022.3.51f1c1\Editor\Data\CGIncludes\UnityCG.cginc"

float4 _Color; // 颜色
float _Shininess; // 光泽度
float _RangeValue; // 范围值
float4 _Offset; // 向量
sampler2D _MainTex; // 实际的2D纹理对象
float4 _MainTex_ST; // 与纹理坐标相关的变换信息（平移和缩放）
samplerCUBE _TextureCube; // 立方体纹理
bool _EnableEffect; // 布尔值

struct vertexStruct
{
    float4 vertex : POSITION; // 顶点位置（世界空间或对象空间）
    float3 normal : NORMAL; // 顶点法线
    float4 color : COLOR; // 顶点颜色（如果有的话）
    float2 uv : TEXCOORD0; // 纹理坐标
};

struct fragmentStruct
{
    float4 pos : SV_POSITION; // 顶点在裁剪空间中的位置
    float4 color : COLOR; // 顶点颜色（经过插值）
    float2 uv : TEXCOORD0; // 纹理坐标（经过插值）
    float3 normal : TEXCOORD1; // 顶点法线（经过插值）
};

fragmentStruct analyzeInputData(vertexStruct v)
{
    fragmentStruct o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.color = v.color;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex); // 自动将_MainTex_ST的内容应用到uv坐标上
    o.normal = v.normal; // 添加法线的初始化
    return o;
}

half4 getOutputData(fragmentStruct i) : SV_Target
{
    return i.color;
}