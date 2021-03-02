﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropRockController : MonoBehaviour
{
    public int rootCount = 3;

    [SerializeField] Transform targetPos;
    [SerializeField] GameObject spider;
    [SerializeField] GameObject boss;
    [SerializeField] AudioClip roar;

    private int count = 0;
    private float timer = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        boss = GameObject.Find("CrawlerBoss");
    }

    // Update is called once per frame
    void Update()
    {
        if (boss == null)
        {
            if (rootCount == 0)
            {
                if (count == 0)
                {
                    spider.GetComponent<AudioSource>().clip = roar;
                    spider.GetComponent<AudioSource>().Play();
                    count++;
                }
                if (timer <= 0)
                {
                    if (count == 1)
                    {
                        GetComponent<AudioSource>().Play();
                        count++;
                    }
                    float step = 40 * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, targetPos.position, step);
                }
                else
                {
                    timer -= Time.deltaTime;
                }
            }

            if (Vector3.Distance(transform.position, targetPos.position) < 5)
            {
                Destroy(spider);
            }
        }
    }

}
