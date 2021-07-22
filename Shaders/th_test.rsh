#ifndef TH_TEST_RSH
#define TH_TEST_RSH

#if PIXEL_SHADER

#include "definitions.rsh"
#include "shared_functions.rsh"
#include "standart.rsh"	 

PS_OUTPUT main_ps_th_test( VS_OUTPUT_STANDART In )
{
	PS_OUTPUT Output;
	Output.RGBColor.rgba = float4(1.0, 0.0, 0.0, 1.0);
	return Output;
}

void calculate_albedo_standart_th(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout Per_pixel_auxiliary_variables pp_aux)
{
	float4 diffuse_texture_color = float4(1.0, 0.0, 0.0, 1.0);
	pp_modifiable.albedo_color = diffuse_texture_color;
	pp_aux.albedo_color_without_effects = pp_modifiable.albedo_color.rgb;
}
#endif

#endif
