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