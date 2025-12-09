using System;
using System.Collections;
using System.Collections.Generic;
using Creature;
using UnityEngine;
using static Team6203.Util;

namespace Creature{
    /// <summary>
    /// States representing the creature's current behavior pattern.
    /// </summary>
    public enum CreatureStatus
    {
        PATROL = 0,    // 순찰 - 기본 상태로 랜덤하게 맵을 돌아다님
        PURSUIT = 1,   // 추적 - 플레이어를 감지하고 추적 중
        ALERTED = 2,   // 경계 - 플레이어를 놓쳤으나 주변을 경계하며 탐색
        AVOIDING = 3,  // 회피 - 특정 크리처가 빛을 피함
        STUNNED = 3,   // 기절 - 일시적으로 행동 불가
    }

    /// <summary>
    /// Base class for all creatures in the game.
    /// Implements AI behaviors including patrol, pursuit, and alert states.
    /// Uses A* pathfinding to navigate the map and detect the player through vision and sound.
    /// </summary>
    public class Creature : Actor
    {
        private SoundDetector soundDetector;
        protected  Detector detector;

        protected int[,] map;


        protected Vector3 targetPosition;
        protected List<Node> path;

        private bool isChasing = false;

        protected Dictionary<CreatureStatus, (Action Start, Action Update)> actions = new Dictionary<CreatureStatus, (Action Start, Action Update)>();

        protected CreatureStatus status;

        private Coroutine alertedCounterCoroutine;

        private LayerMask soundTargetMask;

        private const float ACTION_DELAY = 0.5f;
        private const float TEMP_DELAY = 0.01f;
        private const float ALERT_TIME = 10f;

        protected bool isArrived = false;

        private void InitActions()
        {
            actions.Add(CreatureStatus.PATROL, (PatrolStart, PatrolUpdate));
            actions.Add(CreatureStatus.PURSUIT, (PursuitStart, PursuitUpdate));
            actions.Add(CreatureStatus.ALERTED, (AlerteStart, AlerteUpdate));
        }

        protected override void Awake()
        {
            base.Awake();
            InitActions();
        }

        protected override void Start()
        {
            base.Start();
            status = CreatureStatus.PATROL;
            pathLineRenderer = GetComponent<PathLineRenderer>();
            detector = GetComponent<Detector>();
            detector.SetTargetMask(LayerMask.GetMask("Player"));
            soundDetector = GetComponent<SoundDetector>();
            soundTargetMask = LayerMask.GetMask("Door") | LayerMask.GetMask("Player");
            soundDetector.SetTargetMask(soundTargetMask);
            StartCoroutine(CreatureUpdate());
            StartCoroutine(MoveOnPath());
        }

        public IEnumerator AlertedCounter()
        {
            yield return new WaitForSeconds(ALERT_TIME);
            if (status.Equals(CreatureStatus.ALERTED)) {
                status = CreatureStatus.PATROL;
                actions[status].Start();
            }
            alertedCounterCoroutine = null;
        }

        /// <summary>
        /// Detects the player using both vision and sound detection.
        /// Transitions to PURSUIT state when player is detected.
        /// Transitions to ALERTED state when player is lost during pursuit.
        /// </summary>
        protected void DetectPlayer()
        {
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log(gameObject.name + " | " + this.name + " : Detecting Player...");
            }
#endif
            // Combine vision and sound detection results
            List<Collider2D> detectedPlayerCollider = ConcatenateListWithoutDuplicates(detector.DetectByView(), soundDetector.Detect());

            if (detectedPlayerCollider.Count > 0)
            {
                // Player detected - start pursuit
                Vector3 detectedPlayerPosition = detectedPlayerCollider[0].transform.position;
                if (!targetPosition.Equals(detectedPlayerPosition))
                {
                    targetPosition = detectedPlayerPosition;
                    SetPathToPosition(targetPosition);
                }
                status = CreatureStatus.PURSUIT;
                actions[status].Start();
            } else if (status.Equals(CreatureStatus.PURSUIT) && !isChasing)
            { 
                // Player lost during pursuit - become alerted
                status = CreatureStatus.ALERTED;
                actions[status].Start();
                if (alertedCounterCoroutine != null)
                {
                    StopCoroutine(alertedCounterCoroutine);
                    alertedCounterCoroutine = null;
                }
                alertedCounterCoroutine = StartCoroutine(AlertedCounter());
            }
        }

