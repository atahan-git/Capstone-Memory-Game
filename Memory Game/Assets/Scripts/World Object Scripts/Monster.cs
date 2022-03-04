using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

public class Monster : MonoBehaviour {
	public bool isComingFromLeft;

	public bool isStructure = false;

	public GameObject structureDeathPrefab;
	
	public Lane myLane;

	private bool isAlive = true;
	public float health = 10;
	float maxHealth;

	public float healthPercent {
		get {
			return health / maxHealth;
		}
	}

	[HideIf("isStructure")]
	public float damage = 1;
	[HideIf("isStructure")]
	public float attackInterval = 2;
	private float attackRandomMultiplier = 0.1f;
	public float animHitDelay = 0.4f;
	private float curAttackTimer = 0;

	public float width = 0.2f;
	[HideIf("isStructure")]
	public float walkSpeed = 0.3f;

	[HideIf("isStructure")]
	public float attackRange = 0.05f;

	public enum MonsterStates {idle, attack, dead, walk}

	[HideIf("isStructure")]
	public MonsterStates myState = MonsterStates.idle;
	
	
	private float pathUpdateTimer = 0;
	[HideIf("isStructure")]
	public float pathUpdateDelay = 0.1f;

	

	SkeletonAnimation skeletonAnimation;
	Spine.AnimationState animationState;
	Skeleton skeleton;
	
	NavMeshAgent2D agent;

	private float startZ;

	private RandomClipPlayer hitSounds;

	private void Start() {
		if (!isStructure) {
			skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
			skeleton = skeletonAnimation.Skeleton;
			//skeletonAnimation.Initialize(false); // when not accessing skeletonAnimation.Skeleton,
			// use Initialize(false) to ensure everything is loaded.
			animationState = skeletonAnimation.AnimationState;

			agent = GetComponent<NavMeshAgent2D>();
			agent.speed = walkSpeed;

			startZ = skeletonAnimation.transform.position.z;
			
			ChangeAnimState(MonsterStates.idle);

		}
		
		
		hitSounds = GetComponent<RandomClipPlayer>();

		lastPos = transform.position;
		
		maxHealth = health;
		myLane.activeMonsters.Add(this);
	}

	public void SetLevel(int level) {
		if (level == 2) {
			damage *= 1.1f;
		}
	}

	public void Damage(float amount) {
		if(!isAlive)
			return;
		
		health -= amount;

		if (isStructure) {
			hitSounds.PlayGetHitClip();
		}


		if (health <= 0) {
			Die();	
		}
	}

	public void Die() {
		isAlive = false;
		myLane.activeMonsters.Remove(this);

		
		if (!isStructure) {
			hitSounds.PlayDeathClip();
			ChangeAnimState(MonsterStates.dead);
			agent.enabled = false;
			StartCoroutine(FadeToNothing());
		} else {
			
			Instantiate(structureDeathPrefab, transform.position, transform.rotation);

			foreach (Transform child in transform) {
				child.gameObject.SetActive(false);
			}
		}
	}

	private float fadeSpeed = 0.5f;
	IEnumerator FadeToNothing() {
		var mat = skeletonAnimation.GetComponent<Renderer>().material;

		yield return new WaitForSeconds(0.5f);
		
		while (mat.color.a > 0.05f) {
			mat.color=  new Color(mat.color.r,mat.color.g,mat.color.b,Mathf.MoveTowards(mat.color.a, 0, fadeSpeed * Time.deltaTime));
			yield return null;
		}
		
		Destroy(gameObject);
	}

	private Vector3 lastAgentVelocity;
	//private NavMeshPath2D lastAgentPath;
	private Vector2 lastAgentDestination;
	public void PauseMonster() {
		if (agent) {
			lastAgentVelocity = agent.velocity;
			//lastAgentPath = agent.path;        
			lastAgentDestination = agent.destination;
			agent.velocity = Vector3.zero;
			agent.ResetPath();
		}
	}

