#ifndef THA_SHADING_RSH
#define THA_SHADING_RSH

#ifdef VERTEX_SHADER

void discard_covered_part(inout RGL_VS_INPUT In, inout Per_vertex_modifiable_variables pv_modifiable, inout Vertex_shader_output_type output)
{
	if (!any(g_mesh_vector_argument))
	{
		float3 shouldDiscard = g_mesh_vector_argument.rgb - get_vertex_color(In.color).rgb;
		if (any(shouldDiscard >= float3(0, 0, 0)))
			output.position = float4(0, 0, 0, 1);
	}
}

#endif // VERTEX_SHADER

#if PIXEL_SHADER
#if !defined(SHADOWMAP_PASS) && !defined(POINTLIGHT_SHADOWMAP_PASS) && !defined(GBUFFER_PASS) && !defined(CONSTANT_OUTPUT_PASS)

#if defined(USE_DIRECTX11) || defined(USE_GNM)
	Texture2D texture5 : register(t_custom_5);
#elif defined(USE_DIRECTX12) 
	#define texture5 (Texture2D_table[indices.t_custom_5])
#endif

#define ramp_texture texture5

float4 sample_ramp_texture(float ndl)
{
#if SYSTEM_DONT_FETCH_TEXTURES && defined(GBUFFER_PASS)
	return float4(1, 1, 1, 1);
#endif

	return ramp_texture.Sample(linear_clamp_sampler, float2(0, 1 - ndl));
}

float compute_sun_amount_toon(float3 world_space_position, float2 screen_space_position)
{
	float sun_amount = sample_texture(postfx_shadowmap_texture, point_sampler, screen_space_position).r;

	return sun_amount;
}

direct_lighting_info get_lighting_info_toon(float3 world_space_position, float2 _screen_space_position, float3 world_space_normal)
{
	direct_lighting_info out_l = (direct_lighting_info)0;

	out_l.light_color = g_sun_color.rgb;
	out_l.light_direction = g_sun_direction_inv.xyz;
	
	float ndotl = dot(world_space_normal, g_sun_direction_inv.xyz);
	float _sun_amount = 0;
	
	if (false)
	{
		_sun_amount = compute_sun_amount_from_cascades(world_space_position, _screen_space_position);
	}
	else
	{
		_sun_amount = compute_sun_amount(world_space_position, _screen_space_position);
	}
	
	_sun_amount = saturate(_sun_amount - ddx(_sun_amount) * 0.5);

	out_l.light_amount = lerp(1, _sun_amount, g_shadow_opacity);

	return out_l;

}

float3 compute_lighting_toon(float2 specularity_info, float3 albedo_color,
				float3 light_color, float light_amount, float3 world_space_normal, float3 view_direction, float3 light_direction, float diffuse_ao_factor)
{
	float NdotL = max(1 - g_shadow_opacity, saturate(dot(light_direction, world_space_normal.xyz)));

	float3 diffuse_light = albedo_color.rgb;
	float3 specular_light = float3(0.0, 0.0, 0.0);
	float3 result_color = float3(0.0, 0.0, 0.0);
	float3 reconstructed_pixel_specular_color = construct_specular_color(specularity_info, diffuse_light);

	 diffuse_light = diffuse_light / RGL_PI;

		float3 half_vector = normalize(view_direction.xyz + light_direction);
		float NdotH = saturate(dot(world_space_normal, half_vector));
		float VdotH = saturate(dot(view_direction, half_vector));
		float NdotV = saturate(dot(world_space_normal, view_direction));


		//if(specularity_info.y > 0) //TODO_PERF_SHADERS: create material flag option?
		{
#if USE_METALLIC_WORKFLOW
			float3 F = reconstructed_pixel_specular_color + (1.0f - reconstructed_pixel_specular_color) * pow((1.0 - VdotH), 5);
#else
			float3 F = reconstructed_pixel_specular_color + (max(specularity_info.yyy, reconstructed_pixel_specular_color) - reconstructed_pixel_specular_color) * pow((1.0 - VdotH), 5);
#endif
			//NDF ndotl and ndotv omitted because of cook torrance's denominator
			float roughness = 1.0f - specularity_info.y;
			roughness = max(1e-4, roughness);
			float alpha = roughness * roughness;
			float alpha_sqr = alpha * alpha;
			float ndoth_sqr = NdotH * NdotH;
			float D = alpha_sqr;
			float denominator = (ndoth_sqr * (alpha_sqr - 1) + 1);
			denominator *= denominator;
			denominator = max(denominator, 1e-6);
			D = D / denominator;

#if USE_METALLIC_WORKFLOW
			//Hotness remapping for grazing angles Burley 2012
			float alphag = (0.5 + roughness * 0.5);
			alphag *= alphag;
#else
			float alphag = alpha;
#endif
			float k = alphag * 0.5;
			float G_L = 1.0f / (NdotL * (1.0 - k) + k);
			float G_V = 1.0f / (NdotV * (1.0 - k) + k);
			float G = G_L * G_V;

			specular_light = D * G * F;
			specular_light = specular_light / (4.0 * RGL_PI);
		}

#if USE_METALLIC_WORKFLOW
		diffuse_light = diffuse_light * saturate(1.0 - specularity_info.x);
#else
#ifndef WATER_RENDERING
        diffuse_light = diffuse_light * saturate(1.0 - 1.5f * specularity_info.x);
#endif
#endif
		float ramp_NdotL = max(1 - g_shadow_opacity, min(dot(light_direction, world_space_normal.xyz) * 0.5 + 0.5, light_amount));
		float3 rampColor = sample_ramp_texture(ramp_NdotL).rgb;
		INPUT_TEX_GAMMA(rampColor)
		//float rimVal = 1 - pow(saturate(dot(view_direction, world_space_normal)), 0.25);
		//rimVal = smoothstep(0, 1, rimVal);
		result_color = (diffuse_light + specular_light * diffuse_ao_factor * max(0.1, saturate(specularity_info.y * 10))) * light_color.rgb * rampColor;

	return result_color;
}

