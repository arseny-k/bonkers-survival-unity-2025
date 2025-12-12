using UnityEngine;
using UnityEngine.AI;
public abstract class EnemyBaseState
{
    protected EnemyController Ctx;
    public void Init(EnemyController ctx) { Ctx = ctx; }
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

public class EnemyIdleState : EnemyBaseState
{
    private float _timer;

    public override void Enter()
    {
        MoveToRandomPoint();
    }

    public override void Update()
    {
        if (Ctx.Player != null && Vector3.Distance(Ctx.transform.position, Ctx.Player.position) < Ctx.detectRadius)
        {
            Ctx.ChangeState(Ctx.ChaseState);
            return;
        }

        if (!Ctx.Agent.pathPending && Ctx.Agent.remainingDistance < 0.5f)
        {
            _timer += Time.deltaTime;
            if (_timer > 2f)
            {
                MoveToRandomPoint();
                _timer = 0;
            }
        }
    }

    private void MoveToRandomPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * Ctx.patrolRadius;
        randomDir += Ctx.transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, Ctx.patrolRadius, 1))
        {
            Ctx.Agent.SetDestination(hit.position);
        }
    }

    public override void Exit() { }
}

public class EnemyChaseState : EnemyBaseState
{
    public override void Enter() { }

    public override void Update()
    {
        if (Ctx.Player == null) return;

        Ctx.Agent.SetDestination(Ctx.Player.position);
        float dist = Vector3.Distance(Ctx.transform.position, Ctx.Player.position);

        if (dist <= Ctx.attackRange) Ctx.ChangeState(Ctx.AttackState);
        if (dist > Ctx.detectRadius * 1.5f) Ctx.ChangeState(Ctx.IdleState);
    }

    public override void Exit() => Ctx.Agent.ResetPath();
}

public class EnemyAttackState : EnemyBaseState
{
    private float _timer;
    
    public override void Enter()
    {
        Ctx.Agent.ResetPath();
        _timer = Ctx.attackCooldown;
    }

    public override void Update()
    {
        if (Ctx.Player == null) return;

        Vector3 direction = (Ctx.Player.position - Ctx.transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Ctx.transform.rotation = Quaternion.Slerp(Ctx.transform.rotation, Quaternion.LookRotation(direction), 5 * Time.deltaTime);
        }

        _timer += Time.deltaTime;

        if (_timer >= Ctx.attackCooldown)
        {
            _timer = 0;
            Ctx.animator.SetTrigger("Attack");
            if (Vector3.Distance(Ctx.transform.position, Ctx.Player.position) <= Ctx.attackRange)
            {
                 var hp = Ctx.Player.GetComponent<IDamageable>();
                 hp?.TakeDamage(Ctx.damage, Ctx.transform.position);
            }
        }

        float dist = Vector3.Distance(Ctx.transform.position, Ctx.Player.position);
        if (dist > Ctx.attackRange + 0.5f) Ctx.ChangeState(Ctx.ChaseState);
    }

    public override void Exit() { }
}

public class EnemyHitState : EnemyBaseState
{
    public Vector3 attackerPosition;
    private float _stunTime = 0.5f;
    private float _timer;

    public override void Enter()
    {
        _timer = 0;
        Ctx.Agent.enabled = false; 
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        
        Vector3 knockbackDir = (Ctx.transform.position - attackerPosition).normalized;
        knockbackDir.y = 0;
        Ctx.transform.position += knockbackDir * 5f * Time.deltaTime * (1 - _timer / _stunTime);

        if (_timer >= _stunTime) Ctx.ChangeState(Ctx.ChaseState);
    }

    public override void Exit()
    {
        Ctx.Agent.enabled = true;
    }
}

public class EnemyDeathState : EnemyBaseState
{
    public override void Enter()
    {
        Ctx.Agent.enabled = false;
    }

    public override void Update()
    {
    }

    public override void Exit() { }
}