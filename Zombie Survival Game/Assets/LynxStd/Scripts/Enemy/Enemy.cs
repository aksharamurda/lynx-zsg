using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LynxStd
{
    public class Enemy : MonoBehaviour
    {
        public float radius = 3f;
        public float followDistance;
        public float AttackDamage = 15f;

        private Transform target;
        private float distance;
        private NavMeshAgent agent;
        private Animation anim;
        private Collider capsuleCollider;

        private bool isDeath;
        public bool destroyObject;

        public List<string> animationWalks = new List<string>();
        private int indexAnimWalk;
        public List<string> animationHits = new List<string>();
        private int indexAnimHit;
        public List<string> animationAttacks = new List<string>();
        private int indexAnimAttack;

        public List<string> animationDeaths = new List<string>();

        void Start()
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animation>();
            capsuleCollider = GetComponent<Collider>();



            indexAnimWalk = Random.Range(0, animationWalks.Count);
            indexAnimHit = Random.Range(0, animationHits.Count);
            indexAnimAttack = Random.Range(0, animationAttacks.Count);

            anim[animationWalks[indexAnimWalk]].speed = agent.speed * 4;
            anim.CrossFade(animationWalks[indexAnimWalk]);
        }

        void Update()
        {
            if (!isDeath)
            {
                if (target != null)
                {
                    distance = Vector3.Distance(transform.position, target.position);
                    if (followDistance <= distance)
                    {
                        agent.destination = target.position;
                        agent.stoppingDistance = radius;
                    }
                    else
                        anim.CrossFade(animationAttacks[indexAnimAttack]);

                }
            }

        }

        public void Attack()
        {
            target.SendMessage("OnHit", new Hit(Vector3.one, Vector3.one, AttackDamage, gameObject, target.gameObject),
                                                 SendMessageOptions.DontRequireReceiver);
        }

        public void OnHit(Hit hit)
        {
            StartCoroutine(OnHitWait());
        }

        public void OnDead()
        {
            if (!isDeath)
            {
                isDeath = true;
                capsuleCollider.enabled = false;
                agent.isStopped = true;
                anim.CrossFade(animationDeaths[Random.Range(0, animationDeaths.Count)]);
                agent.enabled = false;

                if (destroyObject)
                    Destroy(gameObject, 1f);
            }

        }

        IEnumerator OnHitWait()
        {
            agent.isStopped = true;
            anim[animationHits[indexAnimHit]].speed = 1.5f;
            anim.CrossFade(animationHits[indexAnimHit]);
            yield return new WaitForSeconds(0.5f);
            if (!isDeath)
            {
                agent.isStopped = false;
                anim.CrossFade(animationWalks[indexAnimWalk]);
            }
            else
                OnDead();
        }
    }
}
