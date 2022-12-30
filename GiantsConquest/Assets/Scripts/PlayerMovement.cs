using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // movement speed of the character
    public float speed = 5.0f;


    // variable to track if the character is on the ground
    public bool isOnGround = false;

    // variable to track if the characters feet are on the ground
    public bool pushOff = false;

    // the left foot collider to check for ground contact
    public CapsuleCollider LfootColider;

    // the right foot collider to check for ground contact
    public CapsuleCollider RfootColider;

    // the sphere collider to check for ground contact
    public SphereCollider basicCollider;

    // the speed at which the character rotates towards the movement direction
    public float rotationSpeed = 90.0f;

    // Update is called once per frame
    void Update()
    {
        // Get input from the horizontal axis (left and right arrow keys by default)
        float horizontalInput = Input.GetAxis("Horizontal");

        // Get input from the vertical axis (up and down arrow keys by default)
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the character's movement direction vector based on the input
        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);

        // Normalize the movement direction vector so that the character doesn't move faster diagonally
        movementDirection = movementDirection.normalized;

        // Check if either the left foot collider or the right foot collider is touching the ground
        pushOff = Physics.CheckCapsule(LfootColider.bounds.center, RfootColider.bounds.center, LfootColider.radius, LayerMask.GetMask("Ground")) ||
                     Physics.CheckCapsule(LfootColider.bounds.center, RfootColider.bounds.center, RfootColider.radius, LayerMask.GetMask("Ground"));

        // Check if the sphere collider is touching the ground
        isOnGround = Physics.CheckSphere(basicCollider.bounds.center, basicCollider.radius, LayerMask.GetMask("Ground"));

        // if the forward key is pressed and the character is on the ground, use the current forward direction of the character as the movement direction
        if (verticalInput > 0 && horizontalInput > 0)
        {
            movementDirection = (transform.forward + transform.right).normalized;
        }
        else if (verticalInput > 0 && horizontalInput < 0)
        {
            movementDirection = (transform.forward - transform.right).normalized;
        }
        else if (verticalInput < 0 && horizontalInput > 0)
        {
            movementDirection = (-transform.forward + transform.right).normalized;
        }
        else if (verticalInput < 0 && horizontalInput < 0)
        {
            movementDirection = (-transform.forward - transform.right).normalized;
        }
        else if (verticalInput > 0)
        {
            movementDirection = transform.forward;
        }
        else if (verticalInput < 0)
        {
            movementDirection = -transform.forward;
        }
        else if (horizontalInput > 0)
        {
            movementDirection = transform.right;
        }
        else if (horizontalInput < 0)
        {
            movementDirection = -transform.right;
        }
        else
        {
            movementDirection = transform.forward;
        }


        // Rotate the character's body towards the movement direction gradually
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(movementDirection), rotationSpeed * Time.deltaTime);

        // Only move the character if it is on the ground and receiving input
        if (isOnGround && (horizontalInput != 0 || verticalInput != 0))
        {
            if (pushOff)
            {
                speed = 15f;
            }
            else
            {
                speed = 10f;
            }
            // Move the character in the movement direction with the set speed
            transform.position += movementDirection * speed * Time.deltaTime;
        }

    }
}