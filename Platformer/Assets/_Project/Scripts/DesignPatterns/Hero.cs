using UnityEngine;


/// <summary>
/// this script belongs to a youtuber gitamendd, the hero.cs acts like a player controller
/// i have my own player controller with many functions  such as statemachines, custome timer system and new input system
/// AnimatuinManager is just a place holder the real script is on the BaseState.cs for the statemachine
/// </summary>
public class Hero : MonoBehaviour
{

    [SerializeField] SpellStragedy[] spells;

   
    

    void CastSpell(int index)
    {
        spells[index].CastSpell(transform);
    }
}