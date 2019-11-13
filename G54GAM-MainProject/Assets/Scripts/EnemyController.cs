using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IState
{
    string stateName { get;}
    void Enter();
    void Execute();
    void Exit();
}


public class StateMachine
{
    IState currentState;

    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.Execute();
        }
    }

    public string getStateName()
    {
        return currentState.stateName;
    }
}

public class State_Boss_Broken : IState
{
    public string stateName { get;} = "Broken";
    EnemyController owner;
    NavMeshAgent agent;
    Waypoint waypoint;

    float timeBroken = 0f;
    float fixTime = 10f;

    public State_Boss_Broken(EnemyController owner) { this.owner = owner; }
    public void Enter()
    {
        agent = owner.GetComponent<NavMeshAgent>();
        agent.destination = owner.transform.position;

        agent.isStopped = true;
        agent.speed = owner.walkSpeed;

        timeBroken = Time.time;
        

        owner.resetWeapons(-owner.transform.up);
    }
    public void Execute()
    {
        if(Time.time > fixTime + timeBroken)
        {
            owner.stateMachine.ChangeState(new State_Boss_Attack(owner));
        }
    }
    public void Exit()
    {
        owner.damageAccumulated = 0;
        // stop moving
        agent.isStopped = true;
    }

}

public class State_Boss_Patrol : IState
{
    public string stateName { get; } = "Patrolling";
    EnemyController owner;
    NavMeshAgent agent;
    Waypoint waypoint;
    public State_Boss_Patrol(EnemyController owner) { this.owner = owner; }
    public void Enter()
    {
        waypoint = owner.closestWaypoint();
        agent = owner.GetComponent<NavMeshAgent>();
        agent.destination = waypoint.transform.position;

        agent.isStopped = false;
        agent.speed = owner.walkSpeed;

        owner.resetWeapons(-owner.transform.forward);
    }
    public void Execute()
    {
        // same as before
        if (!agent.pathPending && agent.remainingDistance < owner.targetDistance)
        {
            Waypoint nextWaypoint = waypoint.nextWaypoint;
            waypoint = nextWaypoint;
            agent.destination = waypoint.transform.position;

        }

        if (owner.seenTarget)
        {
            owner.stateMachine.ChangeState(new State_Boss_Attack(owner));
        }

        //Attempt to relocate target
        if (owner.refindTarget)
        {
            owner.stateMachine.ChangeState(new State_Boss_Searching(owner));
        }
    }
    public void Exit()
    {
        // stop moving
        agent.isStopped = true;
    }
}


public class State_Boss_Attack : IState
{
    public string stateName { get; set; } = "Attacking";
    EnemyController owner;
    NavMeshAgent agent;

    

    public State_Boss_Attack(EnemyController owner) { this.owner = owner; }
    public void Enter()
    {
        agent = owner.GetComponent<NavMeshAgent>();
        agent.speed = owner.runSpeed;
        if (owner.seenTarget)
        {
            agent.destination = owner.lastSeenPosition;
            agent.isStopped = false;

        }
        owner.refindTarget = false;
    }
    public void Execute()
    {
        agent.destination = owner.lastSeenPosition;
        agent.isStopped = false;
        if (!agent.pathPending && (agent.remainingDistance < 1.0f) || (!agent.pathPending && agent.remainingDistance <= owner.weaponRange / 2f))
        {
            agent.isStopped = true;
            owner.faceTarget(owner.lastSeenPosition);
        }

        if (owner.seenTarget != true)
        {
            // search for the player
            owner.stateMachine.ChangeState(new State_Boss_Searching(owner));
        }
        else
        {

            owner.aimWeapons(owner.lastSeenPosition);

            //Stop the charging when charged & in range
            if (owner.chargeComplete && Vector3.Distance(owner.target.transform.position, owner.transform.position) <= owner.weaponRange)
            {
                owner.burstStop = Time.time + (owner.burstChargeUpTime / 4f);
                owner.chargeComplete = false;
                owner.charging = false;
            }

            if (owner.charging)
            {
                stateName = "Charging Weapons";
            }

            if (Time.time < owner.burstStop && !owner.charging)
            {
                stateName = "Attacking";
                //Aim after potentially facing target
                
                //
                //Firing
                if (Time.time > owner.nextFire && Vector3.Distance(owner.target.transform.position, owner.transform.position) <= owner.weaponRange)
                {
                    owner.nextFire = Time.time + (60f / owner.fireRate);
                    owner.fire();
                }
            }
            
            
            
        }

        if(owner.damageAccumulated > owner.healthMax / 6f)
        {
            owner.stateMachine.ChangeState(new State_Boss_Broken(owner));
        }
    }

