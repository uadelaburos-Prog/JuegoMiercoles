using UnityEngine;

// Tipo de dano que trae el arma
public enum DamageType { Insecticida, Fungicida, Bala, Perdigones, Fuego }

// Categoria a la que pertenece el enemigo
public enum EnemyCategory { Insecto, Carnoso, Rocoso, Fungico }

// Resultado de cruzar DamageType con EnemyCategory
public enum Effectiveness { Debil, Neutral, Resistente, Nulo }

public static class EffectivenessExtensions
{
    public static float Multiplier(this Effectiveness e)
    {
        switch (e)
        {
            case Effectiveness.Debil: return 2f;
            case Effectiveness.Neutral: return 1f;
            case Effectiveness.Resistente: return 0.5f;
            default: return 0f;
        }
    }

    // Alpha 0 significa que no parpadea (caso Nulo)
    public static Color FlashColor(this Effectiveness e)
    {
        switch (e)
        {
            case Effectiveness.Debil: return new Color(1f, 0.2f, 0.2f, 1f);
            case Effectiveness.Neutral: return Color.white;
            case Effectiveness.Resistente: return new Color(0.6f, 0.6f, 0.6f, 1f);
            default: return new Color(0f, 0f, 0f, 0f);
        }
    }
}
