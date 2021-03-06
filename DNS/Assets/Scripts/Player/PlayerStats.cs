using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Active Player Stats")]
    [SerializeField] private float stamina, thirst, hunger, health;
    
    [Header("Default Player Stats")]
    [SerializeField] private float defaultHealth;
    [SerializeField] private float defaultStamina, defaultThirst, defaultHunger;

    [Header("Default Drain Stats")]
    [SerializeField] private float defaultHungerDrain, defaultHealthDrain, defaultThirstDrain, defaultHealthRegen;
    
    [Header("Player Components")] 
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator playerAnimator;

    [SerializeField] private Slider healthBar, staminaBar, hungerBar, thirstBar;
    [SerializeField] private Canvas endScreen, playerUI;

    private bool drainHunger;
    private bool drainThirst;
    private bool isDead;


//Awake
    private void Awake()
    {
        playerController = this.gameObject.GetComponent<PlayerController>();
        playerAnimator = this.gameObject.GetComponent<Animator>();
    }

// Set Stats
    private void Start()
    {
        health = defaultHealth;
        stamina = defaultStamina;
        thirst = defaultThirst;
        hunger = defaultHunger;
        drainThirst = true;
        drainHunger = true;
        isDead = false;
    }


// Gain Stats
    public float GainHealth(float gainValue)
    {
        if (health < defaultHealth)
        {
            return health += gainValue;
        }
        else
        {
            return health;
        }
    }

    public float GainStamina(float gainValue)
    {
        if (stamina < defaultStamina)
        {
            return stamina += gainValue;
        }
        else
        {
            return stamina;
        }
    }

    public float GainThirst(float gainValue)
    {
        if (thirst < defaultThirst)
        {
            return thirst += gainValue;
        }
        else
        {
            return thirst;
        }
    }

    public float GainHunger(float gainValue)
    {
        if (hunger < defaultHunger)
        {
            return hunger += gainValue;
        }
        else
        {
            return hunger;
        }
    }
    
// Drain Stats
    public float DrainHealth(float drainValue)
    {
        return health -= drainValue;
    }
    public float DrainStamina(float drainValue)
    {
       return stamina -= drainValue;
    }

    public float DrainThirst(float drainValue)
    {
        return thirst -= drainValue;
    }

    public float DrainHunger(float drainValue)
    {
        return hunger -= drainValue;
    }

    private void Update()
    {
//Kill player when health is <= than 0
        if (health <= 0 && !isDead)
        {
            Die();
            isDead = true;
        }
    }

    private void FixedUpdate()
    {
        
        // Health Regen
        if (hunger > 50)
        {
            GainHealth(defaultHealthRegen);
        }

        // Hunger Drain
        if(drainHunger)
        {
            DrainHunger(defaultHungerDrain);
        } 
        
        
 // Stop hunger from going below 0
        if (hunger > 0)
        {
            drainHunger = true;
        }
        else if (hunger <= 0)
        {
            drainHunger = false;
            hunger = 0;
        }
        
        
// Health Drain if Starving
        if (hunger <= 0)
        {
            DrainHealth(defaultHealthDrain);
        }

        
// Stamina 
        if (!playerAnimator.GetBool("isRunning") && hunger>0)
        {
            GainStamina(1f);
        }
        else if (stamina > 0 && playerAnimator.GetBool("isRunning"))
        {
            DrainStamina(0.2f);
        }
        
        
// Stop player from running and attacking
        if (stamina <= 0)
        {
            playerController.SetCanUseStamina(false);
        }
        else
        {
            playerController.SetCanUseStamina(true);   
        }

        if (stamina > defaultStamina)
        {
            stamina = defaultStamina;
        }
// Drain Thirst
        if (thirst > 0 && drainThirst)
        {
            DrainThirst(defaultThirstDrain);
        }
        else
        {
            drainThirst = false;
            thirst = 0;
        }

        //UI Values

        healthBar.value = health;
        staminaBar.value = stamina;

        hungerBar.value = hunger;
        thirstBar.value = thirst;
        
        
    }

    private void Die()
    {
        playerAnimator.SetTrigger("Die");
    }

    public void GoToGameOver()
    {
        playerUI.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
    }
}
