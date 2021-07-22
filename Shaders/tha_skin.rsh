#ifndef THA_SKIN_RSH
#define THA_SKIN_RSH

struct skin_shading_values
{
	float4 diffuse_sample;
	float4 diffuse2_sample;
	float3 normal_sample;
	float3 normal2_sample;
	float3 specular_sample;

	float early_alpha_value;
	float3 world_space_normal;
	float2 specularity;
	float3 albedo_color;
	float ambient_ao_factor;
	float diffuse_ao_factor;
	float3 occ;
	float blood_amount;
	
	float shadow;
	float translucency;
	float3 vertex_normal;

	float4 resolve_output;

};

struct skin_auxiliary_values
{
	float4 tex_col;
	float wetness_value;
	float4 decal_albedo_alpha;
	float3 decal_normal;
	float2 decal_specularity;
	float3 albedo_color_without_effects;
};

#ifdef PIXEL_SHADER

void calculate_normal_skin_toon(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout Per_pixel_auxiliary_variables pp_aux)
{
#if !defined(SHADOWMAP_PASS)
#if SYSTEM_BLOOD_LAYER
	compute_blood_amount(In, pp_aux.decal_albedo_alpha, pp_aux.decal_normal, pp_aux.decal_specularity, In.local_position.xyz, In.local_normal);
#endif

	float3 _world_space_normal;
		#if USE_OBJECT_SPACE_TANGENT
			float3 world_binormal = cross(In.world_normal.xyz, normalize(In.world_tangent.xyz)) * In.world_tangent.w;
			float3x3 TBN = create_float3x3(normalize(In.world_tangent.xyz), world_binormal.xyz, In.world_normal.xyz);
		#else
			float3x3 TBN = cotangent_frame_from_derivatives(In.world_normal.xyz, In.world_position.xyz, In.tex_coord.xy);
		#endif	
		float3 normalTS = pp_modifiable.normal_sample;
		float3 normalTS_old = pp_modifiable.normal2_sample;

		// 			normalTS.y = 1.0 - normalTS.y;
		// 			normalTS_old.y = 1.0 - normalTS_old.y;
		float oldness = saturate(1.0f - g_mesh_factor2_color.a);
		normalTS = lerp(normalTS, normalTS_old, oldness);

#if HAS_SCAR
		{
			float3 normalTS2;
			normalTS2.xy = (2.0f * sample_texture(TattooNormalMap, linear_sampler, In.tex_coord.xy).rg - 1.0);
			normalTS2.xy *= g_normalmap_power;
			normalTS2.z = sqrt(1.0f - saturate(dot(normalTS2.xy, normalTS2.xy)));
			float alpha = sample_texture(TattooDiffuseMap, anisotropic_sampler, In.tex_coord.xy).a;
			normalTS.xyz = lerp(normalTS.xyz, float3(normalTS.xy * 0.5 + normalTS2.xy, normalTS.z), alpha);
		}
#endif

#if !SYSTEM_DXT5_NORMALMAP && !SYSTEM_BC5_NORMALMAP
		if (g_normalmap_power != 1.0f)
		{
			normalTS.xy *= g_normalmap_power;
			normalTS = normalize(normalTS);
		}
#endif


#if SYSTEM_BLOOD_LAYER
		{
			if (pp_aux.decal_albedo_alpha.a > 0.0)
			{
				normalTS.xyz = normalize(normalTS.xyz);
				pp_aux.decal_normal.xyz = normalize(pp_aux.decal_normal.xyz);
				normalTS.xyz = normalize(float3(normalTS.xy + pp_aux.decal_normal.xy, normalTS.z));
			}
		}
#endif

#if VDECL_HAS_TANGENT_DATA
	{
			_world_space_normal =  mul(normalize(normalTS), TBN);
	}
#else // VDECL_HAS_TANGENT_DATA
	{
#if !defined(SHADOWMAP_PASS) && !defined(POINTLIGHT_SHADOWMAP_PASS)
		_world_space_normal = normalize(In.world_normal.xyz);
#endif
	}
#endif // VDECL_HAS_TANGENT_DATA

	pp_modifiable.world_space_normal = _world_space_normal;
	pp_modifiable.vertex_normal = In.world_normal.xyz;
#endif

}

