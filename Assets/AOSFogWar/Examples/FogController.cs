using FischlWorks_FogWar;
using UnityEngine;

public class FogController : MonoBehaviour
{
    public int radius = 3;
    
    public void Init(bool isPlayer)
    {
        print("FogController Init called. isPlayer: " + isPlayer);
        GetComponent<csFogVisibilityAgent>().enabled = !isPlayer;

        if(isPlayer)
        {
            csFogWar fogWar = FindObjectOfType<csFogWar>();

            csFogWar.FogRevealer revealer = new csFogWar.FogRevealer(transform, radius, true);
            fogWar.AddFogRevealer(revealer);
        }
    }
}
