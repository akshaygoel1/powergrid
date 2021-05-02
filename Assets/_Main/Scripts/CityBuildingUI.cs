using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CityBuildingUI : MonoBehaviour
{
    [HideInInspector] public Node n;
    [HideInInspector] public int cost;

    public Text nameOfCity, costOfCity;


    public void Build()
    {
        GameManager.instance.SpawnHouse(n, cost);
        UIManager.instance.SetCityOwnedText(GameManager.instance.ReturnPlayerWithCurrentTurn().GetComponent<NetworkPlayer>().CitiesOwned.Count);
        this.gameObject.SetActive(false);
    }


}