    public void Exit()
    {
        agent.isStopped = true;
    }
}

public class State_Boss_Searching : IState
{
    public string stateName { get; } = "Searching";
    EnemyController owner;
    NavMeshAgent agent;
    public State_Boss_Searching(EnemyController owner) { this.owner = owner; }
    public void Enter()
    {
        agent = owner.GetComponent<NavMeshAgent>();
        agent.destination = owner.lastSeenPosition;
        agent.speed = owner.runSpeed;
        agent.isStopped = false;



        owner.resetWeapons(-owner.transform.forward);
    }
    public void Execute()
    {
        agent.destination = owner.lastSeenPosition;
        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            //Reset refind target
            owner.refindTarget = false;
            owner.stateMachine.ChangeState(new State_Boss_Patrol(owner));
        }

        if (owner.seenTarget == true)
        {
            //Reset refind target
            owner.refindTarget = false;
            owner.stateMachine.ChangeState(new State_Boss_Attack(owner));

        }
    }

    public void Exit()
    {
        agent.isStopped = true;
    }
}

public class State_Patrol : IState
{
    public string stateName { get; } = "Patroling";
    EnemyController owner;
    NavMeshAgent agent;
    Waypoint waypoint;
    public State_Patrol(EnemyController owner) { this.owner = owner; }
    public void Enter()
    {
        waypoint = owner.closestWaypoint();
        agent = owner.GetComponent<NavMeshAgent>();
        agent.destination = waypoint.transform.position;

        agent.isStopped = false;
        agent.speed = owner.walkSpeed;

        owner.resetWeapons(-owner.transform.forward);
    }
    public void Execute()
    {
        // same as before
        if (!agent.pathPending && agent.remainingDistance < owner.targetDistance)
        {
            Waypoint nextWaypoint = waypoint.nextWaypoint;
            waypoint = nextWaypoint;
            agent.destination = waypoint.transform.position;
            
        }

        if (owner.seenTarget)
        {
            owner.stateMachine.ChangeState(new State_Attack(owner));
        }

        //Attempt to relocate target
        if (owner.refindTarget)
        {
            owner.stateMachine.ChangeState(new State_Searching(owner));
        }
    }
    public void Exit()
    {
        // stop moving
        agent.isStopped = true;
    }
}


public class State_Attack : IState
{
    public string stateName { get; } = "Attacking";
    EnemyController owner;
    NavMeshAgent agent;

    public State_Attack(EnemyController owner) { this.owner = owner; }
    public void Enter()
    {
        agent = owner.GetComponent<NavMeshAgent>();
        agent.speed = owner.runSpeed;
        if (owner.seenTarget)
        {
            agent.destination = owner.lastSeenPosition;
            agent.isStopped = false;

        }
        owner.refindTarget = false;
    }
    public void Execute()
    {
        agent.destination = owner.lastSeenPosition;
        agent.isStopped = false;
        if (!agent.pathPending && (agent.remainingDistance < 1.0f) || (!agent.pathPending && agent.remainingDistance <= owner.weaponRange / 2f && owner.lastShotHit))
        {
            agent.isStopped = true;
            owner.faceTarget(owner.lastSeenPosition);
        }

        if (owner.seenTarget != true)
        {
            // search for the player
            owner.stateMachine.ChangeState(new State_Searching(owner));
        }
        else
        {
            
            //Aim after potentially facing target
            owner.aimWeapons(owner.lastSeenPosition);
            //
            //Firing
            if (Time.time > owner.nextFire && Vector3.Distance(owner.target.transform.position, owner.transform.position) <= owner.weaponRange)
            {
                owner.nextFire = Time.time + (60f / owner.fireRate);
                owner.fire();
            }
        }
    }

    public void Exit()
    {
        agent.isStopped = true;
    }
}

public class State_Searching : IState
{
    public string stateName { get; } = "Searching";
    EnemyController owner;
    NavMeshAgent agent;
    public State_Searching(EnemyController owner) { this.owner = owner; }
    public void Enter()
    {
        agent = owner.GetComponent<NavMeshAgent>();
        agent.destination = owner.lastSeenPosition;
        agent.speed = owner.runSpeed;
        agent.isStopped = false;

        

        owner.resetWeapons(-owner.transform.forward);
    }
    public void Execute()
    {
        agent.destination = owner.lastSeenPosition;
        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            //Reset refind target
            owner.refindTarget = false;
            owner.stateMachine.ChangeState(new State_Patrol(owner));
        }

