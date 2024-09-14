using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Team6203.Util;

namespace Creature{
    public enum CreatureStatus
    {
        PATROL = 0, // 기본
        PURSUIT = 1, // 추적
        ALERTED = 2, // 의심
        AVOIDING = 3,
    }

    public class CreatureAction
    {
        protected Creature creature;
        public virtual void Play() {}
    }

    public class CreaturePatrolAction : CreatureAction
    {
        public CreaturePatrolAction(Creature creature)
        {
            this.creature = creature;
        }

        public override void Play()
        {
            creature.StartCoroutine(creature.PatrolAction());
        }
    }

    public class CreaturePursuitAction : CreatureAction
    {
        public CreaturePursuitAction(Creature creature)
        {
            this.creature = creature;
        }

        public override void Play()
        {
            creature.StartCoroutine(creature.PursuitAction());
        }
    }

    public class CreatureAlertedAction : CreatureAction
    {
        public CreatureAlertedAction(Creature creature)
        {
            this.creature = creature;
        }

        public override void Play()
        {
            creature.StartCoroutine(creature.AlertedAction());
        }
    }

    public class Creature : Actor
    {
        [SerializeField] protected bool debugMode = true;

        private SoundDetector soundDetector;
        private Detector detector;

        private ActorSoundController soundController;

        private PathLineRenderer pathLineRenderer;
        protected int[,] map;
        private Vector3 deltaPosition = new Vector3();
        private Node startNode = new Node(true);
        private Node endNode = new Node(true);
        private float moveFrame = 0.05f;

        protected PathFinderType pathFinderType = PathFinderType.DEFAULT;

        protected Vector3 targetPosition;
        protected List<Node> path;

        private bool isChasing = false;

        protected List<CreatureAction> actions;

        protected CreatureStatus status;
        private Coroutine alertedCounterCoroutine;

        protected int maxSpeed;
        protected int minSpeed;

        private LayerMask soundTargetMask;

        private const float ACTION_DELAY = 0.5f;
        private const float TEMP_DELAY = 0.01f;
        private const float ALERT_TIME = 10f;
        private void InitActions()
        {
            actions = new List<CreatureAction>()
            {
                new CreaturePatrolAction(this),
                new CreaturePursuitAction(this),
                new CreatureAlertedAction(this)
            };

        }

        private void Awake()
        {
            soundController = GetComponent<ActorSoundController>();
            InitActions();
        }

        protected void Start()
        {
            status = CreatureStatus.PATROL;
            pathLineRenderer = GetComponent<PathLineRenderer>();
            detector = GetComponent<Detector>();
            detector.SetTargetMask(LayerMask.GetMask("Player"));
            soundDetector = GetComponent<SoundDetector>();
            soundTargetMask = LayerMask.GetMask("Door") | LayerMask.GetMask("Player");
            soundDetector.SetTargetMask(soundTargetMask);
        }

        public virtual IEnumerator PatrolAction()
        {

#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log(gameObject.name + " | " + this.name + " : Patrol...");
            }
#endif
            speed = minSpeed;
            DetectPlayer();
            yield return new WaitForSeconds(ACTION_DELAY);
            actions[(int)status].Play();
        }

        public virtual IEnumerator PursuitAction()
        {
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log(gameObject.name + " | " + this.name + " : Pursuit...");
            }
#endif
            speed = maxSpeed;
            DetectPlayer();
            yield return new WaitForSeconds(ACTION_DELAY);
            GetPathToPosition(targetPosition);
            actions[(int)status].Play();
        }

        public virtual IEnumerator AlertedAction()
        {
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log(gameObject.name + " | " + this.name + " : Alerted...");
            }
#endif
            speed = minSpeed;
            DetectPlayer();
            detector.setLookingAngle(detector.getLookingAngle() + 10f);
            yield return new WaitForSeconds(ACTION_DELAY);
            SetRandomPath();
            actions[(int)status].Play();
        }

        public IEnumerator AlertedCounter()
        {
            yield return new WaitForSeconds(ALERT_TIME);
            if (status.Equals(CreatureStatus.ALERTED))
                status = CreatureStatus.PATROL;
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
                status = CreatureStatus.PURSUIT;
                Vector3 detectedPlayerPosition = detectedPlayerCollider[0].transform.position;
                targetPosition = detectedPlayerPosition;
            } else if (status.Equals(CreatureStatus.PURSUIT) && !isChasing)
            { 
                status = CreatureStatus.ALERTED;
                if (alertedCounterCoroutine != null)
                {
                    StopCoroutine(alertedCounterCoroutine);
                    alertedCounterCoroutine = null;
                }
                alertedCounterCoroutine = StartCoroutine(AlertedCounter());
            }
        }

        protected void GetPathToPosition(Vector3 targetPosition)
        {
            path = FindPath(targetPosition.x, targetPosition.y);
        }


        protected virtual List<Node> FindPath(float x, float y)
        {
            startNode.SetPosition(transform.position.x, transform.position.y);
            endNode.SetPosition(x, y);
            
            try
            {
                List<Node> path = CreatureManager.Instance.pathFinders[(int)pathFinderType].FindPath(startNode, endNode);
                if (debugMode)
                {
                    pathLineRenderer.SetPoints(path);
                }
                return path;
            }
            catch (Exception e)
            {
                print(e.Message);
                return null;
            }
        }

        protected virtual void SetRandomPath()
        {
            path = CreatureManager.Instance.pathFinders[(int)pathFinderType].GetRandomPath(10, deltaPosition, Vector3ToVector3Int(transform.position));
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

                Node node;

                if (path == null || path.Count == 0)
                {
                    soundController.StopFootstepSoundPlay();
                    isChasing = false;
                    yield return new WaitForSeconds(ACTION_DELAY);
                    continue;
                }

                node = path[0];
                if (node.X == transform.position.x && node.Y == transform.position.y)
                {
                    if (path.Count == 1)
                    {
                        isChasing = false;
                        yield return new WaitForSeconds(ACTION_DELAY);
                        continue;
                    }
                        
                    node = path[1];
                    path.RemoveAt(1);
                }
                path.RemoveAt(0);
                

                isChasing = true;

                float deltaX = node.X - transform.position.x;
                float deltaY = node.Y - transform.position.y;
                deltaPosition.Set(deltaX, deltaY, 0);
                deltaX = Math.Abs(deltaX);
                deltaY = Math.Abs(deltaY);

                detector.SetLookingDirection(deltaPosition);

                deltaPosition.Normalize();
                soundController.StartFootstepSoundPlay(status == CreatureStatus.PURSUIT);
                while (deltaX > 0 || deltaY > 0)
                {
                    deltaX -= Math.Abs(deltaPosition.x * speed * moveFrame);
                    deltaY -= Math.Abs(deltaPosition.y * speed * moveFrame);
                    transform.position += (deltaPosition * speed * moveFrame);
                    yield return new WaitForSeconds(moveFrame);
                }
                transform.position = new Vector3(node.X, node.Y);
            }
        }

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

        protected Vector3Int Vector3ToVector3Int(Vector3 vector)
        {
            return new Vector3Int(
                Mathf.RoundToInt(vector.x),
                Mathf.RoundToInt(vector.y),
                Mathf.RoundToInt(vector.z)
            );
        }
#if UNITY_EDITOR
        CreatureStatus lastStatus = CreatureStatus.PATROL;

        protected void Update()
        {

            if (debugMode)
            {
                if (status != lastStatus)
                {
                    Debug.Log(gameObject.name + " | " + this.name + " : updated status to " + status.ToString());
                    lastStatus = status;
                }
            }
        }
#endif
    }
}

