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

    [SerializeField]
    private Projectile projectile = null;

    public bool InhibShot = false;

    private bool requestJump = false;
    private float horizontalRequest = 0f;
    private Vector2 recorderVelocity;

    private Vector2 AimVector = new Vector2();
    private bool canShoot;
    private Vector2 TrueAimVector = new Vector2();

    private RaycastHit2D[] raycastHits = new RaycastHit2D[10];
    private ContactFilter2D movementContactFilter = new ContactFilter2D();

    private ContactFilter2D shootContactFilter = new ContactFilter2D();
    private ContactFilter2D triggerContactFilter = new ContactFilter2D();

    private List<PathNode> shootNodes = new List<PathNode>();
    private List<TriggerHit> triggerHitted = new List<TriggerHit>();

    private float coyoteTime = 0;

    private Modes mode;
    public enum Modes
    {
        Default,
        Aiming,
    }

    public class PathNode
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Length;
        public float StartDistance;
        public float EndDistance;

        public Vector2 Normal;
        public float TurnRate;
    }

    public class TriggerHit
    {
        public GameObject Trigger;
        public float DistanceHitted;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.movementContactFilter.useLayerMask = true;
        this.movementContactFilter.SetLayerMask(LayerMask.GetMask("Wall") + LayerMask.GetMask("UI"));
        
        this.shootContactFilter.SetLayerMask(LayerMask.GetMask("Wall"));
        this.triggerContactFilter.SetLayerMask(LayerMask.GetMask("Water"));
        this.triggerContactFilter.useTriggers = true;

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
        float rayDistance = 1.5f;
        int nbContacts = UnityEngine.Physics2D.Raycast(this.transform.position, direction, this.movementContactFilter, this.raycastHits, rayDistance);
        Debug.DrawLine(this.transform.position, this.transform.position + (new Vector3(direction.x, direction.y, 0) * rayDistance));
        if (nbContacts > 0)
        {
            walls -= 1f;
        }

        nbContacts = UnityEngine.Physics2D.Raycast(this.transform.position, -direction, this.movementContactFilter, this.raycastHits, rayDistance);
        Debug.DrawLine(this.transform.position, this.transform.position - (new Vector3(direction.x, direction.y, 0) * rayDistance));
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
            if (isGrounded || this.coyoteTime > 0)
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
                    changed = true;
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

        if (isGrounded)
        {
            this.coyoteTime = .2f;
        }
        else if(this.coyoteTime > 0)
        {
            this.coyoteTime -= Time.fixedDeltaTime;
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            this.requestJump = true;
        }

        this.horizontalRequest = Input.GetAxis("Horizontal");

        bool requestAim = Input.GetButton("Fire1") && !this.InhibShot;
        if (requestAim && this.mode == Modes.Default)
        {
            if (!this.projectile.isShooting)
            {
                this.recorderVelocity = this.rigidBody.velocity;
                this.rigidBody.bodyType = RigidbodyType2D.Static;
                this.mode = Modes.Aiming;
                this.canShoot = false;
            }
        }
        else if (!requestAim && this.mode == Modes.Aiming)
        {
            this.rigidBody.bodyType = RigidbodyType2D.Dynamic;
            this.mode = Modes.Default;
            this.rigidBody.velocity = this.recorderVelocity;
            this.projectile.Shoot(this.shootNodes, this.ShootDistance, this.triggerHitted);
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
            this.shootNodes.Clear();
            this.triggerHitted.Clear();
            float traveledDistance = 0f;

            PathNode currentNode = new PathNode
            {
                Position = p1,
                Direction = dir,
                StartDistance = 0,
            };

            this.shootNodes.Add(currentNode);

            for (int i = 0; i < 40; ++i)
            {
                int triggerHit = Physics2D.Raycast(p1 + (dir * .001f), dir, this.triggerContactFilter, this.raycastHits);
                if (triggerHit > 0)
                {
                    for (int triggerIndex = 0; triggerIndex < triggerHit; ++triggerIndex)
                    {
                        ref RaycastHit2D h = ref this.raycastHits[triggerIndex];
                        bool contained = false;
                        foreach (var hitted in this.triggerHitted)
                        {
                            if (hitted.Trigger == h.transform.gameObject)
                            {
                                contained = true;
                                break;
                            }
                        }

                        if (contained)
                        {
                            continue;
                        }

                        this.triggerHitted.Add(new TriggerHit {
                            Trigger = h.transform.gameObject,
                            DistanceHitted = traveledDistance + h.distance,
                        });
                    }
                }

                int hit = Physics2D.Raycast(p1 + (dir * .001f), dir, this.shootContactFilter, this.raycastHits);
                if (hit == 0)
                {
                    remainingDist = 0;
                    break;
                }

                int hitIndex = 0;
                for (int index = 1; index < hit; ++index)
                {
                    if (this.raycastHits[index].distance < this.raycastHits[hitIndex].distance)
                    {
                        hitIndex = index;
                    }
                }

                ref RaycastHit2D rayCastHit = ref this.raycastHits[hitIndex];

                if (remainingDist >= rayCastHit.distance)
                {
                    p1 = rayCastHit.point;
                    dir += 2 * rayCastHit.normal;
                    dir.Normalize();
                    this.shootPreview.positionCount++;
                    this.shootPreview.SetPosition(this.shootPreview.positionCount - 1, p1);
                    remainingDist -= rayCastHit.distance;
                    traveledDistance += rayCastHit.distance;

                    currentNode.Length = rayCastHit.distance;
                    currentNode.EndDistance = traveledDistance;
                    currentNode = new PathNode
                    {
                        Position = p1,
                        Direction = dir,
                        StartDistance = traveledDistance,
                        Normal = rayCastHit.normal,
                        TurnRate = Vector2.Dot(dir, rayCastHit.normal),
                    };

                    this.shootNodes.Add(currentNode);
                }
                else
                {
                    currentNode.Length = remainingDist;
                    currentNode.EndDistance = this.ShootDistance;

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
