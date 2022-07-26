Shader "Leja/GridShader"
{
  
    SubShader
    {
        Tags {"Queue" = "Geometry"  }
        LOD 100
        Pass
        {
           // Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 5.0
            #include "UnityCG.cginc"
           // #include "AutoLight.cginc"

           // #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            struct GridElement {
                float3 Position;
                float Proximity;
            };
     
            StructuredBuffer<GridElement> GridElements;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                half Proximity:TEXCOORD0;
            };

            struct g2f
            {
                half4 pos : SV_POSITION;
               // half Atten:TEXCOORD0;
                half Proximity:TEXCOORD1;
              // SHADOW_COORDS(2)
            };

            //PARAMS
            int LinearVertexCount;
            static const float PI = 3.141592653589793238462;
            float Scale;
            fixed4 _Color;
            fixed4 _ProximityCol;

            v2g vert(appdata v)
            {
                v2g o;
                int X = int(floor(v.uv.x* LinearVertexCount));
                int Y = int(floor(v.uv.y* LinearVertexCount));
                int ID = Y * LinearVertexCount + X;
                GridElement element = GridElements[ID];
                o.vertex = float4(element.Position,1);
                o.Proximity = element.Proximity;
                return o;
            }
             
            g2f MakeVertex(float3 v0, float3 Dir,float3 Normal,float Prox)
            {
                g2f OUT;
                OUT.Proximity = Prox;
                OUT.pos = UnityObjectToClipPos(v0 + Dir * Scale);
              //  OUT.Atten = (dot(normalize(mul(unity_ObjectToWorld, float4(Normal, 1))), _WorldSpaceLightPos0) + 1) / 2;
               // TRANSFER_SHADOW(OUT);
                return OUT;
            }

            [maxvertexcount(42)]
            void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
            {
               
               
                half3 Perp1 = normalize(half3(1, 0, 0));
                half3 Perp2 = normalize(half3(cos(PI / 3), 0, sin(PI / 3)));
                half3 Perp3 = normalize(half3(cos(PI *2/ 3) , 0, sin(PI / 3)));

                half3 vUp = IN[0].vertex.xyz;
                half3 vDown = IN[0].vertex.xyz + float3(0, -40, 0);
                half3 Up = half3(0, 1, 0);

                half Prox = IN[0].Proximity;

                //turn to function, this is messy
                //Triangles
                triStream.Append(MakeVertex(vUp, Perp2, Up, Prox));
                triStream.Append(MakeVertex(vUp, Perp1, Up, Prox));
                triStream.Append(MakeVertex(vUp, 0, Up, Prox));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, Perp3, Up, Prox));
                triStream.Append(MakeVertex(vUp, Perp2, Up, Prox));
                triStream.Append(MakeVertex(vUp, 0, Up, Prox));
                triStream.RestartStrip();


                triStream.Append(MakeVertex(vUp, -Perp1, Up, Prox));
                triStream.Append(MakeVertex(vUp, Perp3, Up, Prox));
                triStream.Append(MakeVertex(vUp, 0, Up, Prox));
                triStream.RestartStrip();


                triStream.Append(MakeVertex(vUp, -Perp2, Up, Prox));
                triStream.Append(MakeVertex(vUp, -Perp1, Up, Prox));
                triStream.Append(MakeVertex(vUp, 0, Up, Prox));
                triStream.RestartStrip();


                triStream.Append(MakeVertex(vUp, -Perp3, Up, Prox));
                triStream.Append(MakeVertex(vUp, -Perp2, Up, Prox));
                triStream.Append(MakeVertex(vUp, 0, Up, Prox));
                triStream.RestartStrip();


                triStream.Append(MakeVertex(vUp, Perp1, Up, Prox));
                triStream.Append(MakeVertex(vUp, -Perp3, Up, Prox));
                triStream.Append(MakeVertex(vUp, 0, Up , Prox));
                triStream.RestartStrip();

                //Quads
                triStream.Append(MakeVertex(vUp, Perp1, Perp1, Prox));
                triStream.Append(MakeVertex(vUp, Perp2, Perp2, Prox));
                triStream.Append(MakeVertex(vDown, Perp1, Perp1, Prox));
                triStream.Append(MakeVertex(vDown, Perp2, Perp2, Prox));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, Perp2, Perp2, Prox));
                triStream.Append(MakeVertex(vUp, Perp3, Perp3, Prox));
                triStream.Append(MakeVertex(vDown, Perp2, Perp2, Prox));
                triStream.Append(MakeVertex(vDown, Perp3, Perp3, Prox));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, Perp3, Perp3, Prox));
                triStream.Append(MakeVertex(vUp, -Perp1, -Perp1, Prox));
                triStream.Append(MakeVertex(vDown, Perp3, Perp3, Prox));
                triStream.Append(MakeVertex(vDown, -Perp1, -Perp1, Prox));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, -Perp1, -Perp1, Prox));
                triStream.Append(MakeVertex(vUp, -Perp2, -Perp2, Prox));
                triStream.Append(MakeVertex(vDown, -Perp1, -Perp1, Prox));
                triStream.Append(MakeVertex(vDown, -Perp2, -Perp2, Prox));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, -Perp2, -Perp2, Prox));
                triStream.Append(MakeVertex(vUp, -Perp3, -Perp3, Prox));
                triStream.Append(MakeVertex(vDown, -Perp2, -Perp2, Prox));
                triStream.Append(MakeVertex(vDown, -Perp3, -Perp3, Prox));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, -Perp3, -Perp3, Prox));
                triStream.Append(MakeVertex(vUp, Perp1, Perp1, Prox));
                triStream.Append(MakeVertex(vDown, -Perp3, -Perp3, Prox));
                triStream.Append(MakeVertex(vDown, Perp1, Perp1, Prox));
                triStream.RestartStrip();


            }


            fixed4 frag (g2f i) : SV_Target
            {
               // half shadow = SHADOW_ATTENUATION(i);
                fixed4 COLOR = lerp(_Color, _ProximityCol, i.Proximity* i.Proximity);
                fixed4 col = COLOR;
               // fixed4 col = COLOR * shadow*((i.Atten>0.5)?1.05:0.95);
                return col;
            }
            ENDCG
        }

        //shadowcaster pass
        pass
        {
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            //Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 5.0
            #pragma multi_compile_shadowcaster
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            struct GridElement {
                float3 Position;
                float Proximity;
            };

            StructuredBuffer<GridElement> GridElements;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
            };

            struct g2f
            {
                half4 pos : SV_POSITION;
            };

            //PARAMS
            int LinearVertexCount;
            static const float PI = 3.141592653589793238462;
            float Scale;


            v2g vert(appdata v)
            {
                v2g o;
                int X = int(floor(v.uv.x * LinearVertexCount));
                int Y = int(floor(v.uv.y * LinearVertexCount));
                int ID = Y * LinearVertexCount + X;
                GridElement element = GridElements[ID];
                o.vertex = float4(element.Position, 1);
                return o;
            }

            g2f MakeVertex(float3 v0, float3 Dir, float3 Normal)
            {
                g2f OUT;
                OUT.pos = UnityObjectToClipPos(v0 + Dir * Scale);
                return OUT;
            }

            [maxvertexcount(42)]
            void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
            {

                half3 Perp1 = normalize(half3(1, 0, 0));
                half3 Perp2 = normalize(half3(cos(PI / 3), 0, sin(PI / 3)));
                half3 Perp3 = normalize(half3(cos(PI * 2 / 3), 0, sin(PI / 3)));

                half3 vUp = IN[0].vertex.xyz;
                half3 vDown = IN[0].vertex.xyz + float3(0, -40, 0);
                half3 Up = half3(0, 1, 0);

                //turn to function, this is messy
                //Triangles
                triStream.Append(MakeVertex(vUp, Perp2, Up));
                triStream.Append(MakeVertex(vUp, Perp1, Up));
                triStream.Append(MakeVertex(vUp, 0, Up));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, Perp3, Up));
                triStream.Append(MakeVertex(vUp, Perp2, Up));
                triStream.Append(MakeVertex(vUp, 0, Up));
                triStream.RestartStrip();


                triStream.Append(MakeVertex(vUp, -Perp1, Up));
                triStream.Append(MakeVertex(vUp, Perp3, Up));
                triStream.Append(MakeVertex(vUp, 0, Up));
                triStream.RestartStrip();


                triStream.Append(MakeVertex(vUp, -Perp2, Up));
                triStream.Append(MakeVertex(vUp, -Perp1, Up));
                triStream.Append(MakeVertex(vUp, 0, Up));
                triStream.RestartStrip();


                triStream.Append(MakeVertex(vUp, -Perp3, Up));
                triStream.Append(MakeVertex(vUp, -Perp2, Up));
                triStream.Append(MakeVertex(vUp, 0, Up));
                triStream.RestartStrip();


                triStream.Append(MakeVertex(vUp, Perp1, Up));
                triStream.Append(MakeVertex(vUp, -Perp3, Up));
                triStream.Append(MakeVertex(vUp, 0, Up));
                triStream.RestartStrip();

                //Quads
                triStream.Append(MakeVertex(vUp, Perp1, Perp1));
                triStream.Append(MakeVertex(vUp, Perp2, Perp2));
                triStream.Append(MakeVertex(vDown, Perp1, Perp1));
                triStream.Append(MakeVertex(vDown, Perp2, Perp2));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, Perp2, Perp2));
                triStream.Append(MakeVertex(vUp, Perp3, Perp3));
                triStream.Append(MakeVertex(vDown, Perp2, Perp2));
                triStream.Append(MakeVertex(vDown, Perp3, Perp3));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, Perp3, Perp3));
                triStream.Append(MakeVertex(vUp, -Perp1, -Perp1));
                triStream.Append(MakeVertex(vDown, Perp3, Perp3));
                triStream.Append(MakeVertex(vDown, -Perp1, -Perp1));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, -Perp1, -Perp1));
                triStream.Append(MakeVertex(vUp, -Perp2, -Perp2));
                triStream.Append(MakeVertex(vDown, -Perp1, -Perp1));
                triStream.Append(MakeVertex(vDown, -Perp2, -Perp2));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, -Perp2, -Perp2));
                triStream.Append(MakeVertex(vUp, -Perp3, -Perp3));
                triStream.Append(MakeVertex(vDown, -Perp2, -Perp2));
                triStream.Append(MakeVertex(vDown, -Perp3, -Perp3));
                triStream.RestartStrip();

                triStream.Append(MakeVertex(vUp, -Perp3, -Perp3));
                triStream.Append(MakeVertex(vUp, Perp1, Perp1));
                triStream.Append(MakeVertex(vDown, -Perp3, -Perp3));
                triStream.Append(MakeVertex(vDown, Perp1, Perp1));
                triStream.RestartStrip();
            }

            float4 frag(g2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
        ENDCG
        }
    }
}