	public void UnpauseMonster() {
		if (agent) {
			// if (agent.destination == lastAgentDestination) {
			// 	agent.SetPath(lastAgentPath);
			// }
			// else {
			if (lastAgentDestination != null)
				agent.SetDestination(lastAgentDestination);
			// }

			agent.velocity = lastAgentVelocity;
		}
	}

	private Vector3 lastPos;
	private bool isFacingLeft = false;
	public void UpdateMonster() {
		if(!isAlive)
			return;
		
		if (isStructure) 
			return;

		if (Mathf.Abs(transform.position.x - lastPos.x) > 0.1f) {
			if (transform.position.x - lastPos.x < 0) {
				if (isFacingLeft) {
					isFacingLeft = false;
					transform.localScale = new Vector3(-1, 1, 1);
				}
			} else {
				if (!isFacingLeft) {
					isFacingLeft = true;
					transform.localScale = new Vector3(1, 1, 1);
				}
			}
			lastPos = transform.position;
		}


		var mat = skeletonAnimation.GetComponent<Renderer>().material;
		var target = ((health / maxHealth) * 0.5f) + 0.5f;
		var val = Mathf.MoveTowards(mat.color.g, target, fadeSpeed*Time.deltaTime);
		mat.color = new Color(1, val, val);


		skeletonAnimation.transform.localPosition = myLane.GetGfxPos(skeletonAnimation.transform.position);
		
		var nearestEnemy = FindNearestEnemy();
		var distance = nearestEnemy.Item2;
		var enemy = nearestEnemy.Item1;

		if (enemy != null) {
			if (distance > attackRange) {

				ChangeAnimState(MonsterStates.walk, walkSpeed);
				if (pathUpdateTimer <= 0) {
					pathUpdateTimer = pathUpdateDelay;
					agent.SetDestination(enemy.transform.position);
				} else {
					pathUpdateTimer -= Time.deltaTime;
				}

			} else {

				agent.SetDestination(transform.position);
				if (curAttackTimer <= 0) {
					ChangeAnimState(MonsterStates.attack);
					curAttackTimer = attackInterval * Random.Range(1-attackRandomMultiplier, 1+attackRandomMultiplier);

					StartCoroutine(DelayedDealDamage(enemy, animHitDelay));
					
				}else {
					ChangeAnimState(MonsterStates.idle);
					curAttackTimer -= Time.deltaTime;
				}

			}
		} else {
			// attack wall
		}
	}

	IEnumerator DelayedDealDamage(Monster target, float delay) {
		yield return new WaitForSeconds(delay);
		if (target != null) {
			target.Damage(damage);
			hitSounds.PlayAttackClip();	
		}
	}

	(Monster, float) FindNearestEnemy() {
		Monster nearestEnemy = null;
		float distance = 10;

		for (int i = 0; i < myLane.activeMonsters.Count; i++) {
			var curMonster = myLane.activeMonsters[i];

			if (isComingFromLeft != curMonster.isComingFromLeft) {
				var curDistance = Vector3.Distance(curMonster.transform.position, transform.position) - curMonster.width - width;
				if (curDistance < distance) {
					distance = curDistance;
					nearestEnemy = curMonster;
				}
			}
		}


		return (nearestEnemy,distance);
	}


	public void ChangeAnimState(MonsterStates changeTo, float speed = 1) {
		if(isStructure)
			return;

		if (changeTo != myState) {
			TrackEntry entry;
			switch (changeTo) {
				case MonsterStates.idle:
					if(myState != MonsterStates.attack || skeletonAnimation.AnimationState.GetCurrent(0).IsComplete)
						entry = skeletonAnimation.AnimationState.SetAnimation(0, "Idle", true);
					else 
						return;
					
					
					break;
				case MonsterStates.attack:
					entry = skeletonAnimation.AnimationState.SetAnimation(0, "Attack", false);
					break;
				case MonsterStates.dead:
					entry = skeletonAnimation.AnimationState.SetAnimation(0, "Dead", false);
					break;
				case MonsterStates.walk:
					entry = skeletonAnimation.AnimationState.SetAnimation(0, "Walk", true);
					break;
				default:
					entry = null;
					break;
			}

			myState = changeTo;
			entry.TimeScale = speed;
		}
	}
}
