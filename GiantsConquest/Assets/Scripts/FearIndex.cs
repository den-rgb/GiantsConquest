using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class FearIndex : MonoBehaviour
{
    int count;
    private checkDestroyed destroyedScript;
    public SingleTerrainGen terrainGen;
    public Dictionary<GameObject, List<GameObject>> village = new Dictionary<GameObject, List<GameObject>>();
    public Dictionary<GameObject, int> villageCounters = new Dictionary<GameObject, int>();
    private HashSet<GameObject> destroyedHouses = new HashSet<GameObject>(); // Keep track of destroyed houses
    private HashSet<GameObject> incrementedVillages = new HashSet<GameObject>(); // Keep track of incremented villages

    public Canvas canvas; // Assign your WorldSpace canvas in the Inspector
    public TMP_FontAsset textFont; // Assign the font you want to use in the Inspector
    public Dictionary<GameObject, TextMeshProUGUI> villageTexts = new Dictionary<GameObject, TextMeshProUGUI>();

    private Camera cam;

    

    [System.Serializable]
    public class VillageFearIndexChangedEvent : UnityEvent<GameObject> { }

    public VillageFearIndexChangedEvent onVillageFearIndexChanged;



    void Start()
    {
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(count==0){
            village = terrainGen.villageDictionary;
            InitializeVillageTexts();
            cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            canvas.worldCamera = cam;
            count++;
        }

        CheckDestroyedHouses();
        PrintVillageCounters();
        
    }

    private void InitializeVillageTexts()
    {
        foreach (var villageCenter in village.Keys)
        {
            GameObject textObject = new GameObject("VillageText");
            textObject.transform.SetParent(canvas.transform, false);
            textObject.transform.position = villageCenter.transform.position + new Vector3(0, 30, 0); // Set the text position above the village center

            TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>(); // Replace this line
            textComponent.font = textFont;
            textComponent.fontSize = 20;
            textComponent.color = Color.black;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.overflowMode = TextOverflowModes.Overflow;
            // Add an outline to the text
            textComponent.fontMaterial.EnableKeyword("UNDERLAY_ON");
            textComponent.fontMaterial.SetColor("_UnderlayColor", Color.black); // Set the outline color to black

            // Modify the outline thickness (change the x and y values to adjust the thickness)
            textComponent.fontMaterial.SetFloat("_FaceDilate", 0.2f);


            villageTexts[villageCenter] = textComponent;
        }
    }


    public void CheckDestroyedHouses()
    {
        foreach (var villageCenter in village.Keys)
        {
            bool villageIntact = IsVillageIntact(villageCenter);

            if (villageIntact && incrementedVillages.Contains(villageCenter))
            {
                incrementedVillages.Remove(villageCenter);
            }

            foreach (var house in village[villageCenter])
            {
                destroyedScript = house.GetComponent<checkDestroyed>();
                if (destroyedScript != null && destroyedScript.destroyed && !destroyedHouses.Contains(house)) // Check if the house is not already destroyed
                {
                    // Add all the houses in the village to the destroyed houses set
                    foreach (var otherHouse in village[villageCenter])
                    {
                        destroyedHouses.Add(otherHouse);
                    }

                    // Increment the counter for the village the destroyed house belongs to
                    IncrementCounter(villageCenter, 3);

                    // Find the two closest village centers and increment their counters (only once)
                    List<GameObject> twoClosestVillages = FindTwoClosestVillages(house);
                    foreach (var closestVillage in twoClosestVillages)
                    {
                        IncrementCounter(closestVillage, int.MaxValue, 0); // Initialize the closest village counter with an initial value of 0

                        if (!incrementedVillages.Contains(closestVillage) && villageCounters[closestVillage] < 1)
                        {
                            IncrementCounter(closestVillage, 1);
                            incrementedVillages.Add(closestVillage);
                        }
                    }


                    //Debug.Log("House " + house.name + " in village " + villageCenter.name + " is destroyed.");
                }
            }
        }
    }

    private void IncrementCounter(GameObject villageCenter, int maxIncrement, int initialValue = 1)
    {
        // Check if the village is the capital
        if (villageCenter == FindObjectOfType<NegotiationIndex>().capital)
        {
            return; // Skip the capital
        }

        if (villageCounters.ContainsKey(villageCenter))
        {
            if (villageCounters[villageCenter] < maxIncrement)
            {
                villageCounters[villageCenter]++;
            }
        }
        else
        {
            villageCounters[villageCenter] = initialValue;
        }
        onVillageFearIndexChanged?.Invoke(villageCenter);
    }



    private bool IsVillageIntact(GameObject villageCenter)
    {
        foreach (var house in village[villageCenter])
        {
            destroyedScript = house.GetComponent<checkDestroyed>();
            if (destroyedScript != null && destroyedScript.destroyed)
            {
                return false;
            }
        }

        return true;
    }

    private List<GameObject> FindTwoClosestVillages(GameObject house)
    {
        List<KeyValuePair<GameObject, float>> villageDistances = new List<KeyValuePair<GameObject, float>>();

        foreach (var villageCenter in village.Keys)
        {
            float distance = Vector3.Distance(house.transform.position, villageCenter.transform.position);
            villageDistances.Add(new KeyValuePair<GameObject, float>(villageCenter, distance));
        }
        villageDistances.Sort((x, y) => x.Value.CompareTo(y.Value));

        List<GameObject> twoClosestVillages = new List<GameObject>();
        for (int i = 0; i < 2 && i < villageDistances.Count; i++)
        {
            twoClosestVillages.Add(villageDistances[i].Key);
        }

        return twoClosestVillages;
    }

    public void PrintVillageCounters()
    {
        //print(villageCounters.Keys.Count + " village counters ");
        foreach (var villageCenter in villageCounters.Keys)
        {
            Debug.Log("Village: " + villageCenter.name + " | Counter: " + villageCounters[villageCenter]);
            villageTexts[villageCenter].text = "" + villageCounters[villageCenter];
        }
    }
}