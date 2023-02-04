using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;

    bool isGrounded;

    private Vector3 lastPosition = new Vector3(0f, 0f, 0f);
    public bool isMoving;
    public bool inWater;
    public bool onSand;

    // Update is called once per frame
    void Update()
    {
        //verificar se tocamos no chão para resetar a velocidade de queda, caso contrário iríamos cair mais rápido da próxima vez
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //direita é o eixo vermelho, frente é o eixo azul
        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        //verificar se o jogador está no chão para poder saltar
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.jumpSound);

            //a equação para saltar
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (lastPosition != gameObject.transform.position && isGrounded)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if (groundCheck.position.y <= 20 && groundCheck.position.y > 14)
        {
            onSand = true;
        }
        else
        {
            onSand = false;
        }

        if (groundCheck.position.y <= 14)
        {
            inWater = true;
        }
        else
        {
            inWater = false;
        }

        if (isMoving)
        {
            if (!inWater && !onSand)
            {
                SoundManager.Instance.waterWalk.Stop();
                SoundManager.Instance.sandWalk.Stop();
                SoundManager.Instance.PlaySound(SoundManager.Instance.grassWalkSound);
            }
            else if (inWater)
            {
                SoundManager.Instance.grassWalkSound.Stop();
                SoundManager.Instance.sandWalk.Stop();
                SoundManager.Instance.PlaySound(SoundManager.Instance.waterWalk);
            }
            else if (onSand)
            {
                SoundManager.Instance.waterWalk.Stop();
                SoundManager.Instance.grassWalkSound.Stop();
                SoundManager.Instance.PlaySound(SoundManager.Instance.sandWalk);
            }
        }
        else
        {
            SoundManager.Instance.grassWalkSound.Stop();
            SoundManager.Instance.waterWalk.Stop();
            SoundManager.Instance.sandWalk.Stop();
        }
        lastPosition = gameObject.transform.position;
    }
}
