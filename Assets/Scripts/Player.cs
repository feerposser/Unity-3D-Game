using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private CharacterController controller;
    private Transform camera;
    Vector3 moveDirection;
    private Animator animator;

    [Header("Atributes")]
    public int health = 100;
    public float speed = 5;
    public float gravity = 10;
    public int damage;
    private bool isAlive = true;

    public float smooth;
    public float turnSmoothSpeed;
    public float sphereColliderRadius;

    // animation controlling
    private bool isWalking = false, isAttacking = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        camera = Camera.main.transform;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        GetMouseInput();
        Die();
    }

    private void Move()
    {
        if (!isAttacking)
        {
            if (controller.isGrounded)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");

                /*Debug.Log("vertical: " + vertical + "\nhorizontal: " + horizontal);*/

                Vector3 direction = new Vector3(horizontal, 0, vertical);

                if (direction.magnitude > 0)
                {

                    isWalking = true;

                    float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    angle += camera.eulerAngles.y;

                    float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref turnSmoothSpeed, smooth);

                    transform.rotation = Quaternion.Euler(0, smoothAngle, 0);

                    moveDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward * speed;

                    animator.SetInteger("transition", 2);
                }
                else
                {
                    isWalking = false;
                    moveDirection = Vector3.zero;
                    animator.SetInteger("transition", 0);
                }
            }

            moveDirection.y -= gravity * speed * Time.deltaTime;

            controller.Move(moveDirection * Time.deltaTime);
        }

    }

    private void GetMouseInput()
    {
        if (controller.isGrounded)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine("Attack");
            }
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetInteger("transition", 3);

        yield return new WaitForSeconds(0.3f);

        List<GameObject> enemies = GetEnemiesCollided();

        foreach (GameObject gameObj in enemies)
        {
            gameObj.GetComponent<SpiderEnemy>().GetDamageByAttack(damage);
        }

        yield return new WaitForSeconds(1.3f);

        animator.SetInteger("transition", 0);
        isAttacking = false;
    }

    private List<GameObject> GetEnemiesCollided()
    {
        List<GameObject> enemies = new List<GameObject>();

        foreach (Collider colision in Physics.OverlapSphere((transform.position + transform.forward + transform.up), sphereColliderRadius))
        {
            if (colision.gameObject.tag == "Enemy")
            {
                enemies.Add(colision.gameObject);
            }
        }

        return enemies;
    }

    public void GetDamageByAttack(int damage)
    {
        if(health <= 0)
        {
            Die();
        } 
        else
        {
            health -= damage;
            animator.SetTrigger("damage");
        }
    }

    private void Die()
    {
        if (isAlive && health <= 0)
        {
            isAlive = false;
            animator.SetTrigger("die");
        }
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((transform.position + transform.forward + transform.up), sphereColliderRadius);

    }
}
