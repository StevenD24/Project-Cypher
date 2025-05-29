using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bullet;
    public Transform firePosition;
    public float timeBetweenShots;
    private bool canShoot = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire3") && canShoot)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Instantiate(bullet, firePosition.position, firePosition.rotation);
        StartCoroutine(ShootDelay());
    }

    IEnumerator ShootDelay()
    {
        canShoot = false;
        yield return new WaitForSeconds(timeBetweenShots);
        canShoot = true;
    }
}
