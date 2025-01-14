﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { normal, rapidfire, boss};

public class Enemy : MonoBehaviour
{
    private GameObject enemySpawner;

    public float speed = 0;
    public int direction = -1; // initial y direction for enemies
    public float maxOffset = 0;

    public EnemyType type = EnemyType.normal;
    public int health = 1;
    public int value = 0;

    public GameObject enemyBulletPrefab;
    public float fireRate = 0.0f;
    public float randomFireOffset = 0.0f;
    private float timeLeftToShoot;

    void Start()
    {
        timeLeftToShoot = fireRate + Random.Range(0.0f, randomFireOffset);
        enemySpawner = GameObject.Find("EnemySpawner");
    }

    void Update()
    {
        if (Mathf.Abs(transform.position.y) >= maxOffset)
            direction = -direction;

        Vector3 currentPosition = transform.position;
        currentPosition.y += direction * speed * Time.deltaTime;
        currentPosition.y = Mathf.Clamp(currentPosition.y, -maxOffset, maxOffset);
        transform.position = currentPosition;

        if (timeLeftToShoot > 0)
        {
            timeLeftToShoot -= Time.deltaTime;
        }
        else
        {
            StartCoroutine(Fire());
            timeLeftToShoot = fireRate + Random.Range(0.0f, randomFireOffset);
        }
    }

    private IEnumerator Fire()
    {
        if (type == EnemyType.normal)
        {
            Instantiate(enemyBulletPrefab, this.transform);
            SoundManager.S.PlayEnemyShootingSound();
        }
        else if (type == EnemyType.rapidfire)
        {
            Instantiate(enemyBulletPrefab, this.transform);
            SoundManager.S.PlayEnemyShootingSound();
            yield return new WaitForSeconds(1.0f);
            Instantiate(enemyBulletPrefab, this.transform);
            SoundManager.S.PlayEnemyShootingSound();
        }
        else if (type == EnemyType.boss)
        {
            StartCoroutine(AllAroundFire());
        }
    }

    private IEnumerator AllAroundFire()
    {
        int totalBullets = 30;
        float angleDifference = 360.0f / totalBullets;
        float angle = 0;
        for (int i = 0; i < totalBullets; i++)
        {
            Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            GameObject bullet = Instantiate(enemyBulletPrefab, this.transform.position, newRotation);
            bullet.GetComponent<EnemyBullet>().ResetVelocity(Quaternion.Euler(0, 0, angle) * Vector2.left);
            angle += angleDifference;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Destroy(collision.gameObject);
            SoundManager.S.PlayEnemyDeathSound();
            health--;

            if (health <= 0)
                Die();
        }
        if (collision.gameObject.tag == "Back Wall") // went out of bounds
        {
            Destroy(this.gameObject);
            enemySpawner.GetComponent<EnemySpawner>().EnemyKilled();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(this.gameObject, 0.5f);
        enemySpawner.GetComponent<EnemySpawner>().EnemyKilled();
        GetComponent<CircleCollider2D>().enabled = false;
        speed = 0;
        GetComponent<Animator>().SetTrigger("Death");
        GameManager.S.AwardPoints(value);
    }
}
