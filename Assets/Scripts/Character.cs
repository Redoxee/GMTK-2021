using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private RaycastHit2D[] raycastHits = new RaycastHit2D[1];
    private ContactFilter2D movementContactFilter = new ContactFilter2D();

    [SerializeField]
    private float JumpImpulse = 1f;
    [SerializeField]
    private float GroundControl = 4f;
    [SerializeField]
    private float AirControl = 2f;
    [SerializeField]
    private float MaxHorizontalVelocity = 2f;


    [SerializeField]
    private Rigidbody2D rigidBody = null;

    private bool requestJump = false;
    private float horizontalRequest = 0f;

    // Start is called before the first frame update
    void Start()
    {
        movementContactFilter.useLayerMask = true;
        LayerMask layerMask = LayerMask.GetMask("Wall");
        movementContactFilter.SetLayerMask(layerMask);
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

    private void MovementsControls()
    {
        bool isGrounded = this.IsTouchingDown();
        Vector2 velocity = this.rigidBody.velocity;
        bool changed = false;
        if (isGrounded && this.requestJump)
        {
            velocity.y = this.JumpImpulse;
            changed = true;
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
    }

    void FixedUpdate()
    {
        this.MovementsControls();   
    }
}
