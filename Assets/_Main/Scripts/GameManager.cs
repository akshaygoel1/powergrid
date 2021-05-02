using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;
using System.Linq;
public enum Phase
{
    DetermineTurnOrder,
    Auctioning,
    BuyingResources,
    Building,
    Earning
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public List<Transform> spawnPoints = new List<Transform>();
    List<int> playerIds = new List<int>();
    public List<Powerplant> powerPlants = new List<Powerplant>();
    public PhotonView gameManagerPhotonView;
    Phase currentPhase = Phase.DetermineTurnOrder;
    int roundNumber = 1;
    List<int> shuffledPowerplants = new List<int>();
    int currentSection = 1;
    public GameObject playerHouse;
    Bidder lastBidder = new Bidder();
    List<int> currentBidders = new List<int>();
    List<int> currentBiddersInSubRound = new List<int>();
    int currentPowerplantTracker = 0;
    int currentPlayerTracker = 0;

    const int maxCountOfCoal = 24;
    const int maxCountOfOil = 24;
    const int maxCountOfTrash = 24;
    const int maxCountOfUranium = 12;

    List<GameObject> coalPieces = new List<GameObject>();
    List<GameObject> oilPieces = new List<GameObject>();
    List<GameObject> trashPieces = new List<GameObject>();
    List<GameObject> uraniumPieces = new List<GameObject>();

    public GameObject coalPrefab, oilPrefab, trashPrefab, uraniumPrefab;

    public List<Transform> coalSpawnPoints, oilSpawnPoints, trashSpawnPoints, uraniumSpawnPoints = new List<Transform>();
    [HideInInspector] public int currentCoalBuffer, currentOilBuffer, currentTrashBuffer, currentUraniumBuffer, currentCostBuffer = 0;

    public Dictionary<int, int> moneyAsPerCitiesPowered = new Dictionary<int, int>();
    public List<SpecialCase> specialCases = new List<SpecialCase>();
    SpecialCase currentSpecialCase = new SpecialCase();
    Dictionary<int, int> completedPoweringCities = new Dictionary<int, int>();
    public List<CityHolder> cityHolders = new List<CityHolder>();
    bool thirdPhaseTriggered = false;
    bool endTriggered = false;

    int passCount = 0;

    public List<Transform> spawnPointsForCityTracker = new List<Transform>();
    public Vector3[] offsets = new Vector3[6];
    public GameObject cityTrackerPrefab;
    public Transform cityTrackerHolder;
    Dictionary<int, GameObject> cityTrackerTokens = new Dictionary<int, GameObject>();
    public int CurrentSection
    {
        get { return currentSection; }
        set
        {
            currentSection = value;
            UIManager.instance.UpdateSectionText();
        }
    }

    public int RoundNumber
    {
        get { return roundNumber; }
    }


