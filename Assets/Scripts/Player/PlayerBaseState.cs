using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerController Ctx;
    public PlayerBaseState(PlayerController context) { Ctx = context; }
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

public class GroundedState : PlayerBaseState
{
    public GroundedState(PlayerController ctx) : base(ctx) { }

    public override void Enter()
    {
    }

    public override void Update()
    {

        if (Ctx.AttackAction.WasPerformedThisFrame())
        {
            Ctx.ChangeState(Ctx.AttackState);
            return;
        }

        // Если мы на земле скорость константа.
        if (Ctx.VerticalVelocity.y < 0)
        {
            Ctx.VerticalVelocity.y = -2f;
        }

        if (Ctx.JumpAction.WasPerformedThisFrame())
        {
            Ctx.VerticalVelocity.y = Mathf.Sqrt(Ctx.jumpHeight * -2f * Ctx.gravityValue);
            Ctx.ChangeState(Ctx.AirState);
            return; // Важно выйти, чтобы не применилась гравитация ниже в этом же кадре
        }

        Vector3 horizontalMove = Ctx.GetMovement();

        Ctx.VerticalVelocity.y += Ctx.gravityValue * Time.deltaTime;

        Ctx.Controller.Move(horizontalMove + Ctx.VerticalVelocity * Time.deltaTime);

        // Если после Move мы оказались в воздухе переходим в AirState
        if (!Ctx.Controller.isGrounded)
        {
            Ctx.ChangeState(Ctx.AirState);
        }

        Ctx.UpdateAnimationState();
    }

    public override void Exit() { }
}

public class AirState : PlayerBaseState
{
    private Vector3 _inertia;

    public AirState(PlayerController ctx) : base(ctx) { }

    public override void Enter()
    {
        Vector3 currentVelocity = Ctx.Controller.velocity;
        currentVelocity.y = 0; // Нас интересует только горизонталь

        _inertia = currentVelocity * Time.deltaTime;
    }

    public override void Update()
    {
        Vector3 targetMove = Ctx.GetMovement();

        float airSmoothness = 2f;

        if (targetMove == Vector3.zero) airSmoothness = 0.5f;

        _inertia = Vector3.Lerp(_inertia, targetMove, Time.deltaTime * airSmoothness);

        Ctx.VerticalVelocity.y += Ctx.gravityValue * Time.deltaTime;

        Ctx.Controller.Move(_inertia + Ctx.VerticalVelocity * Time.deltaTime);

        if (Ctx.Controller.isGrounded && Ctx.VerticalVelocity.y < 0)
        {
            Ctx.ChangeState(Ctx.GroundedState);
        }
    }

    public override void Exit() { }
}

public class AttackState : PlayerBaseState
{
    private float _timer;
    private bool _damageDealt;

    public AttackState(PlayerController ctx) : base(ctx) { }

    public override void Enter()
    {
        _timer = 0f;
        _damageDealt = false;

        Ctx.Controller.Move(Vector3.zero);

        Debug.Log("Attack Start");
        
        Ctx.animator.SetTrigger("Attack");

    }

    public override void Update()
    {
        _timer += Time.deltaTime;

        Ctx.VerticalVelocity.y += Ctx.gravityValue * Time.deltaTime;
        Ctx.Controller.Move(Ctx.VerticalVelocity * Time.deltaTime);

        if (_timer >= Ctx.attackLag && !_damageDealt)
        {
            PerformDamage();
            _damageDealt = true;
        }

        if (_timer >= Ctx.attackDuration)
        {
            Ctx.ChangeState(Ctx.GroundedState);
        }
    }

    public override void Exit() { }

    private void PerformDamage()
    {
        Collider[] hits = Physics.OverlapSphere(Ctx.attackPoint.position, Ctx.attackRange, Ctx.enemyLayers);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(Ctx.attackDamage);
            }
        }
    }
}