        if (owner.seenTarget == true)
        {
            //Reset refind target
            owner.refindTarget = false;
            owner.stateMachine.ChangeState(new State_Attack(owner));

        }
    }

    public void Exit()
    {
        agent.isStopped = true;
    }
}


public class EnemyController : MonoBehaviour
{
    public StateMachine stateMachine = new StateMachine();

    public bool boss = false;
    //Boss vars
    public float damageAccumulated = 0f;
    public float burstChargeUpTime = 15f;
    public float burstStop;
    public float chargeStop;
    public bool chargeComplete = false;
    public bool charging = true;
    public bool fightStarted = false;

    public float health = 100;
    public float healthMax;
    public float fov = 110;
    public float weaponRange = 25f;
    public float damage = 10f;
    public float accuracy = 75f;
    public float callRange = 30f;
    //in RPM
    public float fireRate = 60f;
    public float nextFire = 0f;
    public WaitForSeconds shotDuration = new WaitForSeconds(0.5f);
    //Movement
    public float walkSpeed = 1.5f;
    public float runSpeed = 3f;

    NavMeshAgent agent = null;

    //Distance away from target that is close enough
    public float targetDistance;

    public Waypoint waypoint = null;

    public bool seenTarget = false;
    public bool refindTarget = false;
    public bool lastShotHit = false;
    public bool signal = false;
    public float signalTimeout = 5f;
    private float signalTimer = 0f;
    
    private float timer = 0f;

    private SphereCollider collider;
    public Vector3 lastSeenPosition;
    public GameObject target;
    private LineRenderer laser;
    private AudioSource audio;
    private Queue<Transform> weapons = new Queue<Transform>();
    private Transform eyeTransform;

    private int layerMask = 1 << 10;

    public void Start()
    {
        healthMax = health;

        agent = GetComponent<NavMeshAgent>();
        collider = GetComponent<SphereCollider>();
        target = GameObject.FindWithTag("Player");
        laser = GetComponent<LineRenderer>();
        audio = GetComponent<AudioSource>();
        getEyeTransform(gameObject.transform);

        getWeapons(gameObject.transform);

        if (!boss)
        {
            stateMachine.ChangeState(new State_Patrol(this));
        }
        else
        {
            stateMachine.ChangeState(new State_Boss_Patrol(this));

        }

    }

    public void Update()
    {

        stateMachine.Update();

        //Boss burst timer
        if(Time.time > burstStop && !charging)
        {
            charging = true;
            chargeComplete = false;
            chargeStop = Time.time + burstChargeUpTime;
        }
        else if(Time.time > chargeStop && !chargeComplete && charging)
        {
            chargeComplete = true;
        }

        if (!fightStarted && boss && (seenTarget || damageAccumulated > 0))
        {
            fightStarted = true;
            GameObject.Find("UICanvas").GetComponent<HUDCanvas>().startBossFight(this);
        }


        timer += Time.deltaTime;

        if(Time.time >= signalTimer + signalTimeout || seenTarget)
        {
            signal = false;
        }
    }

    public void getEyeTransform(Transform parent)
    {

        foreach(Transform child in parent)
        {
            getEyeTransform(child);
            if(child.name == "LookTransform")
            {
                eyeTransform = child;
            }
        }
    }

    public void getWeapons(Transform parent)
    {
        foreach(Transform child in parent)
        {
            getWeapons(child);
            
            if(child.tag == "Weapon")
            {
                weapons.Enqueue(child);
            }
        }
    }
    

    public void fire()
    {
        //Fire
        RaycastHit hit;
        nextFire = Time.time + (60f / fireRate);


        Transform weapon = weapons.Dequeue();
        weapons.Enqueue(weapon);
       

        Vector3 direction = HelperFunctions.fireInAccuracy(accuracy, 1 / 6f);

        //Make the direction match the transform
        direction = weapon.GetChild(0).transform.TransformDirection(direction.normalized);

        laser.SetPosition(0, weapon.GetChild(0).transform.position);


        if (Physics.Raycast(weapon.GetChild(0).transform.position, direction, out hit, weaponRange, ~layerMask))
        {

            laser.SetPosition(1, hit.point);


        }
        else
        {
            laser.SetPosition(1, (direction * weaponRange) + weapon.GetChild(0).transform.position);
        }

        StartCoroutine(ShotEffect(laser));
        lastShotHit = false;
        if (hit.collider != null)
        {
            if (hit.collider.gameObject == target)
            {
                target.GetComponent<PlayerController>().takeDamage(damage);
                lastShotHit = true;
            }
        }
    }