    public Phase CurrentPhase
    {
        get { return currentPhase; }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {

        specialCases.Add(new SpecialCase(2, 3, 8, 4, 10, 21, new List<int> { 3, 2, 1, 1 }, new List<int> { 4, 2, 2, 1 }, new List<int> { 3, 4, 3, 1 }));
        specialCases.Add(new SpecialCase(3, 3, 8, 3, 7, 17, new List<int> { 4, 2, 1, 1 }, new List<int> { 5, 3, 2, 1 }, new List<int> { 3, 4, 3, 1 }));
        specialCases.Add(new SpecialCase(4, 4, 4, 3, 7, 17, new List<int> { 5, 3, 2, 1 }, new List<int> { 6, 4, 3, 2 }, new List<int> { 4, 5, 4, 2 }));
        specialCases.Add(new SpecialCase(5, 5, 0, 3, 7, 15, new List<int> { 5, 4, 3, 2 }, new List<int> { 7, 5, 3, 3 }, new List<int> { 5, 6, 5, 2 }));
        specialCases.Add(new SpecialCase(6, 5, 0, 3, 6, 14, new List<int> { 7, 5, 3, 2 }, new List<int> { 9, 6, 5, 3 }, new List<int> { 6, 7, 6, 3 }));

        roundNumber = 1;
        GameObject go = PhotonNetwork.Instantiate("Player", new Vector3(1000, 1000, 1000), Quaternion.identity, 0);
        int viewId = go.GetPhotonView().ViewID;
        gameManagerPhotonView.RPC("RPC_PlayerSetup", RpcTarget.All, viewId);
        SetupResources();

        moneyAsPerCitiesPowered.Add(0, 10);
        moneyAsPerCitiesPowered.Add(1, 22);
        moneyAsPerCitiesPowered.Add(2, 33);
        moneyAsPerCitiesPowered.Add(3, 44);
        moneyAsPerCitiesPowered.Add(4, 54);
        moneyAsPerCitiesPowered.Add(5, 64);
        moneyAsPerCitiesPowered.Add(6, 73);
        moneyAsPerCitiesPowered.Add(7, 82);
        moneyAsPerCitiesPowered.Add(8, 90);
        moneyAsPerCitiesPowered.Add(9, 98);
        moneyAsPerCitiesPowered.Add(10, 105);
        moneyAsPerCitiesPowered.Add(11, 112);
        moneyAsPerCitiesPowered.Add(12, 118);
        moneyAsPerCitiesPowered.Add(13, 124);
        moneyAsPerCitiesPowered.Add(14, 129);
        moneyAsPerCitiesPowered.Add(15, 134);
        moneyAsPerCitiesPowered.Add(16, 138);
        moneyAsPerCitiesPowered.Add(17, 142);
        moneyAsPerCitiesPowered.Add(18, 145);
        moneyAsPerCitiesPowered.Add(19, 148);
        moneyAsPerCitiesPowered.Add(20, 150);
    }

    void SetupResources()
    {
        for (int i = 0; i < 24; i++)
        {
            GameObject g = Instantiate(coalPrefab, coalSpawnPoints[i], false);
            g.transform.localPosition = Vector3.zero;
            coalPieces.Add(g);
            if (i < 18)
            {
                GameObject g2 = Instantiate(oilPrefab, oilSpawnPoints[i], false);
                g2.transform.localPosition = Vector3.zero;
                oilPieces.Add(g2);
            }

            if (i < 6)
            {
                GameObject g2 = Instantiate(trashPrefab, trashSpawnPoints[i], false);
                g2.transform.localPosition = Vector3.zero;

                trashPieces.Add(g2);
            }

            if (i < 4)
            {
                GameObject g2 = Instantiate(uraniumPrefab, uraniumSpawnPoints[i], false);
                g2.transform.localPosition = Vector3.zero;
                uraniumPieces.Add(g2);
            }
        }
    }


    void SetupPowerplants()
    {
        for (int i = 0; i < powerPlants.Count; i++)
        {
            shuffledPowerplants.Add(i);
        }
        ShufflePowerplants();
    }

    public void ShufflePowerplants()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int j = 0;
            List<int> indicesOfCardToRemove = new List<int>();
            for (int i = shuffledPowerplants.Count - 2; i > 8; i--)
            {

                j = Random.Range(9, shuffledPowerplants.Count - 1);

                int temp = shuffledPowerplants[i];
                shuffledPowerplants[i] = shuffledPowerplants[j];
                shuffledPowerplants[j] = temp;
            }

            int counter = 0;

            while (counter < currentSpecialCase.numberOfRandomlyRemovedPowerplants)
            {
                int randomIndex = Random.Range(9, shuffledPowerplants.Count);

                if (!indicesOfCardToRemove.Contains(randomIndex))
                {
                    indicesOfCardToRemove.Add(randomIndex);
                    counter++;
                }
            }


            gameManagerPhotonView.RPC("RPC_ShuffledDeck", RpcTarget.All, shuffledPowerplants.ToArray(), indicesOfCardToRemove.ToArray());
        }
    }


    [PunRPC]
    public void RPC_ShuffledDeck(int[] powerplants, int[] cardsToRemove)
    {

        for (int i = 0; i < powerplants.Length; i++)
        {
            powerPlants.Add(powerPlants[powerplants[i]]);
        }

        int count = powerplants.Length;
        while (count != 0)
        {
            powerPlants.RemoveAt(0);
            count--;
        }


        for (int i = 7; i >= 0; i--)
        {
            UIManager.instance.AddPowerplantToAuction(powerPlants[i]);
            if (i != 0)
            {
                currentPowerplantTracker++;
            }
        }

        foreach (int i in cardsToRemove)
        {
            powerPlants[i] = null;
        }

        powerPlants.RemoveAll(x => x == null);

        //for (int i = 0; i < powerPlants.Count; i++)
        //{
        //    Debug.LogError("Added Powerplant with cost " + powerPlants[i].cost + " at index " + i);
        //}

    }

    [PunRPC]
    public void RPC_PlayerSetup(int viewID)
    {
        if (!playerIds.Contains(viewID))
        {
            playerIds.Add(viewID);
        }

        if (playerIds.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            playerIds.Sort();
            for (int i = 0; i < playerIds.Count; i++)
            {
                GameObject g = Instantiate(cityTrackerPrefab, cityTrackerHolder, true);
                g.transform.position = spawnPointsForCityTracker[0].position + offsets[i];
                cityTrackerTokens.Add(playerIds[i], g);

                var go = PhotonView.Find(playerIds[i]);
                g.GetComponent<SpriteRenderer>().color = GetColor(go.Owner.GetPlayerNumber());
                go.transform.position = spawnPoints[i].position;
                if (i == 0)
                {
                    go.GetComponent<NetworkPlayer>().IsMyTurn = true;
                }
                go.gameObject.GetComponent<SpriteRenderer>().color = GetColor(go.Owner.GetPlayerNumber());
            }
            currentSpecialCase = specialCases.Find(x => x.playerCount == playerIds.Count);
            foreach (CityHolder c in cityHolders)
            {
                c.BlockUnusedCities(currentSpecialCase.numberOfRegionsOnMap);
            }
            DetermineTurnOrder();

        }
    }


    public void AddNextPowerplantCard()
    {
        currentPowerplantTracker++;

        if (currentPowerplantTracker < GameManager.instance.powerPlants.Count)
        {
            if (GameManager.instance.powerPlants[currentPowerplantTracker].isPhaseThreeCard)
            {
                thirdPhaseTriggered = true;
            }
            else
            {
                UIManager.instance.AddPowerplantToAuction(GameManager.instance.powerPlants[currentPowerplantTracker]);
            }
        }
    }

    public Color GetThisColour()
    {
        var go = ReturnClientPlayer().photonView;

        return GetColor(go.Owner.GetPlayerNumber());
    }

    public static Color GetColor(int colorChoice)
    {
        switch (colorChoice)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.grey;
            case 6: return Color.magenta;
            case 7: return Color.white;
        }

        return Color.black;
    }

    public NetworkPlayer ReturnClientPlayer()
    {
        for (int i = 0; i < playerIds.Count; i++)
        {
            var go = PhotonView.Find(playerIds[i]);
            if (go.IsMine)
            {
                return go.gameObject.GetComponent<NetworkPlayer>();
            }
        }
        return null;
    }

    #region States
    public void DetermineTurnOrder(bool shouldGoToAuctioning = true)
    {

        if (thirdPhaseTriggered)
        {
            Phase3Setup();
        }

        currentPhase = Phase.DetermineTurnOrder;
        UIManager.instance.SetPhaseText(currentPhase);
        //Debug.Log("DetermineTurnOrder");
        if (roundNumber > 1)
        {
            completedPoweringCities.Clear();
            currentPhase = Phase.DetermineTurnOrder;
            List<int> scores = new List<int>();
            for (int i = 0; i < playerIds.Count; i++)
            {
                var go = PhotonView.Find(playerIds[i]);
                scores.Add(go.gameObject.GetComponent<NetworkPlayer>().GetScore());
            }

            for (int i = 0; i < scores.Count - 1; i++)
            {
                for (int j = 0; j < scores.Count - i - 1; j++)
                {
                    if (scores[j] < scores[j + 1])
                    {
                        int temp = scores[j];
                        scores[j] = scores[j + 1];
                        scores[j + 1] = temp;

                        int temp2 = playerIds[j];
                        playerIds[j] = playerIds[j + 1];
                        playerIds[j + 1] = temp2;
                    }
                }
            }

            gameManagerPhotonView.RPC("RPC_ReorderPieces", RpcTarget.All, playerIds.ToArray(), 1);
        }
        else
        {
            if (shouldGoToAuctioning)
            {
                StartAuctioning();
            }
            else
            {
                List<int> scores = new List<int>();
                for (int i = 0; i < playerIds.Count; i++)
                {
                    var go = PhotonView.Find(playerIds[i]);
                    scores.Add(go.gameObject.GetComponent<NetworkPlayer>().GetScore());
                }

                for (int i = 0; i < scores.Count - 1; i++)
                {
                    for (int j = 0; j < scores.Count - i - 1; j++)
                    {
                        if (scores[j] < scores[j + 1])
                        {
                            int temp = scores[j];
                            scores[j] = scores[j + 1];
                            scores[j + 1] = temp;

                            int temp2 = playerIds[j];
                            playerIds[j] = playerIds[j + 1];
                            playerIds[j + 1] = temp2;
                        }
                    }
                }
                gameManagerPhotonView.RPC("RPC_ReorderPieces", RpcTarget.All, playerIds.ToArray(), 0);

            }
        }
    }

    [PunRPC]

    public void RPC_ReorderPieces(int[] viewIds, int shouldGoToAuction)
    {
        playerIds.Clear();
        for (int i = 0; i < viewIds.Length; i++)
        {
            playerIds.Add(viewIds[i]);
        }

        for (int i = 0; i < playerIds.Count; i++)
        {
            var go = PhotonView.Find(playerIds[i]);
            go.transform.position = spawnPoints[i].position;
        }
        if (shouldGoToAuction == 1)
        {
            StartAuctioning();
        }
        else
        {
            StartBuyingResources();
        }
    }

    public void StartAuctioning()
    {
        currentBidders.Clear();
        currentBiddersInSubRound.Clear();
        passCount = 0;
        foreach (int i in playerIds)
        {
            currentBidders.Add(i);
            currentBiddersInSubRound.Add(i);
            UIManager.instance.ToggleBidButton(false);
            UIManager.instance.TogglePassButton(false);
        }
        currentPhase = Phase.Auctioning;
        UIManager.instance.SetPhaseText(currentPhase);
        if (roundNumber == 1)
        {
            SetupPowerplants();
            UIManager.instance.TogglePassBuyingPowerplantButton(false);
        }
        else
        {
            UIManager.instance.TogglePassBuyingPowerplantButton(true);
        }
        UIManager.instance.StartAuctioningPhase1();
        gameManagerPhotonView.RPC("RPC_SettingAuctionButtons", RpcTarget.All);

    }

    [PunRPC]
    public void RPC_SettingAuctionButtons()
    {
        for (int i = 0; i < currentBidders.Count; i++)
        {
            var go = PhotonView.Find(currentBidders[i]);
            if (go.GetComponent<NetworkPlayer>().IsMyTurn && go.IsMine)
            {
                UIManager.instance.TurnTopFourAuctionButtonsOn();
                break;
            }
            else
            {
                UIManager.instance.TurnOffAuctionButton();
                break;
            }
        }
    }

    public void StartAuctioning2(Powerplant thisPowerplant)
    {
        gameManagerPhotonView.RPC("RPC_StartAuctioning2", RpcTarget.All, IndexOfPowerplant(thisPowerplant));
    }

    [PunRPC]
    public void RPC_StartAuctioning2(int ind)
    {

        if (currentBidders.Count > 1)
        {
            UIManager.instance.StartAuctioningPhase2(powerPlants[ind]);
            //currentPowerplantTracker++;
            //UIManager.instance.AddPowerplantToAuction(GameManager.instance.powerPlants[currentPowerplantTracker]);
            AddNextPowerplantCard();
            UIManager.instance.DestroyPowerplant(powerPlants[ind]);
        }
        else
        {
            var go = PhotonView.Find(ReturnPlayerWithCurrentTurn().ViewID);

            go.GetComponent<NetworkPlayer>().powerplantsOwned.Add(powerPlants[ind]);
            go.GetComponent<NetworkPlayer>().CurrentMoney -= powerPlants[ind].cost;
            if (go.IsMine)
            {
                UIManager.instance.AddPowerplant(powerPlants[ind]);
            }
            //currentPowerplantTracker++;
            //UIManager.instance.AddPowerplantToAuction(GameManager.instance.powerPlants[currentPowerplantTracker]);
            AddNextPowerplantCard();
            UIManager.instance.DestroyPowerplant(powerPlants[ind]);
            UIManager.instance.auctioningPanel1.SetActive(false);
            UIManager.instance.auctioningPanel2.SetActive(false);

            go.GetComponent<NetworkPlayer>().IsMyTurn = false;
            if (roundNumber == 1)
            {
                DetermineTurnOrder(false);
            }
            else
            {
                StartBuyingResources();
            }
        }

    }

    public void StartBuyingResources()
    {
        if (thirdPhaseTriggered)
        {
            Phase3Setup();
        }
        currentPlayerTracker = playerIds.Count - 1;
        //Debug.Log("StartBuyingResources");

        currentPhase = Phase.BuyingResources;
        UIManager.instance.ToggleBuyingResourcesPanel(true);
        UIManager.instance.SetPhaseText(currentPhase);
        gameManagerPhotonView.RPC("RPC_SettingBuyingResourcesButtons", RpcTarget.All, playerIds.Count - 1);
    }

    void Phase3Setup()
    {
        thirdPhaseTriggered = false;
        CurrentSection = 3;
        UIManager.instance.SetupPowerplantMarketAccordingToPhase3();
        NotificationManager.instance.Notification("Phase 3 Begins");
        //Do Phase 3 setup
    }

    [PunRPC]
    public void RPC_SettingBuyingResourcesButtons(int current)
    {
        for (int i = 0; i < playerIds.Count; i++)
        {
            if (i == current)
            {
                var go = PhotonView.Find(playerIds[i]);
                go.GetComponent<NetworkPlayer>().IsMyTurn = true;
            }
            else
            {
                var go = PhotonView.Find(playerIds[i]);
                go.GetComponent<NetworkPlayer>().IsMyTurn = false;
            }
        }
        if (ReturnClientPlayer().GetComponent<NetworkPlayer>().IsMyTurn)
        {
            UIManager.instance.ShowBuyingResourcesButtons();
            UIManager.instance.UpdateResourceText(0, currentCoalBuffer, currentCostBuffer);
            UIManager.instance.UpdateResourceText(1, currentOilBuffer, currentCostBuffer);
            UIManager.instance.UpdateResourceText(2, currentTrashBuffer, currentCostBuffer);
            UIManager.instance.UpdateResourceText(3, currentUraniumBuffer, currentCostBuffer);
        }
        else
        {
            UIManager.instance.HideBuyingResourcesButtons();
        }
    }


    public void Resource_UpButtonPress(int resourceID)
    {
        switch (resourceID)
        {
            case 0:
                coalPieces[coalPieces.Count - 1 - currentCoalBuffer].GetComponent<Image>().color = Color.green;
                currentCoalBuffer++;
                currentCostBuffer += CalculateNextCost(Fuel.Coal);
                UIManager.instance.UpdateResourceText(resourceID, currentCoalBuffer, currentCostBuffer);
                break;
            case 1:
                oilPieces[oilPieces.Count - 1 - currentOilBuffer].GetComponent<Image>().color = Color.green;
                currentOilBuffer++;
                currentCostBuffer += CalculateNextCost(Fuel.Oil);
                UIManager.instance.UpdateResourceText(resourceID, currentOilBuffer, currentCostBuffer);
                break;
            case 2:
                trashPieces[trashPieces.Count - 1 - currentTrashBuffer].GetComponent<Image>().color = Color.green;
                currentTrashBuffer++;
                currentCostBuffer += CalculateNextCost(Fuel.Trash);
                UIManager.instance.UpdateResourceText(resourceID, currentTrashBuffer, currentCostBuffer);
                break;
            case 3:
                uraniumPieces[uraniumPieces.Count - 1 - currentUraniumBuffer].GetComponent<Image>().color = Color.green;
                currentUraniumBuffer++;
                currentCostBuffer += CalculateNextCost(Fuel.Nuclear);
                UIManager.instance.UpdateResourceText(resourceID, currentUraniumBuffer, currentCostBuffer);
                break;
        }
        UIManager.instance.ShowBuyingResourcesButtons();
    }


    public bool IsCurrentBufferFullWithResource(Fuel f)
    {
        switch (f)
        {
            case Fuel.Coal:
                return currentCoalBuffer >= coalPieces.Count - 1 ? true : false;

            case Fuel.Oil:
                return currentOilBuffer >= oilPieces.Count - 1 ? true : false;

            case Fuel.Trash:
                return currentTrashBuffer >= trashPieces.Count - 1 ? true : false;

            case Fuel.Nuclear:
                return currentUraniumBuffer >= uraniumPieces.Count - 1 ? true : false;
        }
        return true;
    }

    public int CalculateNextCost(Fuel fuel)
    {
        switch (fuel)
        {
            case Fuel.Coal:
                return 8 - Mathf.CeilToInt((coalPieces.Count - currentCoalBuffer) / 3);
            case Fuel.Oil:
                return 8 - Mathf.CeilToInt((oilPieces.Count - currentOilBuffer) / 3);
            case Fuel.Trash:
                return 8 - Mathf.CeilToInt((trashPieces.Count - currentTrashBuffer) / 3);
            case Fuel.Nuclear:
                if (uraniumPieces.Count - currentUraniumBuffer > 4)
                {
                    return 12 - uraniumPieces.Count + currentUraniumBuffer;
                }
                else
                {
                    return 8 + ((4 - uraniumPieces.Count + currentUraniumBuffer) * 2);
                }
            default:
                return 0;
        }
    }

    [PunRPC]
    public void RPC_ConfirmBuyingResources(int viewID, int coalCount, int oilCount, int trashCount, int uraniumCount)
    {
        NetworkPlayer p = PhotonView.Find(viewID).GetComponent<NetworkPlayer>();


        if (coalCount > 0)
        {
            if (p.ResOwned.Exists(x => x.fuelType == Fuel.Coal))
            {
                p.ResOwned.Find(x => x.fuelType == Fuel.Coal).count += coalCount;
            }
            else
            {
                p.ResOwned.Add(new Resource(Fuel.Coal, coalCount));
            }
        }

        if (oilCount > 0)
        {
            if (p.ResOwned.Exists(x => x.fuelType == Fuel.Oil))
            {
                p.ResOwned.Find(x => x.fuelType == Fuel.Oil).count += oilCount;
            }
            else
            {
                p.ResOwned.Add(new Resource(Fuel.Oil, oilCount));
            }
        }

        if (trashCount > 0)
        {
            if (p.ResOwned.Exists(x => x.fuelType == Fuel.Trash))
            {
                p.ResOwned.Find(x => x.fuelType == Fuel.Trash).count += trashCount;
            }
            else
            {
                p.ResOwned.Add(new Resource(Fuel.Trash, trashCount));
            }
        }

        if (uraniumCount > 0)
        {
            if (p.ResOwned.Exists(x => x.fuelType == Fuel.Nuclear))
            {
                p.ResOwned.Find(x => x.fuelType == Fuel.Nuclear).count += uraniumCount;
            }
            else
            {
                p.ResOwned.Add(new Resource(Fuel.Nuclear, uraniumCount));
            }
        }

        if (ReturnPlayerWithCurrentTurn().IsMine)
        {
            string s = "";
            if (p.ResOwned.Count > 0)
            {
                foreach (Resource r in p.ResOwned)
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

        p.CurrentMoney = p.CurrentMoney - currentCostBuffer;


        for (int i = 0; i < coalCount; i++)
        {
            Destroy(coalPieces[coalPieces.Count - 1]);
            coalPieces.RemoveAt(coalPieces.Count - 1);
        }
        for (int i = 0; i < oilCount; i++)
        {
            Destroy(oilPieces[oilPieces.Count - 1]);
            oilPieces.RemoveAt(oilPieces.Count - 1);
        }
        for (int i = 0; i < trashCount; i++)
        {
            Destroy(trashPieces[trashPieces.Count - 1]);
            trashPieces.RemoveAt(trashPieces.Count - 1);
        }
        for (int i = 0; i < uraniumCount; i++)
        {
            Destroy(uraniumPieces[uraniumPieces.Count - 1]);
            uraniumPieces.RemoveAt(uraniumPieces.Count - 1);
        }

        currentCoalBuffer = 0;
        currentOilBuffer = 0;
        currentTrashBuffer = 0;
        currentUraniumBuffer = 0;
        currentCostBuffer = 0;
        gameManagerPhotonView.RPC("RPC_FindNextBuyer", RpcTarget.All, ReturnPlayerWithCurrentTurn().ViewID);
    }

    [PunRPC]
    public void RPC_FindNextBuyer(int currentViewId)
    {
        int indexOfNextPlayer = 0;

        for (int i = 0; i < playerIds.Count; i++)
        {
            if (playerIds[i] == currentViewId)
            {
                if (i != 0)
                {
                    indexOfNextPlayer = i - 1;
                    gameManagerPhotonView.RPC("RPC_SettingBuyingResourcesButtons", RpcTarget.All, indexOfNextPlayer);
                    break;
                }
                else
                {
                    StartBuilding();
                    break;
                }
            }
        }

    }

    public void ConfirmBuyingResources()
    {
        gameManagerPhotonView.RPC("RPC_ConfirmBuyingResources", RpcTarget.All, ReturnClientPlayer().photonView.ViewID, currentCoalBuffer, currentOilBuffer, currentTrashBuffer, currentUraniumBuffer);

    }

    public void Resource_DownButtonPress(int resourceID)
    {
        switch (resourceID)
        {
            case 0:
                coalPieces[coalPieces.Count - currentCoalBuffer].GetComponent<Image>().color = Color.white;
                currentCoalBuffer--;
                currentCostBuffer -= 8 - Mathf.CeilToInt((coalPieces.Count - currentCoalBuffer - 1) / 3);
                UIManager.instance.UpdateResourceText(resourceID, currentCoalBuffer, currentCostBuffer);
                break;
            case 1:
                oilPieces[oilPieces.Count - currentOilBuffer].GetComponent<Image>().color = Color.white;
                currentOilBuffer--;
                currentCostBuffer -= 8 - Mathf.CeilToInt((oilPieces.Count - currentOilBuffer - 1) / 3);
                UIManager.instance.UpdateResourceText(resourceID, currentOilBuffer, currentCostBuffer);
                break;
            case 2:
                trashPieces[trashPieces.Count - currentTrashBuffer].GetComponent<Image>().color = Color.white;
                currentTrashBuffer--;
                currentCostBuffer -= 8 - Mathf.CeilToInt((trashPieces.Count - currentTrashBuffer - 1) / 3);
                UIManager.instance.UpdateResourceText(resourceID, currentTrashBuffer, currentCostBuffer);
                break;
            case 3:
                uraniumPieces[uraniumPieces.Count - currentUraniumBuffer].GetComponent<Image>().color = Color.white;
                currentUraniumBuffer--;
                if (uraniumPieces.Count - currentUraniumBuffer > 4)
                {
                    currentCostBuffer -= 12 - uraniumPieces.Count + currentUraniumBuffer + 1;
                }
                else
                {
                    currentCostBuffer -= 8 + ((5 - uraniumPieces.Count + currentUraniumBuffer) * 2);
                }
                UIManager.instance.UpdateResourceText(resourceID, currentUraniumBuffer, currentCostBuffer);
                break;
        }
        UIManager.instance.ShowBuyingResourcesButtons();
    }


    public void StartBuilding()
    {
        //Debug.Log("StartBuilding");

        currentPhase = Phase.Building;
        UIManager.instance.SetPhaseText(currentPhase);
        gameManagerPhotonView.RPC("RPC_BuildingTurnDecider", RpcTarget.All, playerIds.Count - 1);
    }


    [PunRPC]
    public void RPC_BuildingTurnDecider(int current)
    {
        for (int i = 0; i < playerIds.Count; i++)
        {
            if (i == current)
            {
                var go = PhotonView.Find(playerIds[i]);
                go.GetComponent<NetworkPlayer>().IsMyTurn = true;
            }
            else
            {
                var go = PhotonView.Find(playerIds[i]);
                go.GetComponent<NetworkPlayer>().IsMyTurn = false;
            }
        }
        if (ReturnClientPlayer().GetComponent<NetworkPlayer>().IsMyTurn)
        {
            Board.instance.canBuild = true;
            UIManager.instance.ToggleDoneBuilding(true);
        }
        else
        {
            Board.instance.canBuild = false;
            UIManager.instance.ToggleDoneBuilding(false);
        }
    }

    public void DoneBuilding()
    {

        for (int i = 0; i < playerIds.Count; i++)
        {
            if (playerIds[i] == ReturnPlayerWithCurrentTurn().ViewID)
            {
                if (i == 0)
                {
                    gameManagerPhotonView.RPC("RPC_DoneBuilding", RpcTarget.All);
                }
                else
                {
                    gameManagerPhotonView.RPC("RPC_BuildingTurnDecider", RpcTarget.All, i - 1);

                }
            }
        }

    }

    [PunRPC]
    public void RPC_DoneBuilding()
    {
        StartEarning();
    }

    public void StartEarning()
    {
        //Debug.Log("StartEarning");
        if (thirdPhaseTriggered)
        {
            Phase3Setup();
        }
        currentPhase = Phase.Earning;
        UIManager.instance.SetPhaseText(currentPhase);
        UIManager.instance.ClearPoweredPowerplants();
        UIManager.instance.EnableMyPowerplants();
        UIManager.instance.ToggleDonePoweringCities(true);
        //take fuel from player
        //give them money according to the number of cities they are powering
        //refuel the resource board according to number of players and phase and number of max resources
        if (!endTriggered)
        {
            roundNumber++;
        }
    }
    #endregion

    public void RefuelResources(int coal, int oil, int trash, int uranium)
    {
        for (int i = 0; i < coal; i++)
        {

            GameObject g = Instantiate(coalPrefab, coalSpawnPoints[coalPieces.Count], false);
            g.transform.localPosition = Vector3.zero;
            coalPieces.Add(g);
        }
        for (int i = 0; i < oil; i++)
        {
            GameObject g = Instantiate(oilPrefab, oilSpawnPoints[oilPieces.Count], false);
            g.transform.localPosition = Vector3.zero;
            oilPieces.Add(g);
        }
        for (int i = 0; i < trash; i++)
        {
            GameObject g = Instantiate(trashPrefab, trashSpawnPoints[trashPieces.Count], false);
            g.transform.localPosition = Vector3.zero;
            trashPieces.Add(g);
        }
        for (int i = 0; i < uranium; i++)
        {
            GameObject g = Instantiate(uraniumPrefab, uraniumSpawnPoints[uraniumPieces.Count], false);
            g.transform.localPosition = Vector3.zero;
            uraniumPieces.Add(g);
        }
    }

    public void DonePoweringCities(int citiesPowered)
    {
        int actualCitiesPowered = 0;

        if (citiesPowered > GameManager.instance.ReturnClientPlayer().CitiesOwned.Count)
        {
            actualCitiesPowered = GameManager.instance.ReturnClientPlayer().CitiesOwned.Count;
        }
        else
        {
            actualCitiesPowered = citiesPowered;
        }

        ReturnClientPlayer().CurrentMoney = ReturnClientPlayer().CurrentMoney + moneyAsPerCitiesPowered[actualCitiesPowered];
        gameManagerPhotonView.RPC("CompletedPoweringCities", RpcTarget.All, ReturnClientPlayer().photonView.ViewID, actualCitiesPowered);
    }

    [PunRPC]
    public void CompletedPoweringCities(int viewID, int citiesPowered)
    {
        if (!completedPoweringCities.ContainsKey(viewID))
        {
            int val = PhotonView.Find(viewID).GetComponent<NetworkPlayer>().CurrentMoney;
            val += citiesPowered * 1000;
            completedPoweringCities.Add(viewID, val);
            if (completedPoweringCities.Count == playerIds.Count)
            {

                if (endTriggered)
                {
                    //The one with max cities powered is the winner, if there's a tie, then the money matters bro! like always :P
                    var sortedDict = completedPoweringCities.OrderBy(x => x.Value);//.ToDictionary(x => x.Key, x => x.Value);
                    int rank = 1;
                    foreach (KeyValuePair<int, int> kvp in sortedDict)
                    {
                        if (PhotonView.Find(kvp.Key).IsMine)
                        {
                            UIManager.instance.ShowRank(rank);
                        }

                        PhotonView.Find(kvp.Key).GetComponent<NetworkPlayer>().Rank = rank;
                        rank++;
                    }
                }
                else
                {

                    int coalToRefuel, oilToRefuel, trashToRefuel, uraniumToRefuel = 0;

                    if (CurrentSection == 1)
                    {
                        coalToRefuel = currentSpecialCase.step1Refuelling[0];
                        oilToRefuel = currentSpecialCase.step1Refuelling[1];
                        trashToRefuel = currentSpecialCase.step1Refuelling[2];
                        uraniumToRefuel = currentSpecialCase.step1Refuelling[3];
                    }
                    else if (CurrentSection == 2)
                    {
                        coalToRefuel = currentSpecialCase.step2Refuelling[0];
                        oilToRefuel = currentSpecialCase.step2Refuelling[1];
                        trashToRefuel = currentSpecialCase.step2Refuelling[2];
                        uraniumToRefuel = currentSpecialCase.step2Refuelling[3];
                    }
                    else
                    {
                        coalToRefuel = currentSpecialCase.step3Refuelling[0];
                        oilToRefuel = currentSpecialCase.step3Refuelling[1];
                        trashToRefuel = currentSpecialCase.step3Refuelling[2];
                        uraniumToRefuel = currentSpecialCase.step3Refuelling[3];
                    }

                    int totalCoalOwnedByPlayers = 0;
                    int totalOilOwnedByPlayers = 0;
                    int totalTrashOwnedByPlayers = 0;
                    int totalUraniumOwnedByPlayers = 0;

                    foreach (int id in playerIds)
                    {
                        var go = PhotonView.Find(id);
                        if (go.GetComponent<NetworkPlayer>().ResOwned.Find(x => x.fuelType == Fuel.Coal) != null)
                        {
                            totalCoalOwnedByPlayers += go.GetComponent<NetworkPlayer>().ResOwned.Find(x => x.fuelType == Fuel.Coal).count;
                        }
                        if (go.GetComponent<NetworkPlayer>().ResOwned.Find(x => x.fuelType == Fuel.Oil) != null)
                        {
                            totalOilOwnedByPlayers += go.GetComponent<NetworkPlayer>().ResOwned.Find(x => x.fuelType == Fuel.Oil).count;
                        }
                        if (go.GetComponent<NetworkPlayer>().ResOwned.Find(x => x.fuelType == Fuel.Trash) != null)
                        {
                            totalTrashOwnedByPlayers += go.GetComponent<NetworkPlayer>().ResOwned.Find(x => x.fuelType == Fuel.Trash).count;
                        }
                        if (go.GetComponent<NetworkPlayer>().ResOwned.Find(x => x.fuelType == Fuel.Nuclear) != null)
                        {
                            totalUraniumOwnedByPlayers += go.GetComponent<NetworkPlayer>().ResOwned.Find(x => x.fuelType == Fuel.Nuclear).count;
                        }
                    }

                    if (coalPieces.Count + totalCoalOwnedByPlayers + coalToRefuel > maxCountOfCoal)
                    {
                        if (coalToRefuel > maxCountOfCoal - coalPieces.Count - totalCoalOwnedByPlayers)
                        {
                            coalToRefuel = maxCountOfCoal - coalPieces.Count - totalCoalOwnedByPlayers;
                        }
                    }

                    if (oilPieces.Count + totalOilOwnedByPlayers + oilToRefuel > maxCountOfOil)
                    {
                        if (oilToRefuel > maxCountOfOil - oilPieces.Count - totalOilOwnedByPlayers)
                        {
                            oilToRefuel = maxCountOfOil - coalPieces.Count - totalOilOwnedByPlayers;
                        }
                    }

                    if (trashPieces.Count + totalTrashOwnedByPlayers + trashToRefuel > maxCountOfTrash)
                    {
                        if (trashToRefuel > maxCountOfTrash - trashPieces.Count - totalTrashOwnedByPlayers)
                        {
                            trashToRefuel = maxCountOfTrash - trashPieces.Count - totalTrashOwnedByPlayers;
                        }
                    }

                    if (uraniumPieces.Count + totalUraniumOwnedByPlayers + uraniumToRefuel > maxCountOfUranium)
                    {
                        if (uraniumToRefuel > maxCountOfUranium - uraniumPieces.Count - totalUraniumOwnedByPlayers)
                        {
                            uraniumToRefuel = maxCountOfUranium - uraniumPieces.Count - totalUraniumOwnedByPlayers;
                        }
                    }
                    // gameManagerPhotonView.RPC("RefuelResources", RpcTarget.All, coalToRefuel, oilToRefuel, trashToRefuel, uraniumToRefuel);
                    RefuelResources(coalToRefuel, oilToRefuel, trashToRefuel, uraniumToRefuel);

                    if (CurrentSection < 3)
                    {
                        UIManager.instance.RemoveHighestCard();
                    }
                    else
                    {
                        UIManager.instance.RemoveLowestCard();
                    }
                    //currentPowerplantTracker++;
                    //UIManager.instance.AddPowerplantToAuction(powerPlants[currentPowerplantTracker]);
                    AddNextPowerplantCard();
                    DetermineTurnOrder();
                }
            }
        }
    }


    int RandomRange(int start, int end, List<int> exclusions)
    {
        while (true)
        {
            int r = Random.Range(start, end);
            if (!exclusions.Contains(r))
            {
                return r;
            }
        }
    }

    public void SpawnHouse(Node n, int cost)
    {
        Vector3 offset = Vector3.zero;
        int indexOfNode = Board.instance.nodes.FindIndex(x => x == n);
        if (currentSection == 1)
        {
            offset = new Vector3(0, 0.3f, 0);
        }
        else if (currentSection == 2)
        {
            offset = new Vector3(-0.3f, -0.1f, 0);

        }
        else if (currentSection == 3)
        {
            offset = new Vector3(0.3f, -0.1f, 0);
        }
        gameManagerPhotonView.RPC("RPC_SpawnHouse", RpcTarget.All, indexOfNode, offset, cost, ReturnClientPlayer().photonView.ViewID);
    }

    [PunRPC]
    public void RPC_SpawnHouse(int indexOfNode, Vector3 offset, int cost, int viewID)
    {
        Node n = Board.instance.nodes[indexOfNode];
        var go = PhotonView.Find(viewID);
        go.GetComponent<NetworkPlayer>().CitiesOwned.Add(n);
        go.GetComponent<NetworkPlayer>().CurrentMoney -= cost;
        GameObject g = Instantiate(playerHouse, n.position + offset, Quaternion.identity);
        g.GetComponent<SpriteRenderer>().color = GetColor(go.Owner.GetPlayerNumber());

        GameObject cityT = cityTrackerTokens[viewID];
        int indexForOffset = -1;
        int counter = 0;
        foreach (KeyValuePair<int, GameObject> kvp in cityTrackerTokens)
        {
            if (kvp.Key == viewID)
            {
                indexForOffset = counter;
                break;
            }
            counter++;
        }

        cityT.transform.position = spawnPointsForCityTracker[go.GetComponent<NetworkPlayer>().CitiesOwned.Count].position + offsets[counter];

        UIManager.instance.RemoveLowPowerplantsThan(go.GetComponent<NetworkPlayer>().CitiesOwned.Count);

        if (currentSection == 1 && go.GetComponent<NetworkPlayer>().CitiesOwned.Count >= currentSpecialCase.citiesToTriggerStepTwo)
        {
            CurrentSection = 2;
            NotificationManager.instance.Notification("Phase 2 Begins");
            UIManager.instance.RemoveLowestCard();
            AddNextPowerplantCard();
        }

        if (!endTriggered && go.GetComponent<NetworkPlayer>().CitiesOwned.Count >= currentSpecialCase.citiesToTriggerGameEnd)
        {
            endTriggered = true;
        }
    }

    public bool CanPlayerBuildThisCity(Node city, int viewID)
    {
        /* if (currentPhase != Phase.Building)
             return false;
 */
        var go = PhotonView.Find(viewID);
        if (go.GetComponent<NetworkPlayer>().CitiesOwned.Contains(city))
        {
            return false;
        }

        int occupierCount = 0;

        foreach (int v in playerIds)
        {
            if (v != viewID)
            {
                var otherGo = PhotonView.Find(v);
                if (otherGo.GetComponent<NetworkPlayer>().CitiesOwned.Contains(city))
                {
                    occupierCount++;
                }
            }
        }

        if (occupierCount < currentSection)
        {
            return true;
        }

        return false;
    }

    int IndexOfPowerplant(Powerplant p)
    {
        return powerPlants.FindIndex(x => x == p);
    }

    public void StartedBidding(Powerplant p)
    {
        gameManagerPhotonView.RPC("RPC_StartedBidding", RpcTarget.All, IndexOfPowerplant(p));
    }

    [PunRPC]
    public void RPC_StartedBidding(int powerplantIndex)
    {

        lastBidder.bid = powerPlants[powerplantIndex].cost;
        lastBidder.viewID = currentBidders[0];
        lastBidder.powerplant = powerPlants[powerplantIndex];
        NextBidder();
    }

    public void NextBidder()
    {
        if (currentBiddersInSubRound.Count > 1)
        {

            for (int i = 0; i < currentBiddersInSubRound.Count; i++)
            {
                if (currentBiddersInSubRound[i] == lastBidder.viewID)
                {
                    var go = PhotonView.Find(currentBiddersInSubRound[i]);
                    go.GetComponent<NetworkPlayer>().IsMyTurn = false;

                    var nextPlayer = PhotonView.Find(currentBiddersInSubRound[(i + 1) % currentBiddersInSubRound.Count]);
                    nextPlayer.GetComponent<NetworkPlayer>().IsMyTurn = true;

                }
                if (ReturnClientPlayer().GetComponent<NetworkPlayer>().IsMyTurn)
                {
                    UIManager.instance.ToggleBidButton(true);
                    UIManager.instance.TogglePassButton(true);
                }
                else
                {
                    UIManager.instance.ToggleBidButton(false);
                    UIManager.instance.TogglePassButton(false);
                }
            }
        }
        else
        {
            var go = PhotonView.Find(lastBidder.viewID);
            go.GetComponent<NetworkPlayer>().IsMyTurn = false;
            go.GetComponent<NetworkPlayer>().powerplantsOwned.Add(lastBidder.powerplant);

            if (go.GetComponent<NetworkPlayer>().powerplantsOwned.Count > currentSpecialCase.maxNumberOfPowerplantsOwned)
            {
                Powerplant lowest = null;
                int currentMinCost = 99;

                foreach (Powerplant p in go.GetComponent<NetworkPlayer>().powerplantsOwned)
                {
                    if (p.cost < currentMinCost)
                    {
                        currentMinCost = p.cost;
                        lowest = p;
                    }
                }

                UIManager.instance.RemoveMyPowerplantCard(lowest);
                go.GetComponent<NetworkPlayer>().powerplantsOwned.Remove(lowest);
            }

            go.GetComponent<NetworkPlayer>().CurrentMoney -= lastBidder.bid;

            if (go.IsMine)
            {
                UIManager.instance.AddPowerplant(lastBidder.powerplant);
            }

            currentBidders.Remove(lastBidder.viewID);
            if (currentBidders.Count > 0)
            {
                currentBiddersInSubRound.Clear();
                foreach (int i in currentBidders)
                {
                    currentBiddersInSubRound.Add(i);
                }
                UIManager.instance.StartAuctioningPhase1();

                for (int i = 0; i < currentBidders.Count; i++)
                {
                    var init = PhotonView.Find(currentBidders[i]);
                    if (i == 0)
                    {
                        init.GetComponent<NetworkPlayer>().IsMyTurn = true;
                    }
                    else
                    {
                        init.GetComponent<NetworkPlayer>().IsMyTurn = false;
                    }
                }
                gameManagerPhotonView.RPC("RPC_SettingAuctionButtons", RpcTarget.All);
            }
            else
            {
                //go to buying resources phase
                UIManager.instance.auctioningPanel1.SetActive(false);
                UIManager.instance.auctioningPanel2.SetActive(false);
                if (roundNumber == 1)
                {
                    DetermineTurnOrder(false);
                }
                else
                {
                    StartBuyingResources();
                }
            }
        }
    }

    public void Bid()
    {
        gameManagerPhotonView.RPC("RPC_Bid", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_Bid()
    {

        for (int i = 0; i < currentBiddersInSubRound.Count; i++)
        {
            var go = PhotonView.Find(currentBiddersInSubRound[i]);
            if (go.GetComponent<NetworkPlayer>().IsMyTurn)
            {
                lastBidder.viewID = go.ViewID;
                lastBidder.bid = lastBidder.bid + 1;
                UIManager.instance.UpdateBidder(go.ViewID, lastBidder.bid);
                NextBidder();
                break;
            }

        }
    }
    public void Pass()
    {
        gameManagerPhotonView.RPC("RPC_Pass", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_Pass()
    {
        currentBiddersInSubRound.Remove(ReturnPlayerWithCurrentTurn().ViewID);
        ReturnPlayerWithCurrentTurn().GetComponent<NetworkPlayer>().IsMyTurn = false;
        NextBidder();
    }

    public void PassBuyingPowerPlant()
    {
        gameManagerPhotonView.RPC("RPC_PassBuyingPowerPlant", RpcTarget.All, ReturnPlayerWithCurrentTurn().ViewID);
    }
    [PunRPC]
    public void RPC_PassBuyingPowerPlant(int viewId)
    {
        for (int i = 0; i < currentBidders.Count; i++)
        {
            if (currentBidders[i] == viewId)
            {
                passCount += 1;


                var oldP = PhotonView.Find(currentBidders[i]);
                oldP.GetComponent<NetworkPlayer>().IsMyTurn = false;

                if (oldP.IsMine)
                {
                    UIManager.instance.TurnOffAuctionButton();
                }
                if (currentBidders.Count > 1)
                {
                    var go = PhotonView.Find(currentBidders[(i + 1) % currentBidders.Count]);
                    go.GetComponent<NetworkPlayer>().IsMyTurn = true;
                }
                else
                {
                    if (passCount == currentBidders.Count)
                    {
                        UIManager.instance.RemoveLowestCard();
                        AddNextPowerplantCard();
                        //go to buying resources phase

                    }
                    UIManager.instance.auctioningPanel1.SetActive(false);
                    UIManager.instance.auctioningPanel2.SetActive(false);
                    StartBuyingResources();
                }
                currentBidders.Remove(currentBidders[i]);
            }
        }

        gameManagerPhotonView.RPC("RPC_SettingAuctionButtons", RpcTarget.All);
    }

    public PhotonView ReturnPlayer(int viewID)
    {
        for (int i = 0; i < playerIds.Count; i++)
        {
            if (playerIds[i] == viewID)
            {
                return PhotonView.Find(playerIds[i]);
            }
        }
        return null;
    }

    public PhotonView ReturnPlayerWithCurrentTurn()
    {
        for (int i = 0; i < playerIds.Count; i++)
        {
            var go = PhotonView.Find(playerIds[i]);
            if (go.GetComponent<NetworkPlayer>().IsMyTurn)
            {
                return go;
            }
        }
        return null;
    }

}

public class Bidder
{
    public int bid;
    public int viewID;
    public Powerplant powerplant;
}

public class SpecialCase
{
    public int playerCount;
    public int numberOfRegionsOnMap;
    public int numberOfRandomlyRemovedPowerplants;
    public int maxNumberOfPowerplantsOwned;
    public int citiesToTriggerStepTwo;
    public int citiesToTriggerGameEnd;

    public List<int> step1Refuelling = new List<int>();
    public List<int> step2Refuelling = new List<int>();
    public List<int> step3Refuelling = new List<int>();

    public SpecialCase()
    {

    }

    public SpecialCase(int p, int regions, int powerplantsRemoved, int maxPowerplants, int stepTwo, int gameEnd, List<int> step1, List<int> step2, List<int> step3)
    {
        playerCount = p;
        numberOfRegionsOnMap = regions;
        numberOfRandomlyRemovedPowerplants = powerplantsRemoved;
        maxNumberOfPowerplantsOwned = maxPowerplants;
        citiesToTriggerStepTwo = stepTwo;
        citiesToTriggerGameEnd = gameEnd;

        step1Refuelling = step1;
        step2Refuelling = step2;
        step3Refuelling = step3;
    }


}