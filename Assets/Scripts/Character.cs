using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    [SerializeField]
    private float JumpImpulse = 1f;
    [SerializeField]
    private Vector2 WallJumImpulse = new Vector2(4, 4);

    [SerializeField]
    private float GroundControl = 4f;
    [SerializeField]
    private float AirControl = 2f;
    [SerializeField]
    private float MaxHorizontalVelocity = 2f;

    [SerializeField]
    private float ShootPreviewDistance = 50;
    [SerializeField]
    private float ShootDistance = 100;

    [SerializeField]
    private Rigidbody2D rigidBody = null;

    [SerializeField]
    private LineRenderer shootPreview = null;


    private bool requestJump = false;
    private float horizontalRequest = 0f;
    private Vector2 recorderVelocity;

    private Vector2 AimVector = new Vector2();
    private bool canShoot;
    private Vector2 TrueAimVector = new Vector2();

    private RaycastHit2D[] raycastHits = new RaycastHit2D[1];
    private ContactFilter2D movementContactFilter = new ContactFilter2D();

    private ContactFilter2D shootContactFilter = new ContactFilter2D();

    private Modes mode;
    public enum Modes
    {
        Default,
        Aiming,
    }

    // Start is called before the first frame update
    void Start()
    {
        this.movementContactFilter.useLayerMask = true;
        LayerMask layerMask = LayerMask.GetMask("Wall");
        this.movementContactFilter.SetLayerMask(layerMask);
        this.shootContactFilter.SetLayerMask(layerMask);

        mode = Modes.Default;
    }

    internal bool IsTouchingDown()
    {
        float offset = 1f / 3f;
        Vector2 position = this.transform.position;
        position.x -= offset;
        Vector2 direction = Vector2.down;
        float rayDistance = 1.6f;
        for (int i = -1; i <= 1; ++i)
        {
            int nbContacts = UnityEngine.Physics2D.Raycast(position, direction, this.movementContactFilter, this.raycastHits, rayDistance);
            Debug.DrawLine(position, position + direction * rayDistance);
            if (nbContacts > 0)
            {
//                for (int rayIndex = 0; rayIndex < nbContacts; ++rayIndex)
                {
  //                  if (this.raycastHits[rayIndex].distance <= rayDistance)
                    {
                        return true;
                    }
                }
            }

            position.x += offset;
        }

        return false;
    }

    internal float IsTouchingWalls()
    {
        float walls = 0f;
        Vector2 direction = Vector2.right;
        float rayDistance = 1.7f;
        int nbContacts = UnityEngine.Physics2D.Raycast(this.transform.position, direction, this.movementContactFilter, this.raycastHits, rayDistance);
        if (nbContacts > 0)
        {
            walls -= 1f;
        }

        nbContacts = UnityEngine.Physics2D.Raycast(this.transform.position, -direction, this.movementContactFilter, this.raycastHits, rayDistance);
        if (nbContacts > 0)
        {
            walls += 1f;
        }

        return walls;
    }

    private void MovementsControls()
    {
        if (this.mode != Modes.Default)
        {
            return;
        }

        bool isGrounded = this.IsTouchingDown();
        Vector2 velocity = this.rigidBody.velocity;
        bool changed = false;
        if (requestJump)
        {
            if (isGrounded)
            {
                velocity.y = this.JumpImpulse;
                changed = true;
            }
            else
            {
                float wallJump = this.IsTouchingWalls();
                if (wallJump != 0f)
                {
                    velocity.y = this.WallJumImpulse.y;
                    velocity.x = wallJump * this.WallJumImpulse.x;
                }
            }
        }

        if (horizontalRequest != 0f)
        {
            if (isGrounded)
            {
                velocity.x += horizontalRequest * this.GroundControl;
            }
            else
            {
                velocity.x += horizontalRequest * this.AirControl;
            }
            velocity.x = Mathf.Clamp(velocity.x, -this.MaxHorizontalVelocity, this.MaxHorizontalVelocity);
            changed = true;
        }

        if (changed)
        {
            this.rigidBody.velocity = velocity;
        }

        this.requestJump = false;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            this.requestJump = true;
        }

        this.horizontalRequest = Input.GetAxis("Horizontal");

        bool requestAim = Input.GetButton("Fire1");
        if (requestAim && this.mode != Modes.Aiming)
        {
            this.recorderVelocity = this.rigidBody.velocity;
            this.rigidBody.bodyType = RigidbodyType2D.Static;
            this.mode = Modes.Aiming;
            this.canShoot = false;
        }
        else if (!requestAim && this.mode != Modes.Default)
        {
            this.rigidBody.bodyType = RigidbodyType2D.Dynamic;
            this.mode = Modes.Default;
            this.rigidBody.velocity = this.recorderVelocity;
            this.canShoot = false;
        }

        if (this.mode == Modes.Aiming)
        {
            this.AimVector.x = this.horizontalRequest;
            this.AimVector.y = Input.GetAxis("Vertical");
            if (this.AimVector.sqrMagnitude != 0)
            {
                this.TrueAimVector = this.AimVector.normalized;
                this.canShoot = true;
            }
        }
    }

    void FixedUpdate()
    {
        this.MovementsControls();

        if (this.canShoot)
        {
            if (!this.shootPreview.enabled)
            {
                this.shootPreview.enabled = true;
            }

            Vector2 p1 = this.transform.position;
            Vector2 dir = this.TrueAimVector;
            this.shootPreview.positionCount = 1;
            this.shootPreview.SetPosition(0, p1);
            float remainingDist = this.ShootDistance;

            for (int i = 0; i < 40; ++i)
            {
                int hit = Physics2D.Raycast(p1 + (dir * .001f), dir, this.shootContactFilter, this.raycastHits);
                if (hit == 0)
                {
                    remainingDist = 0;
                    break;
                }


                if (remainingDist >= this.raycastHits[0].distance)
                {
                    p1 = this.raycastHits[0].point;
                    dir += 2 * this.raycastHits[0].normal;
                    dir.Normalize();
                    this.shootPreview.positionCount++;
                    this.shootPreview.SetPosition(this.shootPreview.positionCount - 1, p1);
                    remainingDist -= this.raycastHits[0].distance;
                }
                else
                {
                    this.shootPreview.positionCount++;
                    this.shootPreview.SetPosition(this.shootPreview.positionCount - 1, p1 + dir * remainingDist);
                    remainingDist = 0;
                    break;
                }
            }
        }
        else if (this.shootPreview.enabled)
        {
            this.shootPreview.enabled = false;
        }
    }
}