    public void resetWeapons(Vector3 direction)
    {
        foreach (Transform weapon in weapons)
        {
            weapon.rotation = Quaternion.LookRotation(direction);
        }

    }

    public void aimWeapons(Vector3 targetPos)
    {
        Vector3 destination;
        foreach(Transform weapon in weapons)
        {
            destination = targetPos - weapon.GetChild(0).transform.position;
            Vector3 lookPos = weapon.GetChild(0).forward - destination;
            weapon.rotation = Quaternion.LookRotation(lookPos);
        }
        
    }

    public void faceTarget(Vector3 destination)
    {

        destination = destination - transform.position;

        float step = 10f * Time.deltaTime;
        Vector3 lookPos = destination - gameObject.transform.forward;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, destination, step, 0);
        transform.rotation = Quaternion.LookRotation(newDir);
    }

    public Waypoint closestWaypoint()
    {
        float minDistance = float.MaxValue;
        Waypoint closestWaypoint = waypoint;
        float distance = 0;
        Vector3 startPosition = waypoint.transform.position;
        do
        {
            distance = Vector3.Distance(gameObject.transform.position, waypoint.transform.position);
            if (distance < minDistance)
            {
                closestWaypoint = waypoint;
                minDistance = distance;
            }
            waypoint = waypoint.nextWaypoint;
        } while (waypoint.transform.position != startPosition);


        return closestWaypoint;
    }

    private void OnTriggerStay(Collider other)
    {
        // is it the player?
        if (other.gameObject == target)
        {
            Vector3 otherPos = other.transform.position;
            if(other.gameObject.GetComponent<CharacterController>() != null)
            {
                otherPos += new Vector3(0, (other.gameObject.GetComponent<CharacterController>().height/2f) - (other.gameObject.GetComponent<CharacterController>().radius/2f), 0);
            }
            // angle between us and the player
            Vector3 direction = otherPos - eyeTransform.position;
            float angle = Vector3.Angle(direction, eyeTransform.forward);
            
            // reset whether we’ve seen the player
            seenTarget = false;
            RaycastHit hit;
            // is it less than our field of view
            if (angle < fov * 0.5f)
            {

                // if the raycast hits the player we know
                // there is nothing in the way
                // adding transform.up raises up from the floor by 1 unit
                if (Physics.Raycast(eyeTransform.position, direction.normalized, out hit, collider.radius))
                {
                    
                    if (hit.collider.gameObject == target)
                    {
                        // flag that we've seen the player
                        // remember their position
                        seenTarget = true;
                        lastSeenPosition = target.transform.position;
                    }


                }
            }

            
        }
        

        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
        if(enemy != null && (seenTarget || signal))
        {
            //if close enough to call to ally
            if (Vector3.Distance(gameObject.transform.position, enemy.gameObject.transform.position) < callRange)
            {
                enemy.signalTarget(lastSeenPosition);
            }
           
        }
    }

    public void signalTarget(Vector3 location)
    {

        if (!seenTarget)
        {
            signal = true;
            signalTimer = Time.time;
            lastSeenPosition = location;
            refindTarget = true;
        }
        
    }

    public void Damage(float damageAmount, GameObject source)
    {
        

        //If target deals damage to this enemy, enemy will look for target
        if(source == target)
        {
            signalTimer = Time.time;
            signal = true;
            refindTarget = true;
            lastSeenPosition = source.transform.position;
        }

        //subtract damage amount when Damage function is called

        health -= damageAmount;
        damageAccumulated += damageAmount;

        //Check if health has fallen below zero

        if (health <= 0)
        {

            //if health has fallen below zero, deactivate it 
            if (boss)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(this.gameObject);
            }
            

        }

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (collider != null)
        {
            Gizmos.DrawWireSphere(transform.position, collider.radius);
            if (seenTarget)
                Gizmos.DrawLine(transform.position, lastSeenPosition);
            if (lastSeenPosition != Vector3.zero)
            {
                Gizmos.DrawWireSphere(lastSeenPosition, 1);
            }

        }
        
    }

    private IEnumerator ShotEffect(LineRenderer lineRenderer)
    {

        lineRenderer.enabled = true;

        audio.Play();

        yield return shotDuration;

        lineRenderer.enabled = false;


    }
}
