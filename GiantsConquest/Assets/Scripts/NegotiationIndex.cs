using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NegotiationIndex : MonoBehaviour
{
    public FearIndex fearIndex;
    public GameObject capital;
    private GameObject player;
    public GameObject dialogueUI;
    public TextMeshProUGUI negotiationIndexText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public Dictionary<GameObject, float> negotiationDifficulty = new Dictionary<GameObject, float>();
    public Dictionary<GameObject, float> productionRate = new Dictionary<GameObject, float>();
    public Dictionary<GameObject, ResourceRequirements> villageResourceRequirements = new Dictionary<GameObject, ResourceRequirements>();

    public List<GameObject> tradedVillages = new List<GameObject>();
    private HashSet<GameObject> producingVillages = new HashSet<GameObject>();

    private bool gameWon = false;



    float maxDistance = 0;
    int count = 0;

    public class ResourceRequirements
    {
        public int woodRequired;
        public int stoneRequired;

        public ResourceRequirements(int woodRequired, int stoneRequired)
        {
            this.woodRequired = woodRequired;
            this.stoneRequired = stoneRequired;
        }
    }

    void Update()
    {
        if(count==0){
            StartCoroutine(InitializeNegotiationIndex());
            player = GameObject.FindWithTag("Player");
            count++;
        }
    }

    IEnumerator InitializeNegotiationIndex()
    {
        while (fearIndex.village.Count == 0)
        {
            yield return new WaitForSeconds(1f);
        }
        
        ChooseCapital();
        maxDistance = CalculateMaxDistance();
        UpdateNegotiationDifficulty();
        fearIndex.onVillageFearIndexChanged.AddListener(OnVillageFearIndexChanged);
    }

    void ChooseCapital()
    {
        //print("fearIndex.village.Keys.Count: " + fearIndex.village.Keys.Count);
        List<GameObject> villages = new List<GameObject>(fearIndex.village.Keys);
        capital = villages[0];

        // Update the text for the capital village
        if (fearIndex.villageTexts.ContainsKey(capital))
        {
            fearIndex.villageTexts[capital].text = "Capital";
        }
    }


    void UpdateNegotiationDifficulty()
    {
        foreach (var villageCenter in fearIndex.village.Keys)
        {
            UpdateNegotiationDifficultyForVillage(villageCenter);
        }
    }

    void UpdateNegotiationDifficultyForVillage(GameObject villageCenter)
    {
        if (villageCenter == capital)
        {
            return; // Skip the capital
        }
        float distanceToCapital = Vector3.Distance(villageCenter.transform.position, capital.transform.position);
        float normalizedDistance = Mathf.InverseLerp(0, maxDistance, distanceToCapital); // Normalize the distance between 0 and maxDistance
        float baseDifficulty = Mathf.Round(Mathf.Lerp(0.0f, 1.0f, normalizedDistance) * 100f); 
        int fearCounter = fearIndex.villageCounters.ContainsKey(villageCenter) ? fearIndex.villageCounters[villageCenter] : 0;
        float fearFactor = 1.0f - (0.3f * fearCounter);

        float finalDifficulty = baseDifficulty * fearFactor;
        negotiationDifficulty[villageCenter] = finalDifficulty;

        int totalResourcesRequired = 100 - (int)finalDifficulty;

        float woodRatio = Random.Range(0.0f, 1.0f); // Generate a random ratio between 0 and 1 for wood
        int woodRequired = Mathf.RoundToInt(totalResourcesRequired * woodRatio); // Calculate the amount of wood required based on the wood ratio

        int stoneRequired = totalResourcesRequired - woodRequired; // Calculate the amount of stone required based on the remaining resources

        villageResourceRequirements[villageCenter] = new ResourceRequirements(woodRequired, stoneRequired);

        float baseProductionRate = 1.0f;
        float reducedProductionRate = baseProductionRate * fearFactor;
        reducedProductionRate = Mathf.RoundToInt(reducedProductionRate*100f);

        // Set the production rate to 0 if the fear index is 3 or more
        if (fearCounter >= 3)
        {
            reducedProductionRate = 0f;
        }

        productionRate[villageCenter] = reducedProductionRate;
        
    }


    float CalculateMaxDistance()
    {
        float maxDist = 0.0f;
        foreach (var villageCenter in fearIndex.village.Keys)
        {
            if (villageCenter == capital)
            {
                continue; // Skip the capital
            }

            float distanceToCapital = Vector3.Distance(villageCenter.transform.position, capital.transform.position);

            if (distanceToCapital > maxDist)
            {
                maxDist = distanceToCapital;
            }
        }

        return maxDist;
    }



    public void OnVillageFearIndexChanged(GameObject villageCenter)
    {
        UpdateNegotiationDifficultyForVillage(villageCenter);
    }

    public GameObject GetVillageInRange(float range)
    {
        
        foreach (var villageCenter in fearIndex.village.Keys)
        {
            float distance = Vector3.Distance(player.transform.position, villageCenter.transform.position);
            if (distance <= range)
            {
                return villageCenter;
            }
        }

        return null;
    }

    public void CheckAndConsumeResources()
    {
        GameObject villageCenter = GetVillageInRange(100f);
        if (villageResourceRequirements.ContainsKey(villageCenter))
        {
            int woodRequired = villageResourceRequirements[villageCenter].woodRequired;
            int stoneRequired = villageResourceRequirements[villageCenter].stoneRequired;

            int currentWood = PlayerPrefs.GetInt("wood");
            int currentStone = PlayerPrefs.GetInt("stone");
            print("buttonClicked " + currentStone + " " + currentWood);
            if (currentWood >= woodRequired && currentStone >= stoneRequired)
            {
                CheckGameWon(villageCenter);
                print("trade complete");
                PlayerPrefs.SetInt("wood", currentWood - woodRequired);
                PlayerPrefs.SetInt("stone", currentStone - stoneRequired);
                print("resources: " +PlayerPrefs.GetInt("wood") +" " + PlayerPrefs.GetInt("stone"));
                dialogueUI.SetActive(false);
                PlayerPrefs.Save();
                tradedVillages.Add(villageCenter);
                
                
                foreach (var village in tradedVillages)
                {
                    // Check if the village is not already producing resources
                    if (!producingVillages.Contains(village))
                    {
                        producingVillages.Add(village);
                        StartCoroutine(ProduceResources(village));
                    }
                }
                
            }
        }
    }

    private void CheckGameWon(GameObject villageCenter)
    {
        if (villageCenter == capital)
        {
            gameWon = true;
            print("game won");
            SceneManager.LoadScene("WinScene");
        }
    }

    IEnumerator ProduceResources(GameObject villageCenter)
    {
        if (productionRate.ContainsKey(villageCenter))
        {
            float rate = productionRate[villageCenter];
            print(rate + ": rate");
            float timer = rate;
            if(timer!=100){
                float additional = rate - 100f;
                timer = 100f + additional;
                timer/=2f;
            }else timer/=2f;
            print(timer + ": timer");
            while (rate > 0)
            {
                yield return new WaitForSeconds(timer);
                print("got resources");
                int currentWood = PlayerPrefs.GetInt("wood");
                int currentStone = PlayerPrefs.GetInt("stone");
                print(currentStone + " " + currentWood + " : current");
                PlayerPrefs.SetInt("wood", currentWood + 1);
                PlayerPrefs.SetInt("stone", currentStone + 1);
                PlayerPrefs.Save();
            }
        }
    }

}