float3 compute_direct_lighting_toon(direct_lighting_info l_info, float2 specularity_info, float3 albedo_color,
	float3 world_space_normal, float3 view_direction, float3 world_space_position,
	float2 screen_space_position, float diffuse_ao_factor)
{
	return compute_lighting_toon(specularity_info, albedo_color,
		l_info.light_color, l_info.light_amount, world_space_normal, view_direction, l_info.light_direction, diffuse_ao_factor);
}

void calculate_final_pbr_toon(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout PS_OUTPUT_TO_USE Output)
{
#if !BAKE_TERRAIN_DATA
	float3 decal_emission_ambient_term = 0;
#if !defined(WATER_RENDERING) && !defined(STANDART_FOR_HORSE) && !defined(STANDART_FOR_EYE) && !defined(STANDART_FOR_CROWD)
	float3 PositionWS_DX = ddx_fine(pp_static.world_space_position.xyz);
	float3 PositionWS_DY = ddy_fine(pp_static.world_space_position.xyz);

	float2 world_to_uv = pp_static.world_space_position.xy * 0.24f;
	float4 world_to_uv_dxdy = float4(ddx(world_to_uv), ddy(world_to_uv));
#if (!(IS_TERRAIN || IS_GRASS) && (MATERIAL_ID_DEFERRED == my_material_id))
	[branch]
	if ((get_material_id_raw(pp_static.screen_space_position) & 0x80)) // 0x80 is_stationary
#endif
	{
#if !IS_GRASS
		float distance_factor = 1 - saturate(pp_static.view_length / 64.0f);
		pp_modifiable.world_space_normal = lerp(pp_modifiable.world_space_normal, float3(0, 0, 1), (1 - smoothstep(g_water_level, g_water_level + 0.1, pp_static.world_space_position.z)) * distance_factor);
		pp_modifiable.albedo_color.xyz = lerp(pp_modifiable.albedo_color.xyz * 0.5, pp_modifiable.albedo_color.xyz, smoothstep(g_water_level, g_water_level + 0.1, pp_static.world_space_position.z));
#endif
		calculate_add_decal_deferred(pp_static, pp_modifiable, In, decal_emission_ambient_term, PositionWS_DX, PositionWS_DY);
		calculate_weather_effects(In, pp_static, pp_modifiable, world_to_uv, world_to_uv_dxdy);
	}
#endif

	direct_lighting_info l_info = get_lighting_info_toon(pp_static.world_space_position, pp_static.screen_space_position, pp_modifiable.world_space_normal);

#if defined(NO_SHADOWS)
	l_info.light_amount = 1;
#endif

	float3 specular_ambient_term;
	float3 diffuse_ambient_term;
	float sky_visibility;

	get_ambient_terms(pp_static.world_space_position, pp_modifiable.world_space_normal, pp_modifiable.world_space_normal, pp_static.screen_space_position.xy,
		pp_static.view_vector, pp_modifiable.specularity.xy, pp_modifiable.albedo_color, l_info.light_amount,
		specular_ambient_term, diffuse_ambient_term, sky_visibility);

	float3 ambient_light = pp_modifiable.albedo_color.rgb * diffuse_ambient_term;
	ambient_light *= 1.0 - pp_modifiable.specularity.x;
	ambient_light += specular_ambient_term;
	float3 final_color = ambient_light * pp_modifiable.ambient_ao_factor;
	/* --- Lighting with toon --- */
	float3 sun_lighting = compute_direct_lighting_toon(l_info, pp_modifiable.specularity, pp_modifiable.albedo_color.rgb,
			pp_modifiable.world_space_normal, pp_static.view_vector, pp_static.world_space_position,
			pp_static.screen_space_position, pp_modifiable.ambient_ao_factor);	
	final_color += sun_lighting * pp_modifiable.diffuse_ao_factor;

#if VDECL_HAS_DOUBLEUV
#ifdef ADDITIVE_LIGHTMAP
	float3 light_map_lighting = sample_detail_normal_texture(In.tex_coord.zw) * g_mesh_vector_argument.www;
	INPUT_TEX_GAMMA(light_map_lighting.rgb);
	final_color += light_map_lighting * pp_modifiable.albedo_color.rgb;
#endif
#endif

#if SELF_ILLUMINATION
	float3 color_factor = float3(1, 1, 1);
	float2 tex_coord = 0;
#if USE_VERTEX_COLORS
	color_factor = In.vertex_color.rgb;
#endif
	tex_coord = In.tex_coord.xy;

	float3 illumination_color = g_mesh_factor_color.rgb * color_factor;
	apply_self_illumination(In, pp_modifiable, final_color.rgb, tex_coord.xy, pp_modifiable.world_space_normal, pp_static.world_space_position, illumination_color);
#endif

	final_color.rgb += decal_emission_ambient_term;

#ifdef USE_POINT_LIGHTS
	[branch]
	if (g_use_tiled_rendering) // == 1
	{
#ifdef SYSTEM_SHOW_TILED_LIGHT_OVERDRAW
		float light_tile_heatmap_weight = 0;
#endif 
		float3 total_color = 0;
		float2 ss_pos = saturate(pp_static.screen_space_position);
		uint2 tile_counts = (uint2)ceil(g_application_viewport_size / RGL_TILED_CULING_TILE_SIZE);
		uint2 tile_index = (ss_pos * g_application_viewport_size / g_rc_scale) / RGL_TILED_CULING_TILE_SIZE;
		uint start_index = MAX_LIGHT_PER_TILE * (tile_counts.x * tile_index.y + tile_index.x);
		uint probe_index = visible_lights_wDepth[start_index];

		float total_weight = 0;

		float4 occ_vec = sample_ssao_texture(pp_static.screen_space_position);

		while (probe_index != 0xFFFF)
		{
#ifdef SYSTEM_SHOW_TILED_LIGHT_OVERDRAW
			light_tile_heatmap_weight += 1 / g_tiled_lights_overdraw_visualize_limit;
#endif 
			total_color += compute_point_light_contribution_standard(probe_index, pp_modifiable.specularity, pp_modifiable.albedo_color, pp_modifiable.world_space_normal,
				pp_static.view_vector, pp_static.world_space_position, pp_static.screen_space_position, occ_vec, pp_modifiable.vertex_normal);
			probe_index = visible_lights_wDepth[++start_index];
		}

#ifdef SYSTEM_SHOW_TILED_LIGHT_OVERDRAW
		final_color.rgb += heat_map(light_tile_heatmap_weight);
#endif 

		final_color.rgb += (total_color);
	}
#endif

	apply_advanced_fog(final_color.rgb, pp_static.view_vector, pp_static.view_vector_unorm.z, pp_static.view_length, sky_visibility);
	//apply_new_fog(final_color.rgb, pp_static.world_space_position);

	if (bool(USE_SHADOW_DEBUGGING))
	{
		int index = compute_shadow_index(pp_static.world_space_position);

		if (index == 4)
		{
			final_color.rgb *= float3(1, 1, 1);
		}
		else if (index == 3)
		{
			final_color.rgb *= float3(1, 0, 1);
		}
		else if (index == 2)
		{
			final_color.rgb *= float3(0, 0, 1);
		}
		else if (index == 1)
		{
			final_color.rgb *= float3(0, 1, 0);
		}
		else
		{
			final_color.rgb *= float3(1, 0, 0);
		}
	}

#if IS_GRASS
	//float3 view_dir = normalize(pp_static.world_space_position - g_camera_position.xyz);
	float NdotL = saturate(dot(g_sun_direction_inv, pp_modifiable.world_space_normal * float3(-1, -1, 1)));
	float3 translucency_light = pp_modifiable.albedo_color.rgb * sun_lighting;
	final_color.rgb += saturate(translucency_light * NdotL);
#endif

	Output.RGBColor.rgb = output_color(final_color.rgb);
	Output.RGBColor.a = pp_modifiable.early_alpha_value;
#ifdef MATTE_SHADOW
	Output.RGBColor.a = saturate(1.0f - l_info.light_amount) * (g_mesh_vector_argument.x / 255.0f);
#endif
#endif
}

#endif // !defined(SHADOWMAP_PASS) && !defined(POINTLIGHT_SHADOWMAP_PASS) && !defined(GBUFFER_PASS) && !defined(CONSTANT_OUTPUT_PASS)

void apply_alpha_test_skin_toon(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout Per_pixel_auxiliary_variables pp_aux)
{	
	float alpha_val = pp_modifiable.early_alpha_value;
	clip(In.vertex_color.r - g_mesh_vector_argument.r);
	clip(In.vertex_color.g - g_mesh_vector_argument.g);
	clip(In.vertex_color.b - g_mesh_vector_argument.b);
	#if defined(SYSTEM_USE_COMPRESSED_FLORA_INSTANCING)
		#if defined(SYSTEM_DEPTH_PREPASS)
			apply_alpha_test_impl(In, alpha_val);
		#endif
	#else
		apply_alpha_test_impl(In, alpha_val);
	#endif
}

#endif // PIXEL_SHADER

#endif // THA_SHADING_RSH
