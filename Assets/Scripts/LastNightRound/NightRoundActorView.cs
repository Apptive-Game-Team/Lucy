using System;
using UnityEngine;

namespace Lucy.LastNightRound
{
    /// <summary>Presentation follows actual displacement, including checkpoint restores.</summary>
    public sealed class NightRoundActorView : MonoBehaviour
    {
        private SpriteRenderer visual;
        private Sprite[] frames;
        private Vector3 previous;
        private int kind, row, columns;
        private float clock, scale;

        public void Initialize(NightRoundArt art, int actorKind)
        {
            kind = actorKind; columns = 3;
            if (kind == 0)
            {
                frames = new Sprite[12];
                string[] directions = { "front", "right", "back", "left" };
                for (int r = 0; r < 4; r++) for (int c = 0; c < 3; c++)
                {
                    string path = "Lucy_ver SH/sprite/lucy_sprite_" + directions[r] + (c == 0 ? "" : "walk_" + c);
                    frames[r*3+c] = Resources.Load<Sprite>(path);
                    if (frames[r*3+c] == null) throw new InvalidOperationException("Missing Lucy frame: " + path);
                }
            }
            else
            {
                var atlas = art.Actors;
                frames = new Sprite[12];
                for (int r=0;r<4;r++) for (int c=0;c<3;c++) frames[r*3+c]=atlas[r*6+(kind-1)*3+c];
            }
            float height = 0;
            foreach (var frame in frames) height = Mathf.Max(height, frame.bounds.size.y);
            scale = (kind == 2 ? 1.8f : 1.45f) / height;
            var child = new GameObject("Character art"); child.transform.SetParent(transform, false);
            child.transform.localScale = Vector3.one * scale;
            visual = child.AddComponent<SpriteRenderer>(); visual.sprite = frames[0];
            visual.sharedMaterial = art.LitMaterial;
            previous = transform.position;
            Draw();
        }

        private void LateUpdate()
        {
            var delta = transform.position - previous; previous = transform.position;
            bool moving = delta.sqrMagnitude > .000001f && delta.sqrMagnitude < 1;
            if (NightRoundGame.Instance == null || (NightRoundGame.Instance.IsBlocked && !NightRoundGame.Instance.IsEpilogueMoving) || Time.timeScale <= 0) moving = false;
            if (moving)
            {
                bool horizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
                row = horizontal ? (delta.x < 0 ? 3 : 1) : delta.y > 0 ? 2 : 0;
                clock += Time.deltaTime * (kind == 2 ? 10 : 8);
            }
            else clock = 0;
            visual.sprite = frames[row * columns + (moving ? (int)clock % columns : 0)];
            visual.transform.localScale = Vector3.one * scale;
            Draw();
        }

        private void Draw()
        {
            // Character roots are foot positions; align every frame's lower edge.
            visual.transform.localPosition = new Vector3(0, visual.sprite.bounds.extents.y * visual.transform.localScale.y, 0);
            visual.sortingOrder = NightRoundMap.DepthAt(transform.position);
        }
    }
}