void calculate_albedo_skin_toon(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout Per_pixel_auxiliary_variables pp_aux)
{
	float4 tattoo_col = float4(0, 0, 0, 0);

	#ifndef DEFERRED_LIGHTING
		#if TATTOOED_FACE
			tattoo_col = sample_texture(TattooDiffuseMap, anisotropic_sampler, In.tex_coord.xy);
			if (bool(HAS_SCAR))
			{
			INPUT_TEX_GAMMA(tattoo_col.rgb);
			tattoo_col.rgb *= g_mesh_factor_color.rgb;
			}
		#endif
	#endif
	
	float4 tex_col = pp_modifiable.diffuse_sample;

	float oldness = saturate(1.0f - g_mesh_factor2_color.a);
	float3 old_albedo = pp_modifiable.diffuse2_sample.rgb;
	INPUT_TEX_GAMMA(old_albedo);
	tex_col.rgb = lerp(tex_col.rgb, old_albedo, oldness);
	tex_col.rgb *= g_mesh_factor_color.rgb;
	
	//Face color adjustment according to hair color.
	float3 eyebrow_col = g_mesh_factor2_color.rgb;
	
	#if SYSTEM_BLOOD_LAYER
		{
			tex_col.rgb = lerp(tex_col.xyz, blend_hardlight(tex_col.xyz, pp_aux.decal_albedo_alpha.xyz * 0.75), pp_aux.decal_albedo_alpha.a);
		}
	#endif
	

	if (bool(USE_AREAMAP))
	{
		if (!bool(HAS_SCAR))
		{
			if (bool(USE_COLORMAPPING))
			{
				//we are always using 2 team colors on meshes
				float colormap_r = tattoo_col.r;
				float colormap_g = tattoo_col.g;

				float3 real_g_mesh_factor2_color = saturate(g_mesh_vector_argument.rgb);
				INPUT_TEX_GAMMA(real_g_mesh_factor2_color.rgb);
				float3 tatto_ble = tattoo_col.bbb;

				if (!bool(HAS_SCAR))
				{
					tattoo_col.rgb = lerp(0, tatto_ble, colormap_g);
					tattoo_col.rgb = lerp(tattoo_col.rgb, tatto_ble * real_g_mesh_factor2_color.rgb, colormap_r);
				}
			}
			else
			{
				tattoo_col.rgb = tattoo_col.bbb;
			}
		}

		tex_col.rgb = lerp(tex_col.rgb, tattoo_col.rgb, tattoo_col.a);
	}

	if (bool(SYSTEM_RAIN_LAYER))
	{
		tex_col.rgb *= lerp(1.0, 0.3, g_rain_density);    
	}

// 	#if TATTOOED_FACE
// 		if(bool(HAS_DIRT))	//dirt
// 		{
// 			float dirt_amount = sample_diffuse_texture(linear_sampler, (In.tex_coord.zw)).a * g_mesh_vector_argument.w;
// 			tex_col.rgb = lerp(tex_col.rgb, tex_col.rgb * float3(0.76f, 0.52f, 0.4f), dirt_amount);
// 		}
// 	#endif


#ifdef SYSTEM_TEXTURE_DENSITY
	tex_col.rgb = checkerboard(tex_col, In.tex_coord.xy, diffuse_texture, false, In.world_position.xyz);
#endif

	pp_modifiable.albedo_color = tex_col.rgb;
	pp_aux.albedo_color_without_effects = tex_col.rgb;
}

void calculate_specular_skin_toon(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout Per_pixel_auxiliary_variables pp_aux)
{
	pp_modifiable.specularity = pp_modifiable.specular_sample.xy;// sample_specular_texture(In.tex_coord.xy).rg;
	pp_modifiable.specularity.x *= g_specular_coef;
	pp_modifiable.specularity.y *= g_gloss_coef;

#if SYSTEM_BLOOD_LAYER
	pp_modifiable.specularity.x = lerp(pp_modifiable.specularity.x, 1, smoothstep(0.0, 1.0, pp_aux.decal_albedo_alpha.a));	
	pp_modifiable.specularity.y = lerp(pp_modifiable.specularity.y, 0.75, smoothstep(0.0, 1.0, pp_aux.decal_albedo_alpha.a));
#else
	pp_modifiable.specularity.x = saturate(pp_modifiable.specularity.x);
	pp_modifiable.specularity.y = saturate(pp_modifiable.specularity.y);
#endif
}

#endif // PIXEL_SHADER

#endif // THA_SKIN_RSH
