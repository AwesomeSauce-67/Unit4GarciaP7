using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private Rigidbody playerRb;
    public float speed = 5;
    private GameObject focalPoint;
    public bool hasPowerup;
    private float powerupStrength = 15;
    public GameObject powerupIndicator;

    public PowerUpType currentPowerup = PowerUpType.None;
    public GameObject rocketPrefab;
    private GameObject tmpRocket;
    private Coroutine powerupCountdown;

    public float hangTime;
    public float smashSpeed;
    public float explosionForce;
    public float explosionRadius;
    bool smashing=false;
    float floorY;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("FocalPoint");
    }

    // Update is called once per frame
    void Update()
    {
        float fowardInput = Input.GetAxis("Vertical");
        playerRb.AddForce(focalPoint.transform.forward * speed * fowardInput);
        powerupIndicator.transform.position = transform.position+ new Vector3(0, -0.5f, 0);

        if(currentPowerup == PowerUpType.Rockets && Input.GetKeyDown(KeyCode.F))
        {
            LaunchRockets();
        }

        if (currentPowerup == PowerUpType.Smash && Input.GetKeyDown(KeyCode.Space) && !smashing)
        {
            smashing = true;
            StartCoroutine(Smash());
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            currentPowerup = other.gameObject.GetComponent<PowerUp>().powerUpType;
            Destroy(other.gameObject);
            StartCoroutine(PowerupCountdownRoutine());
            powerupIndicator.SetActive(true);
           if(powerupCountdown != null)
           {
                StopCoroutine(powerupCountdown);
           }
           powerupCountdown = StartCoroutine(PowerupCountdownRoutine());
        }
    }
    IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasPowerup = false; 
        currentPowerup = PowerUpType.None;
        powerupIndicator.SetActive(false);
    }

    IEnumerator Smash()
    {
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);


            floorY = transform.position.y-0.1f;

            float jumpTime = Time.time + hangTime;

            while(Time.time < jumpTime)
            {
                playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, smashSpeed, playerRb.linearVelocity.z);
            yield return null;
            }

            while (transform.position.y > floorY)
            {
                playerRb.linearVelocity =new Vector3(playerRb.linearVelocity.x, -smashSpeed * 2, playerRb.linearVelocity.z);
            yield return null;
            }

            for (int i = 0; i < enemies.Length; i++)
            {
            if (enemies[i] != null)
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.0f, ForceMode.Impulse);
            }

            smashing = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && currentPowerup == PowerUpType.Pushback && hasPowerup)
        {
            Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = (collision.gameObject.transform.position - transform.position);
            collision.gameObject.GetComponent<Rigidbody>().AddForce(awayFromPlayer.normalized * powerupStrength, ForceMode.Impulse);
        }


        if (collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromEnemy = (transform.position - collision.gameObject.transform.position);
            playerRb.AddForce(awayFromEnemy * 10, ForceMode.Impulse);

        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = collision.gameObject.transform.position - transform.position;
            float knockbackForce = 15f;
            enemyRb.AddForce(awayFromPlayer.normalized * knockbackForce, ForceMode.Impulse);
        }
    }
    void LaunchRockets()
    {
        foreach(var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            tmpRocket = Instantiate(rocketPrefab, transform.position + Vector3.up, Quaternion.identity);
            tmpRocket.GetComponent<RocketBehaviour>().Fire(enemy.transform);
        }
    }
}
