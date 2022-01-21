using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TowerStates
{
    SeekTarget,
    AttackMinion,
    AttackEnemy
}

public class LolTower : MonoBehaviour
{
    [SerializeField] private Transform towerTop;
    [SerializeField] private GameObject particleObject;

    [ReadOnly] public TowerStates currentState;

    private Minion currentTargetMinion;
    private TeamPlayer currentTargetPlayer;

    private float towerRange => SettingsManager.GameSettings.TowerRange;

    private static Collider[] overlapResults = new Collider[100];

    private void OnDrawGizmos()
    {
        var col = Gizmos.color;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, towerRange);

        if (currentTargetMinion != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(towerTop.position, currentTargetMinion.transform.position);
        }

        if (currentTargetPlayer != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(towerTop.position, currentTargetPlayer.transform.position);
        }

        Gizmos.color = col;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunMachine());
    }

    private IEnumerator GetStateRoutine()
    {
        IEnumerator result = null;
        switch (currentState)
        {
            case TowerStates.SeekTarget:
                result = SeekTarget();
                break;
            case TowerStates.AttackMinion:
                result = AttackMinion();
                break;
            case TowerStates.AttackEnemy:
                result = AttackEnemy();
                break;
            default:
                break;
        }
        return result;
    }

    private IEnumerator RunMachine()
    {
        while (true)
        {
            var stateRoutine = GetStateRoutine();

            yield return stateRoutine;
        }
    }

    private IEnumerator SeekTarget()
    {
        //State Enter

        while (currentState == TowerStates.SeekTarget)
        {
            //State Loop
            var hitCount = Physics.OverlapSphereNonAlloc(transform.position, towerRange, overlapResults, LayerMask.GetMask("Player"));
            var results = overlapResults.Take(hitCount);

            var minions = results.Where(o => o.CompareTag("Minion"))
                .Select(o => o.attachedRigidbody.GetComponent<Minion>());
            
            if (minions.Any(o => !o.IsDead))
            {
                var minion = minions.First(o => !o.IsDead);
                //currentTarget = results.First(o => o.CompareTag("Minion")).transform;
                currentTargetMinion = minion;
                currentState = TowerStates.AttackMinion;
            }
            else if (results.Any(o => o.CompareTag("TeamPlayer")))
            {
                currentTargetPlayer = results.First(o => o.CompareTag("TeamPlayer")).attachedRigidbody.GetComponent<TeamPlayer>();
                currentState = TowerStates.AttackEnemy;
            }

            yield return null;
        }
        //State Exit

    }

    private IEnumerator AttackMinion()
    {
        //State Enter
        float timer = 0f;
        float attackCooldown = 1f;
        bool isAttack = false;
        while (currentState == TowerStates.AttackMinion)
        {
            if (!isAttack)
            {
                Debug.Log("Attack");
                isAttack = true;
                FireEntities(currentTargetMinion.transform);
            }
            timer += Time.deltaTime;
            if (timer >= attackCooldown)
            {
                isAttack = false;
                if (currentTargetMinion.GetHit())
                {

                    currentTargetMinion = null;
                    currentState = TowerStates.SeekTarget;
                    continue;
                }
                timer -= attackCooldown;
            }

            var sqrDistanceToTarget = (currentTargetMinion.transform.position - transform.position).sqrMagnitude;
            if (sqrDistanceToTarget > towerRange * towerRange)
            {
                Debug.Log("sqrdistance");
                
                //currentTargetMinion = null;
                //currentState = TowerStates.SeekTarget;
                //continue;
            }

            //State Loop
            yield return null;
        }
        //State Exit

    }

    private IEnumerator AttackEnemy()
    {
        //State Enter
        float timer = 0f;
        float attackCooldown = 1f;
        bool isAttack = false;
        while (currentState == TowerStates.AttackEnemy)
        {
            if (!isAttack)
            {
                Debug.Log("Attack");
                isAttack = true;
                FireEntities(currentTargetPlayer.transform);
            }
            //State Loop
            timer += Time.deltaTime;

            if (timer >= attackCooldown)
            {
                 isAttack = false;
                if (currentTargetPlayer.GetHit())
                {
                    currentTargetPlayer = null;
                    currentState = TowerStates.SeekTarget;
                    continue;
                }
                timer -= attackCooldown;
            }

            var sqrDistanceToTarget = (currentTargetPlayer.transform.position - transform.position).sqrMagnitude;
            if (sqrDistanceToTarget > towerRange * towerRange)
            {
                currentTargetPlayer = null;
                currentState = TowerStates.SeekTarget;
                continue;
            }

            yield return null;
        }
        //State Exit

    }

    public void Complain(TeamPlayer teamPlayer)
    {
        if (currentState == TowerStates.AttackMinion)
        {
            currentTargetMinion = null;
            currentTargetPlayer = teamPlayer;
            currentState = TowerStates.AttackEnemy;
        }
    }

    private void FireEntities(Transform targetTransform)
    {
        var fireObj = Instantiate(particleObject,transform.position,Quaternion.identity);
        fireObj.GetComponent<FireObject>().FireEntities(targetTransform);
    }
}
