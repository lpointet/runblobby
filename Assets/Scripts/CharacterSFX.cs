using UnityEngine;

public class CharacterSFX : MonoBehaviour {

    public Transform sfxFront;
    public Transform sfxBack;

    private Animator sfxFrontAnim;
    private Animator sfxBackAnim;

    private float playerSize = 32f;
    private float pixelsPerUnit = 32f;

    void Awake () {
        sfxFrontAnim = sfxFront.GetComponent<Animator>();
        sfxBackAnim = sfxBack.GetComponent<Animator>();
    }

    public void PlayAnimation(string animation, bool back = true, bool front = true)
    {
        // On ajuste la position selon les besoins de l'animation
        switch (animation)
        {
            case "magnet_begin":
            case "magnet_end":
                AdjustPosition(96f, 2f);
                break;
            default:
                AdjustPosition();
                break;
        }

        // On joue l'animation
        if (front)
			sfxFrontAnim.SetTrigger(animation);
        if (back)
			sfxBackAnim.SetTrigger(animation);
    }

    private void AdjustPosition(float animSize = 32f, float decallage = 0f)
    {
        // ((Taille animation - Taille joueur) / 2 - Décallage en pixel vers le bas) / Taille pixels per unit
        Vector2 position = new Vector2(0, ((animSize - playerSize) / 2 - decallage) / pixelsPerUnit);

        sfxFront.localPosition = position;
        sfxBack.localPosition = position;
    }
}
