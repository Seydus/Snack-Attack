using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Bread : MonoBehaviour, IActionState
{
    [SerializeField] protected BreadManager breadManager;
    [field: SerializeField] public Transform AttackPosition {get; private set;}
    [field: SerializeField] protected Transform SwanAttackPosition {get; private set;}
    [field: SerializeField] public Animator HealAnimator {get; private set;}
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI manaText;
    [HideInInspector] public float basicAttackDamage;
    [HideInInspector] public float heavyAttackDamage;
    [SerializeField] protected float specialAttack1Damage;
    [SerializeField] protected float specialAttack2Damage;
    [SerializeField] protected float special1Cost;
    [SerializeField] protected float special2Cost;
    bool actionFinished;
    bool checkForFinished;
    public bool Dead {get; private set;}
    public GameObject selectedArrow;
    public Slider breadHealthBarSlider;
    public Slider breadSpecialPowerBarSlider;

    [field: SerializeField] public float breadHealth {get; private set;}
    [field: SerializeField] public float breadMana {get; private set;}
    protected float DamageFromCurrentAttack {get; set;}

    public float maxHealth {get; private set;}
    public float maxMana {get; private set;}

    protected Animator anim;
    protected string animTrigger;
    protected Vector2 startingPosition;

    public enum FightType
    {
        BasicState,
        HeavyState,
        SpecialSkillOne,
        SpecialSkillTwo
    }


    public FightType fightType;

    public enum DamageState
    {
        DamageState,
        DeathState,
    }

    public DamageState damageState;

    void Awake()
    {
        startingPosition = transform.position;
        maxHealth = breadHealth;
        maxMana = breadMana;
        anim = GetComponent<Animator>();

        UpdateHealthUI(breadHealth);
        UpdateManaUI(breadMana);
    }

    public void Heal(float amount)
    {
        breadHealth += amount;

        if(breadHealth >= maxHealth)
            breadHealth = maxHealth;

        OnHeal();

        UpdateHealthUI(breadHealth);
    }

    public abstract void OnHeal();

    public void RestoreMana(float amount)
    {
        breadMana += amount;

        if(breadMana >= maxMana)
            breadMana = maxMana;

        UpdateManaUI(breadMana);
    }

    public void UseMana(float amount)
    {
        breadMana -= amount;
        UpdateHealthUI(breadMana);

        UpdateManaUI(breadMana);
    }

    public void Defend()
    {
        
    }

    void IActionState.Fight()
    {

    }

    public IEnumerator Fight()
    {
        if (Random.Range(0, 2) == 0)
        {
            Debug.Log(name + " used basic attack");
            UIManager.Instance.panelCurrentTurnObj.SetActive(true);
            UIManager.Instance.currentTextCurrentTurn.text = name + " used basic attack";

            yield return new WaitForSeconds(2f);
            UIManager.Instance.panelCurrentTurnObj.SetActive(false);
            UIManager.Instance.currentTextCurrentTurn.text = "";

            fightType = FightType.BasicState;
            animTrigger = "basicAttack";
        }
        else
        {
            Debug.Log(name + " used heavy attack");
            UIManager.Instance.panelCurrentTurnObj.SetActive(true);
            UIManager.Instance.currentTextCurrentTurn.text = name + " used heavy attack";

            yield return new WaitForSeconds(2f);
            UIManager.Instance.panelCurrentTurnObj.SetActive(false);
            UIManager.Instance.currentTextCurrentTurn.text = "";

            fightType = FightType.HeavyState;

            animTrigger = "heavyAttack";
        }
    }

    public abstract void DoAction();

    public abstract void OnAttack(FightType fightType);

    protected void Attack()
    {
        if (GameObject.FindWithTag("Swan").TryGetComponent(out SwanState swanState))
        {
            var swan = GameObject.FindWithTag("Swan").GetComponent<Swan>();
            swan.fightType = Swan.FightType.DefendState;
            swanState.FightState(Swan.FightType.DefendState, swan);
        }
        else
        {
            Debug.LogError("Could not find swan!");
        }
    }

    public abstract IEnumerator SpecialPower();

    public void UseItem()
    {
        
    }

    public void StartTurn()
    {
        actionFinished = false;
        StartCoroutine(nameof(TakeTurn));
    }

    public void EndTurn()
    {
        actionFinished = true;
    }

    public void ReduceBreadHealth(float damage)
    {
        breadHealth -= damage;
        breadHealthBarSlider.value = breadHealth;
        UpdateHealthUI(breadHealth);
        Debug.Log("hit");
        anim.SetTrigger("hit");

        OnDamage(DamageState.DamageState);

        if (breadHealth <= 0)
        {
            OnDamage(DamageState.DeathState);
            Dead = true;
        }
    }


    public abstract void OnDamage(DamageState damageState);

    public void UpdateHealthUI(float value)
    {
        if(value <= 0)
        {
            healthText.text = "000";
            healthText.ForceMeshUpdate();
            return;
        }

        string s = "";

        if(value < 100)
            s = "0";
        else if(value < 10)
            s = "00";

        healthText.text = s + value.ToString("0");
        healthText.ForceMeshUpdate();
    }

    public void UpdateManaUI(float value)
    {
        if(value <= 0)
        {
            manaText.text = "000";
            manaText.ForceMeshUpdate();
            return;
        }

        string s = "";

        if(value < 100)
            s = "0";
        else if(value < 10)
            s = "00";

        manaText.text = s + value.ToString("0");
        manaText.ForceMeshUpdate();
    }

    IEnumerator TakeTurn()
    {
        actionFinished = false;
        selectedArrow.SetActive(false);
        yield return new WaitForSeconds(breadManager.TakeActionDelay);
        breadManager.SetTransparency(breadManager.unfocusedTransparency);
        Debug.Log(name + " taking action");

        UIManager.Instance.panelCurrentTurnObj.SetActive(true);
        UIManager.Instance.currentTextCurrentTurn.text = name + " is taking action";

        yield return new WaitForSeconds(2f);

        UIManager.Instance.panelCurrentTurnObj.SetActive(false);
        UIManager.Instance.currentTextCurrentTurn.text = "";

        ChooseAction();

        yield return new WaitForSeconds(2f);

        Attack();

        yield return new WaitUntil(() => actionFinished);

        DoAction();
        anim.SetTrigger(animTrigger);

        yield return new WaitForSeconds(2f);

        Debug.Log(name + " end turn");
        breadManager.SetTransparency(1);

        breadManager.EndTurn();
    }

    void ChooseAction()
    {
        switch(Random.Range(0, 2))
        {
            case 0:
                StartCoroutine(Fight());
                break;
            case 1: StartCoroutine(SpecialPower());
                break;
            // case 2: UseItem();
            //     break;
            // case 3: Defend();
            //     break;
            
            default: break;
        }
    }

    public void MoveToSwan()
    {
        StartCoroutine(nameof(MoveToSwanCo));
    }

    IEnumerator MoveToSwanCo()
    {
        float timer = 0;

        float duration = 0.25f;

        while(timer < duration)
        {
            transform.position = Vector3.Lerp(startingPosition, SwanAttackPosition.position, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void MoveToStartingPosition()
    {
        StartCoroutine(nameof(MoveToStartingPositionCo));
    }

    IEnumerator MoveToStartingPositionCo()
    {
        float timer = 0;

        float duration = 0.25f;
        while(timer < duration)
        {
            transform.position = Vector3.Lerp(SwanAttackPosition.position, startingPosition, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    void IActionState.SpecialPower()
    {
        throw new System.NotImplementedException();
    }
}
