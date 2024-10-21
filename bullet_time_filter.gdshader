shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform float saturation : hint_range(0.0, 1.0) = 1.0;
uniform float contrast : hint_range(0.0, 2.0) = 1.0; // Contraste avec une valeur par défaut de 1 (normal)

void fragment() {
    // Désaturation
    vec4 texColor = texture(screen_texture, SCREEN_UV); // Utiliser la texture de l'écran
    vec3 color = texColor.rgb;
    float gray = dot(color, vec3(0.299, 0.587, 0.114)); // Luminance pour désaturation
    color = mix(vec3(gray), color, saturation);

	// Augmentation du contraste
    color = (color - 0.5) * contrast + 0.5;
    color = clamp(color, 0.0, 1.0); // Empêcher les valeurs hors de [0, 1]
    
    // Appliquer la couleur finale avec transparence
    COLOR = vec4(color, texColor.a); // Conserver l'alpha d'origine
}