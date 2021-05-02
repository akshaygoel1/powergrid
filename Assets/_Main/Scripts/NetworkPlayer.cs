using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public PhotonView photonView;
    int currentMoney = 50;
    public List<Powerplant> powerplantsOwned = new List<Powerplant>();
    List<Node> citiesOwned = new List<Node>();
    List<Resource> resOwned = new List<Resource>();
    bool isMyTurn = false;
    string playerName = "";
    int rank = -1;
    public bool IsMyTurn
    {
        get
        {
            return isMyTurn;
        }
        set
        {
            isMyTurn = value;
            if (isMyTurn)
            {
                UIManager.instance.SetTurnText(photonView.Owner.NickName);
                if (photonView.IsMine)
                {
                    UIManager.instance.ToggleTurnIndicator(true);
                }
            }
            else
            {
                if (photonView.IsMine)
                {
                    UIManager.instance.ToggleTurnIndicator(false);
                }
            }
        }
    }

    public string ReturnName
    {
        get { return photonView.Owner.NickName; }
    }

    public int CurrentMoney
    {
        get { return currentMoney; }
        set
        {

            currentMoney = value;
            if (photonView.IsMine)
            {
                UIManager.instance.SetMoneyText(currentMoney);
            }
        }
    }

    public List<Node> CitiesOwned
    {
        get { return citiesOwned; }
        set
        {
            citiesOwned = value;
            UIManager.instance.SetCityOwnedText(citiesOwned.Count);
        }
    }

    public List<Resource> ResOwned
    {
        get { return resOwned; }
        set
        {
            resOwned = value;
            string s = "";
            if (resOwned.Count > 0)
            {
                foreach (Resource r in resOwned)
                {
                    s += r.count.ToString() + " " + r.fuelType.ToString() + " ";
                }
            }
            else
            {
                s = "None";
            }
            UIManager.instance.SetResourcesText(s);
        }
    }

    public int Rank
    {
        get { return rank; }
        set
        {
            rank = value;
            //Debug.Log("My Rank is : " + rank);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
        {
            MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
            for (int i = 0; i < scripts.Length; i++)
            {
                if (scripts[i] is NetworkPlayer) continue;
                else if (scripts[i] is PhotonView) continue;
                else if (scripts[i] is PhotonTransformView) continue;

                Destroy(scripts[i]);
            }
        }
        else
        {
            playerName = photonView.Owner.NickName;
        }
        CurrentMoney = 50;
        CitiesOwned = new List<Node>();
        ResOwned = new List<Resource>();
    }

    public int GetScore()
    {
        int highestPowerplant = 0;

        foreach (Powerplant p in powerplantsOwned)
        {
            if (p.cost > highestPowerplant)
            {
                highestPowerplant = p.cost;
            }
        }

        return citiesOwned.Count * 100 + highestPowerplant;
    }

}
