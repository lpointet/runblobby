using UnityEngine;

public static class _StaticFunction {

	// Vérifier qu'une animation possède un paramètre précis
	public static bool HasParameter(string paramName, Animator animator)
	{
		foreach (AnimatorControllerParameter param in animator.parameters)
		{
			if (param.name == paramName) return true;
		}
		return false;
	}

	// Renvoie une Color32 à aprtir d'un code HEX, avec une opacité maximale
	public static Color32 ToColor(int HexVal)
	{
		byte R = (byte)((HexVal >> 16) & 0xFF);
		byte G = (byte)((HexVal >> 8) & 0xFF);
		byte B = (byte)((HexVal) & 0xFF);
		return new Color32(R, G, B, 255);
	}

    // Fonction pour activer/désactiver tous les GameObjects dans un GameObject
    public static void SetActiveRecursively( GameObject rootObject, bool active ) {
        rootObject.SetActive(active);

        foreach (Transform childTransform in rootObject.transform) {
            if (!childTransform.gameObject.activeInHierarchy)
                SetActiveRecursively(childTransform.gameObject, active);
        }
    }

    public static float MathPower(float number, int exposant) {
		float result = 1.0f;

		while (exposant > 0)
		{
			if (exposant % 2 == 1)
				result *= number;
			exposant >>= 1;
			number *= number;
		}
	
		return result;
	}

	// Mapping d'une valeur sur une échelle en fonction d'une autre échelle
	// On utilise la fonction : outCurrent = (inCurrent - inMin) * (outMax - outMin) / (inMax - inMin) + outMin
	// (inCurrent - inMin) -> décalage sur l'axe des abscisses pour que le min corresponde à 0
	// (outMax - outMin) / (inMax - inMin) -> Rapport de conversion entre les deux axes directeurs
	// + outMin -> décalage sur l'axe des ordonnées pour que le outMin soit à 0
	public static float MappingScale (float inCurrent, float inMin, float inMax, float outMin, float outMax) {
		// On évite la division par 0
		if (inMax == inMin)
			return 0;

		return (inCurrent - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
	}

	// Fonction utilisée pour augmenter le volume d'un son progressivement
	public static void AudioFadeIn(AudioSource audio, float volumeMax = 1, float delay = 1) {
		if (audio == null || volumeMax < 0 || delay == 0)
			return;
		if (volumeMax > 1)
			volumeMax = 1;
		if (audio.volume > volumeMax)
			return;

		if (!audio.isPlaying) { // On démarre le son au volume le plus bas
			audio.volume = 0;
			audio.Play ();
		}

		audio.volume += Time.deltaTime / delay;
	}

	// Fonction utilisée pour diminuer le volume d'un son progressivement
	public static void AudioFadeOut(AudioSource audio, float volumeMin = 0, float delay = 1) {
		if (audio == null || volumeMin > 1 || delay == 0)
			return;
		if (volumeMin < 0)
			volumeMin = 0;
		if (audio.volume < volumeMin) { // On stoppe le son lorsqu'on est au min demandé
			audio.Stop ();
			return;
		}

		audio.volume -= Time.deltaTime / delay;
	}

	public static Color ColorFromHSV(float h, float s, float v, float a = 1)
	{
		// no saturation, we can return the value across the board (grayscale)
		if (s == 0)
			return new Color(v, v, v, a);
		
		// which chunk of the rainbow are we in?
		float sector = h / 60;
		
		// split across the decimal (ie 3.87 into 3 and 0.87)
		int i = (int)sector;
		float f = sector - i;
		
		float p = v * (1 - s);
		float q = v * (1 - s * f);
		float t = v * (1 - s * (1 - f));
		
		// build our rgb color
		Color color = new Color(0, 0, 0, a);
		
		switch(i)
		{
		case 0:
			color.r = v;
			color.g = t;
			color.b = p;
			break;
			
		case 1:
			color.r = q;
			color.g = v;
			color.b = p;
			break;
			
		case 2:
			color.r  = p;
			color.g  = v;
			color.b  = t;
			break;
			
		case 3:
			color.r  = p;
			color.g  = q;
			color.b  = v;
			break;
			
		case 4:
			color.r  = t;
			color.g  = p;
			color.b  = v;
			break;
			
		default:
			color.r  = v;
			color.g  = p;
			color.b  = q;
			break;
		}
		
		return color;
	}
	
	public static void ColorToHSV(Color color, out float h, out float s, out float v)
	{
		float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
		float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
		float delta = max - min;
		
		// value is our max color
		v = max;
		
		// saturation is percent of max
		if (!Mathf.Approximately(max, 0))
			s = delta / max;
		else
		{
			// all colors are zero, no saturation and hue is undefined
			s = 0;
			h = -1;
			return;
		}
		
		// grayscale image if min and max are the same
		if (Mathf.Approximately(min, max))
		{
			v = max;
			s = 0;
			h = -1;
			return;
		}
		
		// hue depends which color is max (this creates a rainbow effect)
		if (color.r == max)
			h = (color.g - color.b) / delta;         	// between yellow & magenta
		else if (color.g == max)
			h = 2 + (color.b - color.r) / delta; 		// between cyan & yellow
		else
			h = 4 + (color.r - color.g) / delta; 		// between magenta & cyan
		
		// turn hue into 0-360 degrees
		h *= 60;
		if (h < 0 )
			h += 360;
	}
}
