﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float speed = 10f;
    public float fireRate = 0.3f;
    public float health = 10;
    public int score = 100;

    public int showDamageForFrames = 2;
    public float powerUpDropChance = 1f;

    public bool _____________________________;

    public Color[] originalColors;
    public Material[] materials;
    public int remainingDamageFrames = 0;

    public Bounds bounds;
    public Vector3 boundsCenterOffset;

    private void Awake()
    {
        materials = Utils.GetAllMaterials(this.gameObject);
        originalColors = new Color[materials.Length];
        for(int i=0; i<materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
        InvokeRepeating("CheckOffscreen", 0f, 2f);
    }

    // Update is called once per frame
    void Update () {
        Move();
        if(remainingDamageFrames > 0)
        {
            remainingDamageFrames--;
            if(remainingDamageFrames == 0)
            {
                UnShowDamage();
            }
        }
	}

    public virtual void Move()
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    public Vector3 pos
    {
        get
        {
            return this.transform.position;
        }
        set
        {
            this.transform.position = value;
        }
    }

    void CheckOffscreen()
    {
        //if bounds are still their default value...
        if(bounds.size == Vector3.zero)
        {
            //then set them
            bounds = Utils.CombineBoundsOfChildren(this.gameObject);
            //find the diff between bounds.center and transform.position
            boundsCenterOffset = bounds.center - transform.position;
        }

        //Every time, update the bounds to the current position
        bounds.center = transform.position + boundsCenterOffset;
        //Check to see if bounds are completely offscreen
        Vector3 off = Utils.ScreenBoundsCheck(bounds, BoundsTest.offScreen);
        if(off != Vector3.zero)
        {
            //if the enemy has gone off the bottom edge...
            if(off.y < 0)
            {
                //destroy it
                Destroy(this.gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();
                //Enemies have to be on screen to take damage
                bounds.center = transform.position + boundsCenterOffset;
                if(bounds.extents == Vector3.zero || Utils.ScreenBoundsCheck(bounds, BoundsTest.offScreen) != Vector3.zero)
                {
                    Destroy(other);
                    break;
                }
                //Hurt this enemy
                ShowDamage();
                health -= Main.W_DEFS[p.type].damageOnHit;
                if(health <= 0)
                {
                    Main.S.ShipDestroyed(this);
                    Destroy(this.gameObject);
                }
                Destroy(other);
                break;
        }
    }

    void ShowDamage()
    {
        foreach(Material m in materials)
        {
            m.color = Color.red;
        }
        remainingDamageFrames = showDamageForFrames;
    }

    void UnShowDamage()
    {
        for(int i=0; i<materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
    }
}
