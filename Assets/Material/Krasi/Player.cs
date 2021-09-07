using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Animator anime;

    Vector2 positionHTarget;
    float xMultiplyer = 5f;
    float yMultiplyer = 4f;
    bool canFlip = true;
    [SerializeField] GameObject Solvers;
    [SerializeField] GameObject HTarget;
    [SerializeField] FixedJoystick joystick;
    [SerializeField] FixedJoystick joystick2;
    [SerializeField] float handVelocity;
    [SerializeField] GameObject fHand;


    [SerializeField] Rigidbody2D brb;
    [SerializeField] Transform btransform;
    [SerializeField] Transform originalBTransform;
    [SerializeField]  float bpower;
    [SerializeField] float bvelocity;
    [SerializeField] GameObject buttonGetBallBack;
    bool ballCallback;
    bool stayingJoystick2;
    bool ballAttached = true;
    Vector2 lastKnownDirection;


    Rigidbody2D rb;
    [SerializeField] float speed;
     float speedDef;
    [SerializeField] float jumpForce;
    [SerializeField] [Range(0.0f, 1.0f)] float fHorizontalDamping;

    [SerializeField] [Range(0.0f, 1.0f)] float jumpThreshold;
    bool grounded;
    void Start()
    {
        anime = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        speedDef = speed;
    }

    void Update()
    {
        if(ballAttached)
        {
            joystick2.gameObject.SetActive(true);
            buttonGetBallBack.SetActive(false);
            anime.SetBool("CallBall", false);
        }
        else
        {
            joystick2.gameObject.SetActive(false);
            buttonGetBallBack.SetActive(true);
        }
        if(ballCallback && !ballAttached)
        {
            GetBallBack();
            anime.SetBool("CallBall", true);
        }
        else if (!ballCallback && !ballAttached)
        {
            brb.bodyType = RigidbodyType2D.Dynamic;
            anime.SetBool("CallBall", false);
        }
        if(stayingJoystick2)
        {
            HoldUpdate();
            Solvers.SetActive(true);
        }
        else
        {
            Solvers.SetActive(false);
        }
        HandsCalculations();
        HorizontalMovement_Jump();
    }
    void GetBallBack()
    {
        if (transform.position.x > btransform.position.x)
        {
            if (transform.rotation.y != -1)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.x, 180,
                    transform.rotation.z);
            }
        }
        else if (transform.position.x < btransform.position.x)
        {
            if (transform.rotation.y != 0)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.x, 0,
                    transform.rotation.z);
            }
        }
        if (Mathf.Abs(Vector2.Distance(originalBTransform.position, btransform.position)) > 3)
        {
            brb.bodyType = RigidbodyType2D.Dynamic;
            Vector2 direction = btransform.position - originalBTransform.position;
            Vector2 targetVector = direction.normalized * bvelocity;
            brb.velocity += -targetVector;
        }
        else
        {
            brb.velocity = Vector2.zero;
            brb.bodyType = RigidbodyType2D.Kinematic;
            btransform.position = Vector2.Lerp(btransform.position, originalBTransform.position,
                Time.deltaTime * bvelocity*10);
        }

    }
    public void AttachBall()
    {
        if(ballCallback)
        {
            ballCallback = false;
            brb.bodyType = RigidbodyType2D.Kinematic;
            brb.velocity = Vector2.zero;
            btransform.position = originalBTransform.position;
            btransform.parent = fHand.transform;
            ballAttached = true;
        }

    }
    public void ButtonEnter()
    {
        ballCallback = true;
    }
    public void ButtonExit()
    {
        ballCallback = false;
    }
    public void Joystick2Enter()
    {
        stayingJoystick2 = true;
    }
    public void Joystick2Exit()
    {
        stayingJoystick2 = false;
    }
    void HoldUpdate()
    {
        lastKnownDirection = joystick2.Direction;
    }
    public void BallThrow()
    {
        ballAttached = false;
        brb.bodyType = RigidbodyType2D.Dynamic;
        brb.AddForce(lastKnownDirection * bpower, ForceMode2D.Impulse);
        btransform.parent = null;
    }
    void Jump()
    {
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        grounded = false;
        anime.SetBool("Jump", true);
    }
    void HorizontalMovement_Jump()
    {
        //Joystick check for jumpprep
        if(!grounded && rb.velocity.y < 0)
        {
            anime.SetBool("Jump", false);
            anime.SetBool("grounded", false);
        }
        else if (grounded)
        {
            anime.SetBool("grounded", true);
            if (joystick.Vertical >= 0.5f)
            {
                Jump();
                anime.SetLayerWeight(1, 0);
            }
            else
            {
                anime.SetLayerWeight(1, joystick.Vertical * 2);
            }
        }



        //horizontal
        float fHorizontalVelocity = rb.velocity.x;
        fHorizontalVelocity += joystick.Horizontal * speed;
        fHorizontalVelocity *= Mathf.Pow(1f - fHorizontalDamping,
            Time.deltaTime * 10f);
        rb.velocity = new Vector2(fHorizontalVelocity, rb.velocity.y);
    }
    void HandsCalculations()
    {
        positionHTarget = new Vector2(joystick2.Horizontal * xMultiplyer,
    joystick2.Vertical * yMultiplyer);

        if (transform.rotation.y == -1)
        {
            if(rb.velocity.x > 0)
            {
                speed = speedDef / 2;
            }
            else
            {
                speed = speedDef;
            }
            anime.SetFloat("Speed", -rb.velocity.x / 10);

            HTarget.transform.localPosition = Vector2.Lerp(
                HTarget.transform.localPosition, new Vector2(-positionHTarget.x,positionHTarget.y),
                handVelocity * Time.deltaTime);
        }
        else
        {
            if (rb.velocity.x > 0)
            {
                speed = speedDef;
            }
            else
            {
                speed = speedDef / 2;
            }
            anime.SetFloat("Speed", rb.velocity.x / 10);

            HTarget.transform.localPosition = Vector2.Lerp(
                HTarget.transform.localPosition, positionHTarget,
                handVelocity * Time.deltaTime);
        }
            
        if(HTarget.transform.localPosition.x < 0f && canFlip)
        {
            if (transform.rotation.y == 0)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.x, 180,
                    transform.rotation.z);
                StartCoroutine(FlipRestriction());
            }
            else
            {
                    transform.rotation = Quaternion.Euler(transform.rotation.x, 0,
                    transform.rotation.z);
                    StartCoroutine(FlipRestriction());
            }
        }
        else
        {
            //do nothing
        }

        if(!grounded)
        {
            speed = speedDef * 0.5f;
        }
        else
        {
            speed = speedDef;
        }
    }
    void Check4Ground(Collision2D col)
    {
        if(col.gameObject.CompareTag("Ground"))
        {
            if(!grounded)
            grounded = true;
            else
                StartCoroutine(TurnOffGround());

        }
    }
    IEnumerator FlipRestriction()
    {
        canFlip = false;
        yield return new WaitForSeconds(0.2f);
        canFlip = true;
    }
    IEnumerator TurnOffGround()
    {
        yield return new WaitForSeconds(jumpThreshold);
        if(grounded)
        grounded = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Check4Ground(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        Check4Ground(collision);
    }
}
