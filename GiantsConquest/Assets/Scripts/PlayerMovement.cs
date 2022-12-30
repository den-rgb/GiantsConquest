using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f; // movement speed of the character

    public bool isOnGround = false; // variable to track if the character is on the ground

    public CapsuleCollider LfootColider; // the left foot collider to check for ground contact
    public CapsuleCollider RfootColider; // the right foot collider to check for ground contact

    public float rotationSpeed = 90.0f; // the speed at which the character rotates towards the movement direction

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
        isOnGround = Physics.CheckCapsule(LfootColider.bounds.center, RfootColider.bounds.center, LfootColider.radius, LayerMask.GetMask("Ground")) ||
                     Physics.CheckCapsule(LfootColider.bounds.center, RfootColider.bounds.center, RfootColider.radius, LayerMask.GetMask("Ground"));

        // Rotate the character's body towards the movement direction gradually
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(movementDirection), rotationSpeed * Time.deltaTime);

        // Only move the character if it is on the ground and receiving input
        if (isOnGround && (horizontalInput != 0 || verticalInput != 0))
        {
            // Move the character in the movement direction with the set speed
            transform.position += movementDirection * speed * Time.deltaTime;
        }
    }
}
