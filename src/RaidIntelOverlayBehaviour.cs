using UnityEngine;

namespace RaidIntelOverlay
{
    internal sealed class RaidIntelOverlayBehaviour : MonoBehaviour
    {
        private GUIStyle _boxStyle;
        private GUIStyle _contentStyle;
        private string _content = string.Empty;
        private bool _visible;
        private float _refreshTimer;
        private Vector2 _scrollPosition;

        private const float MaxHeightScreenFraction = 0.72f;
        private const float MinWidth = 260f;
        private const float MaxWidth = 460f;
        private const float HorizontalMargin = 24f;
        private const float InnerPadding = 8f;

        public bool IsVisible => _visible;

        public void Toggle()
        {
            _visible = !_visible;
            if (_visible)
            {
                RefreshNow();
                ScheduleNextRefresh();
            }
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (_visible)
            {
                RefreshNow();
                ScheduleNextRefresh();
            }
        }

        private void Update()
        {
            if (!_visible || PluginCore.Instance == null)
            {
                return;
            }

            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer > 0f)
            {
                return;
            }

            RefreshNow();
            ScheduleNextRefresh();
        }

        private void RefreshNow()
        {
            var tracked = PluginCore.Instance.TrackedItems.TemplateIds;
            var fikaSync = RaidIntelFikaSync.Instance;
            var isRaidClient = fikaSync.IsRaidClient();
            RaidIntelPresenceSnapshot remotePresence = null;

            if (isRaidClient)
            {
                remotePresence = fikaSync.GetRemotePresenceForDisplay();
            }

            var snapshot = RaidIntelCollector.Collect(tracked, remotePresence);
            _content = RaidIntelFormatter.Format(
                snapshot,
                isRaidClient,
                fikaSync.HasRemotePresence,
                fikaSync.IsRemotePresenceStale,
                fikaSync.IsRaidHost());
        }

        private void ScheduleNextRefresh()
        {
            float seconds = Random.Range(2.5f, 4f);
            _refreshTimer = Mathf.Round(seconds * 100f) / 100f;
        }

        private void OnGUI()
        {
            if (!_visible || string.IsNullOrEmpty(_content))
            {
                return;
            }

            EnsureStyles();
            GUI.depth = -10000;

            var content = new GUIContent(_content);
            float innerWidth = MaxWidth - (InnerPadding * 2f) - 20f;
            float contentHeight = _contentStyle.CalcHeight(content, innerWidth);
            float width = Mathf.Clamp(_contentStyle.CalcSize(content).x + (InnerPadding * 2f) + 8f, MinWidth, MaxWidth);
            innerWidth = width - (InnerPadding * 2f) - 20f;
            contentHeight = _contentStyle.CalcHeight(content, innerWidth);

            float maxHeight = Screen.height * MaxHeightScreenFraction;
            float viewHeight = Mathf.Min(contentHeight + (InnerPadding * 2f), maxHeight);
            float x = HorizontalMargin;
            float y = (Screen.height - viewHeight) * 0.5f;

            var outerRect = new Rect(x, y, width, viewHeight);
            GUI.Box(outerRect, GUIContent.none, _boxStyle);

            GUI.BeginGroup(outerRect);

            var scrollViewRect = new Rect(
                InnerPadding,
                InnerPadding,
                width - (InnerPadding * 2f),
                viewHeight - (InnerPadding * 2f));

            var scrollContentRect = new Rect(0f, 0f, scrollViewRect.width - 18f, contentHeight);
            _scrollPosition = GUI.BeginScrollView(scrollViewRect, _scrollPosition, scrollContentRect);
            GUI.Label(new Rect(0f, 0f, scrollContentRect.width, contentHeight), _content, _contentStyle);
            GUI.EndScrollView();

            GUI.EndGroup();
        }

        private void EnsureStyles()
        {
            if (_boxStyle != null && _contentStyle != null)
            {
                return;
            }

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(0, 0, 0, 0)
            };

            _contentStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 14,
                richText = true,
                wordWrap = true,
                padding = new RectOffset(4, 4, 2, 2)
            };
        }
    }
}