        /// <summary>
        /// Calculates and sets a path to the specified target position using A* pathfinding.
        /// </summary>
        /// <param name="targetPosition">The world position to path to</param>
        protected void SetPathToPosition(Vector3 targetPosition)
        {
            path = FindPath(targetPosition.x, targetPosition.y);
#if UNITY_EDITOR
            if (debugMode && path != null)
                pathLineRenderer.SetPoints(path);
            else
                pathLineRenderer.Clear();
#endif
        }

        /// <summary>
        /// Sets a path in the creature's current facing direction for a specified distance.
        /// Used during ALERTED state to search in the last known direction.
        /// </summary>
        protected void SetDirectionPath()
        {
            startNode.SetPosition(transform.position.x, transform.position.y);
            path = creatureManager.pathFinders[(int)pathFinderType].FindDirectionPath(startNode, direction, 5f);
#if UNITY_EDITOR
            if (debugMode && path != null)
                pathLineRenderer.SetPoints(path);
            else
                pathLineRenderer.Clear();
#endif
        }

        /// <summary>
        /// Generates a random path for patrol behavior.
        /// Ensures creatures move naturally during PATROL state.
        /// </summary>
        protected void SetRandomPath()
        {
            startNode.SetPosition(transform.position.x, transform.position.y);
            path = creatureManager.pathFinders[(int)pathFinderType].FindRandomPath(startNode, direction, 5);
#if UNITY_EDITOR
            if (debugMode && path != null)
                pathLineRenderer.SetPoints(path);
            else
                pathLineRenderer.Clear();
#endif
        }

        protected IEnumerator MoveOnPath()
        {
            while(true)
            {
#if UNITY_EDITOR
                if (debugMode && lastStatus != status)
                {
                    Debug.Log(gameObject.name + " | " + this.name + " : is Moving");
                    lastStatus = status;
                }
#endif
                yield return new WaitForSeconds(TEMP_DELAY);

                Node node = GetNextNode();
                if (node == null)
                {
                    isChasing = false;
                    isArrived = true;
                    yield return new WaitWhile(() => path == null || path.Count == 0);
                    continue;
                }

                isChasing = true;

                soundController.StartFootstepSoundPlay(status == CreatureStatus.PURSUIT);

                yield return StartCoroutine(MoveToPosition(node));
            }
        }

        private Node GetNextNode()
        {
            Node node;

            if (path == null || path.Count == 0)
            {
                soundController.StopFootstepSoundPlay();
                return null;
            }

            node = path[0];
            if (node.X == transform.position.x && node.Y == transform.position.y)
            {
                if (path.Count == 1)
                {
                    return null;
                }

                node = path[1];
                path.RemoveAt(1);
            }
            path.RemoveAt(0);
            return node;
        }


#if UNITY_EDITOR
        CreatureStatus lastStatus = CreatureStatus.PATROL;
#endif
        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (debugMode)
            {
                if (status != lastStatus)
                {
                    Debug.Log(gameObject.name + " | " + this.name + " : updated status to " + status.ToString());
                    lastStatus = status;
                }
            }
#endif
        }

        IEnumerator CreatureUpdate()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(ACTION_DELAY);
                actions[status].Update();
            }
        }

        #region Action Definition
        protected virtual void PatrolStart()
        {
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log(gameObject.name + " | " + name + " : Patrol...");
            }
#endif
            speed = minSpeed;
        }

        protected virtual void PatrolUpdate()
        {
            detector.SetLookingDirection(direction);
            DetectPlayer();
            if (path == null || path.Count == 0)
            {
                SetRandomPath();
            }
        }

        protected virtual void PursuitStart()
        {
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log(gameObject.name + " | " + name + " : Pursuit...");
            }
#endif
            speed = maxSpeed;
        }

        protected virtual void PursuitUpdate()
        {
            detector.SetLookingDirection(direction);
            DetectPlayer();
        }

        protected virtual void AlerteStart()
        {
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log(gameObject.name + " | " + name + " : Alerted...");
            }
#endif
            speed = minSpeed;
        }

        protected virtual void AlerteUpdate()
        {
            speed = minSpeed;
            DetectPlayer();
            detector.setLookingAngle(detector.getLookingAngle() + 10f * Time.deltaTime);
            if (isArrived)
            {
                SetDirectionPath();
                isArrived = false;
            }
        }
        #endregion

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
#if UNITY_EDITOR
                if (debugMode)
                {
                    Debug.Log(gameObject.name + " | " + this.name + " : Kill Player...");
                }
#endif
                Destroy(collision.gameObject);
            }
        }
    }
}

