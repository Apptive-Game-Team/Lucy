using System;
using System.Collections;
using System.Collections.Generic;
using Creature;
using UnityEngine;
using static Team6203.Util;

namespace Creature{
    public enum CreatureStatus
    {
        PATROL = 0, // 기본
        PURSUIT = 1, // 추적
        ALERTED = 2, // 의심
        AVOIDING = 3,
        STUNNED = 3,
    }

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

        protected void DetectPlayer()
        {
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log(gameObject.name + " | " + this.name + " : Detecting Player...");
            }
#endif
            List<Collider2D> detectedPlayerCollider = ConcatenateListWithoutDuplicates(detector.DetectByView(), soundDetector.Detect());

            if (detectedPlayerCollider.Count > 0)
            {
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

