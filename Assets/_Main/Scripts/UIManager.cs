using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    public Text moneyText, citiesOwnedText, resourcesText;

    public GameObject powerplantUIPrefab;
    public Transform powerPlantUIHolder;
    public GameObject cityBuildingPanel;
    public Text turnText;
    public Text phaseText;
    public GameObject auctioningPanel1, auctioningPanel2;

    public Text powerplantCost, fuelRequired, citiesPowered, lastBid;

    public GameObject passBuyingPowerplant, bidButton, passButton;
    public Transform myPowerplantsHolder;
    public GameObject powerplantsOwnedPrefab;
    public GameObject buyingResourcesPanel;
    public GameObject upCoal, downCoal, upOil, downOil, upTrash, downTrash, upUranium, downUranium, confirmPurchase;
    public Text coalCountText, oilCountText, trashCountText, uraniumCountText, totalCostText;
    public GameObject doneBuildingButton, donePoweringCitiesButton;
    public GameObject myPowerplants;
    public Text currentCitiesPoweredText;
    bool phase3 = false;
    [HideInInspector] public int currentCitiesPoweredBuffer = 0;
    List<Powerplant> powerplantsAlreadyPowered = new List<Powerplant>();
    public GameObject turnIndicator;
    public Text sectionText;
    public GameObject phaseHolder, turnHolder, sectionHolder;
    public GameObject rankHolder;
    public Text rankText;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void SetMoneyText(int m)
    {
        moneyText.text = "Money : " + m.ToString();
    }

    public void SetCityOwnedText(int c)
    {
        citiesOwnedText.text = "Cities Owned : " + c.ToString();
    }

    public void SetResourcesText(string s)
    {
        resourcesText.text = "Resources : " + s.ToString();
    }

    public void AddPowerplantToAuction(Powerplant p)
    {
        GameObject g = Instantiate(powerplantUIPrefab, powerPlantUIHolder, false);
        PowerplantCardUI pUI = g.GetComponent<PowerplantCardUI>();
        pUI.thisPowerplant = p;
        pUI.costText.text = "Cost : " + p.cost;
        pUI.fuelText.text = "Fuel : " + p.numberOfFuelRequired + " " + p.fuelType.ToString();
        pUI.citiesPoweredText.text = "Cities Powered : " + p.citiesPowered;
        g.name = p.cost.ToString();
        SortPowerplants();
    }

    void SortPowerplants()
    {
        List<int> children = new List<int>();
        List<Transform> c = new List<Transform>();
        foreach (Transform child in powerPlantUIHolder)
        {
            children.Add(int.Parse(child.gameObject.name));
            c.Add(child);
        }
        children.Sort();


        while (c.Count > 0)
        {
            c[0].SetSiblingIndex(children.FindIndex(x => x.CompareTo(int.Parse(c[0].gameObject.name)) == 0));
            c.RemoveAt(0);
        }
    }

    public void ShowCityBuildingCost(Node n, int cost)
    {
        cityBuildingPanel.SetActive(true);
        CityBuildingUI c = cityBuildingPanel.GetComponent<CityBuildingUI>();

        c.nameOfCity.text = n.name;
        c.costOfCity.text = cost.ToString();
        c.cost = cost;
        c.n = n;
        if (cost > GameManager.instance.ReturnClientPlayer().CurrentMoney ||
            !GameManager.instance.CanPlayerBuildThisCity(n, GameManager.instance.ReturnClientPlayer().photonView.ViewID))
        {
            cityBuildingPanel.transform.GetChild(2).GetComponent<Button>().interactable = false;
        }
        else
        {
            cityBuildingPanel.transform.GetChild(2).GetComponent<Button>().interactable = true;
        }
    }

    public void SetTurnText(string playerName)
    {
        turnText.text = playerName + "'s Turn";
        StartCoroutine(UpdatedTextEffect(turnHolder, turnText, playerName + "'s Turn"));
    }

    public void SetPhaseText(Phase phase)
    {
        phaseText.text = phase.ToString();
        StartCoroutine(UpdatedTextEffect(phaseHolder, phaseText, phase.ToString()));
    }

    public void StartAuctioningPhase1()
    {
        auctioningPanel1.SetActive(true);
        auctioningPanel2.SetActive(false);
    }

    public void StartAuctioningPhase2(Powerplant powerplant)
    {
        auctioningPanel1.SetActive(false);
        auctioningPanel2.SetActive(true);
        powerplantCost.text = "Cost : " + powerplant.cost;
        fuelRequired.text = "Fuel : " + powerplant.numberOfFuelRequired + " " + powerplant.fuelType.ToString();
        citiesPowered.text = "Cities Powered : " + powerplant.citiesPowered;
        //lastBid.text = "Bid : " + GameManager.instance.ReturnClientPlayer().photonView.Owner.NickName + " (" + powerplant.cost + ")";
        UpdateBidder(GameManager.instance.ReturnPlayerWithCurrentTurn().ViewID, powerplant.cost);
        GameManager.instance.StartedBidding(powerplant);
    }

    public void UpdateBidder(int viewID, int cost)
    {
        lastBid.text = "Bid : " + GameManager.instance.ReturnPlayer(viewID).gameObject.GetComponent<NetworkPlayer>().ReturnName + " (" + cost + ")";
    }

    public void TogglePassBuyingPowerplantButton(bool active)
    {
        passBuyingPowerplant.SetActive(active);
    }
    public void ToggleBidButton(bool active)
    {
        bidButton.SetActive(active);
    }
    public void TogglePassButton(bool active)
    {
        passButton.SetActive(active);
    }

    public void DestroyPowerplant(Powerplant p)
    {
        foreach (Transform child in powerPlantUIHolder)
        {
            if (child.GetComponent<PowerplantCardUI>().thisPowerplant == p)
            {
                Destroy(child.gameObject);
                return;
            }
        }
    }

    public void TurnOffAuctionButton()
    {

        foreach (Transform child in powerPlantUIHolder)
        {

            child.gameObject.GetComponent<PowerplantCardUI>().auctionButton.interactable = false;
        }
        TogglePassBuyingPowerplantButton(false);

    }

    public void TurnTopFourAuctionButtonsOn()
    {
        int counter = 0;
        foreach (Transform child in powerPlantUIHolder)
        {
            if (!phase3)
            {

                if (GameManager.instance.ReturnPlayerWithCurrentTurn() == null)
                {
                    Debug.LogError("Player with current turn is null");
                }

                if (counter < 4 && child.gameObject.GetComponent<PowerplantCardUI>().thisPowerplant.cost < GameManager.instance.ReturnPlayerWithCurrentTurn().gameObject.GetComponent<NetworkPlayer>().CurrentMoney)
                {
                    child.gameObject.GetComponent<PowerplantCardUI>().auctionButton.interactable = true;
                }
                else
                {
                    child.gameObject.GetComponent<PowerplantCardUI>().auctionButton.interactable = false;
                }
            }
            else
            {
                if (child.gameObject.GetComponent<PowerplantCardUI>().thisPowerplant.cost < GameManager.instance.ReturnPlayerWithCurrentTurn().gameObject.GetComponent<NetworkPlayer>().CurrentMoney)
                {
                    child.gameObject.GetComponent<PowerplantCardUI>().auctionButton.interactable = true;
                }
            }

            counter++;
        }
        if (GameManager.instance.RoundNumber > 1)
        {
            TogglePassBuyingPowerplantButton(true);
        }
    }

    public void AddPowerplant(Powerplant p)
    {
        GameObject g = Instantiate(powerplantsOwnedPrefab, myPowerplantsHolder, false);
        PowerplantCardUI pUI = g.GetComponent<PowerplantCardUI>();
        pUI.thisPowerplant = p;
        pUI.costText.text = "Cost : " + p.cost;
        pUI.fuelText.text = "Fuel : " + p.numberOfFuelRequired + " " + p.fuelType.ToString();
        pUI.citiesPoweredText.text = "Cities Powered : " + p.citiesPowered;

    }

    public void ToggleBuyingResourcesPanel(bool active)
    {
        buyingResourcesPanel.SetActive(active);
    }

    public void ShowBuyingResourcesButtons()
    {
        upCoal.GetComponent<Button>().interactable = true;
        downCoal.GetComponent<Button>().interactable = false;
        upOil.GetComponent<Button>().interactable = true;
        downOil.GetComponent<Button>().interactable = false;
        upTrash.GetComponent<Button>().interactable = true;
        downTrash.GetComponent<Button>().interactable = false;
        upUranium.GetComponent<Button>().interactable = true;
        downUranium.GetComponent<Button>().interactable = false;
        confirmPurchase.GetComponent<Button>().interactable = true;
        SetUpButtonsForBuyingResources();
    }

    public void HideBuyingResourcesButtons()
    {
        upCoal.GetComponent<Button>().interactable = false;
        downCoal.GetComponent<Button>().interactable = false;
        upOil.GetComponent<Button>().interactable = false;
        downOil.GetComponent<Button>().interactable = false;
        upTrash.GetComponent<Button>().interactable = false;
        downTrash.GetComponent<Button>().interactable = false;
        upUranium.GetComponent<Button>().interactable = false;
        downUranium.GetComponent<Button>().interactable = false;
        confirmPurchase.GetComponent<Button>().interactable = false;

    }

    public void SetUpButtonsForBuyingResources()
    {
        /* If the player does not have a powerplant containing the resource x, then turn off the buttons for x
         * If the player has any resources from before, then add it to the count of the resources
         * If the player has reached the max limit of buying a particular resource, then disable the up button
         * Enable the down button only if the count of the resource bought is more than 0
         * If the cost of the resource is more than the player can afford then disable the up button
         * If all the pieces of a resource are selected to be bought, then disable up button
         * */
        NetworkPlayer p = GameManager.instance.ReturnPlayerWithCurrentTurn().GetComponent<NetworkPlayer>();

        if (!p.powerplantsOwned.Exists(x => x.fuelType == Fuel.Coal || x.fuelType == Fuel.CoalOrOil))
        {
            upCoal.GetComponent<Button>().interactable = false;
            downCoal.GetComponent<Button>().interactable = false;
        }
        if (!p.powerplantsOwned.Exists(x => x.fuelType == Fuel.Oil || x.fuelType == Fuel.CoalOrOil))
        {
            upOil.GetComponent<Button>().interactable = false;
            downOil.GetComponent<Button>().interactable = false;
        }
        if (!p.powerplantsOwned.Exists(x => x.fuelType == Fuel.Trash))
        {
            upTrash.GetComponent<Button>().interactable = false;
            downTrash.GetComponent<Button>().interactable = false;
        }
        if (!p.powerplantsOwned.Exists(x => x.fuelType == Fuel.Nuclear))
        {
            upUranium.GetComponent<Button>().interactable = false;
            downUranium.GetComponent<Button>().interactable = false;
        }

        int canHoldCoal = 0;
        foreach (Powerplant powerplant in p.powerplantsOwned)
        {
            if (powerplant.fuelType == Fuel.Coal || powerplant.fuelType == Fuel.CoalOrOil)
            {
                canHoldCoal += powerplant.numberOfFuelRequired * 2;
            }
        }
        int coalHeld = GameManager.instance.currentCoalBuffer;
        foreach (Resource r in p.ResOwned)
        {
            if (r.fuelType == Fuel.Coal)
            {
                coalHeld += r.count;
            }
        }
        if (coalHeld >= canHoldCoal)
        {
            upCoal.GetComponent<Button>().interactable = false;
        }


        int canHoldOil = 0;
        foreach (Powerplant powerplant in p.powerplantsOwned)
        {
            if (powerplant.fuelType == Fuel.Oil || powerplant.fuelType == Fuel.CoalOrOil)
            {
                canHoldOil += powerplant.numberOfFuelRequired * 2;
            }
        }
        int oilHeld = GameManager.instance.currentOilBuffer;
        foreach (Resource r in p.ResOwned)
        {
            if (r.fuelType == Fuel.Oil)
            {
                oilHeld += r.count;
            }
        }
        if (oilHeld >= canHoldOil)
        {
            upOil.GetComponent<Button>().interactable = false;
        }


        int canHoldTrash = 0;
        foreach (Powerplant powerplant in p.powerplantsOwned)
        {
            if (powerplant.fuelType == Fuel.Trash)
            {
                canHoldTrash += powerplant.numberOfFuelRequired * 2;
            }
        }
        int trashHeld = GameManager.instance.currentTrashBuffer;
        foreach (Resource r in p.ResOwned)
        {
            if (r.fuelType == Fuel.Trash)
            {
                trashHeld += r.count;
            }
        }
        if (trashHeld >= canHoldTrash)
        {
            upTrash.GetComponent<Button>().interactable = false;
        }


        int canHoldUranium = 0;
        foreach (Powerplant powerplant in p.powerplantsOwned)
        {
            if (powerplant.fuelType == Fuel.Nuclear)
            {
                canHoldUranium += powerplant.numberOfFuelRequired * 2;
            }
        }
        int uraniumHeld = GameManager.instance.currentUraniumBuffer;
        foreach (Resource r in p.ResOwned)
        {
            if (r.fuelType == Fuel.Nuclear)
            {
                uraniumHeld += r.count;
            }
        }
        if (uraniumHeld >= canHoldUranium)
        {
            upUranium.GetComponent<Button>().interactable = false;
        }


        if (GameManager.instance.currentCoalBuffer > 0)
        {
            downCoal.GetComponent<Button>().interactable = true;
        }
        if (GameManager.instance.currentOilBuffer > 0)
        {
            downOil.GetComponent<Button>().interactable = true;
        }
        if (GameManager.instance.currentTrashBuffer > 0)
        {
            downTrash.GetComponent<Button>().interactable = true;
        }
        if (GameManager.instance.currentUraniumBuffer > 0)
        {
            downUranium.GetComponent<Button>().interactable = true;
        }


        if (GameManager.instance.currentCostBuffer + GameManager.instance.CalculateNextCost(Fuel.Coal) > p.CurrentMoney ||
            GameManager.instance.IsCurrentBufferFullWithResource(Fuel.Coal))
        {
            upCoal.GetComponent<Button>().interactable = false;
        }
        if (GameManager.instance.currentCostBuffer + GameManager.instance.CalculateNextCost(Fuel.Oil) > p.CurrentMoney
            ||
            GameManager.instance.IsCurrentBufferFullWithResource(Fuel.Oil))
        {
            upOil.GetComponent<Button>().interactable = false;
        }
        if (GameManager.instance.currentCostBuffer + GameManager.instance.CalculateNextCost(Fuel.Trash) > p.CurrentMoney ||
            GameManager.instance.IsCurrentBufferFullWithResource(Fuel.Trash))
        {
            upTrash.GetComponent<Button>().interactable = false;
        }
        if (GameManager.instance.currentCostBuffer + GameManager.instance.CalculateNextCost(Fuel.Nuclear) > p.CurrentMoney ||
            GameManager.instance.IsCurrentBufferFullWithResource(Fuel.Nuclear))
        {
            upUranium.GetComponent<Button>().interactable = false;
        }
    }


    public void Up(int resourceID)
    {
        GameManager.instance.Resource_UpButtonPress(resourceID);
    }

    public void Down(int resourceID)
    {
        GameManager.instance.Resource_DownButtonPress(resourceID);
    }

    public void UpdateResourceText(int rId, int val, int cost)
    {
        totalCostText.text = "Total Cost : " + cost;
        switch (rId)
        {
            case 0:
                coalCountText.text = "Coal : " + val.ToString();
                break;
            case 1:
                oilCountText.text = "Oil : " + val.ToString();
                break;
            case 2:
                trashCountText.text = "Trash : " + val.ToString();
                break;
            case 3:
                uraniumCountText.text = "Uranium : " + val.ToString();
                break;
        }
    }

    public void ConfirmBuyingResources()
    {
        GameManager.instance.ConfirmBuyingResources();
        buyingResourcesPanel.SetActive(false);
    }

    public void ToggleDoneBuilding(bool active)
    {
        doneBuildingButton.SetActive(active);
    }

    public void DoneBuilding()
    {
        ToggleDoneBuilding(false);

        GameManager.instance.DoneBuilding();
    }

    public void ToggleDonePoweringCities(bool active)
    {
        donePoweringCitiesButton.SetActive(active);
    }

    public void DonePoweringCities()
    {
        ToggleDonePoweringCities(false);
        myPowerplants.SetActive(false);
        currentCitiesPoweredText.gameObject.SetActive(false);
        myPowerplants.transform.GetChild(1).gameObject.SetActive(true);
        GameManager.instance.DonePoweringCities(currentCitiesPoweredBuffer);
        currentCitiesPoweredBuffer = 0;
    }

    public void ToggleMyPowerplants()
    {
        if (GameManager.instance.CurrentPhase != Phase.Earning)
        {
            if (myPowerplants.activeSelf)
            {
                myPowerplants.SetActive(false);
                //currentCitiesPoweredText.gameObject.SetActive(false);
                //currentCitiesPoweredText.text = "Current Cities Powered : " + currentCitiesPoweredBuffer.ToString() + " ; " + "Profit : " + GameManager.instance.moneyAsPerCitiesPowered[currentCitiesPoweredBuffer].ToString();
            }
            else
            {
                myPowerplants.SetActive(true);
            }
            DisablePowerButtons();
        }
        else
        {
            if (myPowerplants.activeSelf)
            {
                myPowerplants.SetActive(false);
                //currentCitiesPoweredText.gameObject.SetActive(false);
                //currentCitiesPoweredText.text = "Current Cities Powered : " + currentCitiesPoweredBuffer.ToString() + " ; " + "Profit : " + GameManager.instance.moneyAsPerCitiesPowered[currentCitiesPoweredBuffer].ToString();
            }
            else
            {
                myPowerplants.SetActive(true);
            }
        }
    }

    public void EnableMyPowerplants()
    {
        myPowerplants.SetActive(true);
        if (GameManager.instance.CurrentPhase == Phase.Earning)
        {
            SetupPowerbuttons(true);
            currentCitiesPoweredText.gameObject.SetActive(true);
            currentCitiesPoweredText.text = "Current Cities Powered : " + currentCitiesPoweredBuffer.ToString() + " ; " + "Profit : " + GameManager.instance.moneyAsPerCitiesPowered[currentCitiesPoweredBuffer].ToString();
            myPowerplants.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            SetupPowerbuttons(false);
        }
    }


    void DisablePowerButtons()
    {
        foreach (Transform child in myPowerplantsHolder)
        {
            PowerplantCardUI p = child.GetComponent<PowerplantCardUI>();
            p.auctionButton.interactable = false;
        }
    }

    public void SetupPowerbuttons(bool enabled)
    {
        if (enabled)
        {
            foreach (Transform child in myPowerplantsHolder)
            {
                PowerplantCardUI p = child.GetComponent<PowerplantCardUI>();

                if (powerplantsAlreadyPowered.Contains(p.thisPowerplant))
                {
                    p.auctionButton.interactable = false;
                }
                else if (GameManager.instance.ReturnClientPlayer().CitiesOwned.Count <= currentCitiesPoweredBuffer)
                {
                    p.auctionButton.interactable = false;
                }
                else if ((p.thisPowerplant.fuelType == Fuel.Coal && GameManager.instance.ReturnClientPlayer().ResOwned.Exists(x => x.fuelType == Fuel.Coal) && p.thisPowerplant.numberOfFuelRequired <= GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Coal).count))
                {
                    p.auctionButton.interactable = true;
                }
                else if ((p.thisPowerplant.fuelType == Fuel.Oil && GameManager.instance.ReturnClientPlayer().ResOwned.Exists(x => x.fuelType == Fuel.Oil) && p.thisPowerplant.numberOfFuelRequired <= GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Oil).count))
                {
                    p.auctionButton.interactable = true;
                }
                else if ((p.thisPowerplant.fuelType == Fuel.Trash && GameManager.instance.ReturnClientPlayer().ResOwned.Exists(x => x.fuelType == Fuel.Trash) && p.thisPowerplant.numberOfFuelRequired <= GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Trash).count))
                {
                    p.auctionButton.interactable = true;
                }
                else if ((p.thisPowerplant.fuelType == Fuel.Nuclear && GameManager.instance.ReturnClientPlayer().ResOwned.Exists(x => x.fuelType == Fuel.Nuclear) && p.thisPowerplant.numberOfFuelRequired <= GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Nuclear).count))
                {
                    p.auctionButton.interactable = true;
                }
                else if ((p.thisPowerplant.fuelType == Fuel.EcoFriendly))
                {
                    p.auctionButton.interactable = true;
                }
                else if ((p.thisPowerplant.fuelType == Fuel.CoalOrOil && (GameManager.instance.ReturnClientPlayer().ResOwned.Exists(x => x.fuelType == Fuel.Oil) && p.thisPowerplant.numberOfFuelRequired <= GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Oil).count)
                    || (GameManager.instance.ReturnClientPlayer().ResOwned.Exists(x => x.fuelType == Fuel.Coal) && p.thisPowerplant.numberOfFuelRequired <= GameManager.instance.ReturnClientPlayer().ResOwned.Find(x => x.fuelType == Fuel.Coal).count)))
                {
                    p.auctionButton.interactable = true;
                }
                else
                {
                    p.auctionButton.interactable = false;
                }
            }
        }
        else
        {
            foreach (Transform child in myPowerplantsHolder)
            {
                child.GetComponent<PowerplantCardUI>().auctionButton.interactable = false;
            }
        }


    }

    public void PoweringCity(Powerplant powerplant)
    {
        currentCitiesPoweredBuffer += powerplant.citiesPowered;
        string s = "";
        if (GameManager.instance.ReturnClientPlayer().ResOwned.Count > 0)
        {
            foreach (Resource r in GameManager.instance.ReturnClientPlayer().ResOwned)
            {
                s += r.count.ToString() + " " + r.fuelType.ToString() + " ";
            }
        }
        else
        {
            s = "None";
        }

        int actualCitiesPowered = 0;

        if (currentCitiesPoweredBuffer > GameManager.instance.ReturnClientPlayer().CitiesOwned.Count)
        {
            actualCitiesPowered = GameManager.instance.ReturnClientPlayer().CitiesOwned.Count;
        }
        else
        {
            actualCitiesPowered = currentCitiesPoweredBuffer;
        }

        UIManager.instance.SetResourcesText(s);
        currentCitiesPoweredText.text = "Current Cities Powered : " + actualCitiesPowered.ToString() + " ; " + "Profit : " + GameManager.instance.moneyAsPerCitiesPowered[actualCitiesPowered].ToString();
    }

    public void RemoveHighestCard()
    {

        Powerplant p = powerPlantUIHolder.transform.GetChild(powerPlantUIHolder.transform.childCount - 1).GetComponent<PowerplantCardUI>().thisPowerplant;
        Debug.LogError("Removing Highest Card : " + p.cost.ToString());
        GameManager.instance.powerPlants.Add(p);
        Destroy(powerPlantUIHolder.transform.GetChild(powerPlantUIHolder.transform.childCount - 1).gameObject);
    }

    public void RemoveMyPowerplantCard(Powerplant powerplant)
    {
        foreach (Transform child in myPowerplantsHolder)
        {
            if (child.GetComponent<PowerplantCardUI>().thisPowerplant == powerplant)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    public void RemoveLowestCard()
    {
        Debug.LogError("Removing Lowest Card : " + powerPlantUIHolder.transform.GetChild(0).GetComponent<PowerplantCardUI>().thisPowerplant.cost.ToString());
        //Debug.LogError("RemovedLowestCard Powerplant with cost " + powerPlantUIHolder.transform.GetChild(0).GetComponent<PowerplantCardUI>().thisPowerplant.cost);
        Destroy(powerPlantUIHolder.transform.GetChild(0).gameObject);
    }

    public void RemoveLowPowerplantsThan(int numberOfCities)
    {
        bool foundbreak = true;
        while (foundbreak)
        {
            foreach (Transform child in powerPlantUIHolder)
            {
                if (child.GetComponent<PowerplantCardUI>().thisPowerplant.cost <= numberOfCities && child.GetComponent<PowerplantCardUI>().thisPowerplant.cost > 5)
                {
                    Debug.LogError("Removing Deprecated Powerplants : " + child.GetComponent<PowerplantCardUI>().thisPowerplant.cost.ToString());
                    Destroy(child.gameObject);
                    GameManager.instance.AddNextPowerplantCard();
                    foundbreak = true;
                    break;
                }
                else
                {
                    foundbreak = false;
                }
            }
        }
    }

    public void SetupPowerplantMarketAccordingToPhase3()
    {
        powerPlantUIHolder.GetComponent<GridLayoutGroup>().constraintCount = 3;
        phase3 = true;

        while (powerPlantUIHolder.childCount > 6)
        {
            Destroy(powerPlantUIHolder.GetChild(0));
        }

    }

    public void AddToPoweredPowerplants(Powerplant p)
    {
        powerplantsAlreadyPowered.Add(p);
    }

    public void ClearPoweredPowerplants()
    {
        powerplantsAlreadyPowered.Clear();
    }

    public void ToggleTurnIndicator(bool active)
    {
        turnIndicator.SetActive(active);
    }

    public void UpdateSectionText()
    {
        //sectionText.text = "Phase : " + GameManager.instance.CurrentSection;
        StartCoroutine(UpdatedTextEffect(sectionHolder, sectionText, "Phase : " + GameManager.instance.CurrentSection));
    }

    IEnumerator UpdatedTextEffect(GameObject holder, Text t, string s)
    {
        if (GameManager.instance.ReturnClientPlayer().photonView.IsMine)
        {
            while (holder.transform.localScale.x > 0f)
            {
                yield return new WaitForSeconds(0.01f);
                holder.transform.localScale -= Vector3.one * 0.05f;
            }
            holder.transform.localScale = Vector3.zero;
            t.text = s;
            while (holder.transform.localScale.x < 1f)
            {
                yield return new WaitForSeconds(0.01f);
                holder.transform.localScale += Vector3.one * 0.05f;
            }
            holder.transform.localScale = Vector3.one;
        }
    }

    public void ShowRank(int rank)
    {
        rankHolder.SetActive(true);
        rankText.text = "Rank : " + rank;
    }

}
