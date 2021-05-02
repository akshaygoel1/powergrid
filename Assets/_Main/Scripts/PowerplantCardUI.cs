using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PowerplantCardUI : MonoBehaviour
{
    public Text costText, fuelText, citiesPoweredText;
    public Button auctionButton;
    [HideInInspector] public Powerplant thisPowerplant;

    public void AuctionButton()
    {
        GameManager.instance.StartAuctioning2(thisPowerplant);
    }

    public void PowerCities()
    {
        if (thisPowerplant.fuelType == Fuel.CoalOrOil)
        {
            if (GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Coal).count > thisPowerplant.numberOfFuelRequired)
            {
                GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Coal).count -= thisPowerplant.numberOfFuelRequired;
                if (GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Coal).count == 0)
                {
                    GameManager.instance.ReturnClientPlayer().ResOwned.Remove(GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Coal));
                }
            }
            else
            {
                GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Oil).count -= thisPowerplant.numberOfFuelRequired;
                if (GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Oil).count == 0)
                {
                    GameManager.instance.ReturnClientPlayer().ResOwned.Remove(GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Oil));
                }
            }
            UIManager.instance.PoweringCity(thisPowerplant);
        }
        else if (thisPowerplant.fuelType == Fuel.EcoFriendly)
        {
            UIManager.instance.PoweringCity(thisPowerplant);

        }
        else
        {
            GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == thisPowerplant.fuelType).count -= thisPowerplant.numberOfFuelRequired;
            UIManager.instance.PoweringCity(thisPowerplant);
            if (GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == thisPowerplant.fuelType).count == 0)
            {
                GameManager.instance.ReturnClientPlayer().ResOwned.Remove(GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == thisPowerplant.fuelType));
            }
        }

        auctionButton.interactable = false;
        UIManager.instance.AddToPoweredPowerplants(thisPowerplant);
        UIManager.instance.SetupPowerbuttons(true);
    }

}
