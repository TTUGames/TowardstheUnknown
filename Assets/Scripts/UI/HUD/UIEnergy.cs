using UnityEngine;
using UnityEngine.UI;

public class UIEnergy : MonoBehaviour
{
    [SerializeField] private Sprite filledEnergySprite;
    [SerializeField] private Sprite emptyEnergySprite;
    [SerializeField] private Sprite previewedEnergySprite;

    public GameObject energyCellPrefab;

    public float xOffset;

    private PlayerStats playerStats;
    private Image[] energies;

    private int lastCurrentEnergy = 0;
    private int lastPreviewedEnergy = 0;

    private void Awake()
    {
        playerStats = GameScene.Player.Stats;

        energies = new Image[playerStats.MaxEnergy];
        for (int i = 0; i < energies.Length; i++)
        {
            GameObject energy = Instantiate(energyCellPrefab, transform);
            energy.name = "EnergyCell" + i;

            RectTransform rectTransform = energy.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(i * xOffset, 0);
            rectTransform.anchorMax = new Vector2((i + 1) * xOffset, 1f);

            energies[i] = energy.transform.GetChild(0).GetComponent<Image>();
            energies[i].sprite = filledEnergySprite;
        }
    }

    private void OnEnable()
    {
        playerStats.EnergyChanged += UpdateEnergyUI;
        playerStats.EnergyCostPreviewed += SetPreviewedEnergy;
        UpdateEnergyUI();
    }

    private void OnDisable()
    {
        playerStats.EnergyChanged -= UpdateEnergyUI;
        playerStats.EnergyCostPreviewed -= SetPreviewedEnergy;
    }

    private void UpdateEnergyUI()
    {
        if (lastCurrentEnergy == playerStats.CurrentEnergy)
            return;
        lastCurrentEnergy = playerStats.CurrentEnergy;
        lastPreviewedEnergy = 0;
        for (int i = 0; i < energies.Length; i++)
            energies[i].sprite = i < lastCurrentEnergy ? filledEnergySprite : emptyEnergySprite;
    }

    private void SetPreviewedEnergy(int amount) {
        if (lastPreviewedEnergy == amount)
            return;
        lastPreviewedEnergy = amount;

        int currentEnergy = playerStats.CurrentEnergy;
        for (int i = 0; i < currentEnergy; ++i)
            energies[i].sprite = i < currentEnergy - amount ? filledEnergySprite : previewedEnergySprite;
	}
}
