#ifndef THA_OUTLINE_RSH
#define THA_OUTLINE_RSH

#ifdef VERTEX_SHADER

void calculate_render_related_values_outline(inout RGL_VS_INPUT In, inout Per_vertex_modifiable_variables pv_modifiable, inout Vertex_shader_output_type output)
{
	calculate_render_related_values_standart(In , pv_modifiable, output);
	output.tex_coord.xy = In.tex_coord.xy;
	output.vertex_color = pv_modifiable.vertex_color;

#if defined(GBUFFER_PASS)	

	float3 viewNormal = mul(to_float3x3(g_view_proj), pv_modifiable.world_normal.xyz);
	viewNormal = normalize(viewNormal);
	float dis = saturate(length(pv_modifiable.world_position - g_camera_position.xyz) * 0.1f);
	float scale = 0.002f + lerp(0, 0.015f - 0.002f, dis);
	output.position.xy += viewNormal.xy * scale;
	output.position.z -= 0.0001f;
	
#endif // defined(GBUFFER_PASS)
}

#endif // VERTEX_SHADER

#ifdef PIXEL_SHADER

void sample_textures_outline(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout Per_pixel_auxiliary_variables pp_aux)
{
	pp_modifiable.shadow = 1;
	
	// Create lookup data using texture coordinate and Granite translation texture
	#if ENABLE_VIRTUAL_TEXTURING
	GraniteTranslationTexture translationTable = { point_sampler, vt_translation_texture };

	//diffuse1/spec/normal
	{
		GraniteLookupData virtual_texture_lookup;
		GraniteConstantBuffers virtual_tex_cb;
		virtual_tex_cb.tilesetBuffer = g_streaming_tile_data[g_streaming_tileset_index];
		virtual_tex_cb.streamingTextureBuffer = g_streaming_texture_data[0];
		float4 virtual_texture_resolve;
		Granite_Lookup_Anisotropic(virtual_tex_cb, translationTable, In.tex_coord.xy, virtual_texture_lookup, virtual_texture_resolve);
		pp_modifiable.diffuse_sample = sample_diffuse_virtual_texture(In, pp_modifiable, virtual_texture_lookup, virtual_tex_cb);

		#if !defined(SHADOWMAP_PASS)
		if ((In.position.x + In.position.y) % 2)
		{
			set_virtual_texture_resolve_data(In, pp_modifiable, virtual_texture_resolve);
		}
		#endif
	}
	#else
	
	pp_modifiable.diffuse_sample = sample_diffuse_texture_biased(In, anisotropic_sampler, In.tex_coord.xy);
	
	#endif

	INPUT_TEX_GAMMA(pp_modifiable.diffuse_sample.rgb);
	
	pp_modifiable.diffuse2_sample = float4(0, 0, 0, 1);
	pp_modifiable.normal_sample = float4(0.5f, 0.5f, 1, 1);
	pp_modifiable.normal2_sample = float4(0.5f, 0.5f, 1, 1);
	pp_modifiable.specular_sample = float4(0, 0, 0, 1);
}

void calculate_normal_outline(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout Per_pixel_auxiliary_variables pp_aux)
{
	pp_aux.wetness_value = 0;
	float3 _world_space_normal = normalize(g_sun_direction_inv.xyz);
	pp_modifiable.vertex_normal = _world_space_normal;
	pp_modifiable.world_space_normal = _world_space_normal;
}

void calculate_albedo_outline(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout Per_pixel_auxiliary_variables pp_aux)
{
	pp_modifiable.albedo_color = pp_modifiable.diffuse_sample * g_mesh_factor_color;
	pp_aux.albedo_color_without_effects = pp_modifiable.albedo_color.rgb;
}

void calculate_diffuse_ao_factor_outline(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout Per_pixel_auxiliary_variables pp_aux)
{
	pp_modifiable.ambient_ao_factor = 1.0f;
	pp_modifiable.diffuse_ao_factor = 0.0f;
}

#endif // PIXEL_SHADER

#endif // THA_SKIN_RSH